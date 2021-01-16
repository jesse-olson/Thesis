using UnityEngine;

public abstract class TwoHandTechnique : Technique
{
    protected Transform secondaryTrackedObj;

    public void SetSecondaryTrackedObject(Transform transform)
    {
        secondaryTrackedObj = transform;
    }
}
