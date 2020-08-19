using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PalmTrigger : MonoBehaviour
{
    public Collider palm;
    public List<HandGesture> handGestures = new List<HandGesture>();

    private HandGesture currentGesture;
    private bool hasChanged = false;

    
    private void OnTriggerEnter(Collider other)
    {
        hasChanged = true;
        String fingerName = other.name;
        SetFinger(fingerName, true);
    }

    private void OnTriggerExit(Collider other)
    {
        hasChanged = false;
        String fingerName = other.name;
        SetFinger(fingerName, false);
    }

    public void SetFinger(String name, bool inTrigger)
    {
        switch (name)
        {
            case "indexFinger":
                currentGesture.indexFinger = inTrigger;
                break;

            case "middleFinger":
                currentGesture.middleFinger = inTrigger;
                break;

            case "ringFinger":
                currentGesture.ringFinger = inTrigger;
                break;

            case "pinkyFinger":
                currentGesture.pinkyFinger = inTrigger;
                break;

            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (hasChanged)
        {
            foreach (HandGesture handGesture in handGestures)
            {
                if (currentGesture.Equals(handGesture))
                {
                    currentGesture.OnGesture.Invoke();
                    break;
                }
            }
            hasChanged = false;
        }
    }
}
