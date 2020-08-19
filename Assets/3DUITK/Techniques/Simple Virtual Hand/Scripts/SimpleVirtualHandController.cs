﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;


[ExecuteInEditMode]
public class SimpleVirtualHandController : MonoBehaviour {

    void Awake() {

        // Controller only ever needs to be setup once
        SimpleVirtualHand virtualhand = GetComponent<SimpleVirtualHand>();
        if (virtualhand.controllerLeft != null && virtualhand.controllerRight != null) {
            return;
        }

        GameObject leftController = null, rightController = null;
#if SteamVR_Legacy
        SteamVR_ControllerManager CameraRigObject = FindObjectOfType<SteamVR_ControllerManager>();
        if ((CameraRigObject = FindObjectOfType<SteamVR_ControllerManager>()) != null) {
            leftController = CameraRigObject.left;
            rightController = CameraRigObject.right;
            virtualhand.controllerRight = rightController;
            virtualhand.controllerLeft = leftController;
        }
#elif SteamVR_2

	SteamVR_Behaviour_Pose[] controllers = FindObjectsOfType<SteamVR_Behaviour_Pose>();
        if (controllers.Length > 1) {
            leftController = controllers[0].inputSource.ToString() == "LeftHand" ? controllers[0].gameObject : controllers[1].inputSource.ToString() == "LeftHand" ? controllers[1].gameObject : null;
            rightController = controllers[0].inputSource.ToString() == "RightHand" ? controllers[0].gameObject : controllers[1].inputSource.ToString() == "RightHand" ? controllers[1].gameObject : null;
        } else {
            leftController = controllers[0].inputSource.ToString() == "LeftHand" ? controllers[0].gameObject : null;
            rightController = controllers[0].inputSource.ToString() == "RightHand" ? controllers[0].gameObject : null;
        }
        virtualhand.controllerRight = rightController;
        virtualhand.controllerLeft = leftController;
#endif
    }
}
