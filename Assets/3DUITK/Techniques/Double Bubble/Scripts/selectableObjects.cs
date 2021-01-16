using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class selectableObjects : MonoBehaviour {

    private BubbleSelection bubbleSelection;
    public  GameObject radiusBubble;

    private void Start() {
        bubbleSelection = radiusBubble.GetComponent<BubbleSelection>();
    }

    private void OnTriggerStay(Collider collider) {
        //if( 1 << collider.gameObject.layer == bubbleSelection.interactionLayers.value &&
        //    !bubbleSelection.selectableObjects.Contains(collider.gameObject)) 
        //{
        //    bubbleSelection.selectableObjects.Add(collider.gameObject);
        //}
    }

}
