/*
 *  SimpleHighlightFromBendCast - Script that can be attached to an object displaying how to utilize the grab object controllers events to react externally when an object
 *                                is highlighted or selected.
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHighlightFromGrabObject : MonoBehaviour {
	public Material highlightMaterial;
	private Material defaultMaterial;

	public GoGo grabObject;

	// Use this for initialization
	void Start () {
		defaultMaterial = GetComponent<Renderer>().material;
        grabObject.onHover		.AddListener(Highlight);
        grabObject.onUnhover	.AddListener(Unhighlight);
        grabObject.onSelectObject.AddListener(PlaySelectSound);	
	}

    private void Highlight() {
		if(grabObject.highlightedObject == gameObject && !grabObject.selected) 
        {
			print("highlight");
			GetComponent<Renderer>().material = highlightMaterial;
		}		
	}

    private void Unhighlight() {
		if(grabObject.highlightedObject == gameObject) {
			print("unhighlight");
			GetComponent<Renderer>().material = defaultMaterial;
		}		
	}

    private void PlaySelectSound() {
		if(grabObject.selectedObject == gameObject) {
			GetComponent<AudioSource>().Play();
		}	
	}
}
