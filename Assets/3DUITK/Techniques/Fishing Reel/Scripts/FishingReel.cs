/* Fishing Reel implementation by Kieran May
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

public class FishingReel : ScrollingTechnique {
    private static readonly float RAY_DIST = 100.0f;

    public GameObject laserPrefab;
    private GameObject laser;

    private bool castEnable;

    void Awake() {
        if (interactionType == InteractionType.Manipulation_UI) {
            gameObject.AddComponent<SelectionManipulation>();
            GetComponent<SelectionManipulation>().trackedObj = trackedObj.gameObject;
#if SteamVR_2
            this.GetComponent<SelectionManipulation>().m_controllerPress = m_controllerPress;
            this.GetComponent<SelectionManipulation>().m_touchpad = m_touchpad;
            this.GetComponent<SelectionManipulation>().m_touchpadAxis = m_touchpadAxis;
            this.GetComponent<SelectionManipulation>().m_applicationMenu = m_applicationMenu;
#endif
        }
    }

    void Start() {
        FindEventSystem();
        laser = Instantiate(laserPrefab) as GameObject;
        laser.transform.SetParent(trackedObj);
    }

    void Update()
    {
        if (!castEnable) return;

        PadScrolling();
        ReelObject();

        Ray ray = new Ray(trackedObj.position, trackedObj.forward);
        bool didHit = Physics.Raycast(ray, out RaycastHit hit, RAY_DIST, interactionLayers);

        float laserDist = didHit ? hit.distance : RAY_DIST;
        laser.transform.localScale = new Vector3(1, 1, laserDist);

        GameObject hitObject = didHit ? hit.transform.gameObject : null;
        HighlightObject(hitObject);
    }

    private void ReelObject() {
        if (!selected) return;
        Vector3 controllerFwd = trackedObj.forward;
        Vector3 controllerPos = trackedObj.position;

        // Using formula to find a point which lies at distance on a 3D line from vector and direction
        controllerPos += extendDistance * controllerFwd;

        selectedObject.transform.position = controllerPos;
        selectedObject.transform.rotation = trackedObj.rotation;
    }

    public override void SelectObject()
    {
        if (selected || !highlighted) return;
        selected = true;
        selectedObject = highlightedObject;
        switch (interactionType)
        {
            case InteractionType.Manipulation_Movement:
            case InteractionType.Manipulation_Full:
                selectedObject.transform.SetParent(trackedObj);
                extendDistance = Vector3.Distance(trackedObj.position, selectedObject.transform.position);
                break;

            case InteractionType.Manipulation_UI:
                if (!GetComponent<SelectionManipulation>().inManipulationMode)
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
        switch (interactionType) {
            case InteractionType.Manipulation_Movement:
            case InteractionType.Manipulation_Full:
                selectedObject.transform.SetParent(null);
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
    }

    protected override void Disable()
    {
        castEnable = true;
        laser.SetActive(true);
        ReleaseObject();
        HighlightObject(null);
    }
}
