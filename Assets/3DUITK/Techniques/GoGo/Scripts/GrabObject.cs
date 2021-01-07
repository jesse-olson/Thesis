using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

public class GrabObject : Technique {

    public GameObject selection = null; // holds the selected object

    public GameObject collidingObject;
    private GameObject objectInHand;

    void OnEnable() {
        var render = SteamVR_Render.instance;
        if (render == null) {
            enabled = false;
            return;
        }
    }
    
    private void SetCollidingObject(Collider col) {

        if (collidingObject || !col.GetComponent<Rigidbody>()) {
            return;
        }

        collidingObject = col.gameObject;
    }


    public void OnTriggerEnter(Collider other) {
        SetCollidingObject(other);
        if (interactionLayers == (interactionLayers | (1 << other.gameObject.layer)) && objectInHand == null) {
            onHover.Invoke();
        }
    }


    public void OnTriggerStay(Collider other) {
        SetCollidingObject(other);
    }


    public void OnTriggerExit(Collider other) {
        if (!collidingObject) {
            return;
        }
        if (interactionLayers == (interactionLayers | (1 << other.gameObject.layer))) {
            onUnhover.Invoke();
        }

        collidingObject = null;
    }

    private void PickUpObject() {
        objectInHand = collidingObject;

        collidingObject = null;

        var joint = AddFixedJoint();
        joint.connectedBody = objectInHand.GetComponent<Rigidbody>();
    }

    private FixedJoint AddFixedJoint() {
        FixedJoint fx = gameObject.AddComponent<FixedJoint>();
        fx.breakForce = 20000;
        fx.breakTorque = 20000;
        return fx;
    }

    private void ReleaseObject() {

        if (GetComponent<FixedJoint>()) {

            GetComponent<FixedJoint>().connectedBody = null;
            Destroy(GetComponent<FixedJoint>());
#if SteamVR_Legacy
            objectInHand.GetComponent<Rigidbody>().velocity = Controller.velocity;
            objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
#elif SteamVR_2
            objectInHand.GetComponent<Rigidbody>().velocity = trackedObj.GetVelocity();
            objectInHand.GetComponent<Rigidbody>().angularVelocity = trackedObj.GetAngularVelocity();
#else

#endif

        }

        objectInHand = null;
    }
    
    // Update is called once per frame
    void Update() {
        if (ControllerEvents() == ControllerState.TRIGGER_DOWN) {
            if (collidingObject && interactionLayers == (interactionLayers | (1 << collidingObject.gameObject.layer))) {
                onSelectObject.Invoke();
                if (interactionType == InteractionType.Selection) {
                    // Pure selection
                    print("selected " + collidingObject);
                    selection = collidingObject;
                } else {
                    // Manipulation
                    PickUpObject();
                    selection = collidingObject;
                }

            }
        }

        if (ControllerEvents() == ControllerState.TRIGGER_UP) {
            if (objectInHand) {
                ReleaseObject();
            }
        }
    }
}
