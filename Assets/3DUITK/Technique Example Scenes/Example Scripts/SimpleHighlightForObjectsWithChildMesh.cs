﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHighlightForObjectsWithChildMesh : MonoBehaviour {

		
	public Material highlightMaterial;
	private Material defaultMaterial;

	public GoGo selectObject;

	// Use this for initialization
	void Start () {

		// Find a existing default material (this will only work if they all use the same texture)
		foreach(Transform obj in this.transform) {
			Renderer render;
			if((render = obj.GetComponent<Renderer>()) != null) {
				defaultMaterial = render.material;
				break;
			}
		}

		selectObject.onHover.AddListener(highlight);
		selectObject.onUnhover.AddListener(unHighlight);	
		selectObject.onSelectObject.AddListener(playSelectSound);	
	}

	void highlight() {
		if(selectObject.highlightedObject == gameObject && !selectObject.selected) {
			print("highlight");
			foreach(Transform obj in this.transform) {
				Renderer render;
				if((render = obj.GetComponent<Renderer>()) != null) {
					render.material = highlightMaterial;
				}
			}
		}		
	}

	void unHighlight() {
		if(selectObject.highlightedObject == gameObject) {
			print("unhighlight");
			foreach(Transform obj in this.transform) {
				Renderer render;
				if((render = obj.GetComponent<Renderer>()) != null) {
					render.material = defaultMaterial;
				}
			}
		}		
	}

	void playSelectSound() {
		if(selectObject.selectedObject == gameObject) {
			this.GetComponent<AudioSource>().Play();
		}	
	}

}
