using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

[ExecuteInEditMode]
public class GoGoController : Controller {
	public GameObject leftHand;
	public GameObject rightHand;
    
    GoGoShadow shadowLeft;
    GoGoShadow shadowRight;

    private void Awake()
    {
        technique = GetComponent<Technique>();
        // Controller only ever needs to be setup once
        if (    (shadowLeft  = leftHand.GetComponent<GoGoShadow>()  ) != null    && 
                (shadowRight = rightHand.GetComponent<GoGoShadow>() ) != null    )
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
        technique.controllerRight = rightController;
        technique.controllerLeft = leftController;
#else
        OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
        if (cameraRig != null)
        {
            cameraRig.EnsureGameObjectIntegrity();

            //Making controller game objects and attaching them to their respective transform
            leftController = cameraRig.leftControllerAnchor.GetChild(0).gameObject; // new GameObject("leftController");
            leftController.transform.SetParent(cameraRig.leftControllerAnchor);

            rightController = cameraRig.rightControllerAnchor.GetChild(0).gameObject; //new GameObject("rightController");
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

    private void SetUpShadows()
    {
        // Adding variables to shadow scripts
        if ((shadowLeft = leftHand.GetComponent<GoGoShadow>()) != null)
        {
#if SteamVR_Legacy
                shadowLeft.trackedObj = leftController.GetComponent<SteamVR_TrackedObject>();
#elif SteamVR_2
                shadowLeft.trackedObj = leftController.GetComponent<SteamVR_Behaviour_Pose>();
#else
            shadowLeft.trackedObj = leftController;
#endif


            shadowLeft.theController = leftController.transform;
            shadowLeft.theModel      = leftController;
            shadowLeft.cameraRig     = FindObjectOfType<OVRCameraRig>().gameObject;
        }

        if ((shadowRight = rightHand.GetComponent<GoGoShadow>()) != null)
        {
#if SteamVR_Legacy
                shadowRight.trackedObj = rightController.GetComponent<SteamVR_TrackedObject>();
#elif SteamVR_2
                shadowRight.trackedObj = rightController.GetComponent<SteamVR_Behaviour_Pose>();
#else
            shadowRight.trackedObj = rightController;
#endif

            shadowRight.theController = rightController.transform;
            shadowRight.theModel      = rightController;
            shadowRight.cameraRig     = FindObjectOfType<OVRCameraRig>().gameObject;
        }
    }

    protected override void SetUpController()
    {
        base.SetUpController();

        SetUpShadows();
        

        // Adding variables to interaction scripts
        GrabObject grabLeft;
        if ((grabLeft = leftHand.GetComponent<GrabObject>()) != null)
        {
#if SteamVR_Legacy
                grabLeft.trackedObj = leftController.GetComponent<SteamVR_TrackedObject>();
#elif SteamVR_2
                grabLeft.trackedObj = leftController.GetComponent<SteamVR_Behaviour_Pose>();
#else
            //grabLeft.trackedObj = leftController.GetChild(1).gameObject;
#endif

        }
        GrabObject grabRight;
        if ((grabRight = rightHand.GetComponent<GrabObject>()) != null)
        {
#if SteamVR_Legacy
                grabRight.trackedObj = rightController.GetComponent<SteamVR_TrackedObject>();
#elif SteamVR_2
                grabRight.trackedObj = rightController.GetComponent<SteamVR_Behaviour_Pose>();
#else
            //grabRight.trackedObj = rightController.GetChild(1).gameObject;
#endif

        }
    }
}
