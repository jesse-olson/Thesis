using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Technique : MonoBehaviour
{
    public enum PointerPosition
    {
        IndexFinger = 1,
        Wrist       = 2,
        Palm        = 3,
        Thumb       = 4,
        MiddleKnuckle = 5
    }

#if SteamVR_Legacy
    protected SteamVR_TrackedObject trackedObj;
    protected SteamVR_Controller.Device controller;
#elif SteamVR_2
    private SteamVR_Behaviour_Pose trackedObj;
    public SteamVR_Action_Boolean m_controllerPress;
#elif Oculus_Quest_Hands
    public GameObject trackedObj;   // The position of the hand that will be tracked
    public GameObject tipPrefab;    // The prefab used for the tip of the finger
    public GameObject palmPrefab;   // 
    protected GameObject palm;

    public PointerPosition pointFrom;

    public bool fingerPressToggleEnabled;   // Used to see if the user wants a thing
    protected HandGesture fingerPressToggle = new HandGesture().SetAll(false);
#else
    public GameObject trackedObj;
#endif

    public InteractionType interactionType;
    public LayerMask interactionLayers;

    public ControllerPicked controllerPicked;
    public GameObject leftController = null;
    public GameObject rightController = null;
    public GameObject head;

    public GameObject laserPrefab;
    protected GameObject laser;
    protected Transform laserTransform;
    protected GameObject mirroredCube;

    internal bool objectSelected = false;

    public UnityEvent onSelectObject; // Invoked when an object is selected
    public UnityEvent onDropObject; // Invoked when object is dropped

    public UnityEvent onHover;      // Invoked when an object is hovered by technique
    public UnityEvent onUnhover;    // Invoked when an object is no longer hovered by the technique


    protected void InitializeControllers()
    {
        if (controllerPicked == ControllerPicked.Right_Controller)
        {
#if SteamVR_Legacy
            trackedObj = rightController.GetComponent<SteamVR_TrackedObject>();
#elif SteamVR_2
            trackedObj = rightController.GetComponent<SteamVR_Behaviour_Pose>();
#elif Oculus_Quest_Hands
            //QuestDebug.Instance.Log("Making the Hands");
            trackedObj = SetUpHand(rightController.GetComponent<OVRHand>(), OVRHand.Hand.HandRight);
            //QuestDebug.Instance.Log(trackedObj.name);
            SetUpHandInput(leftController.GetComponent<OVRHand>());
#else
            trackedObj = rightController;
#endif
        }
        else if (controllerPicked == ControllerPicked.Left_Controller)
        {
#if SteamVR_Legacy
            trackedObj = leftController.GetComponent<SteamVR_TrackedObject>();
#elif SteamVR_2
            trackedObj = leftController.GetComponent<SteamVR_Behaviour_Pose>();
#elif Oculus_Quest_Hands
            //QuestDebug.Instance.Log("Making the Hands");
            trackedObj = SetUpHand(leftController.GetComponent<OVRHand>(), OVRHand.Hand.HandLeft);
            SetUpHandInput(rightController.GetComponent<OVRHand>());
#else
            trackedObj = leftController;
#endif
        }
        else
        {
            print("Couldn't detect trackedObject, please specify the controller type in the settings.");
            Application.Quit();
        }
    }

    public ControllerState ControllerEvents()
    {
#if SteamVR_Legacy
            if (controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)) {
                return ControllerState.TRIGGER_DOWN;
            }
            if (controller.GetPressUp(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)) {
                return ControllerState.TRIGGER_UP;
            }
#elif SteamVR_2
            if (m_controllerPress.GetStateDown(trackedObj.inputSource)) {
                return ControllerState.TRIGGER_DOWN;
            }
            if (m_controllerPress.GetStateUp(trackedObj.inputSource)) {
                return ControllerState.TRIGGER_UP;
            }

#elif Oculus_Quest_Hands
        PalmTrigger input = palm.GetComponent<PalmTrigger>();

        if (fingerPressToggleEnabled)
        {
            ControllerState toReturn = ControllerState.NONE;
            if (input.GetFingerDown(Finger.indexFinger))
            {
                if (!fingerPressToggle.indexFinger)
                {
                    toReturn = ControllerState.TRIGGER_DOWN;
                }
                else
                {
                    toReturn = ControllerState.TRIGGER_UP;
                }
                fingerPressToggle.indexFinger = !fingerPressToggle.indexFinger;
            }
            return toReturn;
        }
        else
        {
            if (input.GetFingerDown(Finger.indexFinger))
            {
                return ControllerState.TRIGGER_DOWN;
            }
            else if (input.GetFingerUp(Finger.indexFinger))
            {
                return ControllerState.TRIGGER_UP;
            }
        }
#elif Oculus_Quest_Controllers
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            return ControllerState.TRIGGER_DOWN;
        }
        else if(OVRInput.GetUp(OVRInput.Button.One))
        {
            return ControllerState.TRIGGER_UP;
        }
#endif

        return ControllerState.NONE;
    }

#if Oculus_Quest_Hands
    protected GameObject SetUpHand(OVRHand hand, OVRHand.Hand handType)
    {
        OVRCustomSkeleton skeleton = hand.GetComponent<OVRCustomSkeleton>();

        GameObject pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pointer.transform.localScale = 0.03f * Vector3.one;

        pointer.name = "MEMEME";
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
