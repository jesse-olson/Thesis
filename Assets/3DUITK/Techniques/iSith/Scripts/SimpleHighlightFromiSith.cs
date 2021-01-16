using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHighlightFromiSith : MonoBehaviour {

	public Material highlightMaterial;
	private Material defaultMaterial;

	public ISith iSith;

	// Use this for initialization
	void Start () {
		defaultMaterial = this.GetComponent<Renderer>().material;
        iSith.onHover.AddListener(Highlight);
        iSith.onUnhover.AddListener(Unhighlight);
        iSith.onSelectObject.AddListener(PlaySelectSound);	
	}

    private void Highlight() {
		if(iSith.highlightedObject == gameObject) {
			print("highlight");
			this.GetComponent<Renderer>().material = highlightMaterial;
		}		
	}

    private void Unhighlight() {
		if(iSith.highlightedObject == gameObject) {
			print("unhighlight");
			this.GetComponent<Renderer>().material = defaultMaterial;
		}		
	}

    private void PlaySelectSound() {
		if(iSith.selectedObject == gameObject) {
			this.GetComponent<AudioSource>().Play();
		}	
	}
}
