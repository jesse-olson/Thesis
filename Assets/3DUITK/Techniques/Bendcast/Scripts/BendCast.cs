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
using System;

public class Bendcast : Technique
{
    public GameObject selectedObject; // holds the selected object
    public GameObject currentlyPointingAt;

    // Bend in ray is built from multiple other rays
    private static int   numOfLasers = 20; // how many rays to use for the bend (the more the smoother) MUST BE EVEN
    private static float bezierIncrement = 1f / numOfLasers;

    private Vector3 p1; // used for the bezier curve

#if SteamVR_Legacy
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }
#endif
    private GameObject laserParent;

    // Use this for initialization
    void Start()
    {
        InitializeControllers();

        if (interactionType == InteractionType.Manipulation_UI)
        {
            this.gameObject.AddComponent<SelectionManipulation>();
            this.GetComponent<SelectionManipulation>().trackedObj = trackedObj;
        }

        // Initalizing all the lasers
        laserParent = new GameObject();
        // Making child of Tracked object simplifies bezier curve calculations
        laserParent.transform.parent = trackedObj.transform;
        laserParent.transform.localPosition = Vector3.zero;
        laserParent.transform.localRotation = Quaternion.identity;
        laserParent.gameObject.name  = trackedObj.name + " Laser Rays";

        for (int i = 0; i < numOfLasers; i++)
        {
            GameObject laserPart = Instantiate(laserPrefab) as GameObject;
            laserPart.SetActive(true);  // Make sure that the laser part is active
            laserPart.transform.SetParent(laserParent.transform); 
        }
    }

    // Update is called once per frame
    void Update()
    {
        FindClosestObject();
        CastLaserCurve();

        if (ControllerEvents() == ControllerState.TRIGGER_DOWN && currentlyPointingAt != null)
        {
            selectedObject = currentlyPointingAt;
            if (interactionType == InteractionType.Selection)
            {
                // Pure Selection            
                print("selected" + selectedObject);
            }
            else if (interactionType == InteractionType.Manipulation_Full)
            {
                GrabObject();
            }
            else if (interactionType == InteractionType.Manipulation_UI && this.GetComponent<SelectionManipulation>().inManipulationMode == false)
            {
                this.GetComponent<SelectionManipulation>().selectedObject = selectedObject;
            }
            onSelectObject.Invoke();
        }
        if (ControllerEvents() == ControllerState.TRIGGER_UP)
        {
            if (selectedObject != null)
            {
                ReleaseObject();
                onDropObject.Invoke();
            }
        }
    }

    void GrabObject()
    {
        selectedObject.GetComponent<Rigidbody>().velocity = Vector3.zero; // Setting velocity to 0 so can catch without breakforce effecting it
        var joint = AddFixedJoint();
        joint.connectedBody = selectedObject.GetComponent<Rigidbody>();
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
        if (GetComponent<FixedJoint>())
        {
            GetComponent<FixedJoint>().connectedBody = null;
            Destroy(GetComponent<FixedJoint>());
#if SteamVR_Legacy
            lastSelectedObject.GetComponent<Rigidbody>().velocity = Controller.velocity;
            lastSelectedObject.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
#elif SteamVR_2
            lastSelectedObject.GetComponent<Rigidbody>().velocity = trackedObj.GetVelocity();
            lastSelectedObject.GetComponent<Rigidbody>().angularVelocity = trackedObj.GetAngularVelocity();
#endif
        }
    }

    // Using a bezier! Idea from doing flexible pointer
    Vector3 GetBezierPosition(float t)
    {
        if (currentlyPointingAt == null)
        {
            // Fix for more appropriate later
            return new Vector3(0, 0, 0);
        }

        Vector3 p2 = currentlyPointingAt.transform.position - trackedObj.transform.position;

        return trackedObj.transform.position + (t * (2 * (1 - t) * p1 + t * p2));
    }

    void CastLaserCurve()
    {
        Vector3 lastPos = trackedObj.transform.position;
        float currentBezierValue = bezierIncrement;

        for (int i = 0; i < numOfLasers; i++)
        {
            Transform laser = laserParent.transform.GetChild(i);

            Vector3 nextPos = GetBezierPosition(currentBezierValue);
            float distance = Vector3.Distance(lastPos, nextPos);

            laser.position = lastPos;
            laser.LookAt(nextPos);
            laser.localScale = new Vector3(1, 1, distance);

            lastPos = nextPos;

            currentBezierValue += bezierIncrement;
        }
    }


    void FindClosestObject()
    {
        Vector3 remoteFwd   = trackedObj.transform.forward;
        Vector3 remoteUp    = trackedObj.transform.up;
        Vector3 remoteLeft  = -1 * trackedObj.transform.right;
        Vector3 remotePos   = trackedObj.transform.position;

        // This way is quite innefficient but is the way described for the bendcast.
        // Might make an example of a way that doesnt loop through everything
        var allObjects = FindObjectsOfType<GameObject>();

        float closestDist = float.MaxValue;
        GameObject closestObject = null;

        // Loop through objects and look for closest (if of a viable layer)
        foreach(GameObject gameObject in allObjects)
        {
            // dont have to worry about executing twice as an object can only be on one layer
            if ((interactionLayers & (1 << gameObject.layer)) > 0)
            {
                // Check if object is on plane projecting in front of VR remote. Otherwise ignore it. (we dont want our laser aiming backwards)
                Vector3 localTargetPos = gameObject.transform.position - remotePos;
                Vector3 perp = Vector3.Cross(remoteLeft, localTargetPos);
                float   side = Vector3.Dot(perp, remoteUp);

                if(side > 0)
                {
                    // Using vector algebra to get shortest distance between object and vector
                    float dist = Vector3.Cross(localTargetPos, remoteFwd).magnitude;

                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestObject = gameObject;
                        p1 = dist * remoteFwd;
                    }
                }

            }
        }
        // If there is a closest object show the lasers
        laserParent.SetActive(closestObject != null);
        // Invoke un-hover if object with shortest distance is now different to currently hovered
        if (currentlyPointingAt != closestObject)
        {
            onUnhover.Invoke(); // Broadcasting that object is unhovered
            currentlyPointingAt = closestObject; // setting the object that is being pointed at
            onHover.Invoke();   // Broadcasting that object is hovered
        }
        else
        {
            selectedObject = null;
        }
    }
}
