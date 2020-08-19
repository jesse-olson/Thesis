using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    protected GameObject leftController, rightController;
    protected Technique technique;
    void Awake()
    {
        technique = GetComponent<Technique>();
        // Controller only ever needs to be setup once
        if (technique.leftController != null || technique.rightController != null)
        {
            return;
        }

#if SteamVR_Legacy
        SteamVR_ControllerManager CameraRigObject = FindObjectOfType<SteamVR_ControllerManager>();
        if ((CameraRigObject = FindObjectOfType<SteamVR_ControllerManager>()) != null) {
            leftController = CameraRigObject.left;
            rightController = CameraRigObject.right;
            technique.rightController = rightController;
            technique.leftController = leftController;
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
        technique.rightController = rightController;
        technique.leftController = leftController;
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

            technique.leftController = leftController;
            technique.rightController = rightController;

            SetUpController();
            SetUpTechnique();
        }
        else
        {
            Debug.Log("There is no camera rig.");
        }
#endif

    }

    protected Technique SetUpTechnique()
    {
        return new Technique();
    }

    
    protected virtual void SetUpController()
    {

    }
}