using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ControllerEventSystem : MonoBehaviour
{
    public UnityEvent onLeftTriggerDown;
    public UnityEvent onLeftTriggerNearPressDown;
    public UnityEvent onLeftTriggerNearPressUp;
    public UnityEvent onLeftTriggerUp;

    public UnityEvent onRightTriggerDown;
    public UnityEvent onRightTriggerNearPressDown;
    public UnityEvent onRightTriggerNearPressUp;
    public UnityEvent onRightTriggerUp;

    private bool triggerLeftDown;
    private float triggerLeftThreshold = 0.5f;

    private bool  triggerRightDown;
    private float triggerRightThreshold = 0.5f;

    private void FixedUpdate()
    {
        ReadInput();
    }

    private void ReadInput() {
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
            OVRInput.FixedUpdate();
            if (!triggerRightDown && OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) >= triggerRightThreshold)
            {
                triggerRightDown = true;
                onRightTriggerDown.Invoke();
            }

            if (triggerRightDown && OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) < triggerRightThreshold)
            {
                triggerRightDown = false;
                onRightTriggerUp.Invoke();
            }

            if (OVRInput.GetDown(OVRInput.RawTouch.RIndexTrigger))
            {
                onRightTriggerNearPressDown.Invoke();
            }

            if (OVRInput.GetUp(OVRInput.RawTouch.RIndexTrigger))
            {
                onRightTriggerNearPressUp.Invoke();
            }

            if (!triggerLeftDown && OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger) >= triggerLeftThreshold)
            {
                triggerLeftDown = true;
                onLeftTriggerDown.Invoke();
            }

            if (triggerLeftDown && OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger) < triggerLeftThreshold)
            {
                triggerLeftDown = false;
                onLeftTriggerUp.Invoke();
            }

            if (OVRInput.GetDown(OVRInput.RawTouch.LIndexTrigger))
            {
                onLeftTriggerNearPressDown.Invoke();
            }

            if (OVRInput.GetUp(OVRInput.RawTouch.LIndexTrigger))
            {
                onLeftTriggerNearPressUp.Invoke();
            }
#endif
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
}
