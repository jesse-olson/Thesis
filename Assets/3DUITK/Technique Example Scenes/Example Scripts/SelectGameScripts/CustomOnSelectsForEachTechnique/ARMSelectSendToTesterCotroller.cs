using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARMSelectSendToTesterCotroller : MonoBehaviour {

	public ARMLaser armLaser;

	private TesterController controller;

	// Use this for initialization
	void Start () {
        armLaser.onSelectObject.AddListener(TellTesterOfSelection);
		controller = GetComponentInParent<TesterController>();
	}

	void TellTesterOfSelection() {
		print("trying to select");
		if(armLaser.selectedObject == gameObject) {
			print("success");
			controller.objectSelected(gameObject);
		}	
	}
}
