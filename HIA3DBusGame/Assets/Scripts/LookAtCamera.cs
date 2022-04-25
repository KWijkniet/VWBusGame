using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//script that rotates the object that this script is on to always face the camera
public class LookAtCamera : MonoBehaviour
{
    //store target transform
    private Transform target;

    void Start()
    {
        //get target transform via getting the main camera
        target = Camera.main.transform;
    }
    
    void Update()
    {
        //make the object automatically face the camera
        transform.LookAt(target);
    }
}
