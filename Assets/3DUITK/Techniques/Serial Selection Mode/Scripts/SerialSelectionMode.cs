/*  Serial Selection Mode implementation by Kieran May
 *  University of South Australia
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

public class SerialSelectionMode : Technique {

    public GameObject laserPrefab;
    private GameObject laser;

    private Vector3 hitPoint;
    private Vector3 hitPoint2D;

    private bool pickUpObjectsActive = false;
    public Material outlineMaterial;

    private List<GameObject> selectedObjectsList = new List<GameObject>();
    private List<Material> rendererMaterialTrackerList = new List<Material>();

    void Awake() {
    }

    void Start() {
        laser = Instantiate(laserPrefab);
    }


    //void SelectObject(GameObject obj)
    //{
    //    if (!pickUpObjectsActive &&
    //        controllerEvents() == ControllerState.TRIGGER_DOWN)
    //    {
    //        if (obj != null && !selectedObjectsList.Contains(obj))
    //        {
    //            selectedObjectsList.Add(obj);
    //            rendererMaterialTrackerList.Add(obj.transform.GetComponent<Renderer>().material);
    //            obj.transform.GetComponent<Renderer>().material = outlineMaterial;
    //            onSelectObject.Invoke();
    //        }
    //        else
    //        {
    //            for (int i = 0; i < selectedObjectsList.Count; i++)
    //            {
    //                selectedObjectsList[i].transform.GetComponent<Renderer>().material = rendererMaterialTrackerList[i];
    //            }
    //            selectedObjectsList.Clear();
    //        }
    //    }
    //}

    //void ActivatePickupObjects()
    //{
    //    if (controllerEvents() == ControllerState.APPLICATION_MENU) {
    //        pickUpObjectsActive = !pickUpObjectsActive;
    //    }
    //    if (pickUpObjectsActive) {
    //        if (controllerEvents() == ControllerState.TRIGGER_DOWN &&
    //            !objectsSelected &&
    //            interactionType == InteractionType.Manipulation_Movement)
    //        {
    //            for (int i = 0; i < selectedObjectsList.Count; i++) {
    //                if (selectedObjectsList[i].layer != LayerMask.NameToLayer("Ignore Raycast")) {
    //                    selectedObjectsList[i].transform.SetParent(trackedObj.transform);
    //                    objectsSelected = true;
    //                }
    //            }
    //        }
    //        if (controllerEvents() == ControllerState.TRIGGER_UP &&
    //            objectsSelected)
    //        {
    //            for (int i = 0; i < selectedObjectsList.Count; i++) {
    //                if (selectedObjectsList[i].layer != LayerMask.NameToLayer("Ignore Raycast")) {
    //                    selectedObjectsList[i].transform.SetParent(null);
    //                    objectsSelected = false;
    //                }
    //            }
    //        }
    //    }
    //}

    void Update()
    {
        //ActivatePickupObjects();

        Ray ray = new Ray(trackedObj.position, trackedObj.forward);
        Physics.Raycast(ray, out RaycastHit hit, 100, interactionLayers);
        
        //SelectObject(hit.transform.gameObject);
    }

    protected override void Enable()
    {
        throw new System.NotImplementedException();
    }

    protected override void Disable()
    {
        throw new System.NotImplementedException();
    }

    public override void SelectObject()
    {
        throw new System.NotImplementedException();
    }

    public override void ReleaseObject()
    {
        throw new System.NotImplementedException();
    }
}
