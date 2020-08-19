using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Technique : MonoBehaviour
{

#if SteamVR_Legacy
    protected SteamVR_TrackedObject trackedObj;
    protected SteamVR_Controller.Device controller;
#elif SteamVR_2
    private SteamVR_Behaviour_Pose trackedObj;
    public SteamVR_Action_Boolean m_controllerPress;
#else
    public GameObject trackedObj;
#endif

    public InteractionType interactionType;
    public LayerMask interactionLayers;

    public ControllerPicked controllerPicked;
    public GameObject rightController = null;
    public GameObject leftController  = null;

    public GameObject laserPrefab;
    private GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;
    private GameObject mirroredCube;



    internal bool objectSelected = false;

    public UnityEvent selectedObject; // Invoked when an object is selected
    public UnityEvent droppedObject; // Invoked when object is dropped

    public UnityEvent hovered; // Invoked when an object is hovered by technique
    public UnityEvent unHovered; // Invoked when an object is no longer hovered by the technique


    protected void InitializeControllers()
    {
        if (controllerPicked == ControllerPicked.Right_Controller)
        {
#if SteamVR_Legacy
            trackedObj = rightController.GetComponent<SteamVR_TrackedObject>();
#elif SteamVR_2
            trackedObj = rightController.GetComponent<SteamVR_Behaviour_Pose>();
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
#else
        if (OVRInput.Get(OVRInput.Button.One))
        {
            return ControllerState.TRIGGER_DOWN;
        }
        else
        {
            return ControllerState.TRIGGER_UP;
        }
#endif

        return ControllerState.NONE;
    }


}
