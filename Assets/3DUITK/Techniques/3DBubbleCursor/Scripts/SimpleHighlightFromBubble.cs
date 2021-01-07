using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHighlightFromBubble : MonoBehaviour {

	public Material highlightMaterial;
	private Material defaultMaterial;

	public BubbleCursor bubbleCursor;

	// Use this for initialization
	void Start () {
		defaultMaterial = GetComponent<Renderer>().material;
        bubbleCursor.onHover    .AddListener(Highlight);
        bubbleCursor.onUnhover  .AddListener(UnHighlight);	
	}

	void Highlight() {
		if(bubbleCursor.currentlyHovering == gameObject) {
			print("highlight");
			GetComponent<Renderer>().material = highlightMaterial;
		} 
	}

	void UnHighlight() {
		if(bubbleCursor.currentlyHovering == gameObject) {
			GetComponent<Renderer>().material = defaultMaterial;
		}
	}
}
