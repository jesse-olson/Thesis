using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHighlightFromPRISMMovement : MonoBehaviour {



	public Material highlightMaterial;
	private Material defaultMaterial;

	public PRISMMovement selectObject;

	// Use this for initialization
	void Start () {
		defaultMaterial = this.GetComponent<Renderer>().material;
		selectObject.onHover.AddListener(highlight);
		selectObject.onUnhover.AddListener(unHighlight);	
		selectObject.onSelectObject.AddListener(playSelectSound);	
	}

	void highlight() {
		print("trying");
		if(selectObject.collidingObject == this.gameObject && selectObject.objectInHand == null) {
			print("highlight");
			this.GetComponent<Renderer>().material = highlightMaterial;
		}		
	}

	void unHighlight() {
		if(selectObject.collidingObject == this.gameObject) {
			print("unhighlight");
			this.GetComponent<Renderer>().material = defaultMaterial;
		}		
	}

	void playSelectSound() {
		if(selectObject.objectInHand == this.gameObject) {
			this.GetComponent<AudioSource>().Play();
		}	
	}
}
