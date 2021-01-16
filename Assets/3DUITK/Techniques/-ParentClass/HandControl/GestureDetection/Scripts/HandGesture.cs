using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct HandGesture
{
    public bool indexFinger;
    public bool middleFinger;
    public bool ringFinger;
    public bool pinkyFinger;
    public bool thumb;

    public UnityEvent OnGesture;
    public UnityEvent OffGesture;
    
    public void Set(HandGesture toSet)
    {
        this.thumb          = toSet.thumb;
        this.indexFinger    = toSet.indexFinger;
        this.middleFinger   = toSet.middleFinger;
        this.ringFinger     = toSet.ringFinger;
        this.pinkyFinger    = toSet.pinkyFinger;
    }

    public HandGesture SetAll(bool toSet)
    {
        this.thumb =         toSet;
        this.indexFinger =   toSet;
        this.middleFinger =  toSet;
        this.ringFinger =    toSet;
        this.pinkyFinger =   toSet;

        return this;
    }

    public bool Equals(HandGesture toComp)
    {
        bool toReturn = this.thumb == toComp.thumb;
        toReturn &= (this.indexFinger == toComp.indexFinger);
        toReturn &= (this.middleFinger == toComp.middleFinger);
        toReturn &= (this.ringFinger == toComp.ringFinger);
        toReturn &= (this.pinkyFinger == toComp.pinkyFinger);
        return toReturn;
    }

    public new string ToString()
    {
        string toReturn = "Index: " + this.indexFinger + "\n" +
            "Middle: " + this.middleFinger + "\n" +
            "Ring: " + this.ringFinger + "\n" +
            "Pinky: " + this.pinkyFinger + "\n";
        return toReturn;
    }
}
