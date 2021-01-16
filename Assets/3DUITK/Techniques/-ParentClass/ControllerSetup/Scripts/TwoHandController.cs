using UnityEngine;

[ExecuteInEditMode]
public class TwoHandController : Controller
{
    public ControllerPicked dominantHand = ControllerPicked.Right_Controller;
    public TwoHandTechnique technique;

    protected override void SetupController()
    {
        //technique = GetComponent<TwoHandTechnique>();
    }

    protected override void SetupTechnique()
    {
        technique.SetControllerPicked(dominantHand);

        technique.SetHead(head.transform);

        switch (dominantHand)
        {
            case ControllerPicked.Left_Controller:
                technique.SetPrimaryTrackedObject(leftController.transform);
                technique.SetSecondaryTrackedObject(rightController.transform);
                break;

            case ControllerPicked.Right_Controller:
            default:
                technique.SetPrimaryTrackedObject(rightController.transform);
                technique.SetSecondaryTrackedObject(leftController.transform);
                break;
        }
    }
}
