using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WIM_IDHandler : MonoBehaviour {

	public int ID = 0;  // Acts as the key identifier

    private List<ObjectID> objectIDs = new List<ObjectID>();


	public void addID(GameObject obj) {
        ObjectID objectID = obj.AddComponent<ObjectID>();
        objectID.ID = ID++;

        if (obj.GetComponent<Rigidbody>() != null && 
            obj.GetComponent<Rigidbody>().isKinematic == false)
        {
            objectID.movableObject = true;
        }
	}

    public void removeID(int ID)
    {
        return;
    }

    public GameObject FindRealObject(GameObject cloneObject)
    {
        foreach( ObjectID id in objectIDs)
        {
            if (id.clonedObject == cloneObject)
            {
                return id.realObject;
            }
        }
        return null;
    }

    public GameObject FindCloneObject(GameObject realObject)
    {
        foreach (ObjectID id in objectIDs)
        {
            if (id.realObject == realObject)
            {
                return id.clonedObject;
            }
        }
        return null;
    }

    public ObjectID FindObjectIDWith(GameObject realObject)
    {
        foreach (ObjectID id in objectIDs)
        {
            if (id.realObject == realObject)
            {
                return id;
            }
        }
        return null;
    }

    public void SetClone(GameObject realObj, GameObject clone)
    {
        ObjectID id = FindObjectIDWith(realObj);
        id.clonedObject = clone;

        ObjectID toSet = clone.AddComponent<ObjectID>();
        toSet.ID = id.ID;
        toSet.movableObject = id.movableObject;
        toSet.realObject    = id.realObject;
        toSet.clonedObject  = id.clonedObject;
    }
}
