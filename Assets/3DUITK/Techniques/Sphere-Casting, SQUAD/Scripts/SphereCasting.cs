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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

public class SphereCasting : Technique {    
    private PickupObjects pickupObjs;
    private SquadMenu menu;
    public static bool inMenu = false;

    private GameObject laser;
    private Transform laserTransform;

    private GameObject sphereObject;
    public bool squadEnabled = true;

    private float extendDiameter = 0f;  // The diameter of the Sphere being cast
    private float cursorSpeed    = 20f; // Speed at which the diameter increases/ decreases.
                                        // Decrease to make faster, Increase to make slower

    private void ShowLaser(RaycastHit hit) {
        Vector3 hitPos = hit.transform.position;

        laser.SetActive(true);

        if (!inMenu) {
            sphereObject.transform.position = hitPos;
        }
        sphereObject.SetActive(!inMenu);

        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPos, .5f);
        laserTransform.LookAt(hitPos);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, hit.distance);
    }

    private void ShowLaser() {
        laser.SetActive(true);
    }    
        
    void Awake() {
        sphereObject = this.transform.Find("SphereTooltip").gameObject;

        InitializeControllers();

        pickupObjs = sphereObject.AddComponent<PickupObjects>();
        pickupObjs.sphereCasting = this;

        if (squadEnabled) {
            menu = sphereObject.GetComponent<SquadMenu>();
            menu.sphereCasting = this;
        }

    }

    void Start() {
        laser = Instantiate(laserPrefab);
        laserTransform = laser.transform;

        ShowLaser();
    }
    
    void Update() {
#if SteamVR_Legacy
        controller = SteamVR_Controller.Input((int)trackedObj.index);
#endif
        PadScrolling();

        if (Physics.Raycast(trackedObj.transform.position, trackedObj.transform.forward, out RaycastHit hit, 100))
        {
            ShowLaser(hit);

            if (!squadEnabled)
            {
                pickupObjs.PickupObject(pickupObjs.getSelectableObjects());
                pickupObjs.clearList();

            }
            else if (!menu.IsActive() && !menu.QuadrantIsPicked())
            {
                //print("selectable objects:"+menu.getSelectableObjects().Count);
                if (menu.GetSelectableObjects().Count > 1)
                {
                    menu.EnableSQUAD(menu.GetSelectableObjects());
                    menu.ClearList();
                }
                else if (menu.GetSelectableObjects().Count == 1)
                {
                    pickupObjs.PickupObject(pickupObjs.getSelectableObjects());
                    pickupObjs.clearList();
                }

            }
            else if (menu.QuadrantIsPicked() == true)
            {
                //print("object selected:" + hit.transform.gameObject.name);

                //todo check if obj is child of createtriangles panel
                //menu.selectObject(controller, hit.transform.gameObject);

            }
            else
            {
                //menu.selectQuad(controller, hit.transform.gameObject);
                menu.HoverQuad(hit.transform.gameObject);
            }
        }
    }

    private void PadScrolling()
    {
#if SteamVR_Legacy
        if (controller.GetAxis().y != 0) {
            extendDiameter += controller.GetAxis().y / cursorSpeed;
        }
#elif SteamVR_2
        if (m_touchpadAxis.GetAxis(trackedObj.inputSource).y != 0) {
            extendDiameter += m_touchpadAxis.GetAxis(trackedObj.inputSource).y / cursorSpeed;
        }   
#else
        Vector2 thumbStick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        if (thumbStick.y != 0)
        {
            extendDiameter += thumbStick.y / cursorSpeed;
        }
#endif
        if(extendDiameter < 0)
        {
            extendDiameter = 0;
        }

        sphereObject.transform.localScale = Vector3.one * extendDiameter;
    }

}
