using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DPadController : MonoBehaviour, IDirectionSelectable
{
    public GameObject x_axis, y_axis, z_axis;
    public GameObject controlPoint;

    public enum ActiveAxis
    {
        X   =   0x01,
        Y   =   0x02,
        Z   =   0x04,
        XY  =   X | Y,
        XZ  =   X | Z,
        YZ  =   Y | Z,
        XYZ =   X | Y | Z
    }

    public ActiveAxis activeAxis;

    private Vector3 output;


    private void Awake()
    {
        // Activate all of the directions that were requested.
        x_axis.SetActive((int)(activeAxis & ActiveAxis.X) > 0);
        y_axis.SetActive((int)(activeAxis & ActiveAxis.Y) > 0);
        z_axis.SetActive((int)(activeAxis & ActiveAxis.Z) > 0);
    }
    
    public void SetControlPointParent(Transform parent)
    {
        controlPoint.transform.SetParent(parent);
    }

    public float GetX()
    {
        return GetVector().x;
    }

    public float GetY()
    {
        return GetVector().y;
    }

    public float GetZ()
    {
        return GetVector().z;
    }

    public Vector3 GetVector()
    {
        return controlPoint.GetComponent<ControlPoint>().GetOutput();
    }

    public void ResetVector()
    {
        controlPoint.GetComponent<ControlPoint>().ResetOutput();
    }
}