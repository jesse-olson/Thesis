using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
[ExecuteInEditMode]
public class BubbleCursorController : MonoBehaviour {

	void Awake() 
	{		
		BubbleCursor bubble = this.GetComponent<BubbleCursor>();
		if(bubble == null || bubble.cameraHead != null) {
			// Only needs to set up once so will return otherwise
			return;
		}

        GameObject leftController = null, rightController = null, head = null;
#if SteamVR_Legacy
        // Locates the camera rig and its child controllers
        SteamVR_ControllerManager CameraRigObject = FindObjectOfType<SteamVR_ControllerManager>();
        leftController = CameraRigObject.left;
        rightController = CameraRigObject.right;

        bubble.leftController = leftController;
        bubble.rightController = rightController;
        bubble.cameraHead = FindObjectOfType<SteamVR_Camera>().gameObject;
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
            head = controllers[0].transform.parent.GetComponentInChildren<Camera>().gameObject;
        }
        bubble.controllerLeft = leftController;
        bubble.controllerRight = rightController;
        bubble.cameraHead = head;

#else
        OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
        if (cameraRig != null)
        {
            cameraRig.EnsureGameObjectIntegrity();

            //Making controller game objects and attaching them to their respective transform
            leftController = new GameObject("leftController");
            leftController.transform.SetParent(cameraRig.leftControllerAnchor);

            rightController = new GameObject("rightController");
            rightController.transform.SetParent(cameraRig.rightControllerAnchor);

            bubble.leftController = leftController;
            bubble.rightController = rightController;
        }
        else
        {
            Debug.Log("There is no camera rig.");
        }
#endif
    }
}
