using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerColliderWIM : MonoBehaviour {

    private WorldInMiniature worldInMin;
    private GameObject manipulationIcons;

	private List<GameObject> listOfChildrenR  = new List<GameObject>();
	private List<GameObject> listOfChildrenR2 = new List<GameObject>();

	private void EnableRigidBody(GameObject obj){
		if (null == obj)
			return;
		foreach (Transform child in obj.transform){
			if (null == child)
				continue;
			if (child.gameObject.GetComponent<Rigidbody> () != null) {
				child.gameObject.GetComponent<Rigidbody> ().isKinematic = true;
			}
			listOfChildrenR.Add(child.gameObject);
			EnableRigidBody(child.gameObject);
		}
	}

	private void DisableRigidBody(GameObject obj){
		if (null == obj)
			return;
		foreach (Transform child in obj.transform){
			if (null == child)
				continue;
			if (child.gameObject.GetComponent<Rigidbody> () != null) {
				child.gameObject.GetComponent<Rigidbody> ().isKinematic = false;
			}
			listOfChildrenR2.Add(child.gameObject);
			DisableRigidBody(child.gameObject);
		}
	}

    private void OnTriggerStay(Collider col) {
		if(col.gameObject.layer == Mathf.Log(worldInMin.interactableLayer.value, 2)) {
            if (worldInMin.ControllerEvents() == WorldInMiniature.ControllerState.TRIGGER_PRESS && worldInMin.objectPicked == false) {
                Debug.Log("You have collided with " + col.name + " while holding down Touch");
                if (worldInMin.interacionType == InteractionType.Manipulation_Movement) {
					listOfChildrenR.Clear();
                    worldInMin.oldParent = col.gameObject.transform.parent;
                    col.attachedRigidbody.isKinematic = true;
                    col.gameObject.transform.SetParent(this.gameObject.transform);
                    worldInMin.selectedObject = col.gameObject;
                    worldInMin.currentObjectCollided = col.gameObject;
                    worldInMin.objectPicked = true;
					//disableRigidBody(worldInMin.worldInMinParent);
                } else if (worldInMin.interacionType == InteractionType.Selection) {
                    worldInMin.oldParent = col.gameObject.transform.parent;
                    
                    worldInMin.selectedObject = col.gameObject;
                    //worldInMin.selectedObject.transform.GetComponent<Renderer>().material = worldInMin.outlineMaterial;
                    worldInMin.objectPicked = true;
                } else if (this.GetComponent<SelectionManipulation>().inManipulationMode == false) {
                    worldInMin.objectPicked = true;
                    this.GetComponent<SelectionManipulation>().selectedObject = col.gameObject;
                }
            }
            if(worldInMin.ControllerEvents() == WorldInMiniature.ControllerState.TRIGGER_UP && worldInMin.objectPicked == true) {
                worldInMin.currentObjectCollided = null;

                if(worldInMin.interacionType == InteractionType.Manipulation_Movement) {
                    /*if(worldInMin.interacionType == WorldInMiniature.InteractionType.Manipulation_Full) {
                        this.GetComponent<SelectionManipulation>().selectedObject.transform.SetParent(null);
                    }*/
                    worldInMin.objectPicked = false;
                }
            }
        }
    }

    // Use this for initialization
    void Start () {
        worldInMin = GameObject.Find("WorldInMiniature_Technique").GetComponent<WorldInMiniature>();
    }
}
