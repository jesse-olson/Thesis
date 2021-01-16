using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ISithInteractor : MonoBehaviour
{
    ISith iSith;

    private void Start()
    {
        if (iSith == null)
            iSith = GetComponentInParent<ISith>();
    }

    public void OnTriggerEnter(Collider other)
    {
        iSith.SetCollidingObject(other);
    }

    public void OnTriggerStay(Collider other)
    {
        iSith.SetCollidingObject(other);
    }

    public void OnTriggerExit(Collider other)
    {
        iSith.RemoveCollidingObject(other);
    }
}
