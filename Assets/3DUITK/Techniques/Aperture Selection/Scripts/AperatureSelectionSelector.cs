using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

public class AperatureSelectionSelector : Technique {

    public GameObject selection; // holds the selected object

    private List<GameObject> collidingObjects;
    private GameObject objectInHand;
    public GameObject objectHoveredOver;
    public FixedJoint joint;

    public GameObject orientationPlates;

    // Checks if holding object in hand
    public bool HoldingObject() {
        return objectInHand != null;
    }

    void OnEnable() {
        collidingObjects = new List<GameObject>();
        trackedObj = this.transform.gameObject;
        var render = SteamVR_Render.instance;
        if (render == null) {
            enabled = false;
            return;
        }
    }

    void Awake() {
        InitializeControllers();
    }

    // Update is called once per frame
    void Update()
    {
        objectHoveredOver = GetObjectHoveringOver();
        onHover.Invoke();

        ControllerState controllerState = ControllerEvents();

        //Debug.Log(controllerEvents());
        switch (controllerState)
        {
            case ControllerState.TRIGGER_DOWN:
                if (collidingObjects.Count > 0)
                {
                    if (interactionType == InteractionType.Selection)
                    {
                        // Pure selection
                        selection = objectHoveredOver;
                    }
                    else if (interactionType == InteractionType.Manipulation_Full)
                    {
                        //Manipulation
                        GrabObject();
                    }
                    selection = objectHoveredOver;
                    onSelectObject.Invoke();
                }
                break;

            case ControllerState.TRIGGER_UP:
                Debug.Log(objectInHand);
                if (HoldingObject())
                {
                    ReleaseObject();
                }
                break;

            default:
                break;
        }
    }

    private void SetCollidingObject(Collider col) {
        if (collidingObjects.Contains(col.gameObject) || !col.GetComponent<Rigidbody>()) {
            return;
        }
        collidingObjects.Add(col.gameObject);
    }


    public void OnTriggerEnter(Collider other) {
        SetCollidingObject(other);
    }


    public void OnTriggerStay(Collider other) {
        SetCollidingObject(other);
    }


    public void OnTriggerExit(Collider other) {
        if (!collidingObjects.Contains(other.gameObject)) {
            return;
        }
        collidingObjects.Remove(other.gameObject);
    }

    // Attempts to get object in selection by its orientation, if it fails will return null
    public GameObject GetByOrientation() {
        // TODO: add orientational check
        return null;
    }

    private GameObject GetObjectHoveringOver() {
        // Attempt to select the object by its orientation, if that fails it will return null and in that case select via 
        // closest object cone algorithm below it
        GameObject orientationSelection;
        if ((orientationSelection = GetByOrientation()) != null) {
            return orientationSelection;
        }

        double smallestDistance = double.MaxValue;
        GameObject closestObject = null;

        Vector3 controllerFwd = trackedObj.transform.forward;
        Vector3 controllerPos = trackedObj.transform.position;

        foreach (GameObject potentialObject in collidingObjects) {
            // Only check for objects on the interaction layer
            if (interactionLayers == (interactionLayers | (1 << potentialObject.layer))) {
                // Object can only have one layer so can do calculation for object here
                Vector3 objectPosition = potentialObject.transform.position;

                // Using vector algebra to get shortest distance between object and vector 
                Vector3 forwardControllerToObject = trackedObj.transform.position - objectPosition;
                float distanceBetweenRayAndPoint = Vector3.Magnitude(Vector3.Cross(forwardControllerToObject, controllerFwd)) / Vector3.Magnitude(controllerFwd);

                if(0 < distanceBetweenRayAndPoint && distanceBetweenRayAndPoint < smallestDistance)
                {
                    smallestDistance = distanceBetweenRayAndPoint;
                    closestObject = potentialObject;
                }
            }

            if (objectHoveredOver != closestObject)
            {
                onUnhover.Invoke();
            }
        }

        onUnhover.Invoke();
        return closestObject;
    }



    private void GrabObject() {
        objectInHand = objectHoveredOver;

        if (objectHoveredOver != null) {
            collidingObjects.Remove(objectInHand);

            var joint = AddFixedJoint();
            objectInHand.GetComponent<Rigidbody>().velocity = Vector3.zero; // Setting velocity to 0 so can catch without breakforce effecting it
            joint.connectedBody = objectInHand.GetComponent<Rigidbody>();
        }
    }

    private FixedJoint AddFixedJoint() { 
        FixedJoint fx = gameObject.AddComponent<FixedJoint>();
        fx.breakForce = 1000;
        fx.breakTorque = Mathf.Infinity;
        return fx;
    }

    private void ReleaseObject() {

        Debug.Log("Releasing object");

        if (GetComponent<FixedJoint>()) {

            GetComponent<FixedJoint>().connectedBody = null;
            Destroy(GetComponent<FixedJoint>());
#if SteamVR_Legacy
            objectInHand.GetComponent<Rigidbody>().velocity = Controller.velocity * Vector3.Distance(trackedObj.transform.position, objectInHand.transform.position);
            objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
#elif SteamVR_2
            objectInHand.GetComponent<Rigidbody>().velocity = trackedObj.GetVelocity() * Vector3.Distance(trackedObj.transform.position, objectInHand.transform.position);
            objectInHand.GetComponent<Rigidbody>().angularVelocity = trackedObj.GetAngularVelocity();
#else
            //objectInHand.GetComponent<Rigidbody>().velocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch) * Vector3.Distance(trackedObj.transform.position, objectInHand.transform.position);
#endif

            objectInHand = null;
        }

        Debug.Log("Object Released");
    }    
}
