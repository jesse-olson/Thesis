using UnityEngine;

public class HookSelectAndSendToController : MonoBehaviour {

	public Hook selectObject;

	private TesterController controller;

	// Use this for initialization
	void Start () {
		selectObject.onSelectObject.AddListener(tellTesterOfSelection);
		controller = GetComponentInParent<TesterController>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void tellTesterOfSelection() {
		print("trying to select");
		if(selectObject.selectedObject == gameObject) {
			print("success");
			controller.objectSelected(gameObject);
		}	
	}
}
