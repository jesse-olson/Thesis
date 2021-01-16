using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHighlightFromFlexiblePointer : MonoBehaviour {
	public Material highlightMaterial;
	private Material defaultMaterial;

	public FlexiblePointer flexiblePointer;

	// Use this for initialization
	void Start () {
		defaultMaterial = GetComponent<Renderer>().material;
		flexiblePointer.onHover.AddListener(Highlight);
		flexiblePointer.onUnhover.AddListener(UnHighlight);
		flexiblePointer.onSelectObject.AddListener(PlaySelectSound);
	}

	void Highlight() {
		if(flexiblePointer.highlightedObject == gameObject) {
			print("highlight");
			GetComponent<Renderer>().material = highlightMaterial;
		}
	}

	void UnHighlight() {
		if(flexiblePointer.highlightedObject == gameObject) {
			print("unhighlight");
			GetComponent<Renderer>().material = defaultMaterial;
		}
	}

	void PlaySelectSound() {
		if(flexiblePointer.selectedObject == gameObject) {
			GetComponent<AudioSource>().Play();
		}
	}
}
