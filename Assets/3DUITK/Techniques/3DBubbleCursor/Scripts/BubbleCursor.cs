
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

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Valve.VR;

public class BubbleCursor : ScrollingTechnique {
     
    private GameObject[] interactableObjects; // In-game objects
    private GameObject cursor;                // The cursor object in the scene
    private float startRadius = 0f;
    private GameObject radiusBubble;
    private GameObject objectBubble;

    private bool pickedUpObject = false; //ensure only 1 object is picked up at a time

    internal GameObject selectedObject;
    public   GameObject currentlyHovering = null;

    private readonly float bubbleOffset = 0.2f;

    void Awake()
    {
        cursor       = transform.Find("BubbleCursor").gameObject;
        radiusBubble = cursor.transform.Find("RadiusBubble").gameObject;
        objectBubble = transform.Find("ObjectBubble").gameObject;

        InitializeControllers();

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
        //MoveCursorPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (trackedObj == null)
        {
            return;
        }

        PadScrolling();
        cursor.transform.localPosition = new Vector3(0, 0, extendDistance);
        //MoveCursorPosition();

        GameObject[] closestObjects = FindClosestObjects();

        float closestCircleRadius       = closestObjects[0].GetComponent<Renderer>().bounds.SqrDistance(cursor.transform.position);
        float secondClosestCircleRadius = closestObjects[1].GetComponent<Renderer>().bounds.SqrDistance(cursor.transform.position);

        //Different colliders
        if (interactableObjects.Length >= 2)
        {
            cursor.GetComponent<SphereCollider>().radius = closestCircleRadius + 0.1f;
            radiusBubble.transform.localScale = (closestCircleRadius + 0.1f) * new Vector3(2, 2, 2);

            objectBubble.transform.position   = closestObjects[0].transform.position;
            objectBubble.transform.localScale = closestObjects[0].transform.lossyScale + bubbleOffset * Vector3.one;
            //objectBubble.transform.localScale = closestObjects[0].GetComponent<Renderer>().bounds.extents + bubbleOffset * Vector3.one;
            PickupObject(closestObjects[0]);
            
            if (currentlyHovering != closestObjects[0])
            {
                onUnhover.Invoke();
                currentlyHovering = closestObjects[0];
                onHover.Invoke();
            }
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
            if (obj.layer == Mathf.Log(interactionLayers.value, 2)) {
                interactableObjects.Add(obj);
            }
        }
        return interactableObjects.ToArray();
    }

    new private void InitializeControllers() {
        base.InitializeControllers();
        if (controllerPicked == ControllerPicked.Head) {

#if SteamVR_Legacy
            trackedObj = head.GetComponent<SteamVR_TrackedObject>();
#elif SteamVR_2
            trackedObj = head.GetComponent<SteamVR_Behaviour_Pose>();
#else 
            trackedObj = head;
#endif
        } else if(trackedObj == null) {
            print("Couldn't detect trackedObject, please specify the controller type in the settings.");
            Application.Quit();
        }
    }

    void SetCursorParent() {
        cursor.transform.SetParent(trackedObj.transform);
    }

    /// <summary>
    /// Loops through interactable gameObjects within the scene & stores them in a 2D array.
    ///     2D array is used to keep store gameObjects distance & also keep track of their index. 
    ///     eg [0][0] returns objects distance [0][1] returns objects index.
    /// Using Linq 2D array is sorted based on closest distances
    /// </summary>
    /// <returns>2D Array which contains order of objects with the closest distances & their allocated index</returns>
	private GameObject[] FindClosestObjects() {
        float shortestDistance       = float.MaxValue;
        float secondShortestDistance = float.MaxValue;
        GameObject[] toReturn = new GameObject[2];

        for (int index = 0; index < interactableObjects.Length; index++) {
            // TODO: Find a better way to do this. 
            // Maybe use the magnitude of the scale? Or the Magnitude of a bounding box/ sphere
            float dist = interactableObjects[index].GetComponent<Renderer>().bounds.SqrDistance(cursor.transform.position);

            if(dist < shortestDistance)
            {
                secondShortestDistance = shortestDistance;
                shortestDistance = dist;
                toReturn[1] = toReturn[0];
                toReturn[0] = interactableObjects[index];
            }
            else if(dist < secondShortestDistance)
            {
                secondShortestDistance = dist;
                toReturn[1] = interactableObjects[index];
            }
        }
        return toReturn;
    }

    void PickupObject(GameObject obj) {
        if (trackedObj != null) {
            if (ControllerEvents() == ControllerState.TRIGGER_DOWN && pickedUpObject == false) {
                selectedObject = obj; // Storing the object as an instance variable instead of using the obj parameter fixes glitch of it not properly resetting on TriggerUp
                pickedUpObject = true;
                if (interactionType == InteractionType.Manipulation_Movement) {
                    //obj.GetComponent<Collider>().attachedRigidbody.isKinematic = true;
                    obj.transform.SetParent(cursor.transform);
                } else if (interactionType == InteractionType.Manipulation_UI && this.GetComponent<SelectionManipulation>().inManipulationMode == false) {
                    this.GetComponent<SelectionManipulation>().selectedObject = obj;
                } else if (interactionType == InteractionType.Selection) {
                    
                }
                onSelectObject.Invoke();
            }
            if (ControllerEvents() == ControllerState.TRIGGER_UP && pickedUpObject == true) {
                if(interactionType == InteractionType.Manipulation_Movement) {
                    //obj.GetComponent<Collider>().attachedRigidbody.isKinematic = false;
                    selectedObject.transform.SetParent(null);
                    onDropObject.Invoke();
                }
                pickedUpObject = false;
            }
        }
    }

    //void MoveCursorPosition()
    //{
    //    Vector3 controllerFwd = trackedObj.transform.forward;
    //    Vector3 controllerPos = trackedObj.transform.position;
    //    float distance_formula_on_vector = controllerFwd.magnitude;

    //    // Using formula to find a point which lies at distance on a 3D line from vector and direction
    //    controllerPos += (extendDistance / (distance_formula_on_vector)) * controllerFwd;

    //    cursor.transform.position = controllerPos;
    //    cursor.transform.rotation = trackedObj.transform.rotation;
    //}
}
