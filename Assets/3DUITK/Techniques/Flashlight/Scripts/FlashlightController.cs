using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

[ExecuteInEditMode]
public class FlashlightController : MonoBehaviour
{

    public ControllerPicked controllerPicked;

    public OVRCameraRig cameraRig;
    public GameObject leftController;
    public GameObject rightController;

    private GameObject leftFlashlightObject;
    private GameObject rightFlashlightObject;

    // Runs in the editor
    void Awake()
    {
        // As we are working with a prefab we can get the Flashlights
        leftFlashlightObject = transform.GetChild(0).gameObject;
        rightFlashlightObject = transform.GetChild(1).gameObject;

        // If the controllers are null will try to set everything up. Otherwise will run.
        if (leftController == null && rightController == null)
        {
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
#elif Oculus_Quest_Hands
            OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
            if (cameraRig != null)
            {
                cameraRig.EnsureGameObjectIntegrity();

                //Making controller game objects and attaching them to their respective transform
                leftController = cameraRig.leftHandAnchor.GetComponentInChildren<OVRHand>().gameObject;
                rightController = cameraRig.rightHandAnchor.GetComponentInChildren<OVRHand>().gameObject;
            }
#else
            OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
            if (cameraRig != null)
            {
                cameraRig.EnsureGameObjectIntegrity();

                //Making controller game objects and attaching them to their respective transform
                leftController = cameraRig.leftControllerAnchor.gameObject;

                rightController = cameraRig.rightControllerAnchor.gameObject;
            }
            else
            {
                Debug.Log("There is no camera rig.");
            }
#endif
        }

        if (controllerPicked == ControllerPicked.Left_Controller ||
           controllerPicked == ControllerPicked.Both            )
        {
            SetUpHand(leftFlashlightObject, leftController);
        }

        if (controllerPicked == ControllerPicked.Right_Controller ||
           controllerPicked == ControllerPicked.Both)
        {
            SetUpHand(rightFlashlightObject, rightController);
        }
    }

    private void SetUpHand(GameObject flashLightObject, GameObject controller)
    {
        Flashlight flashlight;
        FlashlightSelection selection;

        // Ensure that flashLightObject has both Flashlight and FlashlightSelection Components.
        if ((flashlight = flashLightObject.GetComponent<Flashlight>()) == null ||
            (selection  = flashLightObject.GetComponent<FlashlightSelection>()) == null
            )
        {
            return;
        }

#if SteamVR_Legacy
        flashlight.trackedObj = controller.GetComponent<SteamVR_TrackedObject>();
        flashlight.objectAttachedTo = controller;
        selection.trackedObj = controller.GetComponent<SteamVR_TrackedObject>();
#elif SteamVR_2
        flashlight.trackedObj = controller.GetComponent<SteamVR_Behaviour_Pose>();
        flashlight.objectAttachedTo = controller;
        selection.trackedObj = controller.GetComponent<SteamVR_Behaviour_Pose>();
#else
        flashlight.trackedObj = controller;
        flashlight.objectAttachedTo = controller;
        selection.leftController = leftController;
        selection.rightController = rightController;
#endif
    }
}