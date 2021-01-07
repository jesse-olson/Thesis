using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Spindle : Technique {
    public bool spindleAndWheel = false;
    public GameObject interactionObject;

    // Update is called once per frame
    void Update () {
        setPositionOfInteraction();

    }

    void setPositionOfInteraction()
    {

        Vector3 midPoint = (leftController.transform.position + rightController.transform.position) / 2f;
        interactionObject.transform.position = midPoint;

        interactionObject.transform.LookAt(rightController.transform);

        Vector3 rotation;
        if(spindleAndWheel) {
            rotation = new Vector3(0, 0, interactionObject.transform.eulerAngles.z + rightController.transform.eulerAngles.z);
        } else {
            rotation = new Vector3(0,0,0);
        }
        

        interactionObject.transform.Rotate(rotation);

    }
}
