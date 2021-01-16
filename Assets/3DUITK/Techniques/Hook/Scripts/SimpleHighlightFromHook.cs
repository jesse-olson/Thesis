using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHighlightFromHook : MonoBehaviour {

	public Material highlightMaterial;
	private Material defaultMaterial;

	public Hook selectObject;

	// Use this for initialization
	void Start () {
		defaultMaterial = GetComponent<Renderer>().material;
		selectObject.onHover.AddListener(Highlight);
		selectObject.onUnhover.AddListener(UnHighlight);	
		selectObject.onSelectObject.AddListener(PlaySelectSound);	
	}

	void Highlight() {
		if(selectObject.highlightedObject == gameObject) {
			print("highlight");
			GetComponent<Renderer>().material = highlightMaterial;
		} 
	}

	void UnHighlight() {
		if (selectObject.highlightedObject == gameObject)
		{
			GetComponent<Renderer>().material = defaultMaterial;
		}	
	}

	void PlaySelectSound() {
		if(selectObject.selectedObject == gameObject) {
			GetComponent<AudioSource>().Play();
		}	
	}
}
