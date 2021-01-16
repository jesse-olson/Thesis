using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingReelDisableEnableRigidOnPickup : MonoBehaviour {
	public FishingReel reel;
	// Use this for initialization
	void Start () {
		reel.onSelectObject.AddListener(setRigidKinematic);
		reel.onDropObject.AddListener(setRigidNotKinematic);
	}
	


	void setRigidKinematic() {
		if(reel.selectedObject == gameObject) {
			this.GetComponent<Rigidbody>().isKinematic = true;
		}
		
	}

	void setRigidNotKinematic() {
		if(reel.selectedObject == gameObject) {
			this.GetComponent<Rigidbody>().isKinematic = false;
		}
	}
}
