﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AperatureSelectAndSendToTesterController : MonoBehaviour {

	public AperatureSelectionSelector selectObject;

	private TesterController controller;

	// Use this for initialization
	void Start () {
		selectObject.onSelectObject.AddListener(TellTesterOfSelection);
		controller = GetComponentInParent<TesterController>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void TellTesterOfSelection() {
		print("trying to select");
		if(selectObject.selectedObject == gameObject) {
			print("success");
			controller.objectSelected(gameObject);
		}	
	}
}
