using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracker : MonoBehaviour
{
    public UIManager ui;

    private int index = 1;
    private PathManager pm;

    private void Start()
    {
        pm = PathManager.instance;
    }

    private void Update()
    {
        if (pm == null) { return; }
        transform.position = pm.pathCenters[index].transform.position;
        transform.LookAt(pm.pathCenters[index - 1].transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(TriggerAction(other, false));
    }

    private void OnTriggerStay(Collider other)
    {
        StartCoroutine(TriggerAction(other, true));
    }

    private IEnumerator TriggerAction(Collider other, bool triggerStay)
    {
        if (other.transform.root.gameObject.tag == "Player")
        {
            index++;
            if (index >= pm.pathCenters.Count)
            {
                index = 1;
            }
            if (index == 2)
            {
                ui.CompleteTrack();
            }

            GetComponent<BoxCollider>().enabled = false;
            yield return new WaitForSeconds(.1f);
            GetComponent<BoxCollider>().enabled = true;
        }
    }
}
