using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HOMERSelectAndSendToController : MonoBehaviour {

	public HOMER homer;

	private GameObject originalParent;
	private TesterController controller;

	// Use this for initialization
	void Start () {
		originalParent = transform.parent.gameObject;
		homer.onSelectObject.AddListener(tellTesterOfSelection);
		controller = GetComponentInParent<TesterController>();
	}

	void tellTesterOfSelection() {
		print("trying to select");
		if(homer.selectedObject == gameObject) {
			print("success");
			controller.objectSelected(gameObject);

			// Due to bubble trying to grab with parent can cancel out with this
			Rigidbody bod;
			if((bod = this.GetComponent<Rigidbody>()) != null) {
				bod.isKinematic = false;
				transform.parent = originalParent.transform;
			}
		}	
	}
}
