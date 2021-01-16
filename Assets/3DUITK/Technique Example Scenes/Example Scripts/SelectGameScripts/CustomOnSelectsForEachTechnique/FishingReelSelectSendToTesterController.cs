using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingReelSelectSendToTesterController : MonoBehaviour {

	public FishingReel fishingReel;

	private TesterController controller;

	private GameObject originalParent;

	// Use this for initialization
	void Start () {
		originalParent = this.transform.parent.gameObject;
		fishingReel.onSelectObject.AddListener(TellTesterOfSelection);
		controller = this.GetComponentInParent<TesterController>();
	}
	
	void TellTesterOfSelection() {
		print("trying to select");
		if(fishingReel.selectedObject == gameObject) {
			print("success");
			controller.objectSelected(gameObject);

			// Due to bubble trying to grab with parent can cancel out with this
			Rigidbody bod;
			if((bod = this.GetComponent<Rigidbody>()) != null) {
				bod.isKinematic = false;
				this.transform.parent = originalParent.transform;
			}
		}	
	}
}
