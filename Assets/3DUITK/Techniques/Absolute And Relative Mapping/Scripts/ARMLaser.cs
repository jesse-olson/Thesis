/*
 *  Absolute and relative mapping is in the form of a Raycast (However It could 
 *  be adapted to a simple hand technique). When you press the set button (touchpad) the movement 
 *  of the virtual controller relative to your real controller is scaled with a ration of 10:1. 
 *  By doing this you can be precise when selecting small or distant objects with the ray. This is
 *  because the distance you have to move your hand across an object is amplified 10x due to the ratio.
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

public class ARMLaser : SingleHandTechnique {
    private static readonly float RAY_DIST = 100.0f;

    [Range(1, 20)]
    public float scaleFactor = 10.0f;   // The factor by which the movement should be scaled
    private float scaleBy;

    private bool armOn = false;

    private Vector3    armPosition;     // The origin position set when ARM is toggled on
    private Quaternion armRotation;     // The current rotation of the trackedObj when ARM is toggled on

    [Header("Prefabs")]
    public GameObject modelPrefab;
    public GameObject theModel;         // The model that is used for the controller and the shadow

    public GameObject laserPrefab;
    private GameObject laser;

    void Awake() {
        SetScaleFactor(scaleFactor);

        laser = Instantiate(laserPrefab);
        laser.transform.SetParent(transform);
        laser.SetActive(true);

        if (interactionType == InteractionType.Manipulation_UI) {
            gameObject.AddComponent<SelectionManipulation>();
            GetComponent<SelectionManipulation>().trackedObj = trackedObj.gameObject;
        }
    }

    // Use this for initialization
    void Start() {
        FindEventSystem();
    }

    // Update is called once per frame
    void Update()
    {
        MakeModelChild();

        float scale = armOn ? scaleBy : 1;
        transform.position = Vector3.Lerp(armPosition, trackedObj.position, scale);
        transform.rotation = Quaternion.Lerp(armRotation, trackedObj.rotation, scale);

        Ray ray = new Ray(transform.position, transform.forward);
        bool didHit = Physics.Raycast(ray, out RaycastHit hit, RAY_DIST, interactionLayers);

        float dist = didHit ? hit.distance : RAY_DIST;
        laser.transform.localScale = new Vector3(1, 1, dist);

        GameObject hitObject = didHit ? hit.transform.gameObject : null;
        HighlightObject(hitObject);
    }

    // Using the hack from gogo shadow - will have to fix them all once find a better way
    void MakeModelChild() {
        if (transform.childCount < 2)
        {
#if Oculus_Quest_Controllers
            // If it is just a custom model we can immediately parent
            theModel = Instantiate(modelPrefab);
            theModel.transform.SetParent(transform);
#elif SteamVR_Legacy || SteamVR_2
            if (theModel.transform.childCount > 0) {
                theModel.transform.SetParent(transform);
                // Due to the transfer happening at a random time down the line we need to re-align the model inside the shadow controller to 0 so nothing is wonky.
                theModel.transform.localPosition = Vector3.zero;
                theModel.transform.localRotation = Quaternion.identity;
            }
#else
            // If it is just a custom model we can immediately parent
            theModel.transform.SetParent(transform);
            // Due to the transfer happening at a random time down the line we need to re-align the model inside the shadow controller to 0 so nothing is wonky.
            theModel.transform.localPosition = Vector3.zero;
            theModel.transform.localRotation = Quaternion.identity;
#endif
        }
    }

    void ToggleARM() {
        armPosition = trackedObj.position;
        armRotation = trackedObj.rotation;
        armOn = !armOn;
    }

    public override void SelectObject()
    {
        ToggleARM();
    }

    public override void ReleaseObject()
    {
        ToggleARM();
    }

    protected override void Enable()
    {
    }

    protected override void Disable()
    {
    }

    public void SetScaleFactor(float scaleFactor)
    {
        this.scaleFactor = scaleFactor;
        scaleBy = 1 / scaleFactor;
    }
}
