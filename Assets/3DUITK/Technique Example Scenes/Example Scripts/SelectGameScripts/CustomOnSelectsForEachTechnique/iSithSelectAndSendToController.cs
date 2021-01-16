using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class iSithSelectAndSendToController : MonoBehaviour {
	
	public ISith iSith;

	private TesterController controller;

	// Use this for initialization
	void Start () {
		iSith.onSelectObject.AddListener(TellTesterOfSelection);
		controller = this.GetComponentInParent<TesterController>();
	}

	void TellTesterOfSelection() {
		print("trying to select");
		if(iSith.selectedObject == gameObject) {
			print("success");
			controller.objectSelected(gameObject);
		}	
	}
}
