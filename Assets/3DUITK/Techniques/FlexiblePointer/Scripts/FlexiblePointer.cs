/*
 *  Flexible pointer - VR interaction tool allowing the user to bend a ray cast
 *  along a bezier curve utilizng the HTC Vive controllers.
 *
 *  Copyright(C) 2018  Ian Hanan
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

public class FlexiblePointer : TwoHandTechnique {
    private enum CloserTracked
    {
        Primary,
        Secondary
    }

    public GameObject controlPoint;
    public bool controlPointVisible = true;

    public float scaleFactor = 2f;

    private Vector3 p0; // The point from which the curve starts
    private Vector3 p1; // Bezier curve control point
    private Vector3 p2; // The end of the bezier curve

    private Vector3 _2p1;
    private Vector3 p2_2p1;

    // Laser vars
    private static readonly int   numOfLasers = 20;   // The number of segments that will make up the bezier curve
    private static readonly float bezierIncrement = 1.0f / numOfLasers;

    public GameObject laserPrefab;
    private GameObject laserParent;

    private bool castEnabled;

    // Use this for initialization
    void Awake() {
        FindEventSystem();
        InitializeLasers();

        if (interactionType == InteractionType.Manipulation_UI) {
            gameObject.AddComponent<SelectionManipulation>();
            GetComponent<SelectionManipulation>().trackedObj = trackedObj.gameObject;
        }
    }

    private void Start()
    {
        SetControlPointVisibility();
        SetControlPoints();
    }

    // Update is called once per frame
    void Update()
    {
        if (castEnabled)
        {
            SetControlPoints();
            CastBezierRay();
        }
    }

    private void InitializeLasers()
    {
        laserParent = new GameObject
        {
            name = "LaserParent"
        };

        // Make the laser parts should all align well :)
        for (int i = 0; i < numOfLasers; i++)
        {
            GameObject laserPart = Instantiate(laserPrefab) as GameObject;
            laserPart.transform.SetParent(laserParent.transform);
        }
    }

    private CloserTracked CloserController() {
        //QuestDebug.Instance.Log("Closer Controller");
        Vector3 playerPos = head.position + 0.3f * Vector3.down;
        float distToPrimary  = Vector3.Distance(playerPos, trackedObj.position);
        float distToSecondary = Vector3.Distance(playerPos, secondaryTrackedObj.position);

        return distToPrimary < distToSecondary ?
            CloserTracked.Primary : CloserTracked.Secondary;
    }

    void SetControlPoints() {
        //QuestDebug.Instance.Log("Set Control Points");
        // Setting test points
        Vector3 controller1Pos;     // The closer controller
        Vector3 controller1Fwd;
        Vector3 controller2Pos;     // The further controller

        switch (CloserController())
        {
            case CloserTracked.Primary:
                controller1Pos = trackedObj.position;
                controller1Fwd = trackedObj.forward;
                controller2Pos = secondaryTrackedObj.position;
                break;

            case CloserTracked.Secondary:
            default:
                controller1Pos = secondaryTrackedObj.position;
                controller1Fwd = secondaryTrackedObj.forward;
                controller2Pos = trackedObj.position;
                break;
        }

        p0 = controller1Pos;

        Vector3 localController2Pos = (controller2Pos - controller1Pos);
        p2 = scaleFactor * localController2Pos;

        // A . B = ||A|| * ||B|| * cos(@);
        // A_norm . B_norm = cos(@);
        // cos = ||A||/||B||
        //...||B|| = ||A|| / cos(@);
        Vector3 A = p2 / 2; // Midpoint between p0 and p2;
        Vector3 B_norm = controller1Fwd.normalized;
        float cos   = Vector3.Dot(A.normalized, B_norm);
        float B_mag = A.magnitude / cos;

        p1 = B_mag * B_norm;
        controlPoint.transform.position = p0 + p1;
        _2p1 = 2 * p1;
        p2_2p1 = p2 - _2p1;
    }

    private void CastBezierRay() {
        Vector3 lastPos = p0;
        float currentBezierValue = bezierIncrement;

        bool didHit = false;
        GameObject hitObject = null;
        foreach (Transform laser in laserParent.transform)
        {
            laser.gameObject.SetActive(!didHit);
            if (!didHit)
            {
                Vector3 nextPos = GetBezierPosition(currentBezierValue);

                Vector3 dir = nextPos - lastPos;
                float dist  = dir.magnitude;

                laser.position = lastPos;
                laser.LookAt(nextPos);
                laser.localScale = new Vector3(1, 1, dist);

                if (Physics.Raycast(lastPos, dir, out RaycastHit hit, dist, interactionLayers))
                {
                    hitObject = hit.transform.gameObject;
                    didHit = true;
                }

                lastPos = nextPos;
                currentBezierValue += bezierIncrement;
            }
        }
        HighlightObject(hitObject);
    }


    // t being betweek 0 and 1 to get a spot on the curve
    Vector3 GetBezierPosition(float t) {
      // P0 is always going to be (0, 0, 0) so it was removed
        //return p0 + t * (2 * (1 - t) * p1 + t * p2);
        return t * (_2p1 + t * p2_2p1);
    }


    // sets whether the user can see the control point. Will be called if user changes the bool variable setting
    private void SetControlPointVisibility() {
        controlPoint.GetComponent<MeshRenderer>().enabled = controlPointVisible;
    }

    public override void SelectObject()
    {
        if (selected || !highlighted) return;

        selected = true;
        selectedObject = highlightedObject;

        switch(interactionType){
            case InteractionType.Selection:
                break;

            case InteractionType.Manipulation_Movement:
            case InteractionType.Manipulation_Full:
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
        switch (interactionType)
        {
            case InteractionType.Selection:
                break;

            case InteractionType.Manipulation_Movement:
            case InteractionType.Manipulation_Full:
                break;

            case InteractionType.Manipulation_UI:
                break;

            default:
                break;
        }
        onDropObject.Invoke();
        selected = false;
        selectedObject = null;
    }


    protected override void Enable()
    {
        castEnabled = true;
        laserParent.SetActive(true);
    }

    protected override void Disable()
    {
        castEnabled = false;
        laserParent.SetActive(false);
        if (selected) ReleaseObject();
        if (highlighted) HighlightObject(null);
    }
}
