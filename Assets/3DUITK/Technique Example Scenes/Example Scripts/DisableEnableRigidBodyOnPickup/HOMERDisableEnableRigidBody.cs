using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HOMERDisableEnableRigidBody : MonoBehaviour {

	public HOMER homer;
	// Use this for initialization
	void Start () {
		homer.onSelectObject.AddListener(setRigidKinematic);
		homer.onDropObject.AddListener(setRigidNotKinematic);
	}



	void setRigidKinematic() {
		if(homer.selectedObject == gameObject) {
			this.GetComponent<Rigidbody>().isKinematic = true;
		}

	}

	void setRigidNotKinematic() {
		if(homer.selectedObject == gameObject) {
			this.GetComponent<Rigidbody>().isKinematic = false;
		}
	}
}
