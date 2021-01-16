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

using UnityEngine;
using System.Linq;
using Valve.VR;

public class BubbleCursor3D : ScrollingTechnique {
    private static readonly float MIN_RAD = 1f;

    private GameObject[] interactableObjects; // In-game objects
    internal GameObject cursor;
    private  GameObject radiusBubble;
    internal GameObject objectBubble;

    public BubbleSelection bubbleSelection;
    
    public readonly float bubbleOffset = 0.6f;

    public GameObject controllerLeft;
    public GameObject controllerRight;
    public GameObject cameraHead;
    

    void Awake() {
        cursor       = transform.Find("BubbleCursor").gameObject;
        radiusBubble = cursor.transform.Find("RadiusBubble").gameObject;
        objectBubble = transform.Find("ObjectBubble").gameObject;

        FindEventSystem();
        //InitializeControllers();

        if (interactionType == InteractionType.Manipulation_UI) {
            gameObject.AddComponent<SelectionManipulation>();
            GetComponent<SelectionManipulation>().trackedObj = trackedObj.gameObject;
        }
    }

    // Use this for initialization
    void Start () {
        interactableObjects = GetInteractableObjects();
        extendDistance = Vector3.Distance(trackedObj.position, cursor.transform.position);
    
        cursor.transform.SetParent(trackedObj);

        //bubbleSelection.trackedObj = trackedObj.gameObject;
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

                float closestRadius = Mathf.Min (closestCircleRadius, secondClosestCircleRadius);

                float newRadius = Mathf.Max(closestRadius, MIN_RAD);

                cursor.GetComponent<SphereCollider>().radius = newRadius;

                radiusBubble.transform.localScale = 2 * newRadius * Vector3.one;

                objectBubble.SetActive(closestCircleRadius > secondClosestCircleRadius);
                objectBubble.transform.position   = closestObjects[0].transform.position;
                objectBubble.transform.localScale = closestObjects[0].transform.localScale + bubbleOffset * Vector3.one;

                //if (bubbleSelection.GetSelectableObjects().Count > 1 && bubbleSelection.tempObjectStored == null)
                //{
                //    bubbleSelection.EnableMenu(bubbleSelection.GetSelectableObjects());
                //}
                //else
                //{
                //    PickupObject(closestObjects[0]);
                //}
            }
        }
    }


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

    
    private GameObject[] GetInteractableObjects() {
        GameObject[] AllSceneObjects = FindObjectsOfType<GameObject>();
        interactableObjects = new GameObject[0];
        
        foreach(GameObject obj in AllSceneObjects) {
            if(1 << obj.layer == interactionLayers) {
                interactableObjects.Append(obj);
            }
        }
        return interactableObjects.ToArray();
    }

    public override void SelectObject()
    {
        selected = true;
        selectedObject = highlightedObject;
        switch (interactionType)
        {
            case InteractionType.Selection:
                break;

            case InteractionType.Manipulation_Movement:
            case InteractionType.Manipulation_Full:
                selectedObject.transform.SetParent(cursor.transform);
                break;

            case InteractionType.Manipulation_UI:
                if (!GetComponent<SelectionManipulation>().inManipulationMode)
                    GetComponent<SelectionManipulation>().selectedObject = selectedObject;
                break;
        }
        onSelectObject.Invoke();
    }

    public override void ReleaseObject()
    {
        switch (interactionType) {
            case InteractionType.Manipulation_Movement:
            case InteractionType.Manipulation_Full:
                selectedObject.transform.SetParent(null);
                break;

            default:
                // Do nothing
                break;
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
