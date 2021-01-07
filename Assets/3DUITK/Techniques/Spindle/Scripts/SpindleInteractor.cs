using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

public class SpindleInteractor : Technique {

    private float distanceBetweenControllersOnPickup;
    private Vector3 objectScaleOnPickup;

    // Pickup Vars
    public GameObject collidingObject;
    public GameObject objectInHand;

    void Awake()
    {
        InitializeControllers();
    }

    protected void InitializeController()
    {
#if Oculus_Quest_Hands
            //QuestDebug.Instance.Log("Making the Hands");
            SetUpHandInput(leftController.GetComponent<OVRHand>());
            SetUpHandInput(rightController.GetComponent<OVRHand>());
#endif
    }

    public enum ControllerState {
        TRIGGER_UP1, TRIGGER_DOWN1, TRIGGER_UP2, TRIGGER_DOWN2, NONE
    }

    new private ControllerState ControllerEvents() {
#if SteamVR_Legacy
        if (Controller1.GetHairTriggerDown()) {
            return ControllerState.TRIGGER_DOWN1;
        }
        if (Controller1.GetHairTriggerUp()) {
            return ControllerState.TRIGGER_UP1;
        }
        if (Controller2.GetHairTriggerDown()) {
            return ControllerState.TRIGGER_DOWN2;
        }
        if (Controller2.GetHairTriggerUp()) {
            return ControllerState.TRIGGER_UP2;
        }
#elif SteamVR_2
        if (m_controllerPress.GetStateDown(leftController.inputSource)) {
            return ControllerState.TRIGGER_DOWN1;
        }
        if (m_controllerPress.GetStateUp(leftController.inputSource)) {
            return ControllerState.TRIGGER_UP1;
        }
        if (m_controllerPress.GetStateDown(rightController.inputSource)) {
            return ControllerState.TRIGGER_DOWN2;
        }
        if (m_controllerPress.GetStateUp(rightController.inputSource)) {
            return ControllerState.TRIGGER_UP2;
        }
#elif Oculus_Quest_Controllers
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            return ControllerState.TRIGGER_DOWN1;
        }
        else if (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            return ControllerState.TRIGGER_UP1;
        }
        else if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
        {
            return ControllerState.TRIGGER_DOWN2;
        }
        else if (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.LTouch))
        {
            return ControllerState.TRIGGER_UP2;
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
                    toReturn = ControllerState.TRIGGER_DOWN1;
                }
                else
                {
                    toReturn = ControllerState.TRIGGER_UP1;
                }
                fingerPressToggle.indexFinger = !fingerPressToggle.indexFinger;
            }
            return toReturn;
        }
        else
        {
            if (input.GetFingerDown(Finger.indexFinger))
            {
                return ControllerState.TRIGGER_DOWN1;
            }
            else if (input.GetFingerUp(Finger.indexFinger))
            {
                return ControllerState.TRIGGER_UP1;
            }
        }
#endif

        return ControllerState.NONE;
    }

    // Used to adjust the scale of the object that is currently being held
    void adjustScale()
    {
        float currentDistanceBetweenControllers = Vector3.Distance(leftController.transform.position, rightController.transform.position);
        float changeInDistance =  currentDistanceBetweenControllers - distanceBetweenControllersOnPickup;
        objectInHand.transform.localScale = objectScaleOnPickup + (changeInDistance * Vector3.one);
    }

    void pickupWithController()
    {
        if(leftController == null && rightController == null)
        {
            return;
        }
        ControllerState currentState = ControllerEvents();

        if (currentState == ControllerState.TRIGGER_DOWN1 ||
            currentState == ControllerState.TRIGGER_DOWN2) {
            if (collidingObject)
            {
                GrabObject();
                distanceBetweenControllersOnPickup = Vector3.Distance(leftController.transform.position, rightController.transform.position);
                objectScaleOnPickup = objectInHand.transform.localScale;
            }      
        }

        if (currentState == ControllerState.TRIGGER_UP1 || 
            currentState == ControllerState.TRIGGER_UP2) {
            if (objectInHand)
            {
                ReleaseObject();
            }

        }
    }

	// Update is called once per frame
	void Update () {
        pickupWithController();
        if (objectInHand != null)
        {
            adjustScale();
        } 
    }

    // Pickup methods
    private void SetCollidingObject(Collider other)
    {

		if (collidingObject || !other.GetComponent<Rigidbody>() || interactionLayers != (interactionLayers | (1 << other.gameObject.layer)))
        {
            return;
        }
        collidingObject = other.gameObject;
		onHover.Invoke ();
    }


    public void OnTriggerEnter(Collider other)
    {
        SetCollidingObject(other);
    }


    public void OnTriggerStay(Collider other)
    {
        SetCollidingObject(other);
    }


    public void OnTriggerExit(Collider other)
    {
		if (!collidingObject || interactionLayers != (interactionLayers | (1 << other.gameObject.layer)))
        {
            return;
        }
		onUnhover.Invoke ();
        collidingObject = null;
    }

    private void GrabObject()
    {
        objectInHand = collidingObject;
        onSelectObject.Invoke();
        collidingObject = null;

        var joint = AddFixedJoint();
        objectInHand.transform.position = this.transform.position;
        joint.connectedBody = objectInHand.GetComponent<Rigidbody>();
    }

    private FixedJoint AddFixedJoint()
    {
        FixedJoint fx = gameObject.AddComponent<FixedJoint>();
        fx.breakForce = Mathf.Infinity;
        fx.breakTorque = Mathf.Infinity;
        return fx;
    }

    private void ReleaseObject()
    {
        if (GetComponent<FixedJoint>())
        {

            GetComponent<FixedJoint>().connectedBody = null;
            Destroy(GetComponent<FixedJoint>());
#if SteamVR_Legacy
            objectInHand.GetComponent<Rigidbody>().velocity = Controller1.velocity;
            objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller1.angularVelocity;
#elif SteamVR_2
            objectInHand.GetComponent<Rigidbody>().velocity = leftController.GetVelocity();
            objectInHand.GetComponent<Rigidbody>().angularVelocity = leftController.GetAngularVelocity();
#endif

        }
        onDropObject.Invoke();
        objectInHand = null;
    }
}
