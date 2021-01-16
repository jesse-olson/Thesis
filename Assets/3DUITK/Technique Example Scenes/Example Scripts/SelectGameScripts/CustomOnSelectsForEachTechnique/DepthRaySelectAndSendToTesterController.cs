using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthRaySelectAndSendToTesterController : MonoBehaviour {

	public DepthRay depthRay;

	private TesterController controller;

	private GameObject originalParent;

	// Use this for initialization
	void Start () {
		originalParent = transform.parent.gameObject;
		depthRay.onSelectObject.AddListener(TellTesterOfSelection);
		controller = GetComponentInParent<TesterController>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void TellTesterOfSelection() {
		print("trying to select");
		if(depthRay.selectedObject == gameObject) {
			print("success");
			controller.objectSelected(gameObject);

			// Due to bubble trying to grab with parent can cancel out with this
			Rigidbody bod;
			if((bod = GetComponent<Rigidbody>()) != null) {
				bod.isKinematic = false;
				this.transform.parent = originalParent.transform;
			}
		}	
	}
}
