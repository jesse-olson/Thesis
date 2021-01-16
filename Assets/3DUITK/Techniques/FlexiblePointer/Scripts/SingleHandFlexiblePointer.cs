using UnityEngine;

public class SingleHandFlexiblePointer : Technique
{
    [Range(2, 30)]
    public float maxDistance = 2f;

    [Header("Control Point Options")]
    [Tooltip("Display the control point")]
    public bool visible = true;

[Range(0, 90)]
    [Tooltip("Max angle from forward (degrees)")]
    public float maxAngle = 45;
    //[Range(0, 1)]
    //[Tooltip("Max angle from forward (degrees)")]
    //public float distanceAlong = 0.5f;

    [Header("Prefabs")]
    public GameObject controlPointPrefab;
    private GameObject controlPoint;

    public GameObject laserPrefab;
    private GameObject laserParent;

    // Laser vars
    private static readonly int numOfLasers = 20;   // The number of segments that will make up the bezier curve
    private static readonly float bezierIncrement = 1.0f / numOfLasers;

    private Vector3 p0; // The point from which the curve starts
    private Vector3 p1; // Bezier curve control point
    private Vector3 p2; // The end of the bezier curve

    private bool castEnabled;

    // Use this for initialization
    void Awake()
    {
        InitializeLasers();

        controlPoint = Instantiate(controlPointPrefab) as GameObject;

        if (interactionType == InteractionType.Manipulation_UI)
        {
            gameObject.AddComponent<SelectionManipulation>();
            GetComponent<SelectionManipulation>().trackedObj = trackedObj.gameObject;
        }
        Disable();
    }

    private void Start()
    {
        FindEventSystem();
        SetControlPointVisibility();
        SetControlPoints();
    }

    // Update is called once per frame
    void Update()
    {
        if (castEnabled)
        {
            SetControlPoints();
            CastBezierRay();
        }
    }

    private void InitializeLasers()
    {
        laserParent = new GameObject
        {
            name = "LaserParent"
        };

        // Make the laser parts should all align well :)
        for (int i = 0; i < numOfLasers; i++)
        {
            GameObject laserPart = Instantiate(laserPrefab) as GameObject;
            laserPart.transform.SetParent(laserParent.transform);
        }
    }

    void SetControlPoints()
    {
        // Setting test points
        Vector3 controller1Pos = trackedObj.position;
        Vector3 controller1Fwd = trackedObj.forward;

        p0 = controller1Pos;
        p2 = maxDistance * controller1Fwd;

        OVRInput.RawAxis2D cont = hand == Hand.Left ?
            OVRInput.RawAxis2D.LThumbstick : OVRInput.RawAxis2D.RThumbstick;
        Vector2 j = OVRInput.Get(cont);

        Vector3 A = p2 / 2; // Midpoint between p0 and p2;
        float B_mag = Mathf.Tan(Mathf.Deg2Rad * maxAngle) * A.magnitude * j.magnitude;
        j *= B_mag;
        Vector3 C = A + j.x * trackedObj.right + j.y * trackedObj.up;

        p1 = C;
        controlPoint.transform.position = p0 + p1;
    }

    private void CastBezierRay()
    {
        Vector3 lastPos = p0;
        float currentBezierValue = bezierIncrement;

        bool didHit = false;
        GameObject hitObject = null;
        foreach (Transform laser in laserParent.transform)
        {
            laser.gameObject.SetActive(!didHit);
            if (!didHit)
            {
                Vector3 nextPos = GetBezierPosition(currentBezierValue);

                Vector3 dir = nextPos - lastPos;
                float dist = dir.magnitude;

                laser.position = lastPos;
                laser.LookAt(nextPos);
                laser.localScale = new Vector3(1, 1, dist);

                if (Physics.Raycast(lastPos, dir, out RaycastHit hit, dist, interactionLayers))
                {
                    hitObject = hit.transform.gameObject;
                    didHit = true;
                }

                lastPos = nextPos;
                currentBezierValue += bezierIncrement;
            }
        }
        HighlightObject(hitObject);
    }


    // t being betweek 0 and 1 to get a spot on the curve
    Vector3 GetBezierPosition(float t)
    {
        // P0 is always going to be (0, 0, 0) so it was removed
        return p0 + t * (2 * (1 - t) * p1 + t * p2);
    }


    // sets whether the user can see the control point. Will be called if user changes the bool variable setting
    private void SetControlPointVisibility()
    {
        controlPoint.GetComponent<MeshRenderer>().enabled = visible;
    }

    public override void SelectObject()
    {
        if (selected || !highlighted) return;

        selected = true;
        selectedObject = highlightedObject;

        switch (interactionType)
        {
            case InteractionType.Selection:
                break;

            case InteractionType.Manipulation_Movement:
            case InteractionType.Manipulation_Full:
                break;

            case InteractionType.Manipulation_UI:
                if (!GetComponent<SelectionManipulation>().inManipulationMode)
                    GetComponent<SelectionManipulation>().selectedObject = selectedObject;
                break;

            default:
                break;
        }
        onSelectObject.Invoke();
    }

    public override void ReleaseObject()
    {
        if (!selected) return;
        switch (interactionType)
        {
            case InteractionType.Selection:
                break;

            case InteractionType.Manipulation_Movement:
            case InteractionType.Manipulation_Full:
                break;

            case InteractionType.Manipulation_UI:
                break;

            default:
                break;
        }
        onDropObject.Invoke();
        selected = false;
        selectedObject = null;
    }


    protected override void Enable()
    {
        castEnabled = true;
        laserParent.SetActive(true);
        controlPoint.SetActive(true);
    }

    protected override void Disable()
    {
        castEnabled = false;
        laserParent.SetActive(false);
        controlPoint.SetActive(false);
        if (selected) ReleaseObject();
        if (highlighted) HighlightObject(null);
    }
}
