/*
 *  AperatureSelectionController Controller - This controller class gets attatched to a prefab.
 *  
 *  This method allows a user to drag the bend cast prefab into the scene and immediately use it.
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

[ExecuteInEditMode]
public class AperatureSelectionController : MonoBehaviour {

    public enum ControllerPicked
    {
        Left_controller,
        Right_Controller
    }

    public GameObject leftController, rightController;
    public GameObject headset;

    public GameObject aperatureVolume;

    // Use this for initialization
    void Awake() {
        AperatureSelection selector;
        if ((selector = this.GetComponent<AperatureSelection>()) == null )

        {
            return;
        }

        if (selector.trackedObj != null &&
            selector.headsetTrackedObj != null)
        {
            return;
        }

#if SteamVR_Legacy
        // Locates the camera rig and its child controllers
        SteamVR_ControllerManager CameraRigObject = FindObjectOfType<SteamVR_ControllerManager>();
        rightController = CameraRigObject.right;

        GameObject eye = FindObjectOfType<SteamVR_Camera>().gameObject;
        headset = eye.transform.parent.gameObject;

        controller = rightController.gameObject;
        selector.trackedObj = rightController.GetComponent<SteamVR_TrackedObject>();
        selector.headsetTrackedObj = head.GetComponent<SteamVR_TrackedObject>();
#elif SteamVR_2
        SteamVR_Behaviour_Pose[] controllers = FindObjectsOfType<SteamVR_Behaviour_Pose>();
        if (controllers.Length > 1) {
            leftController = controllers[0].inputSource.ToString() == "LeftHand" ? controllers[0].gameObject : controllers[1].inputSource.ToString() == "LeftHand" ? controllers[1].gameObject : null;
            rightController = controllers[0].inputSource.ToString() == "RightHand" ? controllers[0].gameObject : controllers[1].inputSource.ToString() == "RightHand" ? controllers[1].gameObject : null;
        } else if (controllers.Length == 1) {
            leftController = controllers[0].inputSource.ToString() == "LeftHand" ? controllers[0].gameObject : null;
            rightController = controllers[0].inputSource.ToString() == "RightHand" ? controllers[0].gameObject : null;
        } else {
            return;
        }

        if (controllers[0] != null) {
            headset = controllers[0].transform.parent.GetComponentInChildren<Camera>().gameObject;
        }

        controller = rightController.gameObject;
		selector.trackedObj = rightController.GetComponent<SteamVR_Behaviour_Pose>();
		selector.headsetTrackedObj = head;

#elif Oculus_Quest_Hands
        OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
        if (cameraRig != null)
        {
            cameraRig.EnsureGameObjectIntegrity();

            //Making controller game objects and attaching them to their respective transform
            leftController = cameraRig.leftHandAnchor.GetComponentInChildren<OVRHand>().gameObject;
            rightController = cameraRig.rightHandAnchor.GetComponentInChildren<OVRHand>().gameObject;

            headset = new GameObject("head");
            headset.transform.SetParent(cameraRig.centerEyeAnchor);
        }

        rightController = rightController.gameObject;
        selector.trackedObj = rightController;
        selector.headsetTrackedObj = headset;

#elif Oculus_Quest_Controllers
        OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
        if (cameraRig != null)
        {
            cameraRig.EnsureGameObjectIntegrity();

            //Making controller game objects and attaching them to their respective transform
            leftController = cameraRig.leftControllerAnchor.gameObject;
            rightController = cameraRig.rightControllerAnchor.gameObject;
            
            headset = cameraRig.rightEyeAnchor.gameObject;

            selector.trackedObj = rightController;
            selector.headsetTrackedObj = headset;
        }
        else
        {
            Debug.Log("There is no camera rig.");
        }

#endif

        AperatureSelectionSelector objectSelection;
        if ((objectSelection = aperatureVolume.GetComponent<AperatureSelectionSelector>()) != null) {
#if SteamVR_Legacy
            objectSelection.trackedObj = rightController.GetComponent<SteamVR_TrackedObject>();
#elif SteamVR_2
            objectSelection.trackedObj = rightController.GetComponent<SteamVR_Behaviour_Pose>();
#elif Oculus_Quest_Hands
            objectSelection.leftController = leftController;
            objectSelection.rightController = rightController;
#elif Oculus_Quest_Controllers
            objectSelection.leftController = leftController;
            objectSelection.rightController = rightController;
#endif
        }
    }
}
