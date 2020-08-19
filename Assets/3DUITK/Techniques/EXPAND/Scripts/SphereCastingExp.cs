﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class SphereCastingExp : MonoBehaviour {

    /* Sphere-Casting implementation for EXPAND by Kieran May
    * University of South Australia
    * 
    * */

#if SteamVR_Legacy
    internal SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device controller;
#elif SteamVR_2
    public SteamVR_Action_Boolean m_controllerPress;
    public SteamVR_Action_Vector2 m_touchpadAxis;
    internal SteamVR_Behaviour_Pose trackedObj;
#else
    public GameObject trackedObj;
#endif
    public GameObject cameraHead;
    public GameObject controllerLeft;
    public GameObject controllerRight;

    public static bool inMenu = false;
    internal ExpandMenu menu;

    public GameObject laserPrefab;
    private GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;
    private GameObject mirroredCube;
    private GameObject sphereObject;

    public GameObject selectedObject;

    public enum InteractionType { Selection, Manipulation_Movement, Manipulation_UI };
    public InteractionType interactionType;

    public enum ControllerPicked { Left_Controller, Right_Controller };
    public ControllerPicked controllerPicked;

    private void ShowLaser(RaycastHit hit) {
        //print("object hit:" + hit.transform.gameObject.name);
        //menu.selectQuad(controller, hit.transform.gameObject);
        //print(inMenu);
        mirroredCube.SetActive(false);
        laser.SetActive(true);
        //sphereObject.transform.position = hit.transform.position;
        //sphereObject.transform.position = hitPoint;
        if (hit.transform.gameObject.name != "Mirrored Cube" && inMenu == false) {
            //sphereObject.transform.position = hitPoint;
            sphereObject.transform.position = hit.transform.position;
            sphereObject.SetActive(true);
        } else {
            //sphereObject.SetActive(false);

        }
        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, .5f);
        laserTransform.LookAt(hitPoint);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, hit.distance);
    }

    private void ShowLaser() {
        laser.SetActive(true);
        mirroredCube.SetActive(true);
    }

    private float extendRadius = 0f;
    private float cursorSpeed = 20f; // Decrease to make faster, Increase to make slower

    private void PadScrolling() {
        Vector3 controllerPos = trackedObj.transform.forward;
#if SteamVR_Legacy
        if (controller.GetAxis().y != 0) {
            extendRadius += controller.GetAxis().y / cursorSpeed;
            sphereObject.transform.localScale = new Vector3((extendRadius) * 2, (extendRadius) * 2, (extendRadius) * 2);
        }
#elif SteamVR_2
        if (m_touchpadAxis.GetAxis(trackedObj.inputSource).y != 0) {
            extendRadius += m_touchpadAxis.GetAxis(trackedObj.inputSource).y / cursorSpeed;
            sphereObject.transform.localScale = new Vector3((extendRadius) * 2, (extendRadius) * 2, (extendRadius) * 2);
        }
#endif
    }

    private void initializeControllers() {
        if (controllerPicked == ControllerPicked.Right_Controller) {
#if SteamVR_Legacy
            trackedObj = controllerRight.GetComponent<SteamVR_TrackedObject>();
#elif SteamVR_2
            trackedObj = controllerRight.GetComponent<SteamVR_Behaviour_Pose>();
#endif
        } else if (controllerPicked == ControllerPicked.Left_Controller) {
#if SteamVR_Legacy
            trackedObj = controllerLeft.GetComponent<SteamVR_TrackedObject>();
#elif SteamVR_2
            trackedObj = controllerLeft.GetComponent<SteamVR_Behaviour_Pose>();
#endif
        } else {
            print("Couldn't detect trackedObject, please specify the controller type in the settings.");
            Application.Quit();
        }

    }

    public enum ControllerState {
        TRIGGER_UP, TRIGGER_DOWN, NONE
    }

    public ControllerState controllerEvents() {
#if SteamVR_Legacy
        if (controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)) {
            return ControllerState.TRIGGER_DOWN;
        }
        if (controller.GetPressUp(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)) {
            return ControllerState.TRIGGER_UP;
        }
#elif SteamVR_2
        if (m_controllerPress.GetStateDown(trackedObj.inputSource)) {
            return ControllerState.TRIGGER_DOWN;
        }
        if (m_controllerPress.GetStateUp(trackedObj.inputSource)) {
            return ControllerState.TRIGGER_UP;
        }
#endif
        return ControllerState.NONE;
    }

    void Awake() {
        mirroredCube = this.transform.Find("Mirrored Cube").gameObject;
        sphereObject = this.transform.Find("SphereTooltip").gameObject;
        menu = sphereObject.GetComponent<ExpandMenu>();
        menu.cameraHead = this.cameraHead;
        menu.sphereCasting = this;
        initializeControllers();
        if (interactionType == InteractionType.Manipulation_UI) {
            this.gameObject.AddComponent<SelectionManipulation>();
            this.GetComponent<SelectionManipulation>().trackedObj = trackedObj;
        }
    }

    void Start() {
        laser = Instantiate(laserPrefab);
        laserTransform = laser.transform;
    }

    void mirroredObject() {
        Vector3 controllerPos = trackedObj.transform.forward;
        float distance_formula_on_vector = Mathf.Sqrt(controllerPos.x * controllerPos.x + controllerPos.y * controllerPos.y + controllerPos.z * controllerPos.z);
        Vector3 mirroredPos = trackedObj.transform.position;

        mirroredPos.x = mirroredPos.x + (100f / (distance_formula_on_vector)) * controllerPos.x;
        mirroredPos.y = mirroredPos.y + (100f / (distance_formula_on_vector)) * controllerPos.y;
        mirroredPos.z = mirroredPos.z + (100f / (distance_formula_on_vector)) * controllerPos.z;

        mirroredCube.transform.position = mirroredPos;
        mirroredCube.transform.rotation = trackedObj.transform.rotation;
    }


    void Update() {
#if SteamVR_Legacy
        controller = SteamVR_Controller.Input((int)trackedObj.index);
#endif
        if (inMenu == false) {
            mirroredObject();
            PadScrolling();
        }
        ShowLaser();
        RaycastHit hit;
        if (Physics.Raycast(trackedObj.transform.position, trackedObj.transform.forward, out hit, 100)) {
            //print("hit:" + hit.transform.name);
            hitPoint = hit.point;
            ShowLaser(hit);
            if (menu.isActive() == false && menu.getSelectableObjects().Count > 0) {
                menu.enableEXPAND(menu.getSelectableObjects());
                menu.clearList();
            } else if (menu.isActive() == true) {
                menu.selectObject(hit.transform.gameObject);
            }
        }
    }

}
