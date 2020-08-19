using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using Valve.VR;

public class DepthRay : Technique {

    /* Depth Ray implementation by Kieran May
     * University of South Australia
	 *
	 *  Copyright(C) 2019 Kieran May
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

#if SteamVR_Legacy
    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device controller;
#elif SteamVR_2
    public SteamVR_Action_Boolean m_controllerPress;
    private SteamVR_Behaviour_Pose trackedObj;
    public SteamVR_Action_Vector2 m_touchpadAxis;
#else
    //public GameObject trackedObj;
#endif

    //public GameObject rightController;
    //public GameObject leftController;

    //public LayerMask interactionLayers;


    private GameObject mirroredCube;
    private RaycastHit[] raycastObjects;

    //public GameObject laserPrefab;
    private GameObject laser;
    private Transform laserTransform;
    private GameObject cubeAssister;
    private Vector3 hitPoint;
    private Vector3 hitPoint2D;
    private RaycastHit[] oldHits;

    //internal bool objectSelected = false;

    //public enum InteractionType { Selection, Manipulation_Movement, Manipulation_Full, Manipulation_UI};
    public enum SelectionAssister { Hide_Closest_Only, Hide_All_But_Closest };

    //public enum ControllerPicked { Left_Controller, Right_Controller };
    //public ControllerPicked controllerPicked;

    //public UnityEvent selectedObject; // Invoked when an object is selected

    //public UnityEvent droppedObject; // Invoked when an object is dropped

    //public UnityEvent hovered; // Invoked when an object is hovered by technique
    //public UnityEvent unHovered; // Invoked when an object is no longer hovered by the technique

    private void ShowLaser(RaycastHit hit) {
        mirroredCube.SetActive(false);
        laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        //cubeAssister.transform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        laserTransform.LookAt(hitPoint);
        //laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, distance*10);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, hit.distance);
        //print(distance);
    }
    private void ShowLaser() {
        laser.SetActive(true);
        mirroredCube.SetActive(true);


        //laser.transform.position = trackedObj.transform.position*2;
        //laser.transform.position = new Vector3(trackedObj.transform.position.x, trackedObj.transform.position.y, trackedObj.transform.position.z*1.25f);
        //laser.transform.position = Vector3.Lerp(trackedObj.transform.position, forward, 0.6f);
        //laserTransform.LookAt(forward);
        //laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, 1f);
        //laserTransform.position = Vector3.Lerp(trackedObj.transform.position, forward, .5f);
        /*laserTransform.position = Vector3.Lerp(trackedObj.transform.position, forward, .5f);
        laserTransform.LookAt(trackedObj.transform.position);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, 10f);*/
    }

    private float extendDistance = 0f;
    private float cursorSpeed = 20f; // Decrease to make faster, Increase to make slower

    private void PadScrolling() {
#if SteamVR_Legacy
        if (controller.GetAxis().y != 0) {
            extendDistance += controller.GetAxis().y / cursorSpeed;
            moveCubeAssister();
        }
#elif SteamVR_2
        if (m_touchpad.GetAxis(trackedObj.inputSource).y != 0) {
            extendDistance += m_touchpad.GetAxis(trackedObj.inputSource).y / cursorSpeed;
            moveCubeAssister();
        }
#else
        Vector2 thumbStick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        if(thumbStick.y != 0)
        {
            extendDistance += thumbStick.y / cursorSpeed;
            MoveCubeAssister();
        }
#endif
    }
    
    private bool pickedUpObject = false; //ensure only 1 object is picked up at a time
    private GameObject tempObjectStored;

    void PickupObject(GameObject obj) {
        if (interactionLayers != (interactionLayers | (1 << obj.layer))) {
            // check if is an interactable object if not return 
            return;
        }
        if (trackedObj != null) {
            if (ControllerEvents() == ControllerState.TRIGGER_DOWN && pickedUpObject == false) {
                if (interactionType == InteractionType.Manipulation_Movement || interactionType == InteractionType.Manipulation_Full) {
                    obj.transform.SetParent(trackedObj.transform);
                    tempObjectStored = obj; // Storing the object as an instance variable instead of using the obj parameter fixes glitch of it not properly resetting on TriggerUp
                    pickedUpObject = true;
                } else if (interactionType == InteractionType.Selection) {
                    tempObjectStored = obj;
                    objectSelected = true;
                    print("Selected object in pure selection mode:" + tempObjectStored.name);
                } else if (interactionType == InteractionType.Manipulation_UI && this.GetComponent<SelectionManipulation>().inManipulationMode == false) {
                    tempObjectStored = obj;
                    objectSelected = true;
                    this.GetComponent<SelectionManipulation>().selectedObject = obj;
                }
                selectedObject.Invoke();
            }
            if (ControllerEvents() == ControllerState.TRIGGER_UP && pickedUpObject == true) {
                if (interactionType == InteractionType.Manipulation_Movement) {
                    tempObjectStored.transform.SetParent(null);
                    pickedUpObject = false;
                } else if (interactionType == InteractionType.Selection) {
                    objectSelected = false;
                }
                droppedObject.Invoke();
            }
        }
    }


    void MoveCubeAssister() {
        //getControllerPosition();
        Vector3 controllerPos = trackedObj.transform.forward;
        Vector3 normalisedControllerPos  = controllerPos / controllerPos.magnitude;

        // Using formula to find a point which lies at distance on a 3D line from vector and direction
        if (extendDistance < 0) {
            extendDistance = 0;
        }

        cubeAssister.transform.position = trackedObj.transform.position + extendDistance * normalisedControllerPos;
        cubeAssister.transform.rotation = trackedObj.transform.rotation;

        mirroredCube.transform.position = trackedObj.transform.position + 100f * normalisedControllerPos;
        mirroredCube.transform.rotation = trackedObj.transform.rotation;
    }

    float dist = 100f;

    private int ClosestObject() {
        int lowestValue = 0;
        float lowestDist = 0;

        for (int i = 0; i < raycastObjects.Length; i++) {
            float dist = Vector3.Distance(cubeAssister.transform.position, raycastObjects[i].transform.position) / 2f;
            if (i == 0) {
                lowestDist = dist;
                lowestValue = 0;
            } else {
                if (dist < lowestDist) {
                    lowestDist = dist;
                    lowestValue = i;
                }
            }
        }

        return lowestValue;
    }

    private void ResetAllMaterials() {
        if (oldHits != null) {
            foreach (RaycastHit hit in oldHits) {
                hit.transform.gameObject.GetComponent<Renderer>().material = defaultMat;
            }
        }
    }

    private bool Contains(GameObject obj, RaycastHit[] hits) {
        if (hits.Length >= 1) {
            foreach (RaycastHit hit in hits) {
                if (hit.transform.gameObject == obj) {
                    return true;
                }
            }
        }
        //print(interactableObject.Count);
        //obj.GetComponent<Renderer>().material = interactableObject.Find(d => d == obj).transform.GetComponent<Renderer>().material;
        obj.GetComponent<Renderer>().material = defaultMat;
        return false;
    }

    private void InitializeControllers() {
        if (controllerPicked == ControllerPicked.Right_Controller) {
#if SteamVR_Legacy
            trackedObj = rightController.GetComponent<SteamVR_TrackedObject>();
#elif SteamVR_2
            trackedObj = rightController.GetComponent<SteamVR_Behaviour_Pose>();
#else
            trackedObj = rightController;
#endif
        } else if (controllerPicked == ControllerPicked.Left_Controller) {
#if SteamVR_Legacy
            trackedObj = leftController.GetComponent<SteamVR_TrackedObject>();
#elif SteamVR_2
            trackedObj = leftController.GetComponent<SteamVR_Behaviour_Pose>();
#else
            trackedObj = leftController;
#endif
        } else {
            print("Couldn't detect trackedObject, please specify the controller type in the settings.");
            Application.Quit();
        }
    }

    void Awake() {
        mirroredCube = this.transform.Find("Mirrored Cube").gameObject;
        cubeAssister = this.transform.Find("Cube Assister").gameObject;

        InitializeControllers();

        if (interactionType == InteractionType.Manipulation_UI) {
            this.gameObject.AddComponent<SelectionManipulation>();
            this.GetComponent<SelectionManipulation>().trackedObj = trackedObj;
        }
    }

    void Start() {
        laser = Instantiate(laserPrefab);
        laserTransform = laser.transform;
        cubeAssister.transform.position = trackedObj.transform.position;
        /*GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int count = 0;
        for (int i=0; i<allObjects.Length; i++) {
            if (allObjects[i].layer != LayerMask.NameToLayer("Ignore Raycast")) {
                raycastObjects[count] = allObjects[i];
            }
            count++;
        }*/
    }


    float distance = 0f;
    Vector3 forward;
    public GameObject currentClosestObject;
    public Material outlineMaterial;
    public Material defaultMat;
    private Material currentClosestObjectMaterial;

    void Update()
    {
#if SteamVR_Legacy
        controller = SteamVR_Controller.Input((int)trackedObj.index);
#endif

        MoveCubeAssister();
        PadScrolling();
        forward = trackedObj.transform.TransformDirection(Vector3.forward) * 10;
        ShowLaser();
        RaycastHit[] hits = Physics.RaycastAll(trackedObj.transform.position, forward, 100.0F);
        if (hits.Length >= 1)
        {
            raycastObjects = hits;
            int closestVal = ClosestObject();
            if (raycastObjects[closestVal].transform.name != "Mirrored Cube")
            {

                if (currentClosestObject != raycastObjects[closestVal].transform.gameObject)
                {
                    currentClosestObject = raycastObjects[closestVal].transform.gameObject;
                }
                //print ("closest obj:"+currentClosestObject);
                PickupObject(currentClosestObject);
            }
        }

        //print("hit length:" + hits.Length);
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            //print("hit:" + hit.transform.name + " index:"+i);
            distance = hit.distance;
            hitPoint = hit.point;
            //hit.transform.gameObject.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0.5f);
            ShowLaser(hit);
        }
        if (hits.Length > 1)
        {
            oldHits = hits;
        }
    }

//    void GrabObject(GameObject toGrab)
//    {
//        lastSelectedObject = toGrab;

//        lastSelectedObject.GetComponent<Rigidbody>().velocity = Vector3.zero; // Setting velocity to 0 so can catch without breakforce effecting it
//        var joint = AddFixedJoint();
//        joint.connectedBody = lastSelectedObject.GetComponent<Rigidbody>();
//    }

//    private FixedJoint AddFixedJoint()
//    {
//        FixedJoint fx = trackedObj.AddComponent<FixedJoint>();
//        fx.breakForce = 1000;
//        fx.breakTorque = Mathf.Infinity;
//        return fx;
//    }

//    private void ReleaseObject()
//    {
//        FixedJoint joint = trackedObj.GetComponent<FixedJoint>();
//        if (joint)
//        {

//            joint.connectedBody = null;
//            Destroy(joint);
//#if SteamVR_Legacy
//            lastSelectedObject.GetComponent<Rigidbody>().velocity = Controller.velocity;
//            lastSelectedObject.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
//#elif SteamVR_2
//            lastSelectedObject.GetComponent<Rigidbody>().velocity = trackedObj.GetVelocity();
//            lastSelectedObject.GetComponent<Rigidbody>().angularVelocity = trackedObj.GetAngularVelocity();
//#else
//            //Vector3 newVelocity        = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch);
//            //Vector3 newAngularVelocity = OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.RTouch);
//            //lastSelectedObject.GetComponent<Rigidbody>().velocity        = newVelocity;
//            //lastSelectedObject.GetComponent<Rigidbody>().angularVelocity = newAngularVelocity;
//#endif
//        }

//        lastSelectedObject = null;
//    }
}
