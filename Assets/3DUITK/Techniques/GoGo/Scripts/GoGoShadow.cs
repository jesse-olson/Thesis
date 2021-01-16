using UnityEngine;

public class GoGoShadow : MonoBehaviour {
    public GoGo gogo;

    void Awake()
    {
        gogo = GetComponentInParent<GoGo>();
    }

    public void OnTriggerEnter(Collider other)
    {
        gogo.SetHighlighted(other);
    }


    public void OnTriggerStay(Collider other)
    {
        gogo.SetHighlighted(other);
    }


    public void OnTriggerExit(Collider other)
    {
        gogo.RemoveHighlighted(other);
    }
}
