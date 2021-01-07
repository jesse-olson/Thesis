/* Depth Ray implementation by Kieran May
 * University of South Australia
 *
 *  Copyright(C) 2019 Kieran May
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

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using Valve.VR;

public class DepthRay : ScrollingTechnique {
    private static readonly float LASER_LENGTH = 100f;

    private GameObject cubeAssister;
    private RaycastHit[] oldHits;

    public enum SelectionAssister { Hide_Closest_Only, Hide_All_But_Closest };

    public GameObject currentClosestObject;

    public Material outlineMaterial;
    public Material defaultMat;
    private Material currentClosestObjectMaterial;

    private GameObject selectedObject;
    private bool objectBeingHeld;

    void Awake() {
        cubeAssister = transform.Find("Cube Assister").gameObject;

        InitializeControllers();

        if (interactionType == InteractionType.Manipulation_UI) {
            this.gameObject.AddComponent<SelectionManipulation>();
            this.GetComponent<SelectionManipulation>().trackedObj = trackedObj;
        }
    }

    void Start() {
        laser = Instantiate(laserPrefab);
        laser.SetActive(true);
        laserTransform = laser.transform;
        laserTransform.SetParent(trackedObj.transform);
        laserTransform.localScale = new Vector3(1, 1, LASER_LENGTH);

        cubeAssister.transform.SetParent(trackedObj.transform);
        cubeAssister.transform.localPosition = new Vector3(0, 0, extendDistance);
    }
    void Update()
    {
        PadScrolling();
        MoveCubeAssister();

        if (!objectBeingHeld)
        {
            RaycastHit[] hits =
                Physics.RaycastAll(trackedObj.transform.position, trackedObj.transform.forward, LASER_LENGTH, interactionLayers);
            GameObject closestObject = ClosestObject(hits);
            currentClosestObject = closestObject;
            PickupObject(closestObject);
            oldHits = hits;
        }
        else if (ControllerEvents() == ControllerState.TRIGGER_UP)
        {
            if (interactionType == InteractionType.Manipulation_Movement || interactionType == InteractionType.Manipulation_Full)
            {
                selectedObject.transform.SetParent(null);
            }
            selectedObject = null;
            onDropObject.Invoke();
            objectBeingHeld = false;

        }

    }
    
    void PickupObject(GameObject obj) {
        if(ControllerEvents() == ControllerState.TRIGGER_DOWN)
        {
            selectedObject = obj; // Storing the object as an instance variable instead of using the obj parameter fixes glitch of it not properly resetting on TriggerUp
            switch (interactionType)
            {
                case InteractionType.Selection:
                    print("Selected object in pure selection mode:" + selectedObject.name);
                    break;

                case InteractionType.Manipulation_Movement:
                case InteractionType.Manipulation_Full:
                    obj.transform.SetParent(trackedObj.transform);
                    objectBeingHeld = true;
                    break;

                case InteractionType.Manipulation_UI:
                    objectSelected = true;
                    GetComponent<SelectionManipulation>().selectedObject = obj;
                    break;
            }
            onSelectObject.Invoke();
        }
    }


    void MoveCubeAssister() {
        cubeAssister.transform.localPosition = new Vector3(0, 0,  extendDistance);
    }
    
    private GameObject ClosestObject(RaycastHit[] hits) {
        GameObject closestObject = null;
        float closestDist = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            //hit.transform.gameObject.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0.5f);
            float dist = Vector3.Distance(cubeAssister.transform.position, hit.point);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestObject = hit.transform.gameObject;
            }
        }
        return closestObject;
    }

    private void ResetAllMaterials() {
        if (oldHits != null) {
            foreach (RaycastHit hit in oldHits) {
                hit.transform.gameObject.GetComponent<Renderer>().material = defaultMat;
            }
        }
    }

//    void GrabObject(GameObject toGrab)
//    {
//        lastSelectedObject = toGrab;

//        lastSelectedObject.GetComponent<Rigidbody>().velocity = Vector3.zero; // Setting velocity to 0 so can catch without breakforce effecting it
//        var joint = AddFixedJoint();
//        joint.connectedBody = lastSelectedObject.GetComponent<Rigidbody>();
//    }

//    private FixedJoint AddFixedJoint()
//    {
//        FixedJoint fx = trackedObj.AddComponent<FixedJoint>();
//        fx.breakForce = 1000;
//        fx.breakTorque = Mathf.Infinity;
//        return fx;
//    }

//    private void ReleaseObject()
//    {
//        FixedJoint joint = trackedObj.GetComponent<FixedJoint>();
//        if (joint)
//        {

//            joint.connectedBody = null;
//            Destroy(joint);
//#if SteamVR_Legacy
//            lastSelectedObject.GetComponent<Rigidbody>().velocity = Controller.velocity;
//            lastSelectedObject.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
//#elif SteamVR_2
//            lastSelectedObject.GetComponent<Rigidbody>().velocity = trackedObj.GetVelocity();
//            lastSelectedObject.GetComponent<Rigidbody>().angularVelocity = trackedObj.GetAngularVelocity();
//#else
//            //Vector3 newVelocity        = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
//            //Vector3 newAngularVelocity = OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RTouch);
//            //lastSelectedObject.GetComponent<Rigidbody>().velocity        = newVelocity;
//            //lastSelectedObject.GetComponent<Rigidbody>().angularVelocity = newAngularVelocity;
//#endif
//        }

//        lastSelectedObject = null;
//    }
}
