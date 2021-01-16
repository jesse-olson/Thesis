using UnityEngine;

[ExecuteInEditMode]
public class BothHandController : Controller
{
    public ControllerPicked controller = ControllerPicked.Both;

    public Technique leftTechnique;
    public Technique rightTechnique;

    protected override void SetupController()
    {
        Technique[] techniques = GetComponentsInChildren<Technique>(true);

        leftTechnique  = techniques[0];
        rightTechnique = techniques[1];

        //leftTechnique.gameObject.transform.SetParent(leftController.transform);
        //rightTechnique.gameObject.transform.SetParent(rightController.transform);
    }

    protected override void SetupTechnique()
    {
        leftTechnique.SetPrimaryTrackedObject(leftController.transform);
        rightTechnique.SetPrimaryTrackedObject(rightController.transform);

        leftTechnique.SetControllerPicked(ControllerPicked.Left_Controller);
        rightTechnique.SetControllerPicked(ControllerPicked.Right_Controller);

        leftTechnique.SetHand(Technique.Hand.Left);
        rightTechnique.SetHand(Technique.Hand.Right);

        leftTechnique.SetHead(head.transform);
        rightTechnique.SetHead(head.transform);

        switch (controller)
        {
            case ControllerPicked.Left_Controller:
                leftTechnique.gameObject.SetActive(true);
                rightTechnique.gameObject.SetActive(false);
                break;

            case ControllerPicked.Right_Controller:
                leftTechnique.gameObject.SetActive(false);
                rightTechnique.gameObject.SetActive(true);
                break;

            case ControllerPicked.Both:
                leftTechnique.gameObject.SetActive(true);
                rightTechnique.gameObject.SetActive(true);
                break;

            default:
                leftTechnique.gameObject.SetActive(false);
                rightTechnique.gameObject.SetActive(false);
                break;
        }
    }
}
