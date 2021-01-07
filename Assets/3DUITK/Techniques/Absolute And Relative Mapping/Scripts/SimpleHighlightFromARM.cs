using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHighlightFromARM : MonoBehaviour {

	public Material highlightMaterial;
	private Material defaultMaterial;

	public ARMLaser selectObject;

	// Use this for initialization
	void Start () {
		defaultMaterial = this.GetComponent<Renderer>().material;
		selectObject.onHover.AddListener(highlight);
		selectObject.onUnhover.AddListener(unHighlight);	
		selectObject.onSelectObject.AddListener(playSelectSound);	
	}

	void highlight() {
		if(selectObject.currentlyPointingAt == this.gameObject) {
			print("highlight");
			this.GetComponent<Renderer>().material = highlightMaterial;
		}		
	}

	void unHighlight() {
		if(selectObject.currentlyPointingAt == this.gameObject) {
			print("unhighlight");
			this.GetComponent<Renderer>().material = defaultMaterial;
		}		
	}

	void playSelectSound() {
		if(selectObject.currentlyPointingAt == this.gameObject) {
			this.GetComponent<AudioSource>().Play();
		}	
	}
}
