﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class GoGoShadow : MonoBehaviour {

    private Camera playerCamera;            // Center Camera of the HMD

#if SteamVR_Legacy
    public SteamVR_TrackedObject trackedObj; 
    private SteamVR_Controller.Device device;
#elif SteamVR_2
    public SteamVR_Behaviour_Pose trackedObj;
    public SteamVR_Action_Boolean m_touchpadPress;
#else
    public GameObject trackedObj;
#endif

    public GameObject cameraRig;                 // So shadow can attach itself to the camera rig on game start

    public enum ToggleArmLengthCalculator {
        on,
        off
    }
    // If toggled on the user can press down on the touchpad with their arm extended to take a measurement of the arm
    // If it is off the user must inut a manual estimate of what the users arm length would be
    public ToggleArmLengthCalculator armLengthCalculator = ToggleArmLengthCalculator.off;

    public float armLength;                      // Either manually inputted or will be set to the arm length when calculated
    public float armLengthCoeff;                 // The scaling factor for when the technique takes effect

    public float distanceFromHeadToChest = 0.3f; // estimation of the distance from the users headset to their chest area

    public Transform theController;              // controller for the gogo to access inout

    public GameObject theModel;                  // the model of the controller that will be shadowed for gogo use

    public float extensionVariable = 10f;        // this variable in the equation controls the multiplier for how far the arm can extend with small movements

    bool calibrated = false;
    Vector3 chestPosition;
    Vector3 relativeChestPos;

    // Use this for initialization
    void Start()
    {
        this.transform.parent = cameraRig.transform;
        if (Camera.main != null)
        {
            playerCamera = Camera.main;
        }
        else
        {
            playerCamera = cameraRig.GetComponentInChildren<Camera>();
        }

        MakeModelChild();
    }


    // Update is called once per frame
    void Update()
    {
        MakeModelChild();

        //this.GetComponentInChildren<SteamVR_RenderModel>().gameObject.SetActive(false);
        Renderer[] renderers = this.transform.parent.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (renderer.material.name == "Standard (Instance)")
            {
                renderer.enabled = true;
            }
        }

        CheckForAction();
        MoveControllerForward();
    }


    void MakeModelChild() {
        if (this.transform.childCount == 0) {
            if (theModel.GetComponent<SteamVR_RenderModel>() != null) { // The steamVR_RenderModel is generated after code start so we cannot parent right away or it wont generate. 
                if (theModel.transform.childCount > 0) {
                    theModel.transform.parent = this.transform;
                    // Due to the transfer happening at a random time down the line we need to re-align the model inside the shadow controller to 0 so nothing is wonky.
                    theModel.transform.localPosition = Vector3.zero;
                    theModel.transform.localRotation = Quaternion.identity;
                }
            } else {
                // If it is just a custom model we can immediately parent
                theModel.transform.parent = this.transform;
                // Due to the transfer happening at a random time down the line we need to re-align the model inside the shadow controller to 0 so nothing is wonky.
                theModel.transform.localPosition = Vector3.zero;
                theModel.transform.localRotation = Quaternion.identity;
            }
        }

    }

    // Might have to have a manual calibration for best use
    float GetDistanceToExtend() {
        // Estimating chest position using an assumed distance from head to chest and then going that distance down the down vector of the camera. 
        // This will not allways be optimal especially when leaning is involved.
        // To improve gogo to suit your needs all you need to do is implement your own algorithm to estimate chest (or shoulder for greater
        // accuracy) position and set the chest position vector to match it

        Vector3 direction = playerCamera.transform.up * -1;     // Getting the down vector
        Vector3 normalizedDirectionPlusDistance = direction.normalized * distanceFromHeadToChest;

        chestPosition = playerCamera.transform.position + normalizedDirectionPlusDistance;

        float distChestPos = Vector3.Distance(trackedObj.transform.position, chestPosition);

        float boundaryLength = armLengthCoeff * armLength;    // 2/3 of users arm length
        
        if (distChestPos >= boundaryLength) {
            float extensionDistance = distChestPos + extensionVariable * (float)Math.Pow(distChestPos - boundaryLength, 2);
            // Dont need both here as we only want the distance to extend by not the full distance
            // but we want to keep the above formula matching the original papers formula so will then calculate just the distance to extend below
            return extensionDistance - distChestPos;
        }
        return 0; // dont extend
    }

    

    void MoveControllerForward() {
        // Using the origin and the forward vector of the remote the extended positon of the remote can be calculated
        // Vector3 theVector = theController.transform.forward;
        Vector3 theVector = theController.position - chestPosition;

        Vector3 pose   = theController.position;
        Quaternion rot = theController.rotation;

        float distance_formula_on_vector = theVector.magnitude;

        float distanceToExtend = GetDistanceToExtend();

        if (distanceToExtend != 0) {
            // Using formula to find a point which lies at distance on a 3D line from vector and direction
            pose += (distanceToExtend / (distance_formula_on_vector)) * theVector;
        }

        transform.position = pose;
        transform.rotation = rot;
    }

    public enum ControllerState {
        TOUCHPAD_UP, NONE
    }

    private ControllerState ControllerEvents() {
#if SteamVR_Legacy
        if (device.GetPressUp(SteamVR_Controller.ButtonMask.Axis0)) {
            return ControllerState.TOUCHPAD_UP;
        }
#elif SteamVR_2
        if (m_touchpadPress.GetStateUp(trackedObj.inputSource)) {
            return ControllerState.TOUCHPAD_UP;
        }
#else
        if (OVRInput.Get(OVRInput.Button.One))
        {
            return ControllerState.TOUCHPAD_UP;
        }
#endif
        return ControllerState.NONE;
    }

    void CheckForAction() {
#if SteamVR_Legacy
        device = SteamVR_Controller.Input((int)trackedObj.index);
#endif
        // (will only register if arm length calculator is on)
        if (    armLengthCalculator == ToggleArmLengthCalculator.on  && 
                ControllerEvents()  == ControllerState.TOUCHPAD_UP   ) 
        {
            armLength = Vector3.Distance(trackedObj.transform.position, chestPosition);
            calibrated = true;
        }
    }
}
