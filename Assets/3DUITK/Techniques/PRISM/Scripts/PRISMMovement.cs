using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

public class PRISMMovement : SingleHandTechnique {
    public GameObject collidingObject;
	public GameObject objectInHand = null;

	// Track last position of controller to get the direction it is moving
	private Vector3 lastPosition;
	
    // keeping track of time passed resets every 500ms
	private float timePassedTracker;
	private float delayTime = 0;

	public float minS = 0.001f;
	public float scaledConstant = 0.5f;
	public float maxS = 2f;

	// OFFSET RECOVERY VARIABLES
	private float offset = 0;
	private float timeThreshhold = 0;
    

	// Offset recovery as specified by paper Time.time is in seconds
	private void OffsetRecovery() {
		if(offset == 0) {
			// no offset so no need to recover
			return;
		}

		if(timeThreshhold == 0) {
            timeThreshhold = Time.time;
			return; // Just started recovery on next call will recover
		}

		float currentTime = Time.time;
        float deltaTime = currentTime - timeThreshhold;
        		
		if(0 < deltaTime && deltaTime < 0.5f) {
			offset *= 0.2f; // will recover offset by making offset 80% of itself
        } else if (0.5f < deltaTime && deltaTime < 1f) {
            offset *= 0.5f; // will recover offset by making offset 50% of itself
        } else if (deltaTime > 1f) {
            // will completely recover offset making it 0
            offset = 0;
            timeThreshhold = 0;
		}

		Vector3 normalizedDirection = (trackedObj.transform.position - objectInHand.transform.position).normalized;
		objectInHand.transform.position += offset * normalizedDirection;		
	}


	private void SetCollidingObject(Collider other)
    {
		
		if (collidingObject || !other.GetComponent<Rigidbody>() || interactionLayers != (interactionLayers | (1 << other.gameObject.layer)))
        {
            return;
        }
        collidingObject = other.gameObject;		
		onHover.Invoke();
    }

    public void OnTriggerEnter(Collider other)
    {       
        SetCollidingObject(other);
    }

    public void OnTriggerStay(Collider other)
    {
        SetCollidingObject(other);
    }

    public void OnTriggerExit(Collider other)
    {
		if (!collidingObject || interactionLayers != (interactionLayers | (1 << other.gameObject.layer)))
        {
            return;
        }    
		onUnhover.Invoke();
        collidingObject = null;
    }



	// Need to change this one
	private void PickUpObject()
    {
        
        objectInHand = collidingObject;
		Rigidbody bod;
		if((bod = objectInHand.GetComponent<Rigidbody>()) != null) {
			bod.isKinematic = true;
		}
		onSelectObject.Invoke();
        collidingObject = null;
	}

	private void ReleaseObject()
    {  
		Rigidbody bod;
		if((bod = objectInHand.GetComponent<Rigidbody>()) != null) {
			bod.isKinematic = false;
		}
        objectInHand = null;
    }
	

	// Use this for initialization
	void Awake () {
        InitializeControllers();
        transform.parent = trackedObj.transform;
	}

    // Update is called once per frame
    void Update()
    {
        ControllerState controllerState = ControllerEvents();

        if (controllerState == ControllerState.TRIGGER_DOWN)
        {
            if (collidingObject)
            {
                PickUpObject();
            }
        }


        if (controllerState == ControllerState.TRIGGER_UP)
        {
            if (objectInHand)
            {
                ReleaseObject();
            }
        }

        UpdateLastPosition();
    }


    // Only updates if millisecondDelayTime (500ms) has passed
    private void UpdateLastPosition() {
		if(timePassedTracker >= delayTime) {
			MoveObjectInHand();
			lastPosition = trackedObj.transform.position;

			timePassedTracker = 0;
		}
		timePassedTracker += Time.deltaTime * 1000f;
	}

	private void MoveObjectInHand() {
		if(objectInHand != null && lastPosition != null) {			
            Vector3 direction = GetDirectionControllerMoving();
            
            Vector3 movement = Vector3.zero;
            for(int i = 0; i < 3; i++)
            {
                movement[i] = Mathf.Sign(direction[i]) * ScaleMovement(GetDistanceTraveled(i));
            }

            // Moving object
            objectInHand.transform.position += movement;
			
			// calculating offset
			offset = Vector3.Distance(objectInHand.transform.position, trackedObj.transform.position);
			
			float speed = HandSpeed(GetDistanceTraveledSinceLastPosition());

			// recover offset if it exists
			if(maxS < speed) {
				OffsetRecovery();
			}
		}
	}

	private float ScaleMovement(float d_hand) {
        float handSpeed = HandSpeed(d_hand);
        
        //print(distanceHandMoved);
		float k = 0;
		if(handSpeed >= scaledConstant) {
			k = 1;
		} else if (minS < handSpeed && handSpeed < scaledConstant) {
			k = handSpeed / scaledConstant;
		}

		return k * d_hand;
	}

	private float HandSpeed(float distanceTraveled) {
		return distanceTraveled / (timePassedTracker/1000);
	}

	private Vector3 GetDirectionControllerMoving() {
		return (trackedObj.transform.position - lastPosition).normalized;
	}

	private float GetDistanceTraveledSinceLastPosition() {
		return Vector3.Distance(trackedObj.transform.position, lastPosition);
	}
    
    private float GetDistanceTraveled(int axis)
    {
        return Mathf.Abs(trackedObj.transform.position[axis] - lastPosition[axis]);
    }
}
