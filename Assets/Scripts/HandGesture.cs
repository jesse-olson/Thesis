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

    public UnityEvent OnGesture;
    public UnityEvent OffGesture;

    public bool Equals(HandGesture toComp)
    {
        bool toReturn = this.indexFinger == toComp.indexFinger;
        toReturn &= this.middleFinger == toComp.middleFinger;
        toReturn &= this.ringFinger == toComp.ringFinger;
        toReturn &= this.pinkyFinger == toComp.pinkyFinger;
        return toReturn;
    }
}
