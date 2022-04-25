using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour{

    //todo:
    //add toggle editor option
    //replace waypoints with flags (see "WaypointManager.cs")

    //get object references
    public Text raceTimerDisplay;
    public Text speedDisplay;
    public GameObject historyContainer;
    public GameObject historyPrefab;
    [HideInInspector] public bool startTimer;

    //store object values
    private float raceTimer;
    private float prevTimer;
    private string raceTimeText;
    private bool firstRound;

    private void Start()
    {
        //at begin its always first round (should be reset when track has changed)
        firstRound = true;
    }

    private void Update()
    {
        //can start timer?
        if (startTimer)
        {
            //increase timer
            raceTimer += Time.deltaTime;

            //convert float to correct format
            string[] parts = raceTimer.ToString().Split('.');
            string part01 = parts[0];
            string part02 = parts[1].Length > 1 ? parts[1].Substring(0, 2) : "00";

            //show correct format
            raceTimeText = part01 + ":" + part02;
            raceTimerDisplay.text = raceTimeText;
        }
    }

    public void CompleteTrack()
    {
        //check if firstRound is false
        if (!firstRound)
        {
            //show timer in history.
            GameObject obj = Instantiate(historyPrefab, historyContainer.transform);
            obj.transform.SetAsFirstSibling();
            if (prevTimer <= raceTimer && prevTimer != 0)
            {
                obj.GetComponent<Text>().text = "- " + raceTimeText;
                obj.GetComponent<Text>().color = Color.red;
            }
            else
            {
                obj.GetComponent<Text>().text = "+ " + raceTimeText;
                obj.GetComponent<Text>().color = Color.green;
            }
            prevTimer = raceTimer;
            raceTimer = 0;
        }
        else
        {
            //start timer by setting firstRound to false
            firstRound = false;
        }
    }

    public void SetSpeedDisplay(float _amount)
    {
        //show current speed
        speedDisplay.text = (Mathf.CeilToInt(_amount) == 1 ? 0 : Mathf.CeilToInt(_amount)).ToString() + "KM/u";
    }
}
