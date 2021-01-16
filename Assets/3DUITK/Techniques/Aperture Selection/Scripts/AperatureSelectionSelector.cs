using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class AperatureSelectionSelector : Technique {

    private List<GameObject> collidingObjects;

    public GameObject orientationPlates;

    void OnEnable() {
        collidingObjects = new List<GameObject>();
        //trackedObj = transform.gameObject;
        var render = SteamVR_Render.instance;
        if (render == null) {
            enabled = false;
            return;
        }
    }

    void Awake() {
    }

    // Update is called once per frame
    void Update()
    {
        HighlightObject(FindByOrientation());
    }



    public void OnTriggerEnter(Collider other) {
        SetCollidingObject(other);
    }

    public void OnTriggerStay(Collider other) {
        SetCollidingObject(other);
    }

    public void OnTriggerExit(Collider other) {
        RemoveCollidingObject(other);
    }

    private void SetCollidingObject(Collider col)
    {
        if (!collidingObjects.Contains(col.gameObject)      &&  // Check to see if the GameObject is already in the list
            1 << col.gameObject.layer == interactionLayers  &&
            col.GetComponent<Rigidbody>())                      // And that it has a RigidBody
        {
            collidingObjects.Add(col.gameObject);
        }
    }

    private void RemoveCollidingObject(Collider col)
    {
        collidingObjects.Remove(col.gameObject);
    }


    // Attempts to get object in selection by its orientation, if it fails will return null
    public GameObject FindByOrientation() {
        // Attempt to select the object by its orientation, if that fails it will return null and in that case select via 
        // closest object cone algorithm below it

        // TODO: add orientational check

        return FindByDistance();
    }

    private GameObject FindByDistance() {
        double closestDist  = double.MaxValue;
        GameObject closestObject = null;

        Vector3 controllerFwd = trackedObj.forward;
        Vector3 controllerPos = trackedObj.position;

        foreach (GameObject potentialObject in collidingObjects) {
            // Object can only have one layer so can do calculation for object here
            Vector3 objectPos = potentialObject.transform.position - controllerPos;

            // Using vector algebra to get shortest distance between object and vector
            float dist = Vector3.Cross(objectPos, controllerFwd).magnitude;

            if(dist < closestDist)
            {
                closestDist = dist;
                closestObject = potentialObject;
            }
        }

        return closestObject;
    }

    private void GrabObject() {
        FixedJoint joint = AddFixedJoint();
        Rigidbody  body = selectedObject.GetComponent<Rigidbody>();
        body.velocity = Vector3.zero; // Setting velocity to 0 so can catch without breakforce effecting it
        joint.connectedBody = body;
    }

    private FixedJoint AddFixedJoint() { 
        FixedJoint fx = gameObject.AddComponent<FixedJoint>();
        fx.breakForce  = Mathf.Infinity;
        fx.breakTorque = Mathf.Infinity;
        return fx;
    }

    public override void SelectObject()
    {
        if (selected) return;
        else if (highlighted)
        {
            selected = true;
            selectedObject = highlightedObject;

            switch (interactionType)
            {
                case InteractionType.Manipulation_Movement:
                case InteractionType.Manipulation_Full:
                    GrabObject();
                    break;

                case InteractionType.Manipulation_UI:
                    //TODO: Manipulation Icons
                    break;

                default:
                    // Do nothing
                    break;
            }

            onSelectObject.Invoke();
        }
    }

    public override void ReleaseObject()
    {
        if (!selected) return;
        FixedJoint joint = GetComponent<FixedJoint>();
        if (joint != null)
        {
            joint.connectedBody = null;
            Destroy(joint);
#if SteamVR_Legacy
            objectInHand.GetComponent<Rigidbody>().velocity = Controller.velocity * Vector3.Distance(trackedObj.transform.position, objectInHand.transform.position);
            objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
#elif SteamVR_2
            objectInHand.GetComponent<Rigidbody>().velocity = trackedObj.GetVelocity() * Vector3.Distance(trackedObj.transform.position, objectInHand.transform.position);
            objectInHand.GetComponent<Rigidbody>().angularVelocity = trackedObj.GetAngularVelocity();
#else
            //objectInHand.GetComponent<Rigidbody>().velocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch) * Vector3.Distance(trackedObj.transform.position, objectInHand.transform.position);
#endif
        }

        onDropObject.Invoke();
        selected = false;
        selectedObject = null;
    }

    protected override void Enable()
    {
        throw new System.NotImplementedException();
    }

    protected override void Disable()
    {
        throw new System.NotImplementedException();
    }
}
