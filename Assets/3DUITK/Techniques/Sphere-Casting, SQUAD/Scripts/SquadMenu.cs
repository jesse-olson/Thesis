/* SQUAD implementation by Kieran May
*  University of South Australia
* 
*  SQUAD is an extension of Sphere-Casting which places selected objects into a QUAD menu for more precise selection
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
using UnityEngine.SceneManagement;

public class SquadMenu : MonoBehaviour {
    public GameObject panel;
    public GameObject prefabText;
    public Material quadrantMaterial;
    public Material outlineMaterial;
    public Material triangleMaterial;
    public GameObject cameraHead;
    private bool quadrantPicked = false;
    private Transform[] TriangleQuadrant = new Transform[4];
	public bool enableText = false;
    internal SphereCasting sphereCasting;
    public List<GameObject> selectableObjects = new List<GameObject>();

    private void DestroyChildGameObjects() {
        for(int i = 0; i < 4; i++) {
            foreach(Transform child in TriangleQuadrant[i].transform) {
                GameObject.Destroy(child.gameObject);
            }
        }
        foreach (Transform child in panel.transform) {
            if (child.gameObject.name != "CreateTrianglesSprite" && !child.gameObject.name.Contains("TriangleQuad")) {
                GameObject.Destroy(child.gameObject);
            } else if (child.gameObject.name.Contains("TriangleQuad") && !child.gameObject.name.Contains("Placeholder")) {
                child.gameObject.transform.GetComponent<Renderer>().material = triangleMaterial;
            }
        }

    }

    public bool IsActive() {
        return panel.activeInHierarchy;
    }

    public bool QuadrantIsPicked() {
        return quadrantPicked;
    }

    public void HideAllGameObjects() {
        GameObject[] allObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject obj in allObjects) {
            if (obj.name != "CreateTriangles" && obj.name != "[CameraRig]" && obj.name != "[SteamVR]") {
                print("Hidden Object:" + obj.name);
                obj.SetActive(false);
            }
        }
    }

    public void ShowAllGameObjects() {
        GameObject[] allObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject obj in allObjects) {
            if (obj.name != "CreateTriangles" && obj.name != "[CameraRig]" && obj.name != "[SteamVR]") {
                print("Hidden Object:" + obj.name);
                obj.SetActive(true);
            }
        }
    }

    public void DisableSQUAD() {
        ClearList();
		initialLoop = false;
        //showAllGameObjects();
        panel.transform.SetParent(cameraHead.transform);
        panel.SetActive(false);
        SphereCasting.inMenu = false;
        quadrantPicked = false;
        pickedObject = null;
        DestroyChildGameObjects();
        print("Squad disabled..");
    }

    private bool pickedUpObject = false; //ensure only 1 object is picked up at a time
    private GameObject pickedObject;
    private GameObject lastPickedObject;
    private Material oldPickedObjectMaterial;

    public void SelectObject(GameObject obj) {
        //print("picked object:" + pickedObject);
        quadrantPicked = true;
        ControllerState cs = sphereCasting.ControllerEvents();
        if (cs == ControllerState.TRIGGER_DOWN &&
            QuadrantIsPicked() == true)
        {
        //if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) && quadrantIsPicked() == true && pickedObject == null && obj.transform.parent == panel.transform && !obj.name.Contains("TriangleQuad")) {
            //disableSQUAD();
            //pickedObject = obj;
            //string objName = obj.name.Substring(0, obj.name.Length-7);
            string objName = obj.name;
            //print("obj picked:" + objName);
            pickedObject = GameObject.Find(objName);
            lastPickedObject = pickedObject;
            print("Final picked object:" + objName);
			if (pickedObject.transform.GetComponent<Renderer> () != null) {
				oldPickedObjectMaterial = pickedObject.transform.GetComponent<Renderer> ().material;
				pickedObject.transform.GetComponent<Renderer> ().material = outlineMaterial;
			}
			DisableSQUAD();
        }
    }

    private void ClearObjects() {

    }

    public void RefineQuad(GameObject obj) {
        int val = 0;
        int count = 0;
        if (obj.name == "TriangleQuad North") {
            val = 0;
        } else if(obj.name == "TriangleQuad South") {
            val = 1;
        } else if(obj.name == "TriangleQuad East") {
            val = 2;
        } else if(obj.name == "TriangleQuad West") {
            val = 3;
        }
        List<GameObject> quadObjs = new List<GameObject>();
        foreach (Transform child in TriangleQuadrant[val]) {
            print(child.name+" in: " + TriangleQuadrant[val]);
            quadObjs.Add(child.gameObject);
            count++;
        }
        DestroyChildGameObjects();
        print("----- REFINING QUAD -----");
        print(count);
        if(count == 1) {
            SelectObject(quadObjs[0]);
        } else {
			initialLoop = true;
            Generate2DObjects(quadObjs);
        }
    }

    private void SetQuadrants() { //NSEW
        foreach (Transform child in panel.transform) {
            print("quad:"+child.transform.name);
            if (child.name == "TriangleQuad North Placeholder") {
                TriangleQuadrant[0] = child;
            } else if(child.name == "TriangleQuad South Placeholder") {
                TriangleQuadrant[1] = child;
            } else if(child.name == "TriangleQuad East Placeholder") {
                TriangleQuadrant[2] = child;
            } else if (child.name == "TriangleQuad West Placeholder") {
                TriangleQuadrant[3] = child;
            }
        }
    }


    public void EnableSQUAD(List<GameObject> obj) {
        if (sphereCasting.trackedObj != null) {
            ControllerState cs = sphereCasting.ControllerEvents();
            if (cs == ControllerState.TRIGGER_DOWN &&
                pickedUpObject == false)
            {
                print("EnableSquad() called");
                SphereCasting.inMenu = true;
                panel.SetActive(true);
                SetQuadrants();
                //hideAllGameObjects();
                Generate2DObjects(obj);
                if (lastPickedObject != null) {
                    lastPickedObject.transform.GetComponent<Renderer>().material = oldPickedObjectMaterial;
                }
            }
        }
    }

    private GameObject lastQuad;
    private Material oldMaterial;
    public void HoverQuad(GameObject obj) {
        //print("obj contians:"+obj.name.Contains("TriangleQuad"));
        if(obj.name.Contains("TriangleQuad") && IsActive() == true) {
            if(lastQuad == null) {
                oldMaterial = obj.transform.GetComponent<Renderer>().material;
                lastQuad = obj;
                obj.transform.GetComponent<Renderer>().material = quadrantMaterial;
            } else {
                if (lastQuad != obj) {
                    lastQuad.transform.GetComponent<Renderer>().material = oldMaterial;
                    obj.transform.GetComponent<Renderer>().material = quadrantMaterial;
                    lastQuad = obj;
                }
                ControllerState cs = sphereCasting.ControllerEvents();
                if (cs == ControllerState.TRIGGER_DOWN)
                {
                    print("Quad selected:" + obj.name);
                    RefineQuad(obj);
                }
            }
        }
    }


    public void SelectQuad(GameObject obj) {
        ControllerState cs = sphereCasting.ControllerEvents();
        if (cs == ControllerState.TRIGGER_DOWN)
        {
            if (obj.name.Contains("TriangleQuad") && IsActive() == true && quadrantPicked == false) {
                Renderer rend = obj.transform.GetComponent<Renderer>();
                //rend.material.color = Color.blue;
                rend.material = quadrantMaterial;
                quadrantPicked = true;
                //obj.transform.GetComponent<Renderer>().material.color = Color.clear;
            }
        }
    }

    //Stores 36 objects
    private GameObject pickedObjText = null;
    private GameObject pickedObj = null;

    private readonly Vector2[][] positions = new Vector2[][] {
        left, right, up, down
    };

    private static readonly Vector2[] left = new Vector2[] {
                    new Vector2(-0.35f,  0.2f),
        new Vector2(-0.40f,  0.1f), new Vector2(-0.30f,  0.1f),
        new Vector2(-0.40f,  0.0f), new Vector2(-0.30f,  0.0f), new Vector2(-0.20f,  0.0f),
        new Vector2(-0.40f, -0.1f), new Vector2(-0.30f, -0.1f),
                    new Vector2(-0.35f, -0.2f)
    };

    private static readonly Vector2[] right = new Vector2[] { 
                    new Vector2(0.35f,  0.2f),
        new Vector2(0.40f,  0.1f), new Vector2(0.30f,  0.1f),
        new Vector2(0.40f,  0.0f), new Vector2(0.30f,  0.0f), new Vector2(0.20f,  0.0f),
        new Vector2(0.40f, -0.1f), new Vector2(0.30f, -0.1f),
                    new Vector2(0.35f, -0.2f)
    };

    private static readonly Vector2[] up = new Vector2[] { 
        new Vector2( -0.1f, 0.4f), new Vector2(  0.0f, 0.4f), new Vector2(  0.1f, 0.4f ),
        new Vector2( -0.1f, 0.3f), new Vector2(  0.0f, 0.3f), new Vector2(  0.1f, 0.3f ),
        new Vector2( -0.1f, 0.2f), new Vector2(  0.0f, 0.2f), new Vector2(  0.1f, 0.2f )
    };

    private static readonly Vector2[] down = new Vector2[] { 
        new Vector2( -0.1f, -0.4f ), new Vector2( 0f, -0.4f ), new Vector2( 0.1f, -0.4f ),
        new Vector2( -0.1f, -0.3f ), new Vector2( 0f, -0.3f ), new Vector2( 0.1f, -0.3f ),
        new Vector2( -0.1f, -0.2f ), new Vector2( 0f, -0.2f ), new Vector2( 0.1f, -0.2f )
    };

    private float scaleAmount = 10f;
	private bool initialLoop = false;
    void Generate2DObjects(List<GameObject> pickedObjects) {

        int[] imageSlots = new int[4];      // Stores the number of images currently in quadrant

        panel.transform.SetParent(null);

        string name = pickedObj.name;

        for (int i = 0; i < pickedObjects.Count; i++) {
            pickedObj = pickedObjects[i];

            int stage = i % 4;

            //Setting up the 2D Object
            GameObject pickedObj2D = 
                Instantiate(pickedObj, Vector3.zero, Quaternion.identity) as GameObject;
            pickedObj2D.name = name;

            pickedObj2D.transform.SetParent(panel.transform, false);

            if (initialLoop == false)
            {
                pickedObj2D.transform.localScale = pickedObj.transform.lossyScale / scaleAmount;
            }

			if (pickedObj2D.GetComponent<Collider>()  != null) { Destroy (pickedObj2D.GetComponent<Collider> ());	}
			if (pickedObj2D.GetComponent<Rigidbody>() == null) {
                pickedObj2D.gameObject.AddComponent<Rigidbody> ();
                pickedObj2D.GetComponent<Rigidbody>().isKinematic = true;
            }
        
            imageSlots[stage]++;
            int pos = imageSlots[stage] - 1;
            Vector2 position = positions[stage][pos];
            pickedObj2D.transform.localPosition = 
                new Vector3(position.x, position.y, -0.00001f);

            int quadrant = 0;
            switch (stage) {
                case 0:
                    quadrant = 3;
                    break;
                case 1:
                    quadrant = 2;
                    break;
                case 2:
                    quadrant = 0;
                    break;
                case 3:
                    quadrant = 1;
                    break;
            }

            pickedObj2D.transform.SetParent(TriangleQuadrant[quadrant], true);

            prefabText.GetComponent<TextMesh>().text = pickedObj.gameObject.name;
            if (enableText == true)
            {
                pickedObjText = Instantiate(
                    prefabText, 
                    new Vector3(0f, -0.6f, 0f), 
                    Quaternion.identity ) as GameObject;

                pickedObjText.transform.SetParent(pickedObj2D.transform, false);
                pickedObjText.GetComponent<TextMesh>().fontSize = 250;
            }
        }
    }

    private void Start() {
        //disableSQUAD();
		//initializeTriangles();
    }

    public void ClearList() {
        selectableObjects.Clear();
    }

    public List<GameObject> GetSelectableObjects() {
        return selectableObjects;
    }

    public int SelectableObjectsCount() {
        return selectableObjects.Count;
    }

    private void OnTriggerStay(Collider collider) {
        int layerValue = this.transform.parent
            .GetComponent<SphereCasting>()
            .interactionLayers.value;


        if (collider.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast") && 
            !selectableObjects.Contains(collider.gameObject) && 
            collider.gameObject.layer == Mathf.Log(layerValue, 2))
        {
            selectableObjects.Add(collider.gameObject);
        }
    }

}
