/* Simple Virtual Hand implementation by Kieran May
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

public class SimpleVirtualHand : Technique
{
    private bool IsInteractionlayer(GameObject obj) {
        return interactionLayers == (interactionLayers | (1 << obj.layer));
    }

    public void RemoveHighlighted(Collider other)
    {
        if (selected || !IsInteractionlayer(other.gameObject)) return;
        HighlightObject(null);
    }

    public void SetHighlighted(Collider other)
    {
        if (selected || !IsInteractionlayer(other.gameObject)) return;
        HighlightObject(other.gameObject);
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

        switch(interactionType){
            case InteractionType.Selection:
                print("Selected object in pure selection mode:" + selectedObject.name);
                break;

            case InteractionType.Manipulation_Movement:
            case InteractionType.Manipulation_Full:
                selectedObject.transform.SetParent(trackedObj);
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
                selectedObject.transform.SetParent(null);
                break;

            default:
                break;
        }
        print("Object dropped..");
        onDropObject.Invoke();
        selected = false;
        selectedObject = null;
    }
}
