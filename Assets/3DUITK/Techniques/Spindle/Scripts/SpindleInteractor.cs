using UnityEngine;

public class SpindleInteractor : MonoBehaviour {
    private Spindle spindle;

    void Awake()
    {
        spindle = GetComponentInParent<Spindle>();
    }

    public void OnTriggerEnter(Collider other)
    {
        spindle.SetHighlighted(other);
    }


    public void OnTriggerStay(Collider other)
    {
        spindle.SetHighlighted(other);
    }


    public void OnTriggerExit(Collider other)
    {
        spindle.RemoveHighlighted(other);
    }
}
