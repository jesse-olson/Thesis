/* Scaled-world grab implementation by Kieran May
 * University of South Australia
 * 
 * The Scaled-world grab algorithm used was based off: (pg 37) 
 * https://people.cs.vt.edu/~bowman/3dui.org/course_notes/siggraph2001/basic_techniques.pdf 
 * The initial selection technique used in this implementation is ray-casting
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

public class ScaledWorldGrab : Technique {
    private static readonly float RAY_DIST = 100.0f;

    public GameObject controllerCollider;

    public GameObject laserPrefab;
    private GameObject laser;

    public GameObject cameraRig;

    private Transform oldParent;

    float scaleAmount;

    public static int grabbedAmount;

    private GameObject entered;

    private Vector3 headScale;
    private Vector3 rigScale;
    private Vector3 rigPos;

    void Awake()
    {
        controllerCollider.transform.parent = trackedObj;
    }

    // Use this for initialization
    void Start()
    {
        laser = Instantiate(laserPrefab, transform);
    }

    // Update is called once per frame
    void Update()
    {
        if (!selected) CastRay();
        else WorldGrab();
    }

    internal void ResetProperties()
    {
        head.localScale = headScale;
        cameraRig.transform.localScale = rigScale;
        cameraRig.transform.localPosition = rigPos;
    }

    private void WorldGrab()
    {
        //Vector3 currPos = trackedObj.position - head.position;
        //float currDist  = currPos.magnitude;      // Physical hand distance
        //float virtDist  = scaleAmount * currDist; // Virtual hand distance
        //Vector3 virtPos = head.position + virtDist * currPos;

        //trackedObj.transform.position = virtPos;
        //trackedObj.transform.localRotation = trackedObj.localRotation;
    }

    private void CastRay()
    {
        Ray ray = new Ray(trackedObj.position, trackedObj.forward);
        bool didHit = Physics.Raycast(ray, out RaycastHit hit, RAY_DIST, interactionLayers);

        GameObject hitObject = didHit ? hit.transform.gameObject : null;
        HighlightObject(hitObject);
    }


    protected override void Enable()
    {
        //throw new System.NotImplementedException();
    }

    protected override void Disable()
    {
        //throw new System.NotImplementedException();
    }

    public override void SelectObject()
    {
        if (selected || !highlighted) return;

        selected = true;
        selectedObject = highlightedObject;

        oldParent = selectedObject.transform.parent;

        laser.SetActive(false);

        headScale = head.localScale;
        rigScale  = cameraRig.transform.localScale;
        rigPos    = cameraRig.transform.localPosition;

        float Disth = Vector3.Distance(head.position, trackedObj.position);
        float Disto = Vector3.Distance(head.position, selectedObject.transform.position);

        scaleAmount = Disto / Disth;
        headScale = head.localScale;
        rigScale  = cameraRig.transform.localScale;
        ScaleAround(cameraRig.transform, head, scaleAmount * Vector3.one);

        //Keep eye distance proportionate to original position
        head.localScale /= scaleAmount;

        switch (interactionType)
        {
            //case InteractionType.Selection:
            case InteractionType.Manipulation_Movement:
            case InteractionType.Manipulation_Full:
                selectedObject.transform.SetParent(trackedObj);
                break;
            //case InteractionType.Manipulation_UI:

            default:
                break;
        }
        onSelectObject.Invoke();
    }

    //Tham's scale method
    public void ScaleAround(Transform target, Transform pivot, Vector3 scale) {
        Transform pivotParent = pivot.parent;
        Vector3   pivotPos    = pivot.position;
        pivot.parent          = target;
        target.localScale     = scale;
        target.position      += pivotPos - pivot.position;
        pivot.parent          = pivotParent;
    }


    public override void ReleaseObject()
    {
        if (!selected) return;
        switch (interactionType)
        {
            //case InteractionType.Selection:
            case InteractionType.Manipulation_Movement:
            case InteractionType.Manipulation_Full:
                selectedObject.transform.SetParent(oldParent);
                break;
            //case InteractionType.Manipulation_UI:

            default:
                break;
        }

        head.localScale = headScale;
        cameraRig.transform.localScale = rigScale;
        cameraRig.transform.localPosition = rigPos;

        onDropObject.Invoke();
        selected = false;
        selectedObject = null;
    }
}
