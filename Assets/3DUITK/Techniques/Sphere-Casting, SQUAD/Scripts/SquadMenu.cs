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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SquadMenu : Technique {
    private float scaleAmount = 10f;

    private readonly Transform[] quadrants = new Transform[4];

    public List<GameObject> selectableObjects = new List<GameObject>();

    public GameObject panel;
    public GameObject prefabText;

    public Material quadrantMaterial;
    public Material outlineMaterial;
    public Material triangleMaterial;

    public GameObject cameraHead;

	public bool enableText = false;

    internal SphereCasting sphereCasting;

    private GameObject highlightedQuad;
    private GameObject selectedQuad;

    private static readonly Vector2[] positions = new Vector2[] {
                                                             new Vector2(0.4f, -0.4f),
                                  new Vector2(0.25f, -0.2f), new Vector2(0.4f, -0.2f),
        new Vector2(0.1f,  0.0f), new Vector2(0.25f,  0.0f), new Vector2(0.4f,  0.0f),
                                  new Vector2(0.25f,  0.2f), new Vector2(0.4f,  0.2f),
                                                             new Vector2(0.4f,  0.4f)
    };


    private void Start()
    {
        SetQuadrants();
        DisableSQUAD();
    }


    private void SetQuadrants()
    {
        foreach (Transform child in panel.transform)
        {
            if (child.name == "North")
            {
                quadrants[0] = child;
            }
            else if (child.name == "South")
            {
                quadrants[1] = child;
            }
            else if (child.name == "East")
            {
                quadrants[2] = child;
            }
            else if (child.name == "West")
            {
                quadrants[3] = child;
            }
        }
    }


    private void DestroyChildGameObjects() {
        for(int i = 0; i < 4; i++) {
            foreach(Transform child in quadrants[i].transform) {
                Destroy(child.gameObject);
            }
        }
        selectableObjects.Clear();
    }


    public void EnableSQUAD(List<GameObject> objects)
    {
        SphereCasting.inMenu = true;

        panel.SetActive(true);

        Generate2DObjects(objects, true);
    }

    public void DisableSQUAD()
    {
        SphereCasting.inMenu = false;

        panel.SetActive(false);

        DestroyChildGameObjects();
    }

    public void HoverQuad()
    {
        Ray ray = new Ray(trackedObj.position, trackedObj.forward);
        bool didHit = Physics.Raycast(ray, out RaycastHit hit, 100, interactionLayers);

        GameObject hitQuad = didHit ? hit.transform.gameObject : null;
        HighlightObject(hitQuad);
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
    }

    public override void ReleaseObject()
    {
        throw new System.NotImplementedException();
    }


    void Generate2DObjects(List<GameObject> pickedObjects, bool initialLoop)
    {
        for (int i = 0; i < pickedObjects.Count; i++) {
            GameObject pickedObj = pickedObjects[i];
            string name = pickedObj.name;

            int stage = i % 4;
            int pos   = i / 4;

            //Setting up the 2D Object
            GameObject pickedObj2D =
                Instantiate(pickedObj, quadrants[stage]) as GameObject;
            pickedObj2D.name = name;

            if (initialLoop)
            {
                pickedObj2D.transform.localScale = pickedObj.transform.lossyScale / scaleAmount;
                // If the Object has a Collider Destroy the Collider!
			    if (pickedObj2D.GetComponent<Collider>()  != null)
                    Destroy (pickedObj2D.GetComponent<Collider>());
                if (pickedObj2D.GetComponent<Rigidbody>() == null)
                {
                    pickedObj2D.gameObject.AddComponent<Rigidbody>();
                    pickedObj2D.GetComponent<Rigidbody>().isKinematic = true;
                }
            }
        
            Vector2 position2D = positions[pos];
            Vector3 position3D = new Vector3(position2D.x, position2D.y);

            pickedObj2D.transform.localPosition = position3D;

            if (enableText)
            {
                GameObject pickedObjText =
                    Instantiate(prefabText, new Vector3(0f, -0.6f, 0f), Quaternion.identity, pickedObj2D.transform) as GameObject;
                TextMesh mesh = pickedObjText.GetComponent<TextMesh>();
                mesh.text = name;
                mesh.fontSize = 250;
            }
        }
    }

    public void RefineQuad(GameObject obj)
    {
        int val = 0;

        if (obj.name == "North")
        {
            val = 0;
        }
        else if (obj.name == "South")
        {
            val = 1;
        }
        else if (obj.name == "East")
        {
            val = 2;
        }
        else if (obj.name == "West")
        {
            val = 3;
        }

        List<GameObject> quadObjs = new List<GameObject>();

        foreach (Transform child in quadrants[val])
        {
            quadObjs.Add(child.gameObject);
        }

        DestroyChildGameObjects();

        print("----- REFINING QUAD -----");
        if (quadObjs.Count > 1)
        {
            Generate2DObjects(quadObjs, false);
        }
        else
        {
            selected = true;
            selectedObject = quadObjs[0];
        }
    }
}
