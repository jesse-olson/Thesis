using UnityEngine;
using Valve.VR;

[ExecuteInEditMode]
public class ISithController : TwoHandController {
    public enum SelectionController {
        LeftController,
        RightController
    }

    public GameObject laserPrefab;

    //public SelectionController selectionController = SelectionController.RightController;

    //public float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up) // change to own code
    //{
    //    Vector3 perp = Vector3.Cross(fwd, targetDir);
    //    float dir = Vector3.Dot(perp, up);

    //    if (dir > 0.0f) return 1.0f;
    //    if (dir < 0.0f) return -1.0f;
    //    return 0.0f;
    //}

    protected override void SetupController()
    {
        //technique = GetComponent<TwoHandTechnique>();
        Instantiate(laserPrefab, leftController.transform);
        Instantiate(laserPrefab, rightController.transform);
    }

    protected override void SetupTechnique()
    {
        base.SetupTechnique();
        //((ISith)technique).SetInteractor()
    }
}
