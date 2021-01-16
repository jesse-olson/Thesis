/* Raycasting implementation by Kieran May
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


// TODO: Need to redo the parenting system. I think that the RigidBody method
// used by other techniques is the way to go.
using UnityEngine;

public class Raycasting : Technique {
    private static readonly float RAY_DIST = 100.0f;

    private bool castEnabled;
    private GameObject manipulationIcons;

    public GameObject laserPrefab;
    private GameObject laser;

    private Transform oldParent;

    void Awake()
    {
        FindEventSystem();
        transform.SetParent(trackedObj.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (interactionType == InteractionType.Manipulation_UI)
        {
            manipulationIcons    = GameObject.Find("Manipulation_Icons");
            selectionManipulator = gameObject.AddComponent<SelectionManipulation>();
            selectionManipulator.trackedObj = trackedObj.gameObject;
            selectionManipulator.manipulationIcons = manipulationIcons;
        }

        laser = Instantiate(laserPrefab) as GameObject;
        laser.transform.SetParent(trackedObj.transform);
        laser.SetActive(true);
    }

    void Update()
    {
        CastRay();
    }

    protected override void Enable()
    {
        castEnabled = true;
        laser.SetActive(true);
    }

    protected override void Disable()
    {
        castEnabled = false;
        if (selected) ReleaseObject();
        if (highlighted) HighlightObject(null);
        laser.SetActive(false);
    }

    private void CastRay()
    {
        if (!castEnabled) return;
        Ray ray = new Ray(trackedObj.transform.position, trackedObj.transform.forward);
        bool didHit = Physics.Raycast(ray, out RaycastHit hit, RAY_DIST, interactionLayers);

        GameObject hitObject = didHit ? hit.transform.gameObject : null;
        float laserDist = didHit ? hit.distance : RAY_DIST;

        laser.transform.localScale = new Vector3(1, 1, laserDist);

        HighlightObject(hitObject);
    }
    

    public override void SelectObject() {
        if (selected || !highlighted) return;

        selected = true;
        selectedObject = highlightedObject;

        switch (interactionType)
        {
            case InteractionType.Manipulation_Movement:
            case InteractionType.Manipulation_Full:
                oldParent = selectedObject.transform.parent;
                selectedObject.transform.SetParent(transform);
                break;

            case InteractionType.Manipulation_UI:
                if (!selectionManipulator.inManipulationMode)
                    selectionManipulator.selectedObject = selectedObject;
                break;

            default:
                //Do Nothing
                break;
        }
        onSelectObject.Invoke();
    }

    public override void ReleaseObject()
    {
        if (!selected) return;

        switch(interactionType)
        {
            case InteractionType.Manipulation_Movement:
            case InteractionType.Manipulation_Full:
                selectedObject.transform.SetParent(oldParent);
                break;

            default:
                break;
        }

        onDropObject.Invoke();
        selected = false;
        selectedObject = null;
    }
}
