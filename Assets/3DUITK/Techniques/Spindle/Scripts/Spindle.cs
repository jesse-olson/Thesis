using UnityEngine;
using Valve.VR;

public class Spindle : TwoHandTechnique {
    public bool spindleAndWheel;

    private Transform interactorTransform;
    private SpindleInteractor interactor;

    private float currentDistance;

    private float pickupDistance;
    private Vector3 pickupScale;

    private void Start()
    {
        FindEventSystem();
        interactor = GetComponentInChildren<SpindleInteractor>();
        interactorTransform = interactor.transform;
    }

    // Update is called once per frame
    void Update () {
        currentDistance = Vector3.Distance(
            trackedObj.position,
            secondaryTrackedObj.position
        );

        AdjustScale();
        SetPositionOfInteraction();
    }

    private void SetPositionOfInteraction()
    {
        Vector3 midPoint = (trackedObj.position + secondaryTrackedObj.position) / 2;

        interactorTransform.position = midPoint;

        //This may cause Issue due to the change of just using Spindle
        transform.LookAt(secondaryTrackedObj.transform);

        //Need to look into the Spindle and Wheel
        if (spindleAndWheel) {
            Vector3 rotation = new Vector3(0, 0, transform.eulerAngles.z + trackedObj.eulerAngles.z);
            transform.Rotate(rotation);
        }
    }

    // Used to adjust the scale of the object that is currently being held
    private void AdjustScale()
    {
        float changeInDistance = currentDistance - pickupDistance;
        selectedObject.transform.localScale = pickupScale + (changeInDistance * Vector3.one);
    }

    public override void SelectObject()
    {
        if (!highlighted) return;

        pickupDistance = currentDistance;
        pickupScale = selectedObject.transform.localScale;

        selectedObject = highlightedObject;
        onSelectObject.Invoke();

        var joint = AddFixedJoint();
        selectedObject.transform.position = transform.position;
        joint.connectedBody = selectedObject.GetComponent<Rigidbody>();

    }

    private FixedJoint AddFixedJoint()
    {
        FixedJoint fx = gameObject.AddComponent<FixedJoint>();
        fx.breakForce = Mathf.Infinity;
        fx.breakTorque = Mathf.Infinity;
        return fx;
    }

    public override void ReleaseObject()
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
        selectedObject = null;
        selected = false;
    }

    public void SetHighlighted(Collider collider)
    {
        if (!highlighted && 1 << collider.gameObject.layer == interactionLayers)
        {
            highlighted = true;
            highlightedObject = collider.gameObject;
            onHover.Invoke();
        }
    }

    public void RemoveHighlighted(Collider collider)
    {
        if (collider.gameObject == highlightedObject)
        {
            onUnhover.Invoke();
            highlighted = false;
            highlightedObject = null;
        }
    }

    public GameObject GetHighlighted()
    {
        return highlightedObject;
    }

    public bool HasSelected()
    {
        return selected;
    }

    protected override void Enable()
    {

    }

    protected override void Disable()
    {

    }
}
