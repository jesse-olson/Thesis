/* Raycasting implementation by Kieran May
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
using UnityEngine.Events;
using Valve.VR;

public class Raycasting : Technique {

    private bool pickedUpObject = false; //ensure only 1 object is picked up at a time
    public GameObject lastSelectedObject;

    private GameObject manipulationIcons;

    void Awake()
    {
        mirroredCube = this.transform.Find("Mirrored Cube").gameObject;
        InitializeControllers();
        if (interactionType == InteractionType.Manipulation_Full)
        {
            this.gameObject.AddComponent<SelectionManipulation>();
            this.GetComponent<SelectionManipulation>().trackedObj = trackedObj;
            manipulationIcons = GameObject.Find("Manipulation_Icons");
            this.GetComponent<SelectionManipulation>().manipulationIcons = manipulationIcons;
        }

    }

    void Start()
    {
        laser = Instantiate(laserPrefab);
        laserTransform = laser.transform;
    }

    void Update()
    {
        mirroredObject();

        ShowLaser();

        if (Physics.Raycast(trackedObj.transform.position, trackedObj.transform.forward, out RaycastHit hit, 100))
        {
            PickupObject(hit.transform.gameObject);
            ShowLaser(hit);
        }
    }

    private void ShowLaser() {
        laser.SetActive(true);
        mirroredCube.SetActive(true);
    }

    private void ShowLaser(RaycastHit hit) {
        Vector3 hitPoint = hit.point;
        mirroredCube.SetActive(false);
        laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        laserTransform.LookAt(hitPoint);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, hit.distance);
    }
    
    public void PickupObject(GameObject obj) {
        if (interactionLayers != (interactionLayers | (1 << obj.layer))) {
            // object is wrong layer so return immediately 
            return;
        }
        if(lastSelectedObject != obj) {
            // is a different object from the currently highlighted so unhover
            onUnhover.Invoke();
        }
        onHover.Invoke();
        
        if (trackedObj != null) {
            if (ControllerEvents() == ControllerState.TRIGGER_DOWN && pickedUpObject == false) {
                if (interactionType == InteractionType.Selection)
                {
                    lastSelectedObject = obj;
                    objectSelected = true;
                }
                else if (interactionType == InteractionType.Manipulation_Movement)
                {
                    obj.transform.SetParent(trackedObj.transform);
                    lastSelectedObject = obj; // Storing the object as an instance variable instead of using the obj parameter fixes glitch of it not properly resetting on TriggerUp
                    pickedUpObject = true;
                }
                else if (interactionType == InteractionType.Manipulation_Full && this.GetComponent<SelectionManipulation>().inManipulationMode == false)
                {
                    lastSelectedObject = obj;
                    objectSelected = true;
                    this.GetComponent<SelectionManipulation>().selectedObject = obj;
                    onSelectObject.Invoke();
                }
            }

            if (ControllerEvents() == ControllerState.TRIGGER_UP && pickedUpObject == true) {
                if (interactionType == InteractionType.Manipulation_Movement) {
                    lastSelectedObject.transform.SetParent(null);
                    pickedUpObject = false;
                    onDropObject.Invoke();
                }
                objectSelected = false;               
            }
        }
    }

    void mirroredObject() {
        Vector3 controllerPos = trackedObj.transform.forward;
        float distance_formula_on_vector = controllerPos.magnitude;
        Vector3 mirroredPos = trackedObj.transform.position;

        mirroredPos += (100f / (distance_formula_on_vector)) * controllerPos;

        mirroredCube.transform.position = mirroredPos;
        mirroredCube.transform.rotation = trackedObj.transform.rotation;
    }
}
