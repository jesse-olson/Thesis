/*
 *  BendCast - Similar to a ray-cast except it will bend towards the closest object
 *  VR Interaction technique for the HTC Vive.
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
using Valve.VR;
using Valve.VR.InteractionSystem;
using UnityEngine.Events;
using System;

public class BendCast : Technique
{
    public GameObject lastSelectedObject;   // holds the selected object

    public GameObject currentlyPointingAt;  //     

    private Vector3 castingBezierFrom;

    // Bend in ray is built from multiple other rays
    public readonly int numOfLasers = 20;   // How many rays to use for the bend (the more the smoother) MUST BE EVEN
    private GameObject[] lasers;
    private Transform[] laserTransform;

    private GameObject[] interactableObjects;

    private Vector3 p1PointLocation; // Used for the bezier curve


#if SteamVR_Legacy
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }
#endif
    private GameObject laserHolderGameobject;

    // Use this for initialization
    void Start()
    {
        interactableObjects = GetInteractableObjects();
        InitializeControllers();
        InitialiseLaser();
    }

    // Update is called once per frame
    void Update()
    {
        CheckSurroundingObjects();
        CastLaserCurve();

        ControllerState controllerState = ControllerEvents();

        if(controllerState == ControllerState.TRIGGER_DOWN && 
            currentlyPointingAt != null && 
            lastSelectedObject == null  )
        {
            if(interactionType == InteractionType.Selection) {
                // Pure Selection            
                print("selected" + currentlyPointingAt);
                
            } else if(interactionType == InteractionType.Manipulation_Full) {
                GrabObject(currentlyPointingAt);
                //selectedObject.Invoke();

            } else if (interactionType == InteractionType.Manipulation_UI && this.GetComponent<SelectionManipulation>().inManipulationMode == false) {
                lastSelectedObject = currentlyPointingAt;
                this.GetComponent<SelectionManipulation>().selectedObject = lastSelectedObject;
                //selectedObject.Invoke();
            }
            selectedObject.Invoke();
        }

        if (controllerState == ControllerState.TRIGGER_UP)
        {
            if (lastSelectedObject != null)
            {
                ReleaseObject();
            }
        }
    }

    private void InitialiseLaser()
    {
        // Initalizing all the lasers
        laserHolderGameobject = new GameObject();
        laserHolderGameobject.transform.parent = this.transform;
        laserHolderGameobject.gameObject.name = trackedObj.name + " Laser Rays";

        lasers         = new GameObject[numOfLasers];
        laserTransform = new Transform[numOfLasers];
        for (int i = 0; i < numOfLasers; i++)
        {
            GameObject laserPart = Instantiate(laserPrefab, new Vector3((float)i, 1, 0), Quaternion.identity) as GameObject;
            laserTransform[i] = laserPart.transform;
            lasers[i] = laserPart;
            laserPart.transform.parent = laserHolderGameobject.transform;
        }
    }

    private GameObject[] GetInteractableObjects()
    {
        var allObjects = FindObjectsOfType<GameObject>();

        List<GameObject> interactableObjects = new List<GameObject>();
        foreach (GameObject gameObject in allObjects)
        {
            if (interactionLayers == (interactionLayers | (1 << gameObject.layer)))
            {
                interactableObjects.Add(gameObject);
            }
        }

        return interactableObjects.ToArray();
    }

    void GrabObject(GameObject toGrab)
    {
        lastSelectedObject = toGrab;

        lastSelectedObject.GetComponent<Rigidbody>().velocity = Vector3.zero; // Setting velocity to 0 so can catch without breakforce effecting it
        FixedJoint joint = AddFixedJoint();
        joint.connectedBody = lastSelectedObject.GetComponent<Rigidbody>();
    }

    private FixedJoint AddFixedJoint()
    {
        FixedJoint fx = trackedObj.AddComponent<FixedJoint>();
        fx.breakForce = 1000;
        fx.breakTorque = Mathf.Infinity;
        return fx;
    }

    private void ReleaseObject()
    {
        FixedJoint joint = trackedObj.GetComponent<FixedJoint>();
        if (joint)
        {
 
            joint.connectedBody = null;
            Destroy(joint);
#if SteamVR_Legacy
            lastSelectedObject.GetComponent<Rigidbody>().velocity = Controller.velocity;
            lastSelectedObject.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
#elif SteamVR_2
            lastSelectedObject.GetComponent<Rigidbody>().velocity = trackedObj.GetVelocity();
            lastSelectedObject.GetComponent<Rigidbody>().angularVelocity = trackedObj.GetAngularVelocity();
#else
            Vector3 newVelocity        = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
            Vector3 newAngularVelocity = OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RTouch);
            lastSelectedObject.GetComponent<Rigidbody>().velocity        = newVelocity;
            lastSelectedObject.GetComponent<Rigidbody>().angularVelocity = newAngularVelocity;
#endif
        }

        lastSelectedObject = null;
    }

    void CastLaserCurve()
    {
        Vector3 prevPos = castingBezierFrom;
        
        for (int index = 0; index < numOfLasers; index++)
        {
            lasers[index].SetActive(true);
            Vector3 nextPos = GetBezierPosition(index + 1);
            float dist = Vector3.Distance(prevPos, nextPos);

            laserTransform[index].position = Vector3.Lerp(prevPos, nextPos, .5f);
            laserTransform[index].LookAt(nextPos);
            laserTransform[index].localScale = 
                new Vector3(
                    laserTransform[index].localScale.x, 
                    laserTransform[index].localScale.y,
                    dist);
            prevPos = nextPos;
        }
    }

    // Using a bezier! Idea from doing flexible pointer
    Vector3 GetBezierPosition(float segmentNumber)
    {
        float t = segmentNumber / numOfLasers;

        if (currentlyPointingAt == null)
        {
            // Fix for more appropriate later
            return new Vector3(0, 0, 0);
        }


        Vector3 p0 = castingBezierFrom;
        Vector3 p2 = currentlyPointingAt.transform.position;

        return Mathf.Pow(1f - t, 2f) * p0 +
               2f * (1f - t) * t * p1PointLocation +
               Mathf.Pow(t, 2) * p2;
    }

    void CheckSurroundingObjects()
    {
        if(lastSelectedObject != null)
        {
            return;
        }

        Vector3 controllerForward = trackedObj.transform.forward;
        Vector3 controllerPos     = trackedObj.transform.position;

        // This way is quite innefficient but is the way described for the bendcast.
        // Might make an example of a way that doesnt loop through everything
        bool hasShortestDistance = false;
        float shortestDistance = float.MaxValue;
        GameObject objectWithShortestDistance = null;
        
        // Loop through objects and look for closest (if of a viable layer)
        foreach (GameObject gameObject in interactableObjects)
        {
            Vector3 objectPosition = gameObject.transform.position;
            
            // Check if object is on plane projecting in front of VR remote. Otherwise ignore it. (we dont want our laser aiming backwards)
            Vector3 targObject = controllerPos - objectPosition;

            Vector3 perp = Vector3.Cross(trackedObj.transform.right, targObject);
            float side   = Vector3.Dot(perp, trackedObj.transform.up);
            if (side > 0)
            {
                hasShortestDistance = true;
                
                // Using vector algebra to get shortest distance between object and vector 
                float distanceBetweenRayAndPoint = Vector3.Magnitude(Vector3.Cross(targObject, controllerForward));

                if (distanceBetweenRayAndPoint < shortestDistance)
                {
                    shortestDistance = distanceBetweenRayAndPoint;
                    objectWithShortestDistance = gameObject;
                }
            }
        }

        if (hasShortestDistance)
        {
            p1PointLocation = controllerPos + shortestDistance * controllerForward;

            // Invoke un-hover if object with shortest distance is now different to currently hovered
            if (currentlyPointingAt != objectWithShortestDistance) {
                unHovered.Invoke();
            }
                        
            hovered.Invoke(); // Broadcasting that object is hovered
        }

        castingBezierFrom   = controllerPos;
        currentlyPointingAt = objectWithShortestDistance;
        laserHolderGameobject.SetActive(hasShortestDistance);
    }
}
