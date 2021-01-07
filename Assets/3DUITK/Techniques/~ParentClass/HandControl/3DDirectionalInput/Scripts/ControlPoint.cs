using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPoint : MonoBehaviour
{
    private Vector3 output;

    private void OnTriggerEnter(Collider other)
    {
        output = other.transform.localPosition;
    }

    private void OnTriggerStay(Collider other)
    {
        output = other.transform.localPosition;
    }

    private void OnTriggerExit(Collider other)
    {
        output = Vector3.zero;
    }

    public void ResetOutput()
    {
        output = Vector3.zero;
    }

    public Vector3 GetOutput()
    {
        return output;
    }
}
