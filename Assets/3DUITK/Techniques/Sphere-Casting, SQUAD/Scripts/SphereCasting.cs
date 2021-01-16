/* Sphere-Casting implementation by Kieran May
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

public class SphereCasting : ScrollingTechnique
{
    private static readonly float RAY_DIST = 100.0f;

    public List<GameObject> selectableObjects = new List<GameObject>();

    private SquadMenu menu;
    public static bool inMenu = false;

    private Transform sphereObject;
    public bool squadEnabled = true;

    public GameObject laserPrefab;
    private GameObject laser; 
    
    private void ShowLaser(RaycastHit hit) {
        Vector3 hitPos = hit.transform.position;

        sphereObject.position = hitPos;

        //if (!inMenu) {
        //    sphereObject.transform.position = hitPos;
        //}
        //sphereObject.SetActive(!inMenu);
    } 
        
    void Awake() {
        sphereObject = transform.Find("SphereTooltip");

        if (squadEnabled) {
            menu = sphereObject.GetComponent<SquadMenu>();
            menu.sphereCasting = this;
        }
    }

    void Start() {
        laser = Instantiate(laserPrefab) as GameObject;
        laser.transform.SetParent(transform);
        laser.SetActive(true);
    }
    
    void Update() {
        PadScrolling();
        sphereObject.transform.localScale = extendDistance * Vector3.one;

        Ray ray = new Ray(trackedObj.position, trackedObj.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, RAY_DIST, interactionLayers))
        {
            ShowLaser(hit);

            //if (!squadEnabled)
            //{
            //    selectableObjects.Clear();

            //}
            //else if (!menu.IsActive() && !menu.QuadrantIsPicked())
            //{
            //    if (menu.GetSelectableObjects().Count > 1)
            //    {
            //        menu.ClearList();
            //    }
            //    else if (menu.GetSelectableObjects().Count == 1)
            //    {
            //        selectableObjects.Clear();
            //    }

            //}
            //else if (menu.QuadrantIsPicked() == true)
            //{
            //    //print("object selected:" + hit.transform.gameObject.name);
            //    //todo check if obj is child of createtriangles panel
            //    //menu.selectObject(controller, hit.transform.gameObject);
            //}
            //else
            //{
            //    //menu.selectQuad(controller, hit.transform.gameObject);
            //    menu.HoverQuad(hit.transform.gameObject);
            //}
        }
    }

    public override void SelectObject()
    {
        //for (int i = 0; i < obj.Count; i++)
        //{
        //    if (obj[i].layer != LayerMask.NameToLayer("Ignore Raycast") &&
        //        interactionLayers == 1 << obj[i].layer)
        //    {
        //        obj[i].transform.SetParent(sphereCasting.GetPrimaryTrackedObject().transform);
        //        pickedUpObject = true;
        //    }
        //}
    }

    public override void ReleaseObject()
    {
        //for (int i = 0; i < obj.Count; i++)
        //{
        //    if (obj[i].layer != LayerMask.NameToLayer("Ignore Raycast") &&
        //        obj[i].layer == Mathf.Log(interactableLayer.value, 2))
        //    {
        //        obj[i].transform.SetParent(null);
        //        pickedUpObject = false;
        //        /*if (i == obj.Count-1) {
        //            clearList();
        //        }*/
        //    }
        //}
    }

    protected override void Enable()
    {
        //throw new System.NotImplementedException();
    }

    protected override void Disable()
    {
        //throw new System.NotImplementedException();
    }

    public void AddToSelectable(GameObject gameObject)
    {
        if(interactionLayers == 1 << gameObject.layer &&
            !selectableObjects.Contains(gameObject))
        {
            selectableObjects.Add(gameObject);
        }
    }

    public void RemoveFromSelectable(GameObject gameObject)
    {
        selectableObjects.Remove(gameObject);
    }
}
