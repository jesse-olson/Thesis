/*
 *  Script to be attatched to controllers to allow the flashlight
 *  for the VR technique for the Flaslight to pick up objects as described
 *  for the flaslight for the HTC Vive.
 *  
 *  Copyright(C) 2018  Ian Hanan
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.If not, see<http://www.gnu.org/licenses/>.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

/*
 *  Keeping info on things I have done here for now
 *  So that each flashlight has no collision with eachother put them on seperate layers
*/
public class FlashlightSelection : Technique {

    public GameObject selection; // holds the selected object

    private List<GameObject> collidingObjects;
    private GameObject objectInHand;
    public GameObject objectHoveredOver;

    // Checks if holding object in hand
    public bool HoldingObject() {
        return objectInHand != null;
    }

    void OnEnable() {
        collidingObjects = new List<GameObject>();
        trackedObj = this.transform.gameObject;
        var render = SteamVR_Render.instance;
        if (render == null) {
            enabled = false;
            return;
        }
    }

    void Awake() {
        InitializeControllers();

        //trackedObj = GetComponent<SteamVR_TrackedObject>();
        if (interactionType == InteractionType.Manipulation_UI) {
            this.gameObject.AddComponent<SelectionManipulation>();
            this.GetComponent<SelectionManipulation>().trackedObj = trackedObj;
#if SteamVR_2
            this.GetComponent<SelectionManipulation>().m_controllerPress = m_controllerPress;
#endif
        }
    }

    /**
     * Adds the object involved in a trigger event 
     * to the collidingObjects list
     */
    private bool AddCollidingObject(Collider col) {
        if (!collidingObjects.Contains(col.gameObject)) {
            if (interactionLayers == (interactionLayers | (1 << col.gameObject.layer)))
            {
                collidingObjects.Add(col.gameObject);
                return true;
            }
        }
        return false;
    }

    private bool RemoveCollidingObject(Collider collider)
    {
        if (!collidingObjects.Contains(collider.gameObject))
        {
            collidingObjects.Remove(collider.gameObject);
            return true;
        }
        return false;
    }


    // Trigger events
    public void OnTriggerEnter(Collider other) {
        AddCollidingObject(other);
    }


    public void OnTriggerStay(Collider other) {
        AddCollidingObject(other);
    }


    public void OnTriggerExit(Collider other) {
        RemoveCollidingObject(other);
    }

    private GameObject GetObjectHoveringOver() {
        Vector3 controllerForward = trackedObj.transform.forward;
        float controllerForwardMagnitude = Vector3.Magnitude(controllerForward);

        double smallest = double.PositiveInfinity;
        GameObject closestObject = null;

        foreach (GameObject potentialObject in collidingObjects) {
            // Using vector algebra to get shortest distance between object and vector 
            Vector3 forwardControllerToObject = trackedObj.transform.position - potentialObject.transform.position;
            float distanceBetweenRayAndPoint = Vector3.Magnitude(Vector3.Cross(forwardControllerToObject, controllerForward)) / controllerForwardMagnitude;

            if( smallest > distanceBetweenRayAndPoint)
            {
                smallest = distanceBetweenRayAndPoint;
                closestObject = potentialObject;
            }                
        }

        onUnhover.Invoke();
        return closestObject;
    }

    void GrabObject(GameObject toGrab) {
        objectInHand = toGrab;

        if (objectHoveredOver != null) {
            collidingObjects.Remove(objectInHand);

            var joint = AddFixedJoint();
            objectInHand.GetComponent<Rigidbody>().velocity = Vector3.zero; // Setting velocity to 0 so can catch without breakforce effecting it
            joint.connectedBody = objectInHand.GetComponent<Rigidbody>();
        }
    }

    private FixedJoint AddFixedJoint() {
        FixedJoint fx = gameObject.AddComponent<FixedJoint>();
        fx.breakForce = 1000;
        fx.breakTorque = Mathf.Infinity;
        return fx;
    }

    private void ReleaseObject() {
        if (GetComponent<FixedJoint>()) {

            GetComponent<FixedJoint>().connectedBody = null;
            Destroy(GetComponent<FixedJoint>());
            
            // TODO: Fix this so that it throws with the correct velocity applied
            //objectInHand.GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity;
            //objectInHand.GetComponent<Rigidbody>().angularVelocity = GetComponent<Rigidbody>().angularVelocity;


            //objectInHand.GetComponent<Rigidbody>().velocity = Controller.velocity;
            //objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
#if SteamVR_Legacy
            objectInHand.GetComponent<Rigidbody>().velocity = Controller.velocity * Vector3.Distance(Controller.transform.pos, objectInHand.transform.position);
            objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
#elif SteamVR_2
            objectInHand.GetComponent<Rigidbody>().velocity = trackedObj.GetVelocity() * Vector3.Distance(trackedObj.transform.position, objectInHand.transform.position);
            objectInHand.GetComponent<Rigidbody>().angularVelocity = trackedObj.GetAngularVelocity();
#endif

        }

        objectInHand = null;
    }

    // Update is called once per frame
    void Update() {
        objectHoveredOver = GetObjectHoveringOver();
        onHover.Invoke();

        ControllerState currentState = ControllerEvents();

        if (currentState == ControllerState.TRIGGER_DOWN) {
            if (!HoldingObject())
            {
                    if (interactionType == InteractionType.Selection)
                    {
                        // Pure selection
                        print("selected " + objectHoveredOver);

                    }
                    else if (interactionType == InteractionType.Manipulation_Full)
                    {
                        //Manipulation
                        GrabObject(objectHoveredOver);
                    }
                    else if (interactionType == InteractionType.Manipulation_UI && this.GetComponent<SelectionManipulation>().inManipulationMode == false)
                    {
                        this.GetComponent<SelectionManipulation>().selectedObject = objectHoveredOver;
                    }

                    selection = objectHoveredOver;
                    onSelectObject.Invoke();
            }
        }


        if (currentState == ControllerState.TRIGGER_UP) {
            if (HoldingObject()) {
                ReleaseObject();

                selection = null;
            }
        }
    }
}
