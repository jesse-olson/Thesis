using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingTechnique : Technique
{
#if Oculus_Quest_Hands
    public GameObject dPad;
    private IDirectionSelectable directionSelector;
#endif

    //public interface
    protected float extendDistance = 0f;
    protected float cursorSpeed = 20f; // Decrease to make faster, Increase to make slower

    new protected void InitializeControllers()
    {
        base.InitializeControllers();

#if Oculus_Quest_Hands
        directionSelector = (IDirectionSelectable) dPad.GetComponent( typeof(IDirectionSelectable) );

        if (controllerPicked == ControllerPicked.Right_Controller)
        {
            OVRHand hand = leftController.GetComponent<OVRHand>();
            OVRCustomSkeleton skeleton = hand.GetComponent<OVRCustomSkeleton>();
            directionSelector.SetControlPointParent(
                skeleton.CustomBones[(int)OVRSkeleton.BoneId.Hand_WristRoot].transform
                );
        }
        else if (controllerPicked == ControllerPicked.Left_Controller)
        {
            OVRHand hand = rightController.GetComponent<OVRHand>();
            OVRCustomSkeleton skeleton = hand.GetComponent<OVRCustomSkeleton>();
            directionSelector.SetControlPointParent(
                skeleton.CustomBones[(int)OVRSkeleton.BoneId.Hand_WristRoot].transform
                );
        }

        ////Vector3 wristPosition = skeleton.CustomBones[(int)OVRSkeleton.BoneId.Hand_WristRoot].transform.position;
        ////Vector3 middleKnuckle = skeleton.CustomBones[(int)OVRSkeleton.BoneId.Hand_Middle1].transform.position;
        ////Vector3 midPoint = Vector3.Lerp(wristPosition, middleKnuckle, 0.5f);
        //Debug.Log("Woah: " + directionSelector.ToString());
#endif
    }

    protected void PadScrolling()
    {
#if SteamVR_Legacy
        if (controller.GetAxis().y != 0) {
            extendDistance += controller.GetAxis().y / cursorSpeed;
        }
#elif SteamVR_2
        if (m_touchpad.GetAxis(trackedObj.inputSource).y != 0) {
            extendDistance += m_touchpad.GetAxis(trackedObj.inputSource).y / cursorSpeed;
        }
#elif Oculus_Quest_Hands
        PalmTrigger input = palm.GetComponent<PalmTrigger>();


        if (fingerPressToggleEnabled)
        {
            if (input.GetFingerDown(Finger.middleFinger))
            {
                if (!fingerPressToggle.middleFinger)
                {
                    dPad.SetActive(true);
                    dPad.transform.position = palm.transform.position;
                    dPad.transform.rotation = Quaternion.identity;

                    float angle = head.transform.eulerAngles.y;
                    dPad.transform.Rotate(Vector3.up * angle, Space.Self);
                }
                else
                {
                    dPad.SetActive(false);
                    directionSelector.ResetVector();
                }
                fingerPressToggle.middleFinger = !fingerPressToggle.middleFinger;
            }
        }
        else
        {
            if (input.GetFingerDown(Finger.middleFinger))
            {
                dPad.SetActive(true);
                dPad.transform.position = palm.transform.position;
            }
            else if (input.GetFingerUp(Finger.middleFinger))
            {
                dPad.SetActive(false);
            }
        }
        QuestDebug.Instance.Log(directionSelector.GetVector().ToString());
        extendDistance += directionSelector.GetY() / cursorSpeed;

#else
        Vector2 thumbStick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        if (thumbStick.y != 0)
        {
            extendDistance += thumbStick.y / cursorSpeed;
            if(extendDistance < 0.1f)
            {
                extendDistance = 0.1f;
            }
        }
#endif
    }
}
