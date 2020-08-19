using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

[ExecuteInEditMode]
public class FlashlightController : MonoBehaviour {

    public OVRCameraRig cameraRig;
	public GameObject leftController;
	public GameObject rightController;
	public GameObject leftFlashlight;
	public GameObject rightFlashlight;
	
	// Runs in the editor
	void Awake() 
	{
		// If the controllers are null will try to set everything up. Otherwise will run.
		if(leftController == null && rightController == null) {
            // Locates the camera rig and its child controllers
#if SteamVR_Legacy
            SteamVR_ControllerManager CameraRigObject = FindObjectOfType<SteamVR_ControllerManager>();
			leftController = CameraRigObject.left;
			rightController = CameraRigObject.right;
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
#else 
            leftController = new GameObject("leftController");
            leftController.transform.parent = cameraRig.leftControllerAnchor;


            rightController = new GameObject("rightController");
            rightController.transform.parent = cameraRig.rightControllerAnchor;

            //leftController = new GameObject();
            //Debug.Log("Camera rig Object:");
            //Debug.Log(cameraRig.leftControllerAnchor.gameObject.name);
#endif


            // Setting up of the left hand controller
            if ((leftFlashlight.GetComponent<Flashlight>())!= null) {
#if SteamVR_Legacy
                leftFlashlight.GetComponent<Flashlight>().trackedObj = leftController.GetComponent<SteamVR_TrackedObject>();
                leftFlashlight.GetComponent<Flashlight>().objectAttachedTo = leftController;
#elif SteamVR_2
                leftFlashlight.GetComponent<Flashlight>().trackedObj = leftController.GetComponent<SteamVR_Behaviour_Pose>();
                leftFlashlight.GetComponent<Flashlight>().objectAttachedTo = leftController;
#else
                leftFlashlight.GetComponent<Flashlight>().trackedObj = leftController;
                leftFlashlight.GetComponent<Flashlight>().objectAttachedTo = leftController;
#endif

				FlashlightSelection leftSelection;
				if((leftSelection = leftFlashlight.GetComponent<FlashlightSelection>()) != null) {
#if SteamVR_Legacy
                leftSelection.theController = leftController.GetComponent<SteamVR_TrackedObject>();
#elif SteamVR_2
                leftSelection.theController = leftController.GetComponent<SteamVR_Behaviour_Pose>();
#else
                    leftSelection.theController = leftController;
#endif
                }
            }
			if ((rightFlashlight.GetComponent<Flashlight>())!= null) {
#if SteamVR_Legacy
                rightFlashlight.GetComponent<Flashlight>().trackedObj = rightController.GetComponent<SteamVR_TrackedObject>();
                rightFlashlight.GetComponent<Flashlight>().objectAttachedTo = rightController;
#elif SteamVR_2
                rightFlashlight.GetComponent<Flashlight>().trackedObj = rightController.GetComponent<SteamVR_Behaviour_Pose>();
                rightFlashlight.GetComponent<Flashlight>().objectAttachedTo = rightController;
#else
                rightFlashlight.GetComponent<Flashlight>().trackedObj = rightController;
                rightFlashlight.GetComponent<Flashlight>().objectAttachedTo = rightController;

#endif

                FlashlightSelection rightSelection;
				if((rightSelection = rightFlashlight.GetComponent<FlashlightSelection>()) != null) {
#if SteamVR_Legacy
                rightSelection.theController = rightController.GetComponent<SteamVR_TrackedObject>();
#elif SteamVR_2
                rightSelection.theController = rightController.GetComponent<SteamVR_Behaviour_Pose>();
#else
                    rightSelection.theController = rightController;
#endif
                }
            }
		}	
	}

    private void Update()
    {}
}
