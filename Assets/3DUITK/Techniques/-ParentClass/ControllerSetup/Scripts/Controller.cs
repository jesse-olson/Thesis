using UnityEngine;

[ExecuteInEditMode]
public abstract class Controller : MonoBehaviour
{
    public GameObject leftController, rightController, head;

#if Oculus_Quest_Hands
    public PointerPosition pointFrom = PointerPosition.IndexFinger;
#endif

    public enum Hand
    {
        Left,
        Right,
        Head,
        NONE
    }

    protected void Awake()
    {
        // Controller should only ever needs to be setup once
        if (leftController == null)
            SetupLeftController();

        if (rightController == null)
            SetupRightController();

        if (head == null)
            SetupHead();

        SetupController();

        SetupTechnique();
    }

    // This method should be used to perform any other set up 
    protected abstract void SetupController();

    // This method should be used to set the correct tracked objects for the respective technique
    protected abstract void SetupTechnique();



    private void SetupLeftController()
    {
#if SteamVR_Legacy
        SteamVR_ControllerManager CameraRigObject = FindObjectOfType<SteamVR_ControllerManager>();
        if ((CameraRigObject = FindObjectOfType<SteamVR_ControllerManager>()) != null) {
            leftController = CameraRigObject.left;
        }
#elif SteamVR_2
        SteamVR_Behaviour_Pose[] controllers = FindObjectsOfType<SteamVR_Behaviour_Pose>();
            leftController = controllers[0].inputSource.ToString() == "LeftHand" ? controllers[0].gameObject : null;
        if (controllers.Length > 1)
            leftController = leftController == null && controllers[1].inputSource.ToString() == "LeftHand" ? controllers[1].gameObject : null;
#elif Oculus_Quest_Controllers
        OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
        if (cameraRig != null)
        {
            cameraRig.EnsureGameObjectIntegrity();
            //Making controller game objects and attaching them to their respective transform
            leftController = cameraRig.leftControllerAnchor.gameObject;
        }
#elif Oculus_Quest_Hands
            OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
            if (cameraRig != null)
            {
                cameraRig.EnsureGameObjectIntegrity();
                //Making controller game objects and attaching them to their respective transform
                leftController  = cameraRig.leftHandAnchor.GetComponentInChildren<OVRHand>().gameObject;
                SetupHand(leftController.GetComponent<OVRHand>(), OVRHand.Hand.Left);
            }
#endif
        if (rightController == null)
        {
            Debug.Log("There was an error setting up the left hand.");
        }
    }

    private void SetupRightController()
    {
#if SteamVR_Legacy
        SteamVR_ControllerManager CameraRigObject = FindObjectOfType<SteamVR_ControllerManager>();
        if ((CameraRigObject = FindObjectOfType<SteamVR_ControllerManager>()) != null) {
            rightController = CameraRigObject.right;
        }
#elif SteamVR_2

        SteamVR_Behaviour_Pose[] controllers = FindObjectsOfType<SteamVR_Behaviour_Pose>();
        if (controllers.Length > 1) {
            rightController = controllers[0].inputSource.ToString() == "RightHand" ? controllers[0].gameObject : controllers[1].inputSource.ToString() == "RightHand" ? controllers[1].gameObject : null;
        } else {
            rightController = controllers[0].inputSource.ToString() == "RightHand" ? controllers[0].gameObject : null;
        }
#elif Oculus_Quest_Controllers
        OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
        if (cameraRig != null)
        {
            cameraRig.EnsureGameObjectIntegrity();
            //Making controller game objects and attaching them to their respective transform
            rightController = cameraRig.rightControllerAnchor.gameObject;
        }
#elif Oculus_Quest_Hands
        OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
        if (cameraRig != null)
        {
            cameraRig.EnsureGameObjectIntegrity();
            //Making controller game objects and attaching them to their respective transform
            rightController = cameraRig.rightHandAnchor.GetComponentInChildren<OVRHand>().gameObject;
            SetupHand(rightController.GetComponent<OVRHand>(), OVRHand.Hand.Right);
        }
#endif
        if(rightController == null)
        {
            Debug.Log("There was an error setting up the right hand.");
        }
    }

    // TODO: implement head for SteamVR legacy and V2
    private void SetupHead()
    {
#if SteamVR_Legacy
#elif SteamVR_2
#elif Oculus_Quest_Controllers
        OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
        if (cameraRig != null)
        {
            cameraRig.EnsureGameObjectIntegrity();
            //Making controller game objects and attaching them to their respective transform
            head = cameraRig.centerEyeAnchor.gameObject;
        }
#elif Oculus_Quest_Hands
        OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
        if (cameraRig != null)
        {
            cameraRig.EnsureGameObjectIntegrity();    
            //Making controller game objects and attaching them to their respective transform
            head = cameraRig.centerEyeAnchor.gameObject; 
        }
#endif
        if (head == null)
        {
            Debug.Log("There is no head rig.");
        }
    }


#if Oculus_Quest_Hands
    protected GameObject SetupHand(OVRHand hand, OVRHand.Hand handType)
    {
        OVRCustomSkeleton skeleton = hand.GetComponent<OVRCustomSkeleton>();

        GameObject pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pointer.transform.localScale = 0.03f * Vector3.one;

        pointer.name = handType;
        Debug.Log(pointer.name);

        OVRSkeleton.BoneId parentBone = OVRSkeleton.BoneId.Invalid;
        Vector3 localPosition = Vector3.zero;
        switch (pointFrom)
        {
            case PointerPosition.IndexFinger:
                parentBone = OVRSkeleton.BoneId.Hand_IndexTip;
                break;

            case PointerPosition.MiddleKnuckle:
                parentBone = OVRSkeleton.BoneId.Hand_WristRoot;
                localPosition = skeleton.CustomBones[(int)OVRSkeleton.BoneId.Hand_Middle1].transform.position;
                break;

            case PointerPosition.Palm:
                parentBone = OVRSkeleton.BoneId.Hand_WristRoot;
                localPosition = GetPalmPosition(hand);
                if (handType == OVRHand.Hand.HandLeft)
                {
                    pointer.transform.localRotation = new Quaternion(-0.707f, 0, 0, 0.707f);
                }
                else
                {
                    pointer.transform.localRotation = new Quaternion(0.707f, 0, 0, 0.707f);
                }
                break;

            case PointerPosition.Thumb:
                parentBone = OVRSkeleton.BoneId.Hand_Thumb3;
                break;

            case PointerPosition.Wrist:
                parentBone = OVRSkeleton.BoneId.Hand_WristRoot;
                break;
        }

        pointer.transform.SetParent(skeleton.CustomBones[(int)parentBone].transform);
        pointer.transform.localPosition = localPosition;


        if (handType == OVRHand.Hand.HandLeft)
        {
            pointer.transform.localRotation = new Quaternion(0, -0.707f, 0, 0.707f) * pointer.transform.localRotation;
        }
        else
        {
            pointer.transform.localRotation = new Quaternion(0, 0.707f, 0, 0.707f) * pointer.transform.localRotation;
        }

        return pointer;
    }

    protected void SetUpHandInput(OVRHand hand)
    {
        OVRCustomSkeleton skeleton = hand.GetComponent<OVRCustomSkeleton>();

        OVRSkeleton.BoneId thumbTipIndex = OVRSkeleton.BoneId.Hand_ThumbTip;
        for (int i = 0; i < 5; i++)
        {
            GameObject toAdd = Instantiate(tipPrefab);
            toAdd.transform.SetParent(skeleton.CustomBones[(int)thumbTipIndex + i].transform);
            toAdd.transform.localPosition = Vector3.zero;

            toAdd.GetComponent<FingerTip>().finger = Finger.thumbFinger + i;
        }

        palm = Instantiate(palmPrefab);
        palm.transform.SetParent(skeleton.CustomBones[(int)OVRSkeleton.BoneId.Hand_WristRoot].transform);
        palm.transform.localPosition = GetPalmPosition(hand) + 0.04f * Vector3.up;
    }

    private Vector3 GetPalmPosition(OVRHand hand)
    {
        OVRCustomSkeleton skeleton = hand.GetComponent<OVRCustomSkeleton>();

        Vector3 wristPosition = skeleton.CustomBones[(int)OVRSkeleton.BoneId.Hand_WristRoot].transform.position;
        Vector3 middleKnuckle = skeleton.CustomBones[(int)OVRSkeleton.BoneId.Hand_Middle1].transform.position;
        Vector3 midPoint = Vector3.Lerp(wristPosition, middleKnuckle, 0.5f);

        return midPoint;
    }
#endif
}