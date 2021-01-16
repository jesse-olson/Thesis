using UnityEngine;
using Valve.VR;

public class GoGo : Technique {
    private Camera playerCamera; // Center Camera of the HMD

    // If toggled on the user can press down on the touchpad with their arm extended to take a measurement of the arm
    // If it is off the user must inut a manual estimate of what the users arm length would be
    public bool armLengthCalculator;

    public float  armLength      = 0.66f;    // Unless manually entered should be measured with fully extended arm
    public float  armLengthCoeff = 2.0f / 3; // The scaling factor for when the technique takes effect
    private float boundaryDistance;

    public float  extensionVariable = 10f;        // This variable in the equation controls the multiplier for how far the arm can extend with small movements
    private readonly float distanceFromHeadToChest = 0.3f;// Estimation of the distance from the users headset to their chest area

    private bool ghostSet;
    public GameObject theModel; // The model of the controller that will be shadowed for GoGo use

    Vector3 chestPosition;

    // Use this for initialization
    void Start()
    {
        FindEventSystem();
        boundaryDistance = armLengthCoeff * armLength;

        transform.parent = head.transform;
        if (Camera.main != null)
        {
            playerCamera = Camera.main;
        }
        else
        {
            playerCamera = head.GetComponentInChildren<Camera>();
        }

        MakeModelChild();
    }

    // Update is called once per frame
    void Update()
    {
        MakeModelChild();

        //Renderer[] renderers = transform.parent.GetComponentsInChildren<Renderer>();
        //foreach (Renderer _renderer in renderers)
        //{
        //    if (_renderer.material.name == "Standard (Instance)")
        //    {
        //        _renderer.enabled = true;
        //    }
        //}

        MoveControllerForward();
    }


    void MakeModelChild()
    {
        if (!ghostSet)
        {
            // The steamVR_RenderModel is generated after code start so we cannot parent right away or it wont generate. 
            if (theModel.GetComponent<SteamVR_RenderModel>() != null)
            {
                if (theModel.transform.childCount > 0)
                {
                    theModel.transform.SetParent(trackedObj);
                    ghostSet = true;
                }
            }
            else
            {
                // If it is just a custom model we can immediately parent
                theModel = Instantiate(theModel) as GameObject;
                theModel.transform.SetParent(trackedObj);
                theModel.GetComponent<GoGoShadow>().gogo = this;
                ghostSet = true;
            }
            /* 
            * Due to the transfer happening at a random time down the line 
            * we need to re-align the model inside the shadow controller to 0 
            * so nothing is wonky.
            */
            theModel.transform.localPosition = Vector3.zero;
            theModel.transform.localRotation = Quaternion.identity;
        }
    }

    // Might have to have a manual calibration for best use
    float GetDistanceToExtend()
    {
        /* Estimating chest position using an assumed distance from head to chest 
         * and then going that distance down the down vector of the camera. 
         * This will not allways be optimal especially when leaning is involved.
         * To improve gogo to suit your needs all you need to do is implement 
         * your own algorithm to estimate chest (or shoulder for greater
         * accuracy) position and set the chest position vector to match it
         */
        Vector3 down = -1 * playerCamera.transform.up;     // Getting the down vector (already normalized)
        Vector3 chestPos = down * distanceFromHeadToChest;

        chestPosition = playerCamera.transform.position + chestPos;

        float chestToRemoteDist = Vector3.Distance(trackedObj.position, chestPosition);

        float distancePastBoundary = chestToRemoteDist - boundaryDistance;

        return NonLinearFunction(Mathf.Max(0, distancePastBoundary));
    }



    private float NonLinearFunction(float pastBounds)
    {
        return extensionVariable * pastBounds * pastBounds;
    }



    void MoveControllerForward()
    {
        float distanceToExtend = GetDistanceToExtend();
        Vector3 forwardDistance = distanceToExtend * Vector3.forward;

        theModel.transform.localPosition = forwardDistance;
    }


    void OnEnable() {
        var _renderer = SteamVR_Render.instance;
        if (_renderer == null) {
            enabled = false;
            return;
        }
    }

    public void SetHighlighted(Collider col)
    {
        if (!highlighted &&
            1 << col.gameObject.layer == interactionLayers)
        {
            highlighted = true;
            highlightedObject = col.gameObject;
            onHover.Invoke();
        }
    }

    public void RemoveHighlighted(Collider col)
    {
        if (highlighted &&
            col.gameObject == highlighted)
        {
            onUnhover.Invoke();
            highlighted = false;
            highlightedObject = null;
        }
    }



    private void PickUpObject() {
        var joint = AddFixedJoint();
        joint.connectedBody = selectedObject.GetComponent<Rigidbody>();
    }

    private FixedJoint AddFixedJoint() {
        FixedJoint fx = theModel.AddComponent<FixedJoint>();
        fx.breakForce  = float.PositiveInfinity;
        fx.breakTorque = float.PositiveInfinity;
        fx.enableCollision = false;
        return fx;
    }

    public override void SelectObject()
    {
        if (selected || !highlighted) return;

        selected = true;
        selectedObject = highlightedObject;

        if (interactionType == InteractionType.Selection)
        {
            // Selection
            print("selected " + selectedObject);
        }
        else
        {
            // Manipulation
            PickUpObject();
        }

        onSelectObject.Invoke();
    }

    public override void ReleaseObject()
    {
        if (!selected) return;

        if (interactionType != InteractionType.Selection)
        {
            var joint = theModel.GetComponent<FixedJoint>();
            if (joint != null)
            {
                joint.connectedBody = null;
                Destroy(joint);
                Rigidbody selectedBody = selectedObject.GetComponent<Rigidbody>();

#if SteamVR_Legacy
            selectedBody.velocity        = Controller.velocity;
            selectedBody.angularVelocity = Controller.angularVelocity;
#elif SteamVR_2
            selectedBody.velocity        = trackedObj.GetVelocity();
            selectedBody.angularVelocity = trackedObj.GetAngularVelocity();
#endif
            }
        }
        onDropObject.Invoke();
        selected = false;
        selectedObject = null;
    }

    protected override void Enable()
    {
        throw new System.NotImplementedException();
    }

    protected override void Disable()
    {
        throw new System.NotImplementedException();
    }
}
