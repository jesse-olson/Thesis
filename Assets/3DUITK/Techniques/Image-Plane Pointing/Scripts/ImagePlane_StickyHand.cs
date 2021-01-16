/* ImagePlane_StickyHands implementation by Kieran May
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

public class ImagePlane_StickyHand : Technique
{
    private Transform oldParent;

    void Awake()
    {
        if (interactionType == InteractionType.Manipulation_UI)
        {
            gameObject.AddComponent<SelectionManipulation>();
            GetComponent<SelectionManipulation>().trackedObj = trackedObj.gameObject;
        }
    }

    void Start()
    {
        FindEventSystem();
    }

    void Update()
    {
        CastRay();
    }


    void CheckSurroundingObjects()
    {
        Vector3 remoteFwd  = (trackedObj.position - head.position).normalized;
        Vector3 remoteUp   = head.up;
        Vector3 remoteLeft = Vector3.Cross(remoteUp, remoteFwd);
        Vector3 remotePos  = head.position;

        var allObjects = FindObjectsOfType<GameObject>();

        float closestDist = float.MaxValue;
        GameObject closestObj = null;

        // Loop through objects and look for closest (if of a viable layer)
        foreach (GameObject gameObject in allObjects)
        {
            if (interactionLayers == 1 << gameObject.layer) // Dont have to worry about executing twice as an object can only be on one layer
            {
                // Check if object is on plane projecting in front of VR remote.
                // Otherwise ignore it. (we dont want our laser aiming backwards)
                Vector3 localTargetPos = gameObject.transform.position - remotePos;
                Vector3 perp = Vector3.Cross(remoteLeft, localTargetPos);
                float side = Vector3.Dot(perp, remoteUp);

                if (side >= 0)
                {
                    // Using vector algebra to get shortest distance between object and vector
                    float dist = Vector3.Cross(localTargetPos, remoteFwd).magnitude;

                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestObj = gameObject;
                    }
                }
            }
        }

        HighlightObject(closestObj);
    }

    void CastRay() {
        //CheckSurroundingObjects();

        Vector3 remoteFwd = (trackedObj.position - head.position).normalized;

        bool didHit = Physics.Raycast(head.position, remoteFwd, out RaycastHit hit, 100.0f, interactionLayers);

        GameObject hitObject = didHit ? hit.transform.gameObject : null;

        HighlightObject(hitObject);
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
        if (selected || !highlighted) return;

        selected = true;
        selectedObject = highlightedObject;

        switch (interactionType) {
            case InteractionType.Manipulation_Movement:
                oldParent = selectedObject.transform.parent;
                //float dist = Vector3.Distance(trackedObj.position, obj.transform.position);
                //selectedObject.transform.position =
                //    Vector3.Lerp(trackedObj.position, obj.transform.position, (obj.transform.localScale.x / dist) / dist);
                //selectedObject.transform.localScale = obj.transform.localScale / dist;
                selectedObject.transform.SetParent(trackedObj.transform);
                break;

            case InteractionType.Selection:
                break;


            case InteractionType.Manipulation_UI:
                if(!GetComponent<SelectionManipulation>().inManipulationMode)
                    GetComponent<SelectionManipulation>().selectedObject = selectedObject;
                break;

            default:
                break;
        }
        onSelectObject.Invoke();
    }

    public override void ReleaseObject()
    {
        if (!selected) return;
        if (interactionType == InteractionType.Manipulation_Movement)
        {
            selectedObject.transform.SetParent(oldParent);
        }
        onDropObject.Invoke();
        selected = false;
        selectedObject = null;
    }
}
