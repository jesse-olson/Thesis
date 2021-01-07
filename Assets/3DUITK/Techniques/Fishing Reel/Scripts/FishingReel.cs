/* Fishing Reel implementation by Kieran May
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

public class FishingReel : ScrollingTechnique {
    private bool pickedUpObject = false; //ensure only 1 object is picked up at a time
    public GameObject lastSelectedObject;
    
    void Awake() {
        mirroredCube = this.transform.Find("Mirrored Cube").gameObject;

        InitializeControllers();

        if (interactionType == InteractionType.Manipulation_UI) {
            this.gameObject.AddComponent<SelectionManipulation>();
            this.GetComponent<SelectionManipulation>().trackedObj = trackedObj;
#if SteamVR_2
            this.GetComponent<SelectionManipulation>().m_controllerPress = m_controllerPress;
            this.GetComponent<SelectionManipulation>().m_touchpad = m_touchpad;
            this.GetComponent<SelectionManipulation>().m_touchpadAxis = m_touchpadAxis;
            this.GetComponent<SelectionManipulation>().m_applicationMenu = m_applicationMenu;
#endif
        }

    }

    void Start() {
        laser = Instantiate(laserPrefab);
        laserTransform = laser.transform;
    }

    void Update()
    {
        ShowLaser();
        MirroredObject();
        PadScrolling();
        //PalmTrigger input = palm.GetComponent<PalmTrigger>();
        //QuestDebug.Instance.Log(input.current.ToString());

        if (Physics.Raycast(trackedObj.transform.position, trackedObj.transform.forward, out RaycastHit hit, 100))
        {
            GameObject hitObject = hit.transform.gameObject;
            PickupObject(hitObject);
            if (
                pickedUpObject == true &&
                lastSelectedObject == hitObject
                )
            {
                ReelObject(hitObject);
            }
            else
            {

            }
            ShowLaser(hit);
        }
    }

    private void ShowLaser(RaycastHit hit) {
        Vector3 hitPoint = hit.point;
        mirroredCube.SetActive(false);
        laser.SetActive(true);
        laser.transform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        laser.transform.LookAt(hitPoint);
        laser.transform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, hit.distance);
    }

    private void ShowLaser() {
        laser.SetActive(true);
        mirroredCube.SetActive(true);
    }
    

    public void PickupObject(GameObject obj) {
        if (interactionLayers != (interactionLayers | (1 << obj.layer))) {
            // object is wrong layer so return immediately 
            return;
        }
        if (lastSelectedObject != obj) {
            // Is a different object from the currently highlighted so unhover
            onUnhover.Invoke();
        }

        onHover.Invoke();
        Vector3 controllerPos = trackedObj.transform.forward;
        ControllerState controllerState = ControllerEvents();

        if (trackedObj != null) {
            if (controllerState == ControllerState.TRIGGER_DOWN && 
                pickedUpObject == false)
            {
                if (interactionType == InteractionType.Manipulation_Movement) {
                    obj.transform.SetParent(trackedObj.transform);
                    extendDistance = Vector3.Distance(controllerPos, obj.transform.position);
                    pickedUpObject = true;
                } else if (interactionType == InteractionType.Manipulation_UI && this.GetComponent<SelectionManipulation>().inManipulationMode == false) {
                    objectSelected = true;
                    this.GetComponent<SelectionManipulation>().selectedObject = obj;
                } else if (interactionType == InteractionType.Selection) {
                    objectSelected = true;
                }

                lastSelectedObject = obj; // Storing the object as an instance variable instead of using the obj parameter fixes glitch of it not properly resetting on TriggerUp
                onSelectObject.Invoke();
            }

            if (controllerState == ControllerState.TRIGGER_UP &&
                pickedUpObject == true)
            {
                if (interactionType == InteractionType.Manipulation_Movement) {
                    lastSelectedObject.transform.SetParent(null);
                    pickedUpObject = false;
                    onDropObject.Invoke();
                }
                objectSelected = false;
            }
        }
    }
    
    void ReelObject(GameObject obj) {
        Vector3 controllerPos = trackedObj.transform.forward;
        Vector3 pos = trackedObj.transform.position;
        float distance_formula_on_vector = controllerPos.magnitude;
        // Using formula to find a point which lies at distance on a 3D line from vector and direction
        pos += (extendDistance / (distance_formula_on_vector)) * controllerPos;

        obj.transform.position = pos;
        obj.transform.rotation = trackedObj.transform.rotation;
    }

    void MirroredObject() {
        Vector3 controllerPos = trackedObj.transform.forward;
        float distance_formula_on_vector = controllerPos.magnitude;
        Vector3 mirroredPos = trackedObj.transform.position;

        mirroredPos += (100f / distance_formula_on_vector) * controllerPos;

        mirroredCube.transform.position = mirroredPos;
        mirroredCube.transform.rotation = trackedObj.transform.rotation;
    }

    private GameObject manipulationIcons;
}