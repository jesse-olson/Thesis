using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class iSithLaser : MonoBehaviour {

    public GameObject laserPrefab;
    private GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;

    public GameObject controller = null;

    // Use this for initialization
    void Start () {
        laser = Instantiate(laserPrefab);
        laserTransform = laser.transform;
    }
	
	// Update is called once per frame
	void Update () {
        ShowLaser();
    }

    private void ShowLaser()
    {
        // This is to make it extend infinite. There is DEFINATELY an easier way to do this. Find it later!
        Vector3 theVector = controller.transform.forward;
        hitPoint = controller.transform.position;
        float distance_formula_on_vector = theVector.magnitude;

        // Using formula to find a point which lies at distance on a 3D line from vector and direction
        hitPoint = hitPoint + (100 / (distance_formula_on_vector)) * theVector;

        laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(controller.transform.position, hitPoint, .5f);
        laserTransform.LookAt(hitPoint);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y,
           100);
    }

}
