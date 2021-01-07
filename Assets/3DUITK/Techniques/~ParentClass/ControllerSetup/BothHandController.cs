using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

[ExecuteInEditMode]
public class BothHandController : MonoBehaviour
{

    private enum SelectionController
    {
        LeftController,
        RightController
    }

    SelectionController selectionController = SelectionController.RightController;

    // Use this for initialization
    void Awake()
    {
        GameObject leftController = null, rightController = null;
#if SteamVR_Legacy
        SteamVR_ControllerManager CameraRigObject = FindObjectOfType<SteamVR_ControllerManager>();
		leftController = CameraRigObject.left;
		rightController = CameraRigObject.right;

		Spindle spindleComponent = this.GetComponent<Spindle>();

		if(spindleComponent.leftController == null || spindleComponent.rightController == null) {
			print("here");
			SteamVR_TrackedObject trackedL = leftController.GetComponent<SteamVR_TrackedObject>();
			SteamVR_TrackedObject trackedR = rightController.GetComponent<SteamVR_TrackedObject>();
			spindleComponent.leftController = trackedL;
			spindleComponent.rightController = trackedR;

			SpindleInteractor interactionPointComponent = this.GetComponentInChildren<SpindleInteractor>();
			interactionPointComponent.leftController = trackedL;
			interactionPointComponent.rightController = trackedR;			
		}
#elif SteamVR_2
        SteamVR_Behaviour_Pose[] controllers = FindObjectsOfType<SteamVR_Behaviour_Pose>();
        Spindle spindleComponent = this.GetComponent<Spindle>();
        if (controllers.Length > 1) {
            leftController = controllers[0].inputSource.ToString() == "LeftHand" ? controllers[0].gameObject : controllers[1].inputSource.ToString() == "LeftHand" ? controllers[1].gameObject : null;
            rightController = controllers[0].inputSource.ToString() == "RightHand" ? controllers[0].gameObject : controllers[1].inputSource.ToString() == "RightHand" ? controllers[1].gameObject : null;
        } else {
            leftController = controllers[0].inputSource.ToString() == "LeftHand" ? controllers[0].gameObject : null;
            rightController = controllers[0].inputSource.ToString() == "RightHand" ? controllers[0].gameObject : null;
        }
        if(spindleComponent.leftController == null || spindleComponent.rightController == null) {
        	SteamVR_Behaviour_Pose trackedL = leftController.GetComponent<SteamVR_Behaviour_Pose>();
			SteamVR_Behaviour_Pose trackedR = rightController.GetComponent<SteamVR_Behaviour_Pose>();
			spindleComponent.leftController = trackedL;
			spindleComponent.rightController = trackedR;

			SpindleInteractor interactionPointComponent = this.GetComponentInChildren<SpindleInteractor>();
			interactionPointComponent.leftController = trackedL;
			interactionPointComponent.rightController = trackedR;
        }
#elif Oculus_Quest_Controllers
        OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
        if (cameraRig != null)
        {
            cameraRig.EnsureGameObjectIntegrity();

            //Making controller game objects and attaching them to their respective transform
            leftController = cameraRig.leftControllerAnchor.gameObject;

            rightController = cameraRig.rightControllerAnchor.gameObject;

            Spindle spindleComponent = this.GetComponent<Spindle>();

            if (spindleComponent.leftController == null || spindleComponent.rightController == null)
            {
                print("here");
                spindleComponent.leftController = leftController;
                spindleComponent.rightController = rightController;

                SpindleInteractor interactionPointComponent = this.GetComponentInChildren<SpindleInteractor>();
                interactionPointComponent.leftController = leftController;
                interactionPointComponent.rightController = rightController;
            }
            else
            {
                Debug.Log("There is no camera rig.");
            }
        }

#elif Oculus_Quest_Hands
        OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
        if (cameraRig != null)
        {
            cameraRig.EnsureGameObjectIntegrity();

            //Making controller game objects and attaching them to their respective transform
            leftController = new GameObject("leftController");
            leftController.transform.SetParent(cameraRig.leftControllerAnchor);

            rightController = new GameObject("rightController");
            rightController.transform.SetParent(cameraRig.rightControllerAnchor);

            Spindle spindleComponent = this.GetComponent<Spindle>();

            if (spindleComponent.leftController == null || spindleComponent.rightController == null)
            {
                print("here");
                spindleComponent.leftController = leftController;
                spindleComponent.rightController = rightController;

                SpindleInteractor interactionPointComponent = this.GetComponentInChildren<SpindleInteractor>();
                interactionPointComponent.leftController = leftController;
                interactionPointComponent.rightController = rightController;
            }
            else
            {
                Debug.Log("There is no camera rig.");
            }
        }

#endif

    }
}
