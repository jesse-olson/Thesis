﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class RotationPRISM : Technique {
    
    private GameObject collidingObject;
    private GameObject objectInHand;

    private Quaternion lastHandRotation;
    private Quaternion currentRotation;

    public GameObject test1;
    public GameObject test2;



    // keeping track of time passed resets every 500ms
    private float timePassedTracker;
    private float millisecondsDelayTime = 0;

    public float rotationMinS = 0.015f;
    public float rotationScalingConstant = 0.5f;
    public float rotationMaxS = 2f;


    // Use this for initialization
    void Start() {

        lastHandRotation = this.transform.rotation;
    }
       
    // Update is called once per frame
    void Update() {
        currentRotation = trackedObj.transform.rotation;

        ControllerState controllerState = ControllerEvents();

        if (controllerState == ControllerState.TRIGGER_DOWN) {
            if (collidingObject) {
                pickUpObject();
            }
        }


        if (controllerState == ControllerState.TRIGGER_UP) {
            if (objectInHand) {
                ReleaseObject();
            }
        }
        updateLastRotation();
    }

    private void SetCollidingObject(Collider other) {

        if (collidingObject || !other.GetComponent<Rigidbody>() || interactionLayers != (interactionLayers | (1 << other.gameObject.layer))) {
            return;
        }

        collidingObject = other.gameObject;
    }

    public void OnTriggerEnter(Collider other) {
        SetCollidingObject(other);
    }

    public void OnTriggerStay(Collider other) {
        SetCollidingObject(other);
    }

    public void OnTriggerExit(Collider other) {
        if (!collidingObject || interactionLayers != (interactionLayers | (1 << other.gameObject.layer))) {
            return;
        }
        collidingObject = null;
    }

    // Need to change this one
    private void pickUpObject() {
        objectInHand = collidingObject;
        collidingObject = null;

        // Set objectsPosition to hand
        //objectInHand.transform.position = this.transform.position;
    }

    private void ReleaseObject() {
        objectInHand = null;
    }

    // Only updates if millisecondDelayTime (500ms) has passed
    private void updateLastRotation() {
        if (timePassedTracker >= millisecondsDelayTime) {
            rotateObjectInHand();
            lastHandRotation = this.transform.rotation;
            timePassedTracker = 0;
        }
        timePassedTracker = timePassedTracker += millisecondsSinceLastUpdate();
    }

    private void rotateObjectInHand() {
        if (objectInHand != null && lastHandRotation != null) {
            Quaternion rotationToMoveTo = getNewOrientation();
            // TODO: Make sure 360 is correct for step? It's meant to be instant is that making it instant?

            Quaternion objHandRot = objectInHand.transform.rotation;
            Quaternion newRot = rotationToMoveTo;
            Quaternion trackedRot = trackedObj.transform.rotation;

            test1.transform.rotation = objHandRot;
            test2.transform.rotation = trackedRot;

            print("");
            print("Old rotation: (" + objHandRot.x + ", " + objHandRot.y + ", " + objHandRot.z + ")");
            print("New rotation: (" + newRot.x + ", " + newRot.y + ", " + newRot.z + ")");
            print("Track rotation: (" + objHandRot.x + ", " + objHandRot.y + ", " + objHandRot.z + ")");



            objectInHand.transform.rotation = Quaternion.RotateTowards(objectInHand.transform.rotation, rotationToMoveTo, 360);
        }
    }

    private float millisecondsSinceLastUpdate() {
        return Time.deltaTime * 1000f;
    }

    /** 
		INFORMATION ABOUT EQUATIONS/METHODS BELOW:

		Qdiff is the quaternion representing the angle the hand has rotated in
			the last 200 ms.*

		Qt is the quaternion representing the current hand orientation

		Qt−1 is the quaternion representing the hand orientation 200 ms before
			the current time

		Qnew is the quaternion representing the new orientation of the object

		Qobject is the quaternion representing the current orientation of the
			controlled object

		A is the angle (in degrees) the hand has rotated in the last 200 ms.*
		RS is rotational speed of the hand in degrees/s	
	 */

    // Calculates the rotational difference between the current(Qt) and
    // last orientation (Qt−1) of the hand, in the form of Qdiff. Note, to find the quaternion
    // needed to rotate from q1 to q2, q2 is divided by q1
    private Quaternion getQdiff() {
        return currentRotation * Quaternion.Inverse(lastHandRotation);
    }

    // Converts the angle represented by Qdiff from radians to degrees and Eq.
    // (We just used unity's built in angle calculator)
    private float getAngleRotatedInTimePassed() {
        return Quaternion.Angle(currentRotation, lastHandRotation);
    }

    // Simply divides the angle by 200 ms (we are using our actual time passed converted to seconds)  (the time between Qt and Qt−1) to obtain the rotational
    // speed of the hand
    private float getRotationSpeed() {
        return getAngleRotatedInTimePassed() / (timePassedTracker / 1000f);  // IS IN SECONDS CHECK IF NEED TO CHANGE FORMAT
    }

    // Is used to determine the control display ratio to
    // be used. The inverse of the control display ratio, k, is used to scale rotation
    private float getK() {
        float speed = getRotationSpeed();
        if (speed >= rotationScalingConstant) {
            return 1;
        } else if (rotationMinS < speed && speed < rotationScalingConstant) {
            float scaledK = speed / rotationScalingConstant;
            return speed / rotationScalingConstant;
        } else if (speed <= rotationMinS) {
            return 1;
            return 0;
        }
        return 0; // CHECK IF THATS RIGHT
    }

    // The quaternion representation of the angle the hand has rotated (Qdiff)
    // is scaled by raising it to the power k, where k is a real number between 0 and 1.
    private Quaternion getNewOrientation() {
        // My interpetation of the description of algorithm but using unity methods
        float kValue = getK();
        ;
        if (kValue == 0) {
            // In the paper if the k value is 0 it relys on that causing the below equation to have an infinite (broken result)
            // making the change nothing. However because our equation is different to theirs due to our implementation
            // our result will give the same 1-1 mapping as if k value was 1. Therefore we must manuelly give a result
            // for if k value is 0
            return currentRotation;
        }
        //return Quaternion.RotateTowards(objectInHand.transform.rotation, currentRotation, kValue*Time.deltaTime);

        return powered(currentRotation * Quaternion.Inverse(lastHandRotation), kValue) * objectInHand.transform.rotation;
    }

    private Quaternion powered(Quaternion theQuaternion, float power) {
        // TODO: Clean up this powered function

        Quaternion ln = theQuaternion;
        float r = (float)Mathf.Sqrt(ln.x * ln.x + ln.y * ln.y + ln.z * ln.z);
        float t = r > 0.00001f ? (float)System.Math.Atan2(r, ln.w) / r : 0f;
        ln.w = 0.5f * (float)Mathf.Log(ln.w * ln.w + ln.x * ln.x + ln.y * ln.y + ln.z * ln.z);
        ln.x *= t;
        ln.y *= t;
        ln.z *= t;

        Quaternion scale = ln;
        scale.w *= power;
        scale.x *= power;
        scale.y *= power;
        scale.z *= power;

        Quaternion exp = scale;
        r = (float)Mathf.Sqrt(exp.x * exp.x + exp.y * exp.y + exp.z * exp.z);
        float et = (float)Mathf.Exp(exp.w);
        float s = r >= 0.00001f ? et * (float)Mathf.Sin(r) / r : 0f;
        exp.w = et * (float)Mathf.Cos(r);
        exp.x *= s;
        exp.y *= s;
        exp.z *= s;

        return exp;
    }
}