using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BendcastSelectSendToTesterController : MonoBehaviour {

	public Bendcast bendcast;
	private TesterController controller;

	// Use this for initialization
	void Start () {
        bendcast.onSelectObject.AddListener(TellTesterOfSelection);
		controller = this.GetComponentInParent<TesterController>();
	}

	void TellTesterOfSelection() {
		print("trying to select");
		if(bendcast.selectedObject == gameObject) {
			print("success");
			controller.objectSelected(gameObject);
		}	
	}
}
