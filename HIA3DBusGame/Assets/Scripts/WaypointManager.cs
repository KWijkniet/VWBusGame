using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public GameObject flagPrefab;       //store prefab
    public GameObject emptyPrefab;      //store prefab

    private Vector2 lastPos;            //store last position of touch
    private Camera cam;                 //store camera reference
    private float timer = 0;            //store timer value

    private GameObject storedObj;       //store selected flag.
    private float storedYAxis;          //store selected flag begin Y Axis.

    private void Start()
    {
        //get camera reference
        cam = Camera.main;
    }
    
    void Update()
    {
        //check if pointer is over ui AND pathmanager is found AND user is touching the screen THEN reset timer and dont execute the other code.
        if (IsPointerOverUIObject() || PathManager.instance == null || Input.touches.Length == 0) { timer = 0; return; }
        if (Input.touches[0].fingerId == 0)
        {
            //set isTap to true by default
            bool isTap = true;
            //increase timer
            timer += Time.deltaTime;

            //create raycast from tap position on screen to world position
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                //check fase (if fase = begin)
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    lastPos = Input.GetTouch(0).position;
                    storedObj = hit.collider.gameObject;
                    storedYAxis = storedObj.transform.position.y;
                    //store gameobject touched (if its a flag)
                }

                //check fase (if fase = moved)
                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    isTap = false;
                    
                    if (Vector2.Distance(lastPos, Input.GetTouch(0).position) >= 5)
                    {
                        //move stored gameobject touched
                        if (hit.collider.gameObject.name == "flag")
                        {
                            Debug.Log("Moving");
                            storedObj.transform.position = new Vector3(hit.point.x, storedYAxis, hit.point.z);
                        }
                    }
                }

                //check fase (if fase = end)
                if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    //check if its a tap and it didnt move
                    if (isTap && Vector2.Distance(lastPos, Input.GetTouch(0).position) < 5)
                    {
                        //create and add point to waypoint list.
                        GameObject obj = Instantiate(emptyPrefab);
                        obj.transform.position = hit.point;
                        PathManager.instance.settings.waypoints.Add(obj);
                    }
                    //check if its a tap and it didnt move BUT it was hold longer than 2 seconds
                    else if (isTap && Vector2.Distance(lastPos, Input.GetTouch(0).position) < 5 && timer >= 2)
                    {
                        //delete object

                        //Delete stored gameobject touched (if its a flag)
                        if (hit.collider.gameObject.name == "flag")
                        {
                            PathManager.instance.settings.waypoints.Remove(storedObj);
                            Destroy(storedObj);
                        }
                    }
                    //reset timer
                    timer = 0;
                }
            }
        }
        FixWaypoints();
    }

    //give every waypoint a flag as indication of waypoint in edit mode.
    private void FixWaypoints()
    {
        PathManager pm = PathManager.instance;
        for (int i = 0; i < pm.settings.waypoints.Count; i++)
        {
            GameObject waypoint = pm.settings.waypoints[i];
            if (waypoint.transform.childCount > 0 && waypoint.transform.GetChild(0).GetComponent<Tracker>() == null && waypoint.transform.GetChild(0).name != "Flag")
            {
                GameObject obj = Instantiate(flagPrefab);
                obj.name = "Flag";
                obj.transform.position = waypoint.transform.position;
                obj.transform.rotation = Quaternion.identity;
                obj.transform.SetParent(waypoint.transform);
                obj.transform.SetAsFirstSibling();
            }
        }
    }

    //check if pointer/click is on ui or not
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}