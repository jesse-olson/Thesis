using UnityEngine;

public class DepthRayDisableEnableRigidOnPickup : MonoBehaviour {

	public DepthRay ray;

	// Use this for initialization
	void Start () {
		ray.onSelectObject.AddListener(SetRigidKinematic);
		ray.onDropObject.AddListener(SetRigidNotKinematic);
	}
	
	void SetRigidKinematic() {
		if(ray.selectedObject == gameObject) {
			GetComponent<Rigidbody>().isKinematic = true;
		}
		
	}

	void SetRigidNotKinematic() {
		if(ray.selectedObject == gameObject) {
			GetComponent<Rigidbody>().isKinematic = false;
		}
	}
}
