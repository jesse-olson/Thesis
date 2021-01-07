/* HOMER implementation by Kieran May
 * University of South Australia
 * 
 * The developed HOMER algorithm was based off: (pg 34-35) https://people.cs.vt.edu/~bowman/3dui.org/course_notes/siggraph2001/basic_techniques.pdf 
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
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

public class HOMER : Technique {    
    void Awake()
    {
        //cameraHead = GameObject.Find("Camera (eye)");
        mirroredCube = this.transform.Find("Mirrored Cube").gameObject;
        InitializeControllers();
    }

    // Use this for initialization
    void Start()
    {
        laser = Instantiate(laserPrefab);
        laserTransform = laser.transform;
    }

    // Update is called once per frame
    void Update()
    {
#if SteamVR_Legacy
        controller = SteamVR_Controller.Input((int)trackedObj.index);
#endif
        if (!objSelected)
        {
            MoveMirroredCube();
            CastRay();
        }
        else
        {
            HomerFormula();
        }
    }

    private void ShowLaser()
    {
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

        if (interactionLayers == (interactionLayers | (1 << hit.transform.gameObject.layer))) {
            hoveredObject = hit.transform.gameObject;
            onUnhover.Invoke();
            onHover.Invoke();
            InstantiateObject(hit.transform.gameObject);
        }
    }

    
    float Disth = 0f;
    float Disto = 0f;
    bool objSelected = false;
    private GameObject virtualHand;
    public GameObject objectInHand;
    public GameObject handPrefab;
    private Transform oldParent;
    public GameObject hoveredObject;

    private void InstantiateObject(GameObject obj) {
        if (ControllerEvents() == ControllerState.TRIGGER_DOWN) {
            virtualHand = Instantiate(handPrefab);
            virtualHand.transform.position = obj.transform.position;
            virtualHand.SetActive(true);

            objectInHand = obj;
            oldParent = objectInHand.transform.parent;
            objectInHand.transform.SetParent(virtualHand.transform);
            objSelected = true;

            laser.SetActive(false);
            onSelectObject.Invoke();

            Disth = Vector3.Distance(trackedObj.transform.position, head.transform.position);
            Disto = Vector3.Distance(obj.transform.position, head.transform.position);
        }
    }

    private void HomerFormula() {
        virtualHand.transform.localRotation = trackedObj.transform.localRotation;

        float currentHandDist = Vector3.Distance(trackedObj.transform.position, head.transform.position); // Physical hand distance
        float currentVirtualHandDist = currentHandDist * (Disto / Disth); // Virtual hand distance
        Vector3 currentTrackedDirection = (trackedObj.transform.position - head.transform.position);
        virtualHand.transform.position = head.transform.position + currentVirtualHandDist * currentTrackedDirection;

        if (ControllerEvents() == ControllerState.TRIGGER_DOWN) {
            objSelected = false;
            Destroy(virtualHand);
            objectInHand.transform.SetParent(oldParent);
            onDropObject.Invoke();
        }
    }

    private void CastRay() {
        ShowLaser();

        if (Physics.Raycast(trackedObj.transform.position, trackedObj.transform.forward, out RaycastHit hit, 100))
        {
            ShowLaser(hit);
        }
        else
        {
            onUnhover.Invoke();
        }
    }

    void MoveMirroredCube() {
        //getControllerPosition();
        Vector3 mirroredPos   = trackedObj.transform.position;
        Vector3 controllerPos = trackedObj.transform.forward;
        float distance_formula_on_vector = controllerPos.magnitude;

        mirroredPos += (100f / (distance_formula_on_vector)) * controllerPos;

        mirroredCube.transform.position = mirroredPos;
        mirroredCube.transform.rotation = trackedObj.transform.rotation;
    }
}