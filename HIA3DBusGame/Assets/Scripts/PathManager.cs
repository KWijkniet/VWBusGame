using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
    TODO:
    -Fix mesh. make it 3D with custom height
    -Create Enviroment (lanternpoles, trees, rails in turns)
    -Create respawn system if car goes below lowest part of the track (+ respawn button)
    -Remove old codes and files
    -Toggle edit mode (selected by UI buttons)(see "WaypointManager.cs")
    -In edit mode be able to move waypoints (wich have a flag as child)(see "WaypointManager.cs")
    -In edit mode be able to delete waypoint by holding a flag for 2 seconds(see "WaypointManager.cs")

    Bugs / Problems:
    -Spawns too many flags (see "WaypointManager.cs")

    IDEA:
    -with flags (start/finnish) create distance between waypoint and start/end of the mesh. then add flags with "start" and "finnish" these can act as "teleporters" (this is good for when the LOOP PATH is turned off) (maybe even with a restart button)
    -Timer to show how fast you completed the track
    -crashing (when going to fast through turns) (1 second delay before respawning where it was last on track with speed of 0)

*/

[System.Serializable]
public class PathSettings
{
    public List<GameObject> waypoints;
    public GameObject markerPrefab;
    public GameObject emptyPrefab;
    public bool loopPath = false;
    [Range(0, 15)] public int strength = 1;

    [HideInInspector] public List<GameObject> points;
    [HideInInspector] public float step = 0.01f;
    
    private List<GameObject> _points;
    private GameObject _markerPrefab;
    private GameObject _emptyPrefab;
    private bool _loopPath;
    private float _step;
    private int _strength;

    public bool HasChanged()
    {
        if ((points != _points) ||
            (markerPrefab != _markerPrefab) ||
            (emptyPrefab != _emptyPrefab) ||
            (loopPath != _loopPath) ||
            (loopPath != _loopPath) ||
            (step != _step) ||
            (strength != _strength))
        {
            _points = points;
            _markerPrefab = markerPrefab;
            _emptyPrefab = emptyPrefab;
            _loopPath = loopPath;
            _step = step;
            return true;
        }
        return false;
    }

    public bool HasChangedStrength()
    {
        if (strength != _strength)
        {
            _strength = strength;
            return true;
        }
        return false;
    }
}

public class PathManager : MonoBehaviour {
    public static PathManager instance;

    public PathSettings settings;

    //hide variables but keep public so other scripts can access them
    [HideInInspector] public List<GameObject> pathObjects = new List<GameObject>();
    [HideInInspector] public List<GameObject> pathCenters = new List<GameObject>();
    [HideInInspector] public List<Vector3> pointsCopy = new List<Vector3>();
    [HideInInspector] public Vector3[] controlVerts = new Vector3[0];
    [HideInInspector] public bool hasMesh = false;
    //mesh and mesh data
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    //create instance
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        //create new mesh and assign it to the filter
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        //keep track of waypoints original position (so we later can check if the position has changed)
        for (int i = 0; i < settings.waypoints.Count; i++)
        {
            pointsCopy.Add(settings.waypoints[i].transform.position);
        }
    }

    private void Update()
    {
        //if settings isnt set then return
        if(settings == null) { return; }
        //check if points have moved or no pathCenters have been found or settings have been changed
        if (PointsMoved() || pathCenters.Count <= 0 || settings.HasChanged())
        {
            //if strength setting has changed then fix points.
            if (settings.HasChangedStrength()) { FixPoints(); }
            //clear old data
            ClearPathObjects();
            //fix new list
            controlVerts = FixList();

            float time = 0;

            //Create Parent container
            GameObject empty = Instantiate(settings.emptyPrefab);
            empty.transform.position = Vector3.zero;
            empty.name = "BezierPointsParent";

            //create path
            while (time <= 1)
            {
                time += settings.step;
                Vector3 pos = GetPoint(time, controlVerts);

                //stop if distance between points is smaller then 0.5f a less then 2f from a waypoint.
                if (pathCenters.Count > 0 && Vector3.Distance(pathCenters[pathCenters.Count - 1].transform.position, pos) < 0.5f) { continue; }
                if (Vector3.Distance(settings.waypoints[0].transform.position, pos) < 2f) { continue; }

                //Center marker
                GameObject obj = Instantiate(settings.markerPrefab);
                obj.transform.parent = empty.transform;
                obj.transform.position = pos;
                
                //rotate to face correct direction
                if (pathObjects.Count == 0)
                {
                    obj.transform.LookAt(settings.points[0].transform);
                    obj.transform.Rotate(new Vector3(0, 180, 0));
                }
                else
                {
                    obj.transform.LookAt(pathCenters[pathCenters.Count - 1].transform);
                    obj.transform.Rotate(new Vector3(0, 180, 0));
                }
                pathCenters.Add(obj);

                //Left marker
                GameObject objLeft = Instantiate(settings.markerPrefab);
                objLeft.transform.parent = obj.transform;
                objLeft.transform.localPosition = Vector3.zero - objLeft.transform.right * 50 /*15*/;
                objLeft.transform.localRotation = Quaternion.identity;
                objLeft.name = "Left";
                pathObjects.Add(objLeft);

                //Right marker
                GameObject objRight = Instantiate(settings.markerPrefab);
                objRight.transform.parent = obj.transform;
                objRight.transform.localPosition = Vector3.zero + objRight.transform.right * 50 /*15*/;
                objRight.transform.localRotation = Quaternion.identity;
                objRight.name = "Right";
                pathObjects.Add(objRight);
            }
            GenerateMesh();
        }
    }

    public Vector3 GetPoint(float t, Vector3[] controlVerts)
    {
        return GetPointHelper(t, controlVerts);
    }

    private Vector3 GetPointHelper(float t, Vector3[] verts)
    {
        if (verts.Length == 1) return verts[0];
        if (verts.Length == 2) return Vector3.Lerp(verts[0], verts[1], t);
        
        Vector3[] reducedArray = new Vector3[verts.Length - 1];
        for (int i = 0; i < reducedArray.Length; i++)
        {
            reducedArray[i] = Vector3.Lerp(verts[i], verts[i + 1], t);
        }
        return GetPointHelper(t, reducedArray);
    }

    private Vector3[] FixList()
    {
        Vector3[] controlVerts = new Vector3[settings.loopPath ? settings.points.Count + 1 : settings.points.Count];
        for (int i = 0; i < settings.points.Count; i++)
        {
            controlVerts[i] = settings.points[i].transform.position;
        }

        if (settings.loopPath)
        {
            controlVerts[controlVerts.Length - 1] = settings.points[0].transform.position;
        }

        return controlVerts;
    }

    private void ClearPathObjects()
    {
        if(pathCenters.Count <= 0) { return; }
        Destroy(pathCenters[0].transform.parent.gameObject);
        pathObjects.Clear();
        pathCenters.Clear();
    }

    private bool PointsMoved()
    {
        for(int i = 0; i < pointsCopy.Count; i++)
        {
            if(pointsCopy[i] != settings.waypoints[i].transform.position)
            {
                pointsCopy[i] = settings.waypoints[i].transform.position;
                return true;
            }
        }
        return false;
    }
    
    private void FixPoints()
    {
        settings.points.Clear();

        for (int i = 0; i < settings.waypoints.Count; i++)
        {
            settings.points.Add(settings.waypoints[i]);
            if (settings.waypoints[i].transform.childCount != settings.strength)
            {
                if (settings.waypoints[i].transform.childCount > 0)
                {
                    for (int r = settings.waypoints[i].transform.childCount - 1; r >= 0; r--)
                    {
                        Destroy(settings.waypoints[i].transform.GetChild(r).gameObject);
                    }
                }
                for (int r = 0; r < settings.strength; r++)
                {
                    GameObject obj = Instantiate(settings.emptyPrefab);
                    obj.transform.parent = settings.waypoints[i].transform;
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;

                    settings.points.Add(obj);
                }
            }
        }
    }

    private void GenerateMesh()
    {
        vertices = new Vector3[pathObjects.Count];

        for (int m = 0; m < pathObjects.Count; m++)
        {
            vertices[m] = pathObjects[m].transform.position;
        }

        //disable this
        if (settings.loopPath)
        {
            vertices[vertices.Length - 1] = pathObjects[1].transform.position;
            vertices[vertices.Length - 2] = pathObjects[0].transform.position;
        }
        //

        int vert = 0;
        int tris = 0;
        int loopTimes = pathObjects.Count / 2 - 1;
        triangles = new int[vertices.Length * 6];
        for (int i = 0; i < loopTimes; i++)
        {
            for (int w = 0; w < 1; w++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + 1 + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + 1 + 1;
                triangles[tris + 5] = vert + 1 + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
        
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        mesh.uv = uvs;
        mesh.RecalculateNormals();
        hasMesh = true;
    }
}
