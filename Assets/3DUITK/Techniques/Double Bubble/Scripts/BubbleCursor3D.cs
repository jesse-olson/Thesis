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
using UnityEngine.Events;
using UnityEngine.UI;
using Valve.VR;

public class BubbleCursor3D : ScrollingTechnique {
    private static readonly float MIN_RAD = 1f;
    private static readonly float MIN_DIA = 2 * MIN_RAD;

    private GameObject[] interactableObjects; // In-game objects
    internal GameObject cursor;
    private GameObject radiusBubble;
    internal GameObject objectBubble;

    public BubbleSelection bubbleSelection;
    
    public readonly float bubbleOffset = 0.6f;

    public GameObject controllerRight;
    public GameObject controllerLeft;
    public GameObject cameraHead;
    
    private bool pickedUpObject = false; //ensure only 1 object is picked up at a time
	internal GameObject selectedObject;

    private GameObject[] getInteractableObjects() {
        GameObject[] AllSceneObjects = FindObjectsOfType<GameObject>();
        List<GameObject> interactableObjects = new List<GameObject>();
        foreach(GameObject obj in AllSceneObjects) {
            if(obj.layer == Mathf.Log(interactionLayers.value, 2)) {
                interactableObjects.Add(obj);
            }
        }
        return interactableObjects.ToArray();
    }

    void Awake() {
        cursor = this.transform.Find("BubbleCursor").gameObject;
        radiusBubble = cursor.transform.Find("RadiusBubble").gameObject;
        objectBubble = this.transform.Find("ObjectBubble").gameObject;

        InitializeControllers();

        if (interactionType == InteractionType.Manipulation_UI) {
            this.gameObject.AddComponent<SelectionManipulation>();
            this.GetComponent<SelectionManipulation>().trackedObj = trackedObj;
        }
    }

    // Use this for initialization
	void Start () {
		interactableObjects = getInteractableObjects();
		extendDistance = Vector3.Distance(trackedObj.transform.position, cursor.transform.position);
	
        cursor.transform.SetParent(trackedObj.transform);

        bubbleSelection.trackedObj = trackedObj;
        bubbleSelection.cameraHead = cameraHead;
    }

    // Update is called once per frame
    void Update() {
		if (bubbleSelection.inBubbleSelection == false) {
			PadScrolling ();

			GameObject[] closestObjects = FindClosestObjects();

			//Different colliders
			if (interactableObjects.Length >= 2) {
                float closestCircleRadius       = closestObjects[0].GetComponent<Renderer>().bounds.SqrDistance(cursor.transform.position);
                float secondClosestCircleRadius = closestObjects[1].GetComponent<Renderer>().bounds.SqrDistance(cursor.transform.position);

                float closestValue = 2 * Mathf.Min (closestCircleRadius, secondClosestCircleRadius);

                cursor.GetComponent<SphereCollider>().radius = closestValue / 2f;
                if (cursor.GetComponent<SphereCollider>().radius < MIN_RAD)
                {
                    cursor.GetComponent<SphereCollider>().radius = MIN_RAD;
                }

                radiusBubble.transform.localScale = closestValue * Vector3.one;
                if (radiusBubble.transform.localScale.x < MIN_DIA)
                {
                    radiusBubble.transform.localScale = MIN_DIA * Vector3.one;
                }

                if (closestCircleRadius < secondClosestCircleRadius) {
					objectBubble.transform.localScale = Vector3.zero;
				}
                else {
					objectBubble.transform.position   = closestObjects[0].transform.position;
                    objectBubble.transform.localScale = closestObjects[0].transform.localScale + bubbleOffset * Vector3.one;
				}

                if (bubbleSelection.GetSelectableObjects().Count > 1 && bubbleSelection.tempObjectStored == null)
                {
                    bubbleSelection.EnableMenu(bubbleSelection.GetSelectableObjects());
                }
                else
                {
                    PickupObject(closestObjects[0]);
                }
            }
		}
	}

    /// <summary>
    /// Loops through interactable gameObjects within the scene & stores them in a 2D array.
    ///     -2D array is used to keep store gameObjects distance & also keep track of their index. eg [0][0] returns objects distance [0][1] returns objects index.
    /// Using Linq 2D array is sorted based on closest distances
    /// </summary>
    /// <returns>2D Array which contains order of objects with the closest distances & their allocated index</returns>
	private GameObject[] FindClosestObjects()
    {
        float shortestDistance = float.MaxValue;
        float secondShortestDistance = float.MaxValue;
        GameObject[] toReturn = new GameObject[2];

        for (int index = 0; index < interactableObjects.Length; index++)
        {
            // TODO: Find a better way to do this. 
            // Maybe use the magnitude of the scale? Or the Magnitude of a bounding box/ sphere
            float dist = interactableObjects[index].GetComponent<Renderer>().bounds.SqrDistance(cursor.transform.position);

            if (dist < shortestDistance)
            {
                secondShortestDistance = shortestDistance;
                shortestDistance = dist;
                toReturn[1] = toReturn[0];
                toReturn[0] = interactableObjects[index];
            }
            else if (dist < secondShortestDistance)
            {
                secondShortestDistance = dist;
                toReturn[1] = interactableObjects[index];
            }
        }
        return toReturn;
    }
    

	void PickupObject(GameObject obj) {
		if (ControllerEvents() == ControllerState.TRIGGER_DOWN && pickedUpObject == false) {
            pickedUpObject = true;
            switch (interactionType)
            {
                case InteractionType.Selection:
                    selectedObject = obj;
                    break;

                case InteractionType.Manipulation_Movement:
                case InteractionType.Manipulation_Full:
                    obj.transform.SetParent(cursor.transform);
                    selectedObject = obj; // Storing the object as an instance variable instead of using the obj parameter fixes glitch of it not properly resetting on TriggerUp
                    break;

                case InteractionType.Manipulation_UI:
                    selectedObject = obj;
                    this.GetComponent<SelectionManipulation>().selectedObject = obj;
                    break;
            }
            onSelectObject.Invoke();
		}
		else if (ControllerEvents() == ControllerState.TRIGGER_UP && pickedUpObject == true) {
			if(interactionType == InteractionType.Manipulation_Movement || interactionType == InteractionType.Manipulation_Full) {
				selectedObject.transform.SetParent(null);
				bubbleSelection.tempObjectStored = null;
                onDropObject.Invoke();
			}
			pickedUpObject = false;
		}
	}
}
