using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableEnableRigidBodyOnBubblePickup : MonoBehaviour {

	public BubbleCursor bubble;
	// Use this for initialization
	void Start () {
		bubble.onSelectObject.AddListener(setRigidKinematic);
		bubble.onDropObject.AddListener(setRigidNotKinematic);
	}
	


	void setRigidKinematic() {
		if(bubble.selectedObject == this.gameObject) {
			this.GetComponent<Rigidbody>().isKinematic = true;
		}
		
	}

	void setRigidNotKinematic() {
		if(bubble.selectedObject == this.gameObject) {
			this.GetComponent<Rigidbody>().isKinematic = false;
		}
	}
}
