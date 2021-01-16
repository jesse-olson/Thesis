using UnityEngine;

public class MovingObject : MonoBehaviour {
    private bool moving = true;
    Hook[] allHooks;

    public float speed = 5.0f;
    public float bounds = 25.0f; 

    void Update()
    {
        if (moving) {
            transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
        }

        Vector3 pos = transform.position;
        if(Mathf.Abs(pos.x) > bounds || Mathf.Abs(pos.y) > bounds || Mathf.Abs(pos.z) > bounds) {
            Destroy(gameObject);
        }
        
    }

    void Start() {
        allHooks = FindObjectsOfType<Hook>();
		foreach(Hook hook in allHooks) {
			hook.AddSpawnedObjectToHook(gameObject);
            hook.onSelectObject.AddListener(StopMovingOnSelection);
		}
    }

    void StopMovingOnSelection() {
        foreach(Hook hook in allHooks) {
			if(hook.selectedObject == gameObject) {
                moving = false;
            }
		}      
    }
}
