/* HOMER implementation by Kieran May
 * University of South Australia
 * 
 * The developed HOMER algorithm was based off: (pg 34-35) 
 * https://people.cs.vt.edu/~bowman/3dui.org/course_notes/siggraph2001/basic_techniques.pdf 
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

/* 
 * TODO: I feel like this technique will work better if we were to 
 * use the shoulder rather than the head as the point from which we project.
 */

using UnityEngine;

public class HOMER : Technique {
    private static readonly float RAY_DIST = 100.0f;

    public  GameObject handPrefab;  // Object to be used as the stand in for a virtual hand

    private float virtToRealRatio;  // The distance ratio between the virtual and real hands
    private GameObject virtualHand; // Object representing the virtual hand. Mainly used for visual feedback. May be removed later.
    private Transform  oldParent;   // The original transform parent of the selected object.

    public GameObject laserPrefab;
    private GameObject laser;

    // Use this for initialization
    void Start()
    {
        FindEventSystem();

        laser = Instantiate(laserPrefab) as GameObject;
        laser.transform.SetParent(trackedObj);

        virtualHand = Instantiate(handPrefab) as GameObject;
        virtualHand.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!selected)
        {
            CastRay();
        }
        MoveVirtualHand();
    }

    private void MoveVirtualHand()
    {
        Vector3 handPos = (trackedObj.position - head.transform.position); // Physical hand position relative to head
        Vector3 virtPos = virtToRealRatio * handPos;    // Virtual  hand position relative to head

        virtualHand.transform.position = head.transform.position + virtPos;
        virtualHand.transform.rotation = trackedObj.rotation;
    }

    private void CastRay() {
        Ray  ray    = new Ray(trackedObj.position, trackedObj.forward);
        bool didHit = Physics.Raycast(ray, out RaycastHit hit, RAY_DIST, interactionLayers);

        float laserScale = didHit ? hit.distance : RAY_DIST;
        laser.transform.localScale = new Vector3(1, 1, laserScale);

        GameObject hitObject = didHit ? hit.transform.gameObject : null;
        HighlightObject(hitObject);
    }

    public override void SelectObject()
    {
        if (selected || !highlighted) return;
        laser.SetActive(false);     // Turn off laser
        virtualHand.SetActive(true);

        selected = true;
        selectedObject = highlightedObject;

        virtualHand.transform.position = selectedObject.transform.position;

        float handDist = Vector3.Distance(head.transform.position, trackedObj.position);
        float virtDist = Vector3.Distance(head.transform.position, selectedObject.transform.position);

        virtToRealRatio = virtDist / handDist;

        oldParent = selectedObject.transform.parent;

        selectedObject.transform.SetParent(virtualHand.transform);
        selectedObject.transform.localPosition = Vector3.zero;
        selectedObject.transform.localRotation = Quaternion.identity;

        onSelectObject.Invoke();
    }

    public override void ReleaseObject()
    {
        if (!selected) return;
        laser.SetActive(true);
        virtualHand.SetActive(false);

        onDropObject.Invoke();
        selected = false;
        selectedObject.transform.SetParent(oldParent);
    }

    protected override void Enable()
    {
        //throw new System.NotImplementedException();
    }

    protected override void Disable()
    {
        //throw new System.NotImplementedException();
    }
}