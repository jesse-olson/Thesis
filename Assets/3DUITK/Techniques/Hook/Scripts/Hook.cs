using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Valve.VR;

// Information on hook technique
// http://www.eecs.ucf.edu/isuelab/publications/pubs/Cashion_Jeffrey_A_201412_PhD.pdf pf 13

public class Hook : Technique
{
    //  Used lists instead of arrays incase we want future optimization where it dynamically changes
    //  Instead of looping through every object in the scene
    private List<HookObject> nearbyObjects = new List<HookObject>();
    public bool checkForNewlySpawnedObjects = true;

    // Use this for initialization
    void Start()
    {
        FindEventSystem();
        PopulateGameObjectList();

        // Controller needs a rigidbody to grab objects in hook
        Rigidbody body = trackedObj.gameObject.AddComponent<Rigidbody>();
        body.isKinematic = true;
    }


    // Update is called once per frame
    void Update()
    {
        UpdateHookList();
        HighlightObject(nearbyObjects[0].ContainingObject);
    }

    // Currently of O(n^3)
    // TODO: need to figure out a way to reduce this
    private void UpdateHookList()
    {
        List<HookObject> newList = new List<HookObject>();

        // Remove any null objects from list if they were destroyed
        foreach (HookObject hookObject in nearbyObjects)
        {
            if (hookObject.CheckStillExists())
            {
                hookObject.SetDistance(trackedObj);
                newList.Add(hookObject);
            }
        }

        // Reordering all the nearbyobjects by their newly set distance
        nearbyObjects = newList.OrderBy(w => w.GetDistance()).ToList();

        // now increase or decrease score
        int increasable = nearbyObjects.Count / 3;
        for (int i = 0; i < nearbyObjects.Count; i++)
        {
            HookObject hookObject = nearbyObjects[i];
            if (i <= increasable)
                hookObject.IncreaseScore();
            else
                hookObject.DecreaseScore();
        }
    }

    private void PopulateGameObjectList()
    {
        foreach (GameObject gameObject in FindObjectsOfType<GameObject>())
        {
            AddToHookList(gameObject);
        }
    }

    private bool AddToHookList(GameObject toAdd)
    {
        if(interactionLayers == 1 << toAdd.layer)
        {
            nearbyObjects.Add(new HookObject(toAdd));
            return true;
        }
        return false;
    }

    // If for example new objects are spaned to the scene the user can access the hook with that object and add that object to the hook with this method
    public bool AddSpawnedObjectToHook(GameObject newObject)
    {
        return AddToHookList(newObject);
    }



    private void GrabObject()
    {
        selectedObject.transform.position = trackedObj.position;
        var joint = AddFixedJoint();
        joint.connectedBody = selectedObject.GetComponent<Rigidbody>();
    }


    private FixedJoint AddFixedJoint()
    {
        FixedJoint fx = trackedObj.gameObject.AddComponent<FixedJoint>();
        fx.breakForce = Mathf.Infinity;
        fx.breakTorque = Mathf.Infinity;
        return fx;
    }



    public override void ReleaseObject()
    {
        if (!selected) return;
        FixedJoint joint = trackedObj.gameObject.GetComponent<FixedJoint>();
        if (joint != null)
        {
            joint.connectedBody = null;
            Destroy(joint);
            selectedObject.GetComponent<Rigidbody>().useGravity = true;
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

    public override void SelectObject()
    {
        if (selected || !highlighted) return;

        selected = true;
        selectedObject = highlightedObject;

        switch (interactionType) {
            case InteractionType.Selection:
                break;

            case InteractionType.Manipulation_Movement:
            case InteractionType.Manipulation_Full:
                GrabObject();
                break;

            default:
                break;
        }
        onSelectObject.Invoke();
    }

    protected override void Enable()
    {
        //throw new System.NotImplementedException();
    }

    protected override void Disable()
    {
        //throw new System.NotImplementedException();
    }
}
