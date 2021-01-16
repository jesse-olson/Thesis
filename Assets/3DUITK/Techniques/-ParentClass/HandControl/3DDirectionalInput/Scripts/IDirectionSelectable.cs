using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDirectionSelectable
{
    float GetX();

    float GetY();

    float GetZ();

    Vector3 GetVector();

    void ResetVector();

    void SetControlPointParent(Transform parent);
}
