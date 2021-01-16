using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookObject
{
    private int score;
    public GameObject ContainingObject { get; set; }
    private float distance;

    public HookObject(GameObject ContainingObject)
    {
        this.ContainingObject = ContainingObject;
        score = 0;
        distance = Mathf.Infinity;
    }


    public float GetDistance()
    {
        return distance;
    }

    public void SetDistance(Transform transform)
    {
        if (ContainingObject != null) {
            distance = Vector3.Distance(ContainingObject.transform.position, transform.position);
        }   
    }

    public bool CheckStillExists() {
        // checks if the gameobject this hookobject contains still exists
        return ContainingObject != null;
    }

    public int GetScore()
    {
        return score;
    }

    public void DecreaseScore()
    {
        score = --score < 0 ? 0 : score;
    }

    public void IncreaseScore()
    {
        score++;
    }

    public override bool Equals(object other)
    {
        GameObject compared = other as GameObject;
        return ContainingObject.Equals(compared);
    }

    public override int GetHashCode()
    {
        return 1162374466 + EqualityComparer<GameObject>.Default.GetHashCode(ContainingObject);
    }
}
