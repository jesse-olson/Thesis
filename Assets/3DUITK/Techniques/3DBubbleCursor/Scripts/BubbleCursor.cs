using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor;
using Valve.VR;

public class BubbleCursor : MonoBehaviour {

    /* 3D Bubble Cursor implementation by Kieran May
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
    private SteamVR_Behaviour_Pose trackedObj;
    public SteamVR_Action_Boolean m_controllerPress;
    public SteamVR_Action_Boolean m_touchpad;
    public SteamVR_Action_Vector2 m_touchpadAxis;
#else
    public GameObject trackedObj;
#endif

    private GameObject[] interactableObjects; // In-game objects
    private GameObject cursor;                // The cursor object in the scene
    private float startRadius = 0f;
    private GameObject radiusBubble;
    private GameObject objectBubble;
    public LayerMask interactableLayer;

    public enum InteractionType { Selection, Manipulation_Movement, Manipulation_UI };
    public InteractionType interactionType;

    public enum ControllerPicked { Left_Controller, Right_Controller, Head };
    public ControllerPicked controllerPicked;

    public GameObject currentlyHovering = null;

    public GameObject rightController;
    public GameObject leftController;
    public GameObject cameraHead;
    public UnityEvent selectedObject; // Invoked when an object is selected
    public UnityEvent droppedObject; // Invoked when an object is dropped
    public UnityEvent hovered; // Invoked when an object is hovered by technique
    public UnityEvent unHovered; // Invoked when an object is no longer hovered by the technique

    private readonly float bubbleOffset = 0.6f;

    void Awake()
    {
        cursor = this.transform.Find("BubbleCursor").gameObject;
        radiusBubble = cursor.transform.Find("RadiusBubble").gameObject;
        objectBubble = this.transform.Find("ObjectBubble").gameObject;
        initializeControllers();

        if (interactionType == InteractionType.Manipulation_UI)
        {
            this.gameObject.AddComponent<SelectionManipulation>();
            this.GetComponent<SelectionManipulation>().trackedObj = trackedObj;
        }
    }

    // Use this for initialization
    void Start()
    {
        interactableObjects = GetInteractableObjects();
        startRadius = cursor.GetComponent<SphereCollider>().radius;
        extendDistance = Vector3.Distance(trackedObj.transform.position, cursor.transform.position);

        SetCursorParent();
        moveCursorPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (trackedObj != null)
        {
#if SteamVR_Legacy
            controller = SteamVR_Controller.Input((int)trackedObj.index);
#endif
            PadScrolling();
        }

        float[][] shortestDistances = FindClosestObjects();

        float ClosestCircleRadius = 0f;
        float SecondClosestCircleRadius = 0f;

        //Different colliders
        if (interactableObjects.Length >= 2)
        {
            GameObject closestObj       = interactableObjects[(int)shortestDistances[0][1]];
            GameObject secondClosestObj = interactableObjects[(int)shortestDistances[1][1]];

            ClosestCircleRadius       = shortestDistances[0][0] - (closestObj.transform.lossyScale.x / 2f)       + (closestObj.transform.lossyScale.x / 2f);
            SecondClosestCircleRadius = shortestDistances[1][0] - (secondClosestObj.transform.lossyScale.x / 2f) + (secondClosestObj.transform.lossyScale.x / 2f);

            float closestValue = Mathf.Min(ClosestCircleRadius, SecondClosestCircleRadius);

            cursor.GetComponent<SphereCollider>().radius = closestValue;
            radiusBubble.transform.localScale = 2 * closestValue * Vector3.one;

            if (ClosestCircleRadius < SecondClosestCircleRadius)
            {
                objectBubble.transform.localScale = Vector3.zero;

                PickupObject(closestObj);
            }
            else
            {
                objectBubble.transform.position   = closestObj.transform.position;
                objectBubble.transform.localScale = closestObj.transform.lossyScale + bubbleOffset * Vector3.one;

                PickupObject(closestObj);
            }

            if (currentlyHovering != interactableObjects[(int)shortestDistances[0][1]])
            {
                unHovered.Invoke();
            }
            currentlyHovering = interactableObjects[(int)shortestDistances[0][1]];
            hovered.Invoke();
        }
        else
        {
            print("ERROR: Unable to initialize bubble cursor - Current objects using interactable layer in scene is:" + interactableObjects.Length + " (Requires atleast 2 GameObjects in scene to use bubble cursor)");
            print("- Make sure to set the GameObjects layer type to the Interactable Layer type specified in the prefab settings...");
        }
    }

    private GameObject[] GetInteractableObjects() {
        GameObject[] AllSceneObjects = FindObjectsOfType<GameObject>();

        List<GameObject> interactableObjects = new List<GameObject>();
        foreach (GameObject obj in AllSceneObjects) {
            if (obj.layer == Mathf.Log(interactableLayer.value, 2)) {
                interactableObjects.Add(obj);
            }
        }
        return interactableObjects.ToArray();
    }

    private void initializeControllers() {
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
        } else if (controllerPicked == ControllerPicked.Head) {
#if SteamVR_Legacy
            trackedObj = cameraHead.GetComponent<SteamVR_TrackedObject>();
#elif SteamVR_2
            trackedObj = cameraHead.GetComponent<SteamVR_Behaviour_Pose>();
#else 
            trackedObj = cameraHead;
#endif
        } else {
            print("Couldn't detect trackedObject, please specify the controller type in the settings.");
            Application.Quit();
        }

    }

    void SetCursorParent() {
        cursor.transform.SetParent(trackedObj.transform);
    }

    /// <summary>
    /// Loops through interactable gameObjects within the scene & stores them in a 2D array.
    ///     -2D array is used to keep store gameObjects distance & also keep track of their index. eg [0][0] returns objects distance [0][1] returns objects index.
    /// Using Linq 2D array is sorted based on closest distances
    /// </summary>
    /// <returns>2D Array which contains order of objects with the closest distances & their allocated index</returns>
	private float[][] FindClosestObjects() {
        float shortestDistance = float.PositiveInfinity;
        int   shortestIndex    = 0;

        float secondShortestDistance = float.PositiveInfinity;
        int   secondShortestIndex    = 0;

        for (int index = 0; index < interactableObjects.Length; index++) {
            float dist = Vector3.Distance(
                cursor.transform.position, 
                interactableObjects[index].transform.position
                );
            dist -= (interactableObjects[index].transform.lossyScale.x / 2f);
            
            if (dist < secondShortestDistance) {
                if(dist < shortestDistance)
                {
                    secondShortestDistance = shortestDistance;
                    secondShortestIndex    = shortestIndex;

                    shortestDistance = dist;
                    shortestIndex    = index;
                }
                else
                {
                    secondShortestDistance = dist;
                    secondShortestIndex    = index;
                }
            }
        }

        float[][] toReturn = new float[2][];
        toReturn[0] = new float[2];
        toReturn[1] = new float[2];

        toReturn[0][0] = shortestDistance;
        toReturn[0][1] = shortestIndex;
        toReturn[1][0] = secondShortestDistance;
        toReturn[1][1] = secondShortestIndex;

        return toReturn;
    }

    /// <summary>
    /// Allows player to reposition the bubble cursor fowards & backwards.
    /// -Currently needs alot of work, only modifies the Z axis.
    /// </summary>
    private float extendDistance = 0f;
    public float cursorSpeed = 20f;     // Decrease to make faster, Increase to make slower

    private void PadScrolling() {
#if SteamVR_Legacy
        if (controller.GetAxis().y != 0) {
            extendDistance += controller.GetAxis().y / cursorSpeed;
            moveCursorPosition();
        }
#elif SteamVR_2
        if (m_touchpadAxis.GetAxis(trackedObj.inputSource).y != 0) {
            extendDistance += m_touchpadAxis.GetAxis(trackedObj.inputSource).y / cursorSpeed;
            moveCursorPosition();
        }

#else
        Vector2 joystick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        if (joystick.y > 0)
        {
            extendDistance += joystick.y / cursorSpeed;
            moveCursorPosition();
        }
#endif
    }

    public enum ControllerState {
        TRIGGER_UP, TRIGGER_DOWN, NONE
    }

    private ControllerState ControllerEvents() {
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

    private bool pickedUpObject = false; //ensure only 1 object is picked up at a time
    internal GameObject lastSelectedObject;

    void PickupObject(GameObject obj) {
        if (trackedObj != null) {
            if (ControllerEvents() == ControllerState.TRIGGER_DOWN && pickedUpObject == false) {
                if (interactionType == InteractionType.Manipulation_Movement) {
                    //obj.GetComponent<Collider>().attachedRigidbody.isKinematic = true;
                    obj.transform.SetParent(cursor.transform);
                    lastSelectedObject = obj; // Storing the object as an instance variable instead of using the obj parameter fixes glitch of it not properly resetting on TriggerUp
                    pickedUpObject = true;
                } else if (interactionType == InteractionType.Manipulation_UI && this.GetComponent<SelectionManipulation>().inManipulationMode == false) {
                    lastSelectedObject = obj;
                    pickedUpObject = true;
                    this.GetComponent<SelectionManipulation>().selectedObject = obj;
                } else if (interactionType == InteractionType.Selection) {
                    lastSelectedObject = obj;
                    pickedUpObject = true;                   
                }
                selectedObject.Invoke();
            }
            if (ControllerEvents() == ControllerState.TRIGGER_UP && pickedUpObject == true) {
                if(interactionType == InteractionType.Manipulation_Movement) {
                    //obj.GetComponent<Collider>().attachedRigidbody.isKinematic = false;
                    lastSelectedObject.transform.SetParent(null);
                    pickedUpObject = false;
                    droppedObject.Invoke();
                }
                pickedUpObject = false;
            }
        }
    }

    void moveCursorPosition() {
        Vector3 controllerPos = trackedObj.transform.forward;
        Vector3 pose = trackedObj.transform.position;
        float distance_formula_on_vector = controllerPos.magnitude;

        // Using formula to find a point which lies at distance on a 3D line from vector and direction
        pose += pose + (extendDistance / (distance_formula_on_vector)) * controllerPos;

        cursor.transform.position = pose;
        cursor.transform.rotation = trackedObj.transform.rotation;
    }
}
