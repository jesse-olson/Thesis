using UnityEngine;

public class PRISMMovement : Technique
{
    public bool independentAxisScaling = true;

    // Track last position of controller to get the direction it is moving
    private Vector3 currPos;
    private Vector3 prevPos;

    // keeping track of time passed resets every 500ms
    private float timePassedTracker = 0;    //Seconds
    public float delayTime = 0.020f;        // Seconds

    public float minS = 0.01f;  // Units per second
    public float maxS = 2.0f;   // Units per second
    public float scaledConstant = 0.5f; // Units per second

    // OFFSET RECOVERY VARIABLES
    private float startTimeExceedMaxS = float.MaxValue;

    // Use this for initialization
    protected void Awake()
    {
        FindEventSystem();
        transform.parent = trackedObj;
    }

    // Update is called once per frame
    protected void Update()
    {
        UpdateLastPosition();
    }


    public void OnTriggerEnter(Collider other)
    {
        SetHighlighted(other);
    }

    public void OnTriggerStay(Collider other)
    {
        SetHighlighted(other);
    }

    public void OnTriggerExit(Collider other)
    {
        RemoveHighlighted(other);
    }


    private void SetHighlighted(Collider col)
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


    public override void SelectObject()
    {
        if (selected) return;
        else if (highlighted)
        {
            selected = true;
            selectedObject = highlightedObject;
            Rigidbody body = selectedObject.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.isKinematic = true;
            }
            timePassedTracker = 0;
            prevPos = trackedObj.position;
            onSelectObject.Invoke();
        }
    }


    public override void ReleaseObject()
    {
        if (!selected) return;

        Rigidbody body = selectedObject.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.isKinematic = false;
        }
        onDropObject.Invoke();
        selected = false;
        selectedObject = null;

    }




    // Only updates if delayTime (default 0.500s) has passed
    private void UpdateLastPosition()
    {
        timePassedTracker += Time.deltaTime;

        if (timePassedTracker >= delayTime)
        {
            currPos = trackedObj.position;
            PrismMovement();
            timePassedTracker = 0;
            prevPos = currPos;
        }
    }


    private void PrismMovement()
    {
        if (selected)
        {
            Vector3 velocity = (currPos - prevPos) / timePassedTracker;
            QuestDebug.Instance.Log(velocity.ToString());
            Vector3 translateTo = ScaledVelocity(velocity) * timePassedTracker;

            selectedObject.transform.Translate(translateTo, Space.World);

            OffsetRecovery(velocity);
        }
    }

    // Offset recovery as specified by paper Time.time is in seconds
    private void OffsetRecovery(Vector3 velocity)
    {
        if (velocity.magnitude <= maxS)
        {
            startTimeExceedMaxS = float.MaxValue;
            return;
        }
        else
        {
            float deltaTime = Time.time - startTimeExceedMaxS;
            float offset = 0.8f;    // will recover offset by making offset 80% of itself 

            if (deltaTime < 0)
                startTimeExceedMaxS = Time.time;
            else if (0.5f <= deltaTime && deltaTime < 1.0f)
                offset = 0.5f;      // will recover offset by making offset 50% of itself
            else
                offset = 0.0f;      // will completely recover offset making it 0

            Vector3 dir = (currPos - selectedObject.transform.position);
            selectedObject.transform.position += offset * dir;
        }
    }

    private Vector3 ScaledVelocity(Vector3 input)
    {
        if (independentAxisScaling)
            return new Vector3(CalculateK(input.x) * input.x, CalculateK(input.y) * input.y, CalculateK(input.z) * input.z);
        else
            return CalculateK(input.magnitude) * input;
       
    }

    private float CalculateK(float mag)
    {
        if (mag <= minS)
            return 0;
        else if (scaledConstant < mag)
            return 1;
        else  // minS < mag <= scaledConstant
            return scaledConstant;
    }

    protected override void Enable()
    {
        GetComponent<Rigidbody>().WakeUp();
    }

    protected override void Disable()
    {
        GetComponent<Rigidbody>().Sleep();
        if (selected) ReleaseObject();
        if (highlighted) HighlightObject(null);
    }
}
