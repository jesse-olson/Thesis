using UnityEngine;

public class SpindleSelectAndSendToController : MonoBehaviour {

	public Spindle spindle;

	private TesterController controller;

	private GameObject originalParent;

	// Use this for initialization
	void Start () {
		originalParent = transform.parent.gameObject;
        spindle.onSelectObject.AddListener(TellTesterOfSelection);
		controller = GetComponentInParent<TesterController>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void TellTesterOfSelection() {
		print("trying to select");
        if (spindle.selectedObject == gameObject) {
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
