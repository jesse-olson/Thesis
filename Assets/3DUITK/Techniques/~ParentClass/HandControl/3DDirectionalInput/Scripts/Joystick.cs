using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joystick : MonoBehaviour, IDirectionSelectable
{
    public enum ActiveAxis
    {
        X = 0x01,
        Y = 0x02,
        Z = 0x04,
        XY = X | Y,
        XZ = X | Z,
        YZ = Y | Z,
        XYZ = X | Y | Z
    }

    public ActiveAxis joystickDirections = ActiveAxis.X;
    public float deadzone = 40f;    // The percentage of the joystick that is deadzone

    private static readonly int controlPointIndex = 7;
    private static readonly int pointIndex = controlPointIndex + 1;
    private static readonly int stickIndex = pointIndex + 1;

    public GameObject controlPoint;
    public GameObject point;
    public GameObject stick;

    private Vector3 mask;
    
    // Start is called before the first frame update
    void Awake()
    {
        // Making the mask vector
        int xMask = (int)(ActiveAxis.X & joystickDirections);
        int yMask = (int)(ActiveAxis.Y & joystickDirections) >> 1;
        int zMask = (int)(ActiveAxis.Z & joystickDirections) >> 2;

        mask = new Vector3(xMask, yMask, zMask);

        // Getting all of the relevant child game objects
        //controlPoint = transform.GetChild(controlPointIndex).gameObject;
        //point        = transform.GetChild(pointIndex).gameObject;
        //stick        = transform.GetChild(stickIndex).gameObject;

        // Activating the child relating to the desired directions
        GameObject joystick = transform.GetChild((int)joystickDirections - 1).gameObject;
        joystick.SetActive(true);

        // Setting up the deadzone for the joystick
        Transform deadzoneTransform = joystick.transform.GetChild(0);
        switch(xMask + yMask + zMask)
        {
            case 1:
                deadzoneTransform.localScale = new Vector3(1.01f, deadzone, 1.01f);
                break;
            case 2:
                deadzoneTransform.localScale = new Vector3(deadzone, 1.01f, deadzone);
                break;
            default:
                deadzoneTransform.localScale = new Vector3(deadzone, deadzone, deadzone);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Get the position of the local position of the control point
        Vector3 controllerPos = controlPoint.transform.position - transform.position;

        // As the Joystick's global rotation can be changed we need to rotate the 
        // controllerPos in the opposite direction.
        transform.rotation.ToAngleAxis(out float angle, out Vector3 axis);
        controllerPos = Quaternion.AngleAxis(-angle, axis) * controllerPos;

        Vector3 direction = Vector3.zero; // The direction of the point 
        for (int i = 0; i < 3; i++)
        {
            direction[i] = controllerPos[i] / transform.lossyScale[i]; // Perform scaling
            direction[i] = Mathf.Sign(direction[i]) * Mathf.Min(Mathf.Abs(direction[i]), 1);    // Make sure that the directions is of a value [-1, 1]
            direction[i] *= mask[i];    // Masks the Vector3 so that only the desired basis are returned
        }

        // This is only used to ensure that the users motion is limited to being circular/spherical 
        if (direction.magnitude > 1.0f)
        {
            direction = direction.normalized;
        }

        // Place the point
        point.transform.localPosition = direction;

        // Make the visible cylinder connect
        stick.transform.LookAt(point.transform);
        stick.transform.localScale = new Vector3(1, 1, direction.magnitude);
    }

    public Vector3 GetVector()
    {
        return new Vector3(GetX(), GetY(), GetZ());
    }

    public float GetX()
    {
        return PerformCalculation(0);
    }
    public float GetY()
    {
        return PerformCalculation(1);
    }

    public float GetZ()
    {
        return PerformCalculation(2);
    }

    private float PerformCalculation(int index)
    {
        if (mask[index] == 0)
        {
            return 0;
        }
        float pos = point.transform.localPosition[index];
        return Mathf.Sign(pos) * (Mathf.Max((Mathf.Abs(pos) - deadzone), 0f) / (1 - deadzone));
    }

    public void SetControlPointParent(Transform parent)
    {
        controlPoint.transform.SetParent(parent);
    }

    public void ResetVector()
    {
        point.transform.localPosition = Vector3.zero;
    }
}
