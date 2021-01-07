using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Joystick3D : MonoBehaviour {
    public enum Direction { X, Y, Z, XY, XZ, ALL, NONE}

    public GameObject trackedHand;

    private Vector3 origin;
    private Vector3 currentPos;
    private Vector3 minRad = 0.2f * Vector3.one;
    private Vector3 maxRad = Vector3.one;


    void Awake()
    {
        //HandEventSystem.current;
    }

    void Start()
    {
        
    }

    void Update()
    {
        // Get the position in each direction
        pos = trackedHand.transform.position - origin;
        
        float posX = Math.Sign(posX)(Math.Abs(pos.x) - minRad.x) / (maxRad.x - minRad.x);
        float posY = Math.Sign(posY)(Math.Abs(pos.y) - minRad.y) / (maxRad.y - minRad.y);
        float posZ = Math.Sign(posZ)(Math.Abs(pos.z) - minRad.z) / (maxRad.z - minRad.z);
    }

    public Vector3 GetDirection(Direction direction)
    {
        Vector3 toReturn = currentPos;
    }

    private float Formula(float x)
    {
        float toReturn = x * x;
    }
}
