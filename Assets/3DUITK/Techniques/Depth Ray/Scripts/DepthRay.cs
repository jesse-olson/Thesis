/* Depth Ray implementation by Kieran May
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
using Valve.VR;

public class DepthRay : ScrollingTechnique {
    private static readonly float RAY_DIST = 100.0f;

    private Transform cubeAssister;
    private RaycastHit[] oldHits;

    public Material outlineMaterial;
    public Material defaultMat;

    public GameObject laserPrefab;
    private GameObject laser;

    private bool castEnable;

    void Awake()
    {
        cubeAssister = transform.GetChild(0);

        if (interactionType == InteractionType.Manipulation_UI)
        {
            gameObject.AddComponent<SelectionManipulation>();
            GetComponent<SelectionManipulation>().trackedObj = trackedObj.gameObject;
        }
    }

    void Start()
    {
        FindEventSystem();

        laser = Instantiate(laserPrefab);
        laser.SetActive(true);
        laser.transform.SetParent(trackedObj.transform);
        laser.transform.localScale = new Vector3(1, 1, RAY_DIST);

        cubeAssister.SetParent(trackedObj.transform);
        cubeAssister.localPosition = new Vector3(0, 0, extendDistance);
        Disable();
    }

    void Update()
    {
        if (!castEnable) return;
        PadScrolling();
        cubeAssister.localPosition = new Vector3(0, 0, extendDistance);

        //if (selected) return;
        Ray ray = new Ray(trackedObj.position, trackedObj.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, RAY_DIST, interactionLayers);
        HighlightClosestObject(hits);
        oldHits = hits;
    }
    
    private void HighlightClosestObject(RaycastHit[] hits) {
        GameObject closestObject = null;
        float closestDist = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            float dist = Vector3.Distance(cubeAssister.position, hit.point);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestObject = hit.transform.gameObject;
            }
        }

        HighlightObject(closestObject);
    }

    private void ResetAllMaterials() {
        if (oldHits != null) {
            foreach (RaycastHit hit in oldHits) {
                hit.transform.gameObject.GetComponent<Renderer>().material = defaultMat;
            }
        }
    }

    public override void SelectObject()
    {
        if (selected || !highlighted) return;

        selected = true;
        selectedObject = highlightedObject;

        switch (interactionType)
        {
            case InteractionType.Selection:
                print("Selected object in pure selection mode:" + selectedObject.name);
                break;

            case InteractionType.Manipulation_Movement:
            case InteractionType.Manipulation_Full:
                selectedObject.transform.SetParent(trackedObj.transform);
                break;

            case InteractionType.Manipulation_UI:
                if (!GetComponent<SelectionManipulation>().inManipulationMode)
                {
                    GetComponent<SelectionManipulation>().selectedObject = selectedObject;
                }
                break;
        }
        onSelectObject.Invoke();
    }

    public override void ReleaseObject()
    {
        if (!selected) return;
        switch (interactionType)
        {
            case InteractionType.Manipulation_Movement:
            case InteractionType.Manipulation_Full:
                selectedObject.transform.SetParent(null);
                break;

            case InteractionType.Manipulation_UI:
                if (!GetComponent<SelectionManipulation>().inManipulationMode)
                {
                    GetComponent<SelectionManipulation>().selectedObject = selectedObject;
                }
                break;

            default:
                //Do nothing
                break;
        }
        onDropObject.Invoke();
        selected = false;
        selectedObject = null;
    }

    protected override void Enable()
    {
        castEnable = true;
        laser.SetActive(true);
        cubeAssister.gameObject.SetActive(true);
    }

    protected override void Disable()
    {
        castEnable = false;
        laser.SetActive(false);
        cubeAssister.gameObject.SetActive(false);
        ReleaseObject();
        HighlightObject(null);
    }
}
