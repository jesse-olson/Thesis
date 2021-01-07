using System;
using UnityEngine;

public class HandEventSystem : MonoBehaviour
{
    public HandEventSystem current;

    public GameObject trackedHand;

    private Vector3 origin;
    private Vector3 minRad;
    private Vector3 maxRad;


    void Awake()
    {
        current = this;
    }

    public event Action OnPalmTriggerEnter;

    public void PlamTriggerEnter()
    {
        if( OnPalmTriggerEnter != null)
        {
            OnPalmTriggerEnter();
        }
    }

    void Start()
    {

    }

    void Update()
    {

    }
}
