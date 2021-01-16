using UnityEngine;
using UnityEngine.Events;

public abstract class Technique : MonoBehaviour
{
    public enum PointerPosition
    {
        IndexFinger = 1,
        Wrist       = 2,
        Palm        = 3,
        Thumb       = 4,
        MiddleKnuckle = 5
    }

    public enum Hand
    {
        Left,
        Right
    }

    protected ControllerEventSystem eventSystem;

#if SteamVR_Legacy
    protected SteamVR_TrackedObject trackedObj;
    protected SteamVR_Controller.Device controller;
#elif SteamVR_2
    protected SteamVR_Behaviour_Pose trackedObj;
    public SteamVR_Action_Boolean m_controllerPress;
#elif Oculus_Quest_Hands
    protected GameObject trackedObj;   // The position of the hand that will be tracked
    public GameObject tipPrefab;    // The prefab used for the tip of the finger
    public GameObject palmPrefab;   //
    protected GameObject palm;

    public PointerPosition pointFrom;

    public bool fingerPressToggleEnabled;   // Used to see if the user wants a thing
    protected HandGesture fingerPressToggle = new HandGesture().SetAll(false);
#else
    protected Transform trackedObj;
#endif

    public  InteractionType interactionType;
    protected SelectionManipulation selectionManipulator;

    public LayerMask interactionLayers;

    protected Hand hand;
    protected ControllerPicked controllerPicked;
    protected Transform head;

    internal bool       highlighted;
    internal GameObject highlightedObject;

    internal bool       selected;
    internal GameObject selectedObject;

    public UnityEvent onSelectObject; // Invoked when an object is selected
    public UnityEvent onDropObject; // Invoked when object is dropped

    public UnityEvent onHover;      // Invoked when an object is hovered by technique
    public UnityEvent onUnhover;    // Invoked when an object is no longer hovered by the technique

    protected void FindEventSystem()
    {
        eventSystem = FindObjectOfType<ControllerEventSystem>();
        if (hand == Hand.Left)
        {
            eventSystem.onLeftTriggerDown.AddListener(SelectObject);
            eventSystem.onLeftTriggerUp.AddListener(ReleaseObject);
            eventSystem.onLeftTriggerNearPressDown.AddListener(Enable);
            eventSystem.onLeftTriggerNearPressUp.AddListener(Disable);
        }
        else
        {
            eventSystem.onRightTriggerNearPressDown.AddListener(Enable);
            eventSystem.onRightTriggerNearPressUp.AddListener(Disable);
            eventSystem.onRightTriggerDown.AddListener(SelectObject);
            eventSystem.onRightTriggerUp.AddListener(ReleaseObject);
        }
    }

    protected abstract void Enable();

    protected abstract void Disable();

    public abstract void SelectObject();

    public abstract void ReleaseObject();

    public void HighlightObject(GameObject toHighlight)
    {
        if (selected) return;
        if (highlightedObject != toHighlight)
        {
            onUnhover.Invoke(); // unhover old object
            highlighted = toHighlight != null;
            highlightedObject = toHighlight;

            if (highlighted)
                onHover.Invoke();
        }
    }

    //    private void GrabObject()
    //    {
    //        objectInHand = collidingObject;
    //        onSelectObject.Invoke();
    //        collidingObject = null;

    //        var joint = AddFixedJoint();
    //        objectInHand.transform.position = this.transform.position;
    //        joint.connectedBody = objectInHand.GetComponent<Rigidbody>();
    //    }

    //    private FixedJoint AddFixedJoint()
    //    {
    //        FixedJoint fx = gameObject.AddComponent<FixedJoint>();
    //        fx.breakForce = Mathf.Infinity;
    //        fx.breakTorque = Mathf.Infinity;
    //        return fx;
    //    }

    //    private void ReleaseObject()
    //    {
    //        if (GetComponent<FixedJoint>())
    //        {

    //            GetComponent<FixedJoint>().connectedBody = null;
    //            Destroy(GetComponent<FixedJoint>());
    //#if SteamVR_Legacy
    //            objectInHand.GetComponent<Rigidbody>().velocity = Controller1.velocity;
    //            objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller1.angularVelocity;
    //#elif SteamVR_2
    //            objectInHand.GetComponent<Rigidbody>().velocity = leftController.GetVelocity();
    //            objectInHand.GetComponent<Rigidbody>().angularVelocity = leftController.GetAngularVelocity();
    //#endif

    //    }
    //    onDropObject.Invoke();
    //    objectInHand = null;
    //}

    public void SetPrimaryTrackedObject(Transform transform)
    {
        trackedObj = transform;
    }

    public Transform GetPrimaryTrackedObject()
    {
        return trackedObj;
    }

    public void SetHead(Transform head)
    {
        this.head = head;
    }

    public void SetControllerPicked(ControllerPicked controllerPicked)
    {
        this.controllerPicked = controllerPicked;
    }

    public void SetHand(Hand hand)
    {
        this.hand = hand;
    }
}
