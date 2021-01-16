using UnityEngine;
using Valve.VR;

public class ISith: TwoHandTechnique
{
    private float prevM1 = 0;
    private float prevM2 = 0;

    public GameObject interactorPrefab;
    private GameObject interactor;

    private void Awake()
    {
        if (interactor == null)
            interactor = Instantiate(interactorPrefab, transform);
    }


    // Update is called once per frame
    void Update() {
        SetInteractorLocation();
    }

    void OnEnable() {
        var render = SteamVR_Render.instance;
        if (render == null) {
            enabled = false;
            return;
        }
    }

    void SetInteractorLocation()
    {
        Vector3 p1 = trackedObj.position;
        Vector3 d1 = trackedObj.forward;

        Vector3 p2 = secondaryTrackedObj.position;
        Vector3 d2 = secondaryTrackedObj.forward;

        /* As these two vectors will probably create skew lines (on different planes)
        // have to calculate the points on the lines that are closest to eachother
        // and then getting the midpoint between them giving a fake 'intersection'
        // This is achieved by utilizing parts of the fromula to find the shortest
        // distance between two skew lines */
        float m1 = prevM1;
        float m2 = prevM2;

        Vector3 n = Vector3.Cross(d1, d2);
        if (n.magnitude != 0)   // On the off chance that they are parallel
        {
            Vector3 n1 = Vector3.Cross(d1, n);
            Vector3 n2 = Vector3.Cross(d2, n);

            float _m1 = Vector3.Dot(p2 - p1, n2) / Vector3.Dot(d1, n2);
            float _m2 = Vector3.Dot(p1 - p2, n1) / Vector3.Dot(d2, n1);

            if (_m1 >= 0 && _m2 >= 0)
            {
                m1 = _m1;
                m2 = _m2;
            }
        }

        Vector3 c1 = p1 + m1 * d1;
        Vector3 c2 = p2 + m2 * d2;
        Vector3 midpoint = (c1 + c2) / 2;

        interactor.transform.position = midpoint;
        prevM1 = m1;
        prevM2 = m2;
    }


    public void SetCollidingObject(Collider other) {
        if (highlighted || !other.GetComponent<Rigidbody>() || interactionLayers != 1 << other.gameObject.layer) {
            return;
        }

        HighlightObject(other.gameObject);
    }

    public void RemoveCollidingObject(Collider other)
    {
        if (!highlighted || 1 << other.gameObject.layer != interactionLayers)
        {
            return;
        }
        HighlightObject(null);
    }


    private void GrabObject() {
        var joint = AddFixedJoint();
        joint.connectedBody = selectedObject.GetComponent<Rigidbody>();
    }

    private FixedJoint AddFixedJoint() {
        FixedJoint fx = gameObject.AddComponent<FixedJoint>();
        fx.breakForce = Mathf.Infinity;
        fx.breakTorque = Mathf.Infinity;
        return fx;
    }

    public override void SelectObject()
    {
        if (selected || !highlighted) return;
        selected = true;
        selectedObject = highlightedObject;
        onSelectObject.Invoke();
        switch (interactionType)
        {
            default:
                GrabObject();
                break;
        }
    }

    public override void ReleaseObject() {
        if (!selected) return;
        FixedJoint joint = GetComponent<FixedJoint>();
        if(joint != null) {
            joint.connectedBody = null;
            Destroy(joint);
#if SteamVR_Legacy
            objectInHand.GetComponent<Rigidbody>().velocity = Controller.velocity;
            objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
#elif SteamVR_2
            objectInHand.GetComponent<Rigidbody>().velocity = trackedObj.GetVelocity();
            objectInHand.GetComponent<Rigidbody>().angularVelocity = trackedObj.GetAngularVelocity();
#endif
        }
        onDropObject.Invoke();
        selected = false;
        selectedObject = null;
    }

    protected override void Enable()
    {
        //throw new System.NotImplementedException();
    }

    protected override void Disable()
    {
        //throw new System.NotImplementedException();
    }


    public void SetInteractor(GameObject interactor)
    {
        this.interactor = interactor;
    }
}
