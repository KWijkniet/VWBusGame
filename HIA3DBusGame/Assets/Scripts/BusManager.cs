using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class BusManager : MonoBehaviour {
    //Store objects and data
    public GameObject bus;
    public float breakStep = 0.01f;
    public float driveStep = 0.005f;
    public float maxSpeed = 100;
    public UIManager ui;

    //store car controller reference (from unity's standard assets)
    private UnityStandardAssets.Vehicles.Car.CarController aiControl;
    
    private bool breakCar = false;
    private bool driveCar = false;

    private void Start()
    {
        //set bus active by default
        bus.SetActive(true);
        //store reference of the car controller
        aiControl = bus.GetComponent<UnityStandardAssets.Vehicles.Car.CarController>();
    }

    private void Update()
    {
        //check if reference is found
        if(aiControl != null)
        {
            //update display
            ui.SetSpeedDisplay(aiControl.CurrentSpeed);
            //check if break button is pressed
            if (breakCar && !driveCar)
            {
                //slow down the car by decreasing max speed (ai system will automatically slow down and try to stay below max speed)
                float step = breakStep * Time.deltaTime;
                aiControl.MaxSpeed -= aiControl.MaxSpeed - step >= 0 ? step : 0;
            }
            //check if drive button is pressed
            else if (!breakCar && driveCar)
            {
                //add to max speed. (ai system will automatically speed up and try to reach max speed)
                float step = driveStep * Time.deltaTime;
                aiControl.MaxSpeed += aiControl.MaxSpeed + step <= maxSpeed ? step : 0;
            }
            //both/none are pressed
            else
            {
                //slowly slow down the car by decreasing max speed (ai system will automatically slow down and try to stay below max speed)
                float step = breakStep / 4 * Time.deltaTime;
                aiControl.MaxSpeed -= aiControl.MaxSpeed - step >= 0 ? step : 0;
            }
        }
    }

    //update bool when button is pressed
    public void Break(bool isPressed)
    {
        breakCar = isPressed;
    }

    //update bool when button is pressed
    public void Drive(bool isPressed)
    {
        driveCar = isPressed;
        //start game when drive is pressed for the first time
        ui.startTimer = true;
    }
}
