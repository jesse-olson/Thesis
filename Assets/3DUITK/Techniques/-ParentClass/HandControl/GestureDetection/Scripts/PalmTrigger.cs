using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PalmTrigger : MonoBehaviour
{
    public List<HandGesture> handGestures = new List<HandGesture>();

    private HandGesture next;
    public HandGesture current;
    private HandGesture previous;
    private bool hasChanged = false;

    
    private void OnTriggerEnter(Collider other)
    {
        hasChanged = true;
        Finger finger = other.GetComponent<FingerTip>().finger;
        SetFinger(finger, true);
    }

    private void OnTriggerExit(Collider other)
    {
        hasChanged = true;
        Finger finger = other.GetComponent<FingerTip>().finger;
        SetFinger(finger, false);
    }

    public void SetFinger(Finger finger, bool inTrigger)
    {
        switch (finger)
        {
            case Finger.indexFinger:
                current.indexFinger = inTrigger;
                break;

            case Finger.middleFinger:
                current.middleFinger = inTrigger;
                break;

            case Finger.ringFinger:
                current.ringFinger = inTrigger;
                break;

            case Finger.pinkyFinger:
                current.pinkyFinger = inTrigger;
                break;

            case Finger.thumbFinger:
                current.thumb = inTrigger;
                break;

            default:
                break;
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Check to see that a finger has entered or exited the trigger zone
        if (hasChanged)
        {
            // Go through each hand gesture until you find one that matches the current gesture
            foreach (HandGesture handGesture in handGestures)
            {
                if (current.Equals(handGesture))
                {
                    // Invoke the OnGesture
                    handGesture.OnGesture.Invoke();
                    break;
                }
            }
            hasChanged = false;
        }
        previous.Set(current);
    }

    // Check to see whether the selected finger is in the trigger zone
    public bool Get(Finger finger)
    {
        switch (finger)
        {
            case Finger.thumbFinger:
                return current.thumb;
            case Finger.indexFinger:
                return current.indexFinger;
            case Finger.middleFinger:
                return current.middleFinger;
            case Finger.pinkyFinger:
                return current.pinkyFinger;
            case Finger.ringFinger:
                return current.ringFinger;
        }
        return false;
    }

    // Check to see if the selected finger has entered the trigger zone
    public bool GetFingerDown(Finger finger)
    {
        switch (finger)
        {
            case Finger.thumbFinger:
                if (!current.thumb) { return false; }
                return current.thumb != previous.thumb;

            case Finger.indexFinger:
                if (!current.indexFinger) { return false; }
                return current.indexFinger != previous.indexFinger;

            case Finger.middleFinger:
                if (!current.middleFinger) { return false; }
                return current.middleFinger != previous.middleFinger;

            case Finger.pinkyFinger:
                if (!current.pinkyFinger) { return false; }
                return current.pinkyFinger != previous.pinkyFinger;

            case Finger.ringFinger:
                if (!current.ringFinger) { return false; }
                return current.ringFinger != previous.ringFinger;
        }
        return false;
    }

    // Check to see if the selected finger has exited the trigger zone
    public bool GetFingerUp(Finger finger)
    {
        switch (finger)
        {
            case Finger.thumbFinger:
                if (current.thumb) { return false; }
                return current.thumb != previous.thumb;

            case Finger.indexFinger:
                if (current.indexFinger) { return false; }
                return current.indexFinger != previous.indexFinger;

            case Finger.middleFinger:
                if (current.middleFinger) { return false; }
                return current.middleFinger != previous.middleFinger;

            case Finger.pinkyFinger:
                if (current.pinkyFinger) { return false; }
                return current.pinkyFinger != previous.pinkyFinger;

            case Finger.ringFinger:
                if (current.ringFinger) { return false; }
                return current.ringFinger != previous.ringFinger;
        }
        return false;
    }
}
