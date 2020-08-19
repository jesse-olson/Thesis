using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;
public class WorldInMiniature : MonoBehaviour {

    /* World In Miniature implementation by Kieran May
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

#if SteamVR_Legacy
    internal SteamVR_TrackedObject trackedObj;
    internal SteamVR_TrackedObject trackedObjO; //tracked object other
    private SteamVR_Controller.Device controller;
    internal SteamVR_Controller.Device controllerO; //controller other
#elif SteamVR_2
    internal SteamVR_Behaviour_Pose trackedObj;
    internal SteamVR_Behaviour_Pose trackedObjO; //tracked object other
    public SteamVR_Action_Boolean m_controllerPress;
    public SteamVR_Action_Boolean m_menuButton;
    public SteamVR_Action_Vector2 m_touchpadAxis;
    public SteamVR_Action_Boolean m_touchpadTouch;
#else
    public GameObject trackedObj;
    public GameObject trackedObjO;
#endif

    public GameObject worldInMinParent;
    GameObject[] allSceneObjects;
    public static bool WiMrunning = false;
    public bool WiMactive = false;
    public List<string> ignorableObjectsString = new List<string> { "[CameraRig]", "Directional Light", "background" };
    public float scaleAmount = 20f;
    public LayerMask interactableLayer;
    public Material outlineMaterial;

    private WIM_IDHandler idHandler;

    public InteractionType interacionType;

    public ControllerPicked controllerPicked;

    public GameObject rightController;
    public GameObject leftController;
    public GameObject cameraHead;
    
    public GameObject selectedObject;

    public GameObject currentObjectCollided;


    internal bool objectPicked = false;
    internal Transform oldParent;

    private float tiltAroundY = 0f;
    public float tiltSpeed = 2f; //2x quicker than normal
    private bool startedMoving = false;

    private void InitializeControllers() {
        if (controllerPicked == ControllerPicked.Right_Controller) {
#if SteamVR_Legacy
            trackedObj = rightController.GetComponent<SteamVR_TrackedObject>();
            trackedObjO = leftController.GetComponent<SteamVR_TrackedObject>();
#elif SteamVR_2
            trackedObj = rightController.GetComponent<SteamVR_Behaviour_Pose>();
            trackedObjO = leftController.GetComponent<SteamVR_Behaviour_Pose>();
#else
            trackedObj = rightController;
            trackedObjO = leftController;
#endif
        } else if (controllerPicked == ControllerPicked.Left_Controller) {
#if SteamVR_Legacy
            trackedObj = leftController.GetComponent<SteamVR_TrackedObject>();
            trackedObjO = rightController.GetComponent<SteamVR_TrackedObject>();
#elif SteamVR_2
            trackedObj = leftController.GetComponent<SteamVR_Behaviour_Pose>();
            trackedObjO = rightController.GetComponent<SteamVR_Behaviour_Pose>();
#else
            trackedObj = leftController;
            trackedObjO = rightController;
#endif
        } else {
            print("Couldn't detect trackedObject, please specify the controller type in the settings.");
            Application.Quit();
        }
    }

    void Awake() {
        worldInMinParent = this.transform.Find("WorldInMinParent").gameObject;
        idHandler = GetComponent<WIM_IDHandler>();

        InitializeControllers();
    }

    // Use this for initialization
    void Start() {

        allSceneObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < allSceneObjects.Length; i++) {
            SetIDObject(allSceneObjects[i]);
        }

        worldInMinParent.transform.SetParent(trackedObj.transform);
        ResetAllProperties();

        createWiM();

        //adding colliders and collider scripts to controllers for WIM if they don't allready exist
        SphereCollider col = trackedObj.transform.gameObject.GetComponent<SphereCollider>();
        if (col == null) {

            col = trackedObj.transform.gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.05f;
            col.center = new Vector3(0f, -0.05f, 0f);
            trackedObj.transform.gameObject.AddComponent<ControllerColliderWIM>();
        }
        SphereCollider col0 = trackedObjO.transform.gameObject.GetComponent<SphereCollider>();
        if (col0 == null) {
            col0 = trackedObjO.transform.gameObject.AddComponent<SphereCollider>();
            col0.isTrigger = true;
            col0.radius = 0.05f;
            col0.center = new Vector3(0f, -0.05f, 0f);
            trackedObjO.transform.gameObject.AddComponent<ControllerColliderWIM>();
        }
    }


    private void SetIDObject(GameObject obj) {
        if (null == obj)
            return;

        foreach (Transform child in obj.transform) {
            if (null == child)
            {
                continue;
            }
            idHandler.addID(child.gameObject);
            SetIDObject(child.gameObject);
        }
    }
    
    public enum ControllerState {
        TRIGGER_UP, TRIGGER_DOWN, TRIGGER_PRESS, APPLICATION_MENU, NONE, TOUCHPAD_TOUCH
    }

    public ControllerState ControllerEvents() {
#if SteamVR_Legacy
        if (controller.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)) {
            return ControllerState.TRIGGER_DOWN;
        }
        if (controller.GetPressUp(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)) {
            return ControllerState.TRIGGER_UP;

        }
        if (controllerO.GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)) {
            return ControllerState.TRIGGER_PRESS;
        }
        if (controllerO.GetPressUp(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)) {
            return ControllerState.TRIGGER_UP;

        }
        if (controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu)) {
            return ControllerState.APPLICATION_MENU;
        }
        if (controller.GetTouch(SteamVR_Controller.ButtonMask.Touchpad)) {
            return ControllerState.TOUCHPAD_TOUCH;
        }
        if (controllerO.GetPressUp(SteamVR_Controller.ButtonMask.Trigger)) {
            return ControllerState.TRIGGER_UP;
        }
#elif SteamVR_2
        if (m_controllerPress.GetStateDown(trackedObj.inputSource)) {
            return ControllerState.TRIGGER_DOWN;
        } if (m_controllerPress.GetStateUp(trackedObj.inputSource)) {
            return ControllerState.TRIGGER_UP;
        } if (m_controllerPress.GetState(trackedObjO.inputSource)) {
            return ControllerState.TRIGGER_PRESS;
        } if (m_controllerPress.GetStateUp(trackedObjO.inputSource)) {
            return ControllerState.TRIGGER_UP;
        } if (m_menuButton.GetStateDown(trackedObj.inputSource)) {
            return ControllerState.APPLICATION_MENU;
        } if (m_controllerPress.GetStateDown(trackedObjO.inputSource)) {
            return ControllerState.TRIGGER_DOWN;
        } if (m_touchpadTouch.GetState(trackedObj.inputSource)) {
            return ControllerState.TOUCHPAD_TOUCH;
        }

#endif

        return ControllerState.NONE;
    }

    void createWiM() {
        //if (ControllerEvents() == ControllerState.APPLICATION_MENU) {
            if (!WiMactive) {
                WiMactive = true;
                WiMrunning = true;
                print("Create world clone");

                foreach(GameObject sceneObject in allSceneObjects ){
                    if (!ignorableObjectsString.Contains(sceneObject.name)) {
                        // Making the cloneObject
                        GameObject cloneObject = Instantiate(sceneObject, Vector3.zero, Quaternion.identity) as GameObject;
                        
                        cloneObject.transform.SetParent(worldInMinParent.transform, false);

                        Rigidbody rigidBody = cloneObject.gameObject.GetComponent<Rigidbody>();
                        // For all objects in the scene that are not affected by physics.
                        if (rigidBody == null) {
                            rigidBody = cloneObject.gameObject.AddComponent<Rigidbody>();
                        }

                        rigidBody.isKinematic = true;    // Make sure that all clones are kinematic

                        //cloneObject.gameObject.AddComponent<Collider>();
                        //cloneObject.GetComponent<Collider>().attachedRigidbody.isKinematic = true;

                        cloneObject.transform.localScale    = sceneObject.transform.lossyScale / scaleAmount;
                        cloneObject.transform.localRotation = sceneObject.transform.rotation;
                        cloneObject.transform.localPosition = sceneObject.transform.position / scaleAmount; 
                    }
                }

            worldInMinParent.transform.localEulerAngles = new Vector3(0f, trackedObj.transform.localEulerAngles.y - 45f, 0f);
                worldInMinParent.transform.Rotate(0, tiltAroundY, 0);

            } else if (WiMactive == true) {
                WiMactive = false;
                WiMrunning = false;
                foreach (Transform child in worldInMinParent.transform) {
                    Destroy(child.gameObject);
                }
                ResetAllProperties();
            }
        //}
    }

    private void ResetAllProperties() {
        worldInMinParent.transform.localScale = Vector3.one;
        worldInMinParent.transform.localPosition = Vector3.zero;
        worldInMinParent.transform.localEulerAngles = Vector3.zero;
    }


    public GameObject FindRealObject(GameObject selectedObject) {
        return idHandler.FindRealObject(selectedObject);
    }

    private GameObject FindCloneObject(GameObject obj) {
        return idHandler.FindCloneObject(obj);
    }
    
    // Update is called once per frame
    void Update() {
#if SteamVR_Legacy
        controller = SteamVR_Controller.Input((int)trackedObj.index);
        controllerO = SteamVR_Controller.Input((int)trackedObjO.index);
#endif

        if (WiMactive &&
            selectedObject != null &&
            selectedObject.GetComponent<ObjectID>() != null
            )
        {
            startedMoving = true;
            //selectedObject.transform.localPosition    = realObject.transform.localPosition;
            //selectedObject.transform.localEulerAngles = realObject.transform.localEulerAngles;
        }

        else if (
            //!isMoving() &&
            startedMoving
            )
        {
            startedMoving = false;
        }

        if (WiMactive) {
#if SteamVR_Legacy
            tiltAroundY = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y;
#elif SteamVR_2
            tiltAroundY = m_touchpadAxis.GetAxis(trackedObj.inputSource).y;
#else
            tiltAroundY = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
#endif
            if (ControllerEvents() == ControllerState.TOUCHPAD_TOUCH) {
                worldInMinParent.transform.Rotate(0, tiltAroundY * tiltSpeed, 0);
            }
        }

        if (ControllerEvents() == ControllerState.TRIGGER_UP && selectedObject == true) {
            selectedObject.transform.SetParent(oldParent);
            TransformRealObject(selectedObject);
            objectPicked = false;
        }
    }

    public void TransformRealObject(GameObject selectedObject)
    {
        GameObject realObject = FindRealObject(selectedObject);
        realObject.transform.localPosition = selectedObject.transform.localPosition;
        realObject.transform.localEulerAngles = selectedObject.transform.localEulerAngles;
    }
}