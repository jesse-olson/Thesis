
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

using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class BubbleCursor : ScrollingTechnique {
     
    private GameObject[] interactableObjects; // In-game objects
    private Transform cursor;                // The cursor object in the scene
    private GameObject radiusBubble;
    //private GameObject objectBubble;

    //private readonly float bubbleOffset = 0.2f;

    void Awake()
    {
        cursor       = transform.Find("Cursor");
        radiusBubble = cursor.Find("RadiusBubble").gameObject;
        //objectBubble = transform.Find("ObjectBubble").gameObject;

        if (interactionType == InteractionType.Manipulation_UI)
        {
            gameObject.AddComponent<SelectionManipulation>();
            GetComponent<SelectionManipulation>().trackedObj = trackedObj.gameObject;
        }
    }

    // Use this for initialization
    void Start()
    {
        FindEventSystem();

        interactableObjects = GetInteractableObjects();
        cursor.SetParent(trackedObj.transform);
        extendDistance = cursor.localPosition.magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        PadScrolling();
        cursor.localPosition = new Vector3(0, 0, extendDistance);

        GameObject closest = FindClosestObjects();

        //Different colliders
        if (interactableObjects.Length >= 2)
        {
            float closestRadius = Vector3.Distance(closest.transform.position, cursor.position);
            //float closestRadius = Mathf.Sqrt(closest.GetComponent<Renderer>().bounds.SqrDistance(cursor.position));
            //float secondClosestCircleRadius = closestObjects[1].GetComponent<Renderer>().bounds.SqrDistance(cursor.position);
            cursor.GetComponent<SphereCollider>().radius = closestRadius;
            radiusBubble.transform.localScale = (2 * closestRadius) * Vector3.one;
            //objectBubble.transform.position   = closestObjects[0].transform.position;
            //objectBubble.transform.localScale = closestObjects[0].transform.lossyScale + bubbleOffset * Vector3.one;
            HighlightObject(closest);
        }
    }

    private GameObject[] GetInteractableObjects() {
        GameObject[] AllSceneObjects = FindObjectsOfType<GameObject>();

        List<GameObject> temp = new List<GameObject>();
        foreach (GameObject obj in AllSceneObjects) {
            if (interactionLayers == 1 << obj.layer) {
                temp.Add(obj);
            }
        }
        return temp.ToArray();
    }

    /// <summary>
    /// Loops through interactable gameObjects within the scene & stores them in a 2D array.
    ///     2D array is used to keep store gameObjects distance & also keep track of their index. 
    ///     eg [0][0] returns objects distance [0][1] returns objects index.
    /// Using Linq 2D array is sorted based on closest distances
    /// </summary>
    /// <returns>2D Array which contains order of objects with the closest distances & their allocated index</returns>
	private GameObject FindClosestObjects() {
        float shortestDistance = float.MaxValue;
        //float secondShortestDistance = float.MaxValue;
        GameObject toReturn = null;

        foreach (GameObject obj in interactableObjects) {
            // TODO: Find a better way to do this. 
            // Maybe use the magnitude of the scale? Or the Magnitude of a bounding box/ sphere
            float dist = Vector3.Distance(obj.transform.position, cursor.position);
            //float dist = Mathf.Sqrt(obj.GetComponent<Renderer>().bounds.SqrDistance(cursor.position));

            if(dist < shortestDistance)
            {
                //secondShortestDistance = shortestDistance;
                shortestDistance = dist;
                //toReturn[1] = toReturn[0];
                toReturn = obj;
            }
            //else if(dist < secondShortestDistance)
            //{
            //    secondShortestDistance = dist;
            //    toReturn[1] = obj;
            //}
        }
        return toReturn;
    }

    public override void SelectObject() {
        if (selected) return;
        else if (highlighted) {
            selected = true;
            selectedObject = highlightedObject; // Storing the object as an instance variable instead of using the obj parameter fixes glitch of it not properly resetting on TriggerUp
            if (interactionType == InteractionType.Manipulation_Movement) {
                //obj.GetComponent<Collider>().attachedRigidbody.isKinematic = true;
                selectedObject.transform.SetParent(cursor);
            } else if (interactionType == InteractionType.Manipulation_UI && !GetComponent<SelectionManipulation>().inManipulationMode) {
                GetComponent<SelectionManipulation>().selectedObject = selectedObject;
            }
            onSelectObject.Invoke();
        }
    }

    public override void ReleaseObject()
    {
        if (selected)
        {
            //obj.GetComponent<Collider>().attachedRigidbody.isKinematic = false;
            onDropObject.Invoke();
            selected = false;
            selectedObject.transform.SetParent(null);
        }
    }

    protected override void Enable()
    {
        //throw new System.NotImplementedException();
    }

    protected override void Disable()
    {
        //throw new System.NotImplementedException();
    }
}
