using UnityEngine;

public class SimpleHandInteractor : MonoBehaviour {

    public SimpleVirtualHand simpleVirtualHand;

    public GameObject scaleSelected = null;

    // Use this for initialization
    void Start() {
        if(simpleVirtualHand == null)
            simpleVirtualHand = GetComponent<SimpleVirtualHand>();
    }

    private void OnTriggerEnter(Collider col) {
        simpleVirtualHand.SetHighlighted(col);
    }

    private void OnTriggerStay(Collider col) {
        simpleVirtualHand.SetHighlighted(col);
    }

    private void OnTriggerExit(Collider col) {
        simpleVirtualHand.RemoveHighlighted(col);
    }

}
