using UnityEngine;

public class PRISMRotation : Technique {
    private Quaternion Q_curr;
    private Quaternion Q_prev;

    // keeping track of time passed resets every 200ms
    private float timePassedTracker = 0;   // Seconds
    public float  delayTime = 0.200f;      // Seconds

    public float minS = 0.015f; // Degrees per second
    public float maxS = 2f;     // Degrees per second
    public float scalingConstant = 0.5f;    // Degrees per second

    void Awake()
    {
        FindEventSystem();
        transform.parent = trackedObj;
    }
       
    // Update is called once per frame
    void Update() {
        UpdateLastRotation();
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
            onSelectObject.Invoke();

            Q_prev = trackedObj.transform.rotation;
        }
    }

    public override void ReleaseObject()
    {
        if (selected)
        {
            Rigidbody body = selectedObject.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.isKinematic = false;
            }
            onDropObject.Invoke();
            selected = false;
            selectedObject = null;
        }
    }



    // Only updates if millisecondDelayTime (500ms) has passed
    private void UpdateLastRotation() {
        timePassedTracker += Time.deltaTime;

        if (timePassedTracker >= delayTime)
        {
            Q_curr = trackedObj.transform.rotation;
            PrismRotate();
            timePassedTracker = 0;
            Q_prev = Q_curr;
        }
    }

    private void PrismRotate() {
        if (selected) {
            Quaternion Q_obj  = selectedObject.transform.rotation;

            Quaternion Qdif = Q_curr * Quaternion.Inverse(Q_prev);

            float A  = Mathf.Acos(Qdif.w) * 360 / Mathf.PI;
            float RS = A / delayTime;

            float k = CalculateK(RS);

            Quaternion newRot = Power(Qdif, k) * Q_obj;

            selectedObject.transform.rotation = newRot;
        }
    }

    private float CalculateK(float rS)
    {
        if (rS <= minS)
            return 0;
        else if (scalingConstant <= rS)
            return 1;
        else
            return rS / scalingConstant;
    }


    private Quaternion Power(Quaternion quaternion, float power) {
        float w = quaternion.w;
        Vector3 xyz = new Vector3(quaternion.x, quaternion.y, quaternion.z);

        float r = xyz.magnitude;
        float t = r > 0.00001f ? Mathf.Atan2(r, w) / r : 0f;

        w = 0.5f * Mathf.Log(w * w + xyz.sqrMagnitude) * power;
        xyz *= t * power;

        r = xyz.magnitude;
        float et = Mathf.Exp(w);
        float s = r >= 0.00001f ? et * Mathf.Sin(r) / r : 0f;
        w = et * Mathf.Cos(r);
        xyz *= s;

        return new Quaternion(xyz.x, xyz.y, xyz.z, w);
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