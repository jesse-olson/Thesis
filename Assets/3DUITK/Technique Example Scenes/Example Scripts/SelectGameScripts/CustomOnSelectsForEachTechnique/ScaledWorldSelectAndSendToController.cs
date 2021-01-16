using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaledWorldSelectAndSendToController : MonoBehaviour {

	public ScaledWorldGrab selectObject;

	private TesterController controller;

	private GameObject originalParent;

	// Use this for initialization
	void Awake () {
		selectObject.onSelectObject.AddListener(TellTesterOfSelection);
		controller = GetComponentInParent<TesterController>();
	}

	void Start() {
		originalParent = transform.parent.gameObject;
	}

	void TellTesterOfSelection() {
		print("trying to select");
		if(selectObject.selectedObject == gameObject) {
			print("success");
			controller.objectSelected(gameObject);

			// Due to bubble trying to grab with parent can cancel out with this
			Rigidbody bod;
			if((bod = GetComponent<Rigidbody>()) != null) {
				bod.isKinematic = false;
				transform.parent = originalParent.transform;
			}
		}	
	}
}
