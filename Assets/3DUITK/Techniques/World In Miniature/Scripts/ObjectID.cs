using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectID : MonoBehaviour {

	public int ID;
    public bool movableObject;

    public GameObject realObject;
    public GameObject clonedObject;

    public bool IsMoving() {
        if(movableObject) {
            return !realObject.transform.GetComponent<Rigidbody>().IsSleeping();
        }
        return false;
    }
    	
	// Update is called once per frame
	void Update () {
		if (IsMoving() && clonedObject != null) {
            clonedObject.transform.localPosition    = this.transform.localPosition;
            clonedObject.transform.localEulerAngles = this.transform.localEulerAngles;
        }
	}
}
