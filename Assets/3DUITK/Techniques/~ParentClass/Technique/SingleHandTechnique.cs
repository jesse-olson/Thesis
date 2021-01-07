using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SingleHandTechnique : Technique
{
    new protected void InitializeControllers()
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
            SetUpHandInput(rightController.GetComponent<OVRHand>());
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
            SetUpHandInput(leftController.GetComponent<OVRHand>());
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
}

