/*
 *  Flexible pointer - VR interaction tool allowing the user to bend a ray cast
 *  along a bezier curve utilizng the HTC Vive controllers.
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
using UnityEngine.Events;

public class FlexiblePointer : Technique {
#if SteamVR_Legacy
    public GameObject jkl;
#endif
    public GameObject controlPoint;

    public bool controlPointVisible = true;

    public GameObject laserContainer;

    public float scaleFactor = 2f;

    private Vector3 point0; // The point from which the curve starts
    private Vector3 point1; // Bezier curve control point
    private Vector3 point2; // The end of the bezier curve

    // Laser vars
    private int numOfLasers = 20;   // The number of segments that will make up the bezier curve
    private GameObject[] lasers;    
    private Transform[] laserTransforms;
    
    public GameObject currentlyPointingAt;  // Is the gameobject that the ray is currently touching

    public GameObject selection;
    
    // Use this for initialization
    void Start() {
        InitializeLasers();

        SetControlPoints();

        if (interactionType == InteractionType.Manipulation_UI) {
            this.gameObject.AddComponent<SelectionManipulation>();
            this.GetComponent<SelectionManipulation>().trackedObj = rightController;
        }
    }

    private void InitializeLasers()
    {
        // Initalizing all the lasers
        lasers = new GameObject[numOfLasers];
        laserTransforms = new Transform[numOfLasers];

        for (int i = 0; i < numOfLasers; i++)
        {
            GameObject laserPart = Instantiate(laserPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            lasers[i] = laserPart;
            laserTransforms[i] = laserPart.transform;
            laserPart.transform.parent = laserContainer.transform;
        }
    }

    private ControllerPicked CloserController() {
        Vector3 playerPos = this.transform.position + 0.3f * Vector3.down;
        float distToLeft  = Vector3.Distance(playerPos, leftController.transform.position);
        float distToRight = Vector3.Distance(playerPos, rightController.transform.position);

        if (distToLeft < distToRight)
        {
            return ControllerPicked.Left_Controller;
        }
        else
        {
            return ControllerPicked.Right_Controller;
        }
    }

    void SetControlPoints() {
        // Setting test points
        Vector3 controller1Pos;     // The closer controller
        Vector3 controller1Fwd;
        Vector3 controller2Pos;     // The further controller
        Vector3 controller2Fwd;

        switch (CloserController())
        {
            case ControllerPicked.Left_Controller:
                controller1Pos = leftController.transform.position;
                controller2Pos = rightController.transform.position;
                controller1Fwd =  leftController.transform.forward;
                controller2Fwd = rightController.transform.forward;
                break;

            case ControllerPicked.Right_Controller:
                controller1Pos = rightController.transform.position;
                controller2Pos = leftController.transform.position;
                controller1Fwd = rightController.transform.forward;
                controller2Fwd =  leftController.transform.forward;
                break;

            default:
                controller1Pos = leftController.transform.position;
                controller2Pos = rightController.transform.position;
                controller1Fwd = leftController.transform.forward;
                controller2Fwd = rightController.transform.forward;
                break;
        }

        Vector3 forwardVectorBetweenRemotes = (controller2Pos - controller1Pos).normalized;
        float mag = (controller2Pos - controller1Pos).magnitude;
        // Will extend further based on the scale factor
        // by multiplying the distance between controllers by it
        // and calculating new end control point

        // Start control point
        point0 = controller1Pos;

        // End control point
        point2 = controller2Pos + scaleFactor * mag * forwardVectorBetweenRemotes;

#if SteamVR_Legacy
        // Will use touchpads to calculate touchpoint
        deviceL = SteamVR_Controller.Input((int)leftController.index);
        deviceR = SteamVR_Controller.Input((int)rightController.index);

        Vector2 touchpadL = (deviceL.GetAxis(EVRButtonId.k_EButton_Axis0)); // Getting reference to the touchpad
        Vector2 touchpadR = (deviceR.GetAxis(EVRButtonId.k_EButton_Axis0)); // Getting reference to the touchpad
#elif SteamVR_2
        Vector2 touchpadL = (m_touchpadAxis.GetAxis(leftController.inputSource)); // Getting reference to the touchpad
        Vector2 touchpadR = (m_touchpadAxis.GetAxis(leftController.inputSource)); // Getting reference to the touchpad
#else
        //not supported without SteamVR
        Vector2 touchpadL = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        Vector2 touchpadR = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
#endif


        // Set the controllable distance to be the distance between the end of laser to back remote
        float distanceToMoveControlPoint = Vector3.Distance(point0, point2);
        
        // Checking touchpad L
        float xvalL = touchpadL.x;
        float yvalL = touchpadL.y;

        // Checking touchpad R
        float xvalR = touchpadR.x;
        float yvalR = touchpadR.y;

        Vector3 midPoint = (point0 + point2) / 2.0f;

        Vector3 cp = midPoint + (distanceToMoveControlPoint / 2.0f) * controller1Fwd;

        // getting between the front of the flexible pointer to the back of the remotes
        Vector3 forwardBetweenRemotes = (point2 - point0).normalized;
        Vector3 middleOfRemotes =       (point0 + point2) / 2f;

        // moving along y axis acording to R y
        controlPoint.transform.position = VectorDistanceAlongFoward(distanceToMoveControlPoint, middleOfRemotes, forwardBetweenRemotes);
        
        // now need to move left and right by getting the side vector forward 
        Vector3 sideForward = Vector3.Cross(forwardBetweenRemotes, Vector3.up);
        controlPoint.transform.position = VectorDistanceAlongFoward(distanceToMoveControlPoint, controlPoint.transform.position, sideForward);
        
        // now need to control depth using the other controller
        controlPoint.transform.position = VectorDistanceAlongFoward(distanceToMoveControlPoint, controlPoint.transform.position, Vector3.up);

        controlPoint.transform.position = cp;
        // setting the actual bezier curve point to follow control point
        point1 = controlPoint.transform.position;
    }

    private Vector3 VectorDistanceAlongFoward(float theDistance, Vector3 startPos, Vector3 forward) {
        return startPos + forward.normalized * theDistance;
    }

    public enum ControllerState {
        TRIGGER_UP, TRIGGER_DOWN, NONE
    }

    private ControllerState controllerEvents() {
#if SteamVR_Legacy
        if (deviceR.GetHairTriggerDown()) {
            return ControllerState.TRIGGER_DOWN;
        }
#elif SteamVR_2
        if (m_controllerPress.GetStateDown(rightController.inputSource)) {
            return ControllerState.TRIGGER_DOWN;
        }
#endif
        return ControllerState.NONE;
    }

    // Update is called once per frame
    void Update() {
        SetControlPointVisibility();
        SetControlPoints();

        CastBezierRay();

        ControllerState currentState = controllerEvents();

        // checking for selection
        if (currentState == ControllerState.TRIGGER_DOWN &&
            currentlyPointingAt != null)
        {
            if (interactionType == InteractionType.Selection) {
                // Pure Selection
                selection = currentlyPointingAt;

            } else if (interactionType == InteractionType.Manipulation_Full) {
                // Currently no manipulation
                selection = currentlyPointingAt;

            } else if (interactionType == InteractionType.Manipulation_UI) {
                selection = currentlyPointingAt;
                this.GetComponent<SelectionManipulation>().selectedObject = selection;
            }
            onSelectObject.Invoke();
        }
    }

    void CastBezierRay() {
        float valueToSearchBezierBy = 0f;

        Vector3 lastPos = point0;

        // Used to see if ANY of the lasers collided with an object
        bool foundObject = false;

        for (int index = 0; index < numOfLasers; index++) {
            lasers[index].SetActive(!foundObject);
            if (!foundObject)
            { 
                Vector3 nextPos = getBezierPoint(valueToSearchBezierBy);

                float dist = Vector3.Distance(lastPos, nextPos);

                laserTransforms[index].position = Vector3.Lerp(lastPos, nextPos, 0.5f);
                laserTransforms[index].LookAt(nextPos);
                laserTransforms[index].localScale = 
                    new Vector3(
                        laserTransforms[index].localScale.x, 
                        laserTransforms[index].localScale.y,
                        dist
                        );

                valueToSearchBezierBy += (1f / numOfLasers);

                // Do a ray cast check on each part to check for collision (extended from laser part) 
                // First object collided with is the only one that will select
                Vector3 dir = laserTransforms[index].forward;

                if (Physics.Raycast(lastPos, dir, out RaycastHit hit, dist))
                {
                    // No object previouslly was highlighted so just highlight this one
                    if (interactionLayers == (interactionLayers | (1 << hit.transform.gameObject.layer)))
                    {
                        if (currentlyPointingAt != hit.transform.gameObject)
                        {
                            onUnhover.Invoke(); // unhover old object
                        }

                        currentlyPointingAt = hit.transform.gameObject;
                        onHover.Invoke();
                        foundObject = true;
                    }
                }

                lastPos = nextPos;
            }
        }
        if (!foundObject) {
            // no object was hit so unhover and deselect
            onUnhover.Invoke();
            currentlyPointingAt = null;
        }
    }


    // t being betweek 0 and 1 to get a spot on the curve
    Vector3 getBezierPoint(float t) {
        return (Mathf.Pow(1 - t, 2) * point0 + 2 * (1 - t) * t * point1 + Mathf.Pow(t, 2) * point2);
    }

    // sets whether the user can see the control point. Will be called if user changes the bool variable setting
    private void SetControlPointVisibility() {
        controlPoint.GetComponent<MeshRenderer>().enabled = controlPointVisible;
    }
}
