using UnityEngine;

[ExecuteInEditMode]
public class ScaledWorldGrabController : BothHandController
{
    protected override void SetupTechnique()
    {
        base.SetupTechnique();
        ((ScaledWorldGrab)leftTechnique) .cameraRig = GetComponent<OVRCameraRig>().gameObject;
        ((ScaledWorldGrab)rightTechnique).cameraRig = GetComponent<OVRCameraRig>().gameObject;
    }
}

