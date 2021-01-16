/*
 *  BendCast - Similar to a ray-cast except it will bend towards the closest object
 *  VR Interaction technique for the HTC Vive.
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

public class BendCast : Technique
{
    private static readonly Quaternion q_z_180 = new Quaternion(0, 0, 1, 0);
    // Bend in ray is built from multiple other rays
    private static readonly int   numOfLasers = 20; // how many rays to use for the bend (the more the smoother) MUST BE EVEN
    private static readonly float bezierIncrement = 1f / numOfLasers;

    private Vector3 p0;
    private Vector3 p1; // used for the bezier curve
    private Vector3 p2;

    private Vector3 p1_2;
    private Vector3 p2_p1;

    public GameObject laserPrefab;
    private GameObject laserParent;

    private bool castEnable;

    // Use this for initialization
    void Start()
    {
        FindEventSystem();

        if (interactionType == InteractionType.Manipulation_UI)
        {
            gameObject.AddComponent<SelectionManipulation>();
            GetComponent<SelectionManipulation>().trackedObj = trackedObj.gameObject;
        }
        InitializeLaser();
    }


    // Update is called once per frame
    void Update()
    {
        if (castEnable)
        {
            FindClosestObject();
            CastLaserCurve();
        }
    }

    private void InitializeLaser()
    {
        // Initalizing all the lasers
        // Making child of Tracked object simplifies bezier curve calculations
        laserParent = new GameObject
        {
            name = trackedObj.name + " Laser Rays"
        };
        laserParent.transform.SetParent(trackedObj.transform);
        laserParent.transform.localPosition = Vector3.zero;
        laserParent.transform.localRotation = Quaternion.identity;

        for (int i = 0; i < numOfLasers; i++)
        {
            GameObject laserPart = Instantiate(laserPrefab) as GameObject;
            laserPart.SetActive(true);  // Make sure that the laser part is active
            laserPart.transform.SetParent(laserParent.transform);
        }
    }


    // Using a bezier! Idea from doing flexible pointer
    Vector3 GetBezierPosition(float t)
    {
        return t * (p1_2 + t * p2_p1);
    }

    void CastLaserCurve()
    {
        if (highlighted)
        {
            Vector3 lastPos = Vector3.zero;
            float currentBezierValue = bezierIncrement;

            foreach (Transform laser in laserParent.transform)
            {
                Vector3 nextPos = GetBezierPosition(currentBezierValue);
                Vector3 global  = nextPos - lastPos;
                Vector3 globalDir = global.normalized;
                float globalDist  = global.magnitude;

                Vector3 cross = Vector3.Cross(Vector3.forward, globalDir);
                float   dot   = Vector3.Dot  (Vector3.forward, globalDir);

                laser.position = lastPos + trackedObj.position;
                if(controllerPicked == ControllerPicked.Left_Controller)
                    laser.rotation = new Quaternion(cross.x, cross.y, cross.z, dot);
                else
                    laser.localRotation = new Quaternion(cross.x, cross.y, cross.z, dot);
                laser.localScale = new Vector3(1, 1, globalDist);

                lastPos = nextPos;

                currentBezierValue += bezierIncrement;
            }
        }
    }




    void FindClosestObject()
    {
        Vector3 remoteFwd   = trackedObj.forward;
        Vector3 remoteUp    = trackedObj.up;
        Vector3 remoteLeft  = -1 * trackedObj.right;
        Vector3 remotePos   = trackedObj.position;

        // This way is quite innefficient but is the way described for the bendcast.
        // Might make an example of a way that doesnt loop through everything
        var allObjects = FindObjectsOfType<GameObject>();

        float closestDist = float.MaxValue;
        GameObject closestObject = null;

        // Loop through objects and look for closest (if of a viable layer)
        foreach(GameObject gameObject in allObjects)
        {
            if (interactionLayers == 1 << gameObject.layer) // Dont have to worry about executing twice as an object can only be on one layer
            {
                // Check if object is on plane projecting in front of VR remote.
                // Otherwise ignore it. (we dont want our laser aiming backwards)
                Vector3 localTargetPos = gameObject.transform.position - remotePos;
                Vector3 perp = Vector3.Cross(remoteLeft, localTargetPos);
                float   side = Vector3.Dot(perp, remoteUp);

                if(side >= 0)
                {
                    // Using vector algebra to get shortest distance between object and vector
                    float dist = Vector3.Cross(localTargetPos, remoteFwd).magnitude;

                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestObject = gameObject;
                        p1 = dist * remoteFwd;
                        p2 = localTargetPos;
                        p1_2 = 2 * p1;
                        p2_p1 = p2 - p1_2;
                    }
                }
            }
        }

        HighlightObject(closestObject);
        // If there is a closest object show the lasers
        laserParent.SetActive(highlighted);
    }

    public override void SelectObject()
    {
        if (selected || !highlighted) return;
        selected = true;
        selectedObject = highlightedObject;
        switch (interactionType)
        {
            case InteractionType.Selection:
                // Pure Selection            
                print("selected" + selectedObject);
                break;

            case InteractionType.Manipulation_Movement:
            case InteractionType.Manipulation_Full:
                GrabObject();
                break;

            case InteractionType.Manipulation_UI:
                if (!GetComponent<SelectionManipulation>().inManipulationMode)
                {
                    GetComponent<SelectionManipulation>().selectedObject = selectedObject;
                }
                break;

            default:
                //Do Nothing
                break;
        }
        onSelectObject.Invoke();
    }

    void GrabObject()
    {
        selectedObject.GetComponent<Rigidbody>().velocity = Vector3.zero; // Setting velocity to 0 so can catch without breakforce effecting it
        var joint = AddFixedJoint();
        joint.connectedBody = selectedObject.GetComponent<Rigidbody>();
    }

    private FixedJoint AddFixedJoint()
    {
        FixedJoint fx = trackedObj.gameObject.AddComponent<FixedJoint>();
        fx.breakForce = Mathf.Infinity;
        fx.breakTorque = Mathf.Infinity;
        return fx;
    }

    public override void ReleaseObject()
    {
        if (!selected) return;
        FixedJoint joint = GetComponent<FixedJoint>();
        if (joint != null)
        {
            joint.connectedBody = null;
            Destroy(joint);
#if SteamVR_Legacy
            lastSelectedObject.GetComponent<Rigidbody>().velocity = Controller.velocity;
            lastSelectedObject.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
#elif SteamVR_2
            lastSelectedObject.GetComponent<Rigidbody>().velocity = trackedObj.GetVelocity();
            lastSelectedObject.GetComponent<Rigidbody>().angularVelocity = trackedObj.GetAngularVelocity();
#endif
        }
        onDropObject.Invoke();
        selected = false;
        selectedObject = null;
    }

    protected override void Enable()
    {
        castEnable = true;
    }

    protected override void Disable()
    {
        castEnable = false;
        laserParent.SetActive(false);
        if (selected) ReleaseObject();
        if (highlighted) HighlightObject(null);
    }
}
