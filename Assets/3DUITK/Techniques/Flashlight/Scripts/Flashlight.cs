/*
 *  Flashlight - Script to create a cone "flashlight" for the VR
 *  Flaslight technique within Unity for the HTC Vive.
 *  
 *  Copyright(C) 2018  Ian Hanan
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 * 
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.If not, see<http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

// Must be parented to a steam controller to can access controls to change size

public class Flashlight : ScrollingTechnique {
    private static readonly float MIN_LENGTH = 1;
    private static readonly float MAX_LENGTH = 20;

    private Transform interactor;

    private List<GameObject> collidingObjects;

    [Range(1, 20)]
    public float length = 1;
    [Range(1, 5)]
    public float diameter = 1;

    void OnEnable()
    {
        collidingObjects = new List<GameObject>();
        var render = SteamVR_Render.instance;
        if (render == null)
        {
            enabled = false;
            return;
        }
    }

    void Awake()
    {
        extendDistance = length;
        if (interactionType == InteractionType.Manipulation_UI)
        {
            gameObject.AddComponent<SelectionManipulation>();
            GetComponent<SelectionManipulation>().trackedObj = trackedObj.gameObject;
#if SteamVR_2
            this.GetComponent<SelectionManipulation>().m_controllerPress = m_controllerPress;
#endif
        }
    }

    // Use this for initialization
    void Start() {
        FindEventSystem();
        // Set this flashlight to be child of the object it is set to be attached to
        transform.SetParent(trackedObj);
        // Making sure rotation and position is correct
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update() {
        PadScrolling();
        if(extendDistance < MIN_LENGTH)
            extendDistance = MIN_LENGTH;
        else if (extendDistance > MAX_LENGTH)
            extendDistance = MAX_LENGTH;

        interactor.gameObject.transform.localScale =
            new Vector3(diameter / 2, diameter / 2, length);

        GetObjectHoveringOver();
    }


    public override void SelectObject()
    {
        if (selected || !highlighted) return;

        selected = true;
        selectedObject = highlightedObject;

        switch (interactionType)
        {
            case InteractionType.Selection:
                // Pure selection
                print("selected " + selectedObject);
                break;

            case InteractionType.Manipulation_Movement:
            case InteractionType.Manipulation_Full:
                //Manipulation
                GrabObject();
                break;

            case InteractionType.Manipulation_UI:
                if (!GetComponent<SelectionManipulation>().inManipulationMode)
                    GetComponent<SelectionManipulation>().selectedObject = selectedObject;
                break;

            default:
                // Do nothing
                break;
        }
        onSelectObject.Invoke();
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
            selectedObject.GetComponent<Rigidbody>().velocity = Controller.velocity * Vector3.Distance(Controller.transform.pos, selectedObject.transform.position);
            selectedObject.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
#elif SteamVR_2
            selectedObject.GetComponent<Rigidbody>().velocity = trackedObj.GetVelocity() * Vector3.Distance(trackedObj.transform.position, selectedObject.transform.position);
            selectedObject.GetComponent<Rigidbody>().angularVelocity = trackedObj.GetAngularVelocity();
#endif
        }
        onDropObject.Invoke();
        selected = false;
        selectedObject = null;
    }


    /**
     * Adds the object involved in a trigger event 
     * to the collidingObjects list
     */
    public void AddCollidingObject(Collider col)
    {
        if (!collidingObjects.Contains(col.gameObject) &&
            interactionLayers == 1 << col.gameObject.layer &&
            col.gameObject.GetComponent<Rigidbody>())
        {
            collidingObjects.Add(col.gameObject);
        }
    }

    public void RemoveCollidingObject(Collider col)
    {
        collidingObjects.Remove(col.gameObject);
    }



    private void GetObjectHoveringOver()
    {
        Vector3 controllerPos = trackedObj.position;
        Vector3 controllerFwd = trackedObj.forward;

        float closestDist = float.PositiveInfinity;
        GameObject closestObject = null;

        foreach (GameObject potentialObject in collidingObjects)
        {
            // Using vector algebra to get shortest distance between object and vector 
            Vector3 localObjectPos = potentialObject.transform.position - controllerPos;
            float dist = Vector3.Cross(localObjectPos, controllerFwd).magnitude;

            if (dist < closestDist)
            {
                closestDist = dist;
                closestObject = potentialObject;
            }
        }

        HighlightObject(closestObject);
    }


    void GrabObject()
    {
        var joint = AddFixedJoint();
        selectedObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        joint.connectedBody = selectedObject.GetComponent<Rigidbody>();
    }

    private FixedJoint AddFixedJoint()
    {
        FixedJoint fx = trackedObj.gameObject.AddComponent<FixedJoint>();
        fx.breakForce = Mathf.Infinity;
        fx.breakTorque = Mathf.Infinity;
        return fx;
    }

    protected override void Enable()
    {
        interactor.gameObject.SetActive(true);
    }

    protected override void Disable()
    {
        interactor.gameObject.SetActive(false);
        ReleaseObject();
        HighlightObject(null);
    }

    public void SetInteractor(Transform interactor)
    {
        this.interactor = interactor;
    }
}
