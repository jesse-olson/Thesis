using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using Valve.VR;

public class BubbleSelection : MonoBehaviour {
#if SteamVR_Legacy
    private SteamVR_Controller.Device controller;
    internal SteamVR_TrackedObject trackedObj;
#elif SteamVR_2
    internal SteamVR_Behaviour_Pose trackedObj;
#else
    public GameObject trackedObj;
#endif
    private GameObject[] panelObjects;
    private GameObject[] selectedObjects;

    private BubbleCursor3D bubbleCursor;

    private GameObject panel;
    private GameObject cursor2D;
    private GameObject objectBubble2D;
    internal GameObject cameraHead;

    internal bool inBubbleSelection = false;
    public LayerMask interactionLayers;

    public GameObject currentlyHovering = null;

    private static readonly string suffix = " (panel)";

    //private static readonly float[,] positions = 
    //    new float[,] {  { -0.3f,  0.2f }, { -0.1f,  0.2f }, { 0.1f,  0.2f }, { 0.3f,  0.2f },
    //                    { -0.3f,  0.1f }, { -0.1f,  0.1f }, { 0.1f,  0.1f }, { 0.3f,  0.1f },
    //                    { -0.3f,  0.0f }, { -0.1f,  0.0f }, { 0.1f,  0.0f }, { 0.3f,  0.0f },
    //                    { -0.3f, -0.1f }, { -0.1f, -0.1f }, { 0.1f, -0.1f }, { 0.3f, -0.1f },
    //                    { -0.3f, -0.2f }, { -0.1f, -0.2f }, { 0.1f, -0.2f }, { 0.3f, -0.2f },
    //                    { -0.3f, -0.3f }, { -0.1f, -0.3f }, { 0.1f, -0.3f }, { 0.3f, -0.3f }    };

    public float scaleAmount = 10f;

    private readonly float bubbleOffset = 0.025f;

    void Generate2DObjects(List<GameObject> objects) {
        selectedObjects = objects.ToArray();
        panelObjects = new GameObject[objects.Count];
        panel.transform.SetParent(null);

        print("Amount of objects selected:" + selectedObjects.Length);
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            GameObject pickedObj   = selectedObjects[i];
            print("Generating object:" + pickedObj.name + " | at pos:" + (i + 1));

            GameObject pickedObj2D = Instantiate(pickedObj) as GameObject;
            pickedObj2D.transform.SetParent(panel.transform, false);
            pickedObj2D.transform.localRotation = Quaternion.identity;

            pickedObj2D.gameObject.AddComponent<Rigidbody>();
            pickedObj2D.GetComponent<Collider>().attachedRigidbody.isKinematic = true;
			pickedObj2D.transform.localScale = pickedObj.transform.lossyScale / scaleAmount;
            pickedObj2D.name = pickedObj.name + suffix;

            float posX = -0.3f + 0.2f * (i % 4);
            float posY =  0.2f - 0.1f * (i / 4);
            pickedObj2D.transform.localPosition = new Vector3(posX, posY, 0f);
        }
    }

    void Awake() {
        panel = GameObject.Find("2DBubbleCursor_Panel");
        objectBubble2D = panel.transform.Find("ObjectBubble2D").gameObject;
        cursor2D = panel.transform.Find("Cursor2D").gameObject;
        panel.transform.SetParent(cameraHead.transform);
    }

    void Start() {
        bubbleCursor = GameObject.Find("3DBubbleCursor_Technique").GetComponent<BubbleCursor3D>();
        interactionLayers = bubbleCursor.interactionLayers;
        DisableMenuOnLoad();
    }

    void Update() {
        if (inBubbleSelection == true) {
            MoveCursor();

            GameObject[] closestObjects = ClosestObject();
            
            string panelObjectName = closestObjects[0].name;    // Name of the closest object
            string objectName = panelObjectName.Substring(0, panelObjectName.Length - suffix.Length);

            GameObject originalObject = GameObject.Find(objectName);

            Transform objPos  = closestObjects[0].transform;
            Transform objPos2 = closestObjects[1].transform;
            //print("TARGET:" + pickedObjects[(int)lowestDistances[0][1]].name);

            float closestCircleRadius  = Vector3.Distance(cursor2D.transform.localPosition, objPos.localPosition);
                    closestCircleRadius -= Mathf.Pow(objPos.lossyScale.x, 2) * 10;

            float secondClosestCircleRadius  = Vector3.Distance(cursor2D.transform.localPosition, objPos2.localPosition);
                    secondClosestCircleRadius -= Mathf.Pow(objPos2.lossyScale.x, 2) * 10;

            float closestValue = Mathf.Min(closestCircleRadius, secondClosestCircleRadius);

            float finalVal = closestValue * 10;
            cursor2D.GetComponent<SphereCollider>().radius = finalVal;
            transform.localScale = new Vector3(finalVal * 2, finalVal * 2, 1f);

            if (closestCircleRadius < secondClosestCircleRadius) {
                objectBubble2D.transform.localScale = new Vector3(0f, 0f, 0f);
                //Usually it works with the objectBubble disappearing when the object is fully encapsulated, but it's alot more user-friendly commenting out this part for the 2D selection
                //bubbleCursor.objectBubble.transform.localScale = new Vector3(0f, 0f, 0f);
            } else {
                objectBubble2D.transform.localPosition = closestObjects[0].transform.localPosition;
                objectBubble2D.transform.localScale    = closestObjects[0].transform.localScale + bubbleOffset * Vector3.one;

                bubbleCursor.objectBubble.transform.position   = originalObject.transform.localPosition;
                bubbleCursor.objectBubble.transform.localScale = originalObject.transform.localScale + bubbleCursor.bubbleOffset * Vector3.one;
            }
            currentlyHovering = closestObjects[0];
            DisableMenuOnTrigger(originalObject);
        }
    }


    private GameObject[] ClosestObject() {
        // More like a sort based on distance
		GameObject[] toReturn = panelObjects.OrderBy(obj => {
            float dist = Vector3.Distance(cursor2D.transform.localPosition, obj.transform.localPosition);
            dist -= Mathf.Pow(obj.transform.lossyScale.x, 2) * 10;
            return dist;
        }).ToArray();

		return toReturn;
	}


    public void EnableMenu() {
        panel.SetActive(true);
    }

    public void EnableMenu(List<GameObject> obj) {
        if (bubbleCursor.ControllerEvents() == ControllerState.TRIGGER_DOWN && inBubbleSelection == false) {
                print("size:" + obj.Count);
                panel.SetActive(true);
                bubbleCursor.cursor.SetActive(false);
                inBubbleSelection = true;
                Generate2DObjects(obj);
        }
    }

    public void DisableMenuOnLoad() {
        panel.SetActive(false);
        inBubbleSelection = false;
    }

    public void DisableMenuOnTrigger(GameObject selectedObject) {
        if (bubbleCursor.ControllerEvents() == ControllerState.TRIGGER_DOWN && inBubbleSelection == true) {
            print("Selected object:" + selectedObject);

            DestroyChildGameObjects();  // Destroying all of the selectable objects and clearing the selectableObjects List

            if (bubbleCursor != null) {
                if (bubbleCursor.cursor != null) {
                    bubbleCursor.cursor.SetActive(true);
                }
            }
            if (selectedObject != null) {
                selectedObject.GetComponent<Renderer>().material.color = Color.red;
                bubbleCursor.selectedObject = selectedObject.gameObject;

                if (bubbleCursor.interactionType == InteractionType.Manipulation_Movement) {
                    selectedObject.transform.SetParent(trackedObj.transform);
                } else if (bubbleCursor.interactionType == InteractionType.Manipulation_UI && bubbleCursor.GetComponent<SelectionManipulation>().inManipulationMode == false) {
                    bubbleCursor.GetComponent<SelectionManipulation>().selectedObject = selectedObject.gameObject;
                }
            }

            inBubbleSelection = false;
            panel.SetActive(false); // Hide the panel
        }
    }


    private void DestroyChildGameObjects() {
        foreach (GameObject child in selectedObjects) {
            GameObject.Destroy(child.gameObject);
        }
        selectedObjects = null;
    }


    void MoveCursor() {
        Vector3 controllerFwd = trackedObj.transform.forward;
        Vector3 controllerPos = trackedObj.transform.position;

        float distance_formula_on_vector = controllerFwd.magnitude;

        // Using formula to find a point which lies at distance on a 3D line from vector and direction
        controllerPos.x += (distance_formula_on_vector) * controllerFwd.x;
        controllerPos.y += (distance_formula_on_vector) * controllerFwd.y;
        //controllerPos.z += (distance_formula_on_vector) * controllerFwd.z;

        cursor2D.transform.position = controllerPos;
        cursor2D.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
    }
}