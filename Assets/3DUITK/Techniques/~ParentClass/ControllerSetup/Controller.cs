using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{

    protected GameObject leftController, rightController, head;
    public Technique technique;

    public enum Hand
    {
        Left,
        Right,
        Head,
        NONE
    }

    void Awake()
    {
        if(technique == null)
        {
            technique = GetComponent<Technique>();
        }
        // Controller only ever needs to be setup once
        if (technique.leftController == null || technique.rightController == null)
        {
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
        technique.head = head;

#elif Oculus_Quest_Hands
            OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
            if (cameraRig != null)
            {
                cameraRig.EnsureGameObjectIntegrity();

                //Making controller game objects and attaching them to their respective transform
                leftController  = cameraRig.leftHandAnchor.GetComponentInChildren<OVRHand>().gameObject;
                rightController = cameraRig.rightHandAnchor.GetComponentInChildren<OVRHand>().gameObject;

                technique.leftController = leftController;
                technique.rightController = rightController;

                head = new GameObject("head");
                head.transform.SetParent(cameraRig.centerEyeAnchor);
                technique.head = head;

                SetUpController();
                SetUpTechnique();
            }

#else
        OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
        if (cameraRig != null)
        {
            cameraRig.EnsureGameObjectIntegrity();

            //Making controller game objects and attaching them to their respective transform
            leftController = cameraRig.leftControllerAnchor.gameObject;
            rightController = cameraRig.rightControllerAnchor.gameObject;

            technique.leftController = leftController;
            technique.rightController = rightController;

            head = new GameObject("head");
            head.transform.SetParent(cameraRig.centerEyeAnchor);

            SetUpController();
            SetUpTechnique();
        }
        else
        {
            Debug.Log("There is no camera rig.");
        }
#endif
        }
    }

    protected Technique SetUpTechnique()
    {
        return new Technique();
    }

    
    protected virtual void SetUpController()
    {

    }
}