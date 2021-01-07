/*
 *  Absolute and relative mapping is in the form of a Raycast (However It could 
 *  be adapted to a simple hand technique). When you press the set button (touchpad) the movement 
 *  of the virtual controller relative to your real controller is scaled with a ration of 10:1. 
 *  By doing this you can be precise when selecting small or distant objects with the ray. This is
 *  because the distance you have to move your hand across an object is amplified 10x due to the ratio.
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

public class ARMLaser : SingleHandTechnique {
    public GameObject  theModel;        // The model that is used for the controller and the shadow

    public static float scaleFactor = 10.0f;   // The factor by which the movement should be scaled
    public static float scaleBy     = 1.0f / scaleFactor;

    private bool       ARMOn = false;

    private Vector3    armPosition;     // The origin position set when ARM is toggled on
    private Quaternion armRotation;     // The current rotation of the trackedObj when ARM is toggled on
    
    public GameObject selectedObject;   // holds the selected object

    public GameObject currentlyPointingAt;

    void Awake() {

        InitializeControllers();

        if (interactionType == InteractionType.Manipulation_UI) {
            this.gameObject.AddComponent<SelectionManipulation>();
            this.GetComponent<SelectionManipulation>().trackedObj = trackedObj;
        }
    }

    // Use this for initialization
    void Start() {
        laser = Instantiate(laserPrefab);
        laserTransform = laser.transform;
        laserTransform.SetParent(transform);
        laser.SetActive(true);

        armRotation = trackedObj.transform.rotation;
        armPosition = trackedObj.transform.position;
    }
       
    
    
    // Using the hack from gogo shadow - will have to fix them all once find a better way
    void MakeModelChild() {
        if (transform.childCount < 2)
        {
#if Oculus_Quest_Controllers
            // If it is just a custom model we can immediately parent
            theModel.transform.SetParent(transform);
            // Due to the transfer happening at a random time down the line we need to re-align the model inside the shadow controller to 0 so nothing is wonky.
            theModel.transform.localPosition = Vector3.zero;
            theModel.transform.localRotation = Quaternion.identity;
#elif SteamVR_Legacy || SteamVR_2
            if (theModel.transform.childCount > 0) {
                theModel.transform.SetParent(transform);
                // Due to the transfer happening at a random time down the line we need to re-align the model inside the shadow controller to 0 so nothing is wonky.
                theModel.transform.localPosition = Vector3.zero;
                theModel.transform.localRotation = Quaternion.identity;
            }
#else
            // If it is just a custom model we can immediately parent
            theModel.transform.SetParent(transform);
            // Due to the transfer happening at a random time down the line we need to re-align the model inside the shadow controller to 0 so nothing is wonky.
            theModel.transform.localPosition = Vector3.zero;
            theModel.transform.localRotation = Quaternion.identity;
#endif
        }
    }

    private void ShowLaser() {
        bool hit = Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, 100, interactionLayers);

        float dist = hit ? hitInfo.distance : 100;
        laserTransform.localScale = new Vector3(1, 1, dist);

        // Highlighting the object
        if (!hit)
        {
            if (currentlyPointingAt != null)
            {
                onUnhover.Invoke();
                currentlyPointingAt = null;
            }
        }
        else if(hitInfo.transform.gameObject != currentlyPointingAt) {
            // Unhighlight previous one and highlight this one
            onUnhover.Invoke();
            currentlyPointingAt = hitInfo.transform.gameObject;
            onHover.Invoke();
        }
    }

    void ToggleARM() {
        armPosition = trackedObj.transform.position;
        armRotation = trackedObj.transform.rotation;
        ARMOn = !ARMOn;
    }
    
    // Update is called once per frame
    void Update() {
        MakeModelChild();

        float scale = ARMOn ? scaleBy : 1;
        transform.position = Vector3    .Lerp(armPosition, trackedObj.transform.position, scale);
        transform.rotation = Quaternion .Lerp(armRotation, trackedObj.transform.rotation, scale);

        ShowLaser();

        if (ControllerEvents() == ControllerState.TRIGGER_DOWN ) {
            ToggleARM();
        }

        //// If remote trigger pulled
        //if (ControllerEvents() == ControllerState.TRIGGER_DOWN) {
        //    if (currentlyPointingAt != null) { // If pointing at an object
        //        if (interactionType == InteractionType.Selection) {
        //            lastSelectedObject = currentlyPointingAt;
        //        } else if (interactionType == InteractionType.Manipulation_Full) {
        //            // No manipualtion implemented for this currently
        //            lastSelectedObject = currentlyPointingAt;
        //        } else if (interactionType == InteractionType.Manipulation_UI) {
        //            lastSelectedObject = currentlyPointingAt;
        //            this.GetComponent<SelectionManipulation>().selectedObject = lastSelectedObject;
        //        }
        //        selectedObject.Invoke();
        //    }
        //}

    }
}
