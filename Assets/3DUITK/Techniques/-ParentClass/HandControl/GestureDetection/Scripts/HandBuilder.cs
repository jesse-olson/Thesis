using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandBuilder : MonoBehaviour
{
    public OVRSkeleton skeleton;
    public List<OVRBone> bones;

    private List<OVRSkeleton.BoneId> fingerTipID;

    public OVRHand.Hand handType;

    private Transform wrist;

    private Transform thumbTip;
    private Transform indexTip;
    private Transform middleTip;
    private Transform ringTip;
    private Transform pinkyTip;

    public GameObject fingerPrefab;
    public GameObject palmPrefab;

    private List<GameObject> fingerTips;

    void Awake()
    {
        fingerTipID = new List<OVRSkeleton.BoneId>();
        fingerTipID.Add(OVRSkeleton.BoneId.Hand_IndexTip);
        fingerTipID.Add(OVRSkeleton.BoneId.Hand_MiddleTip);
        fingerTipID.Add(OVRSkeleton.BoneId.Hand_PinkyTip);
        fingerTipID.Add(OVRSkeleton.BoneId.Hand_RingTip);
        fingerTipID.Add(OVRSkeleton.BoneId.Hand_ThumbTip);
    }

    // Start is called before the first frame update
    void Start()
    {
        bones = new List<OVRBone>(skeleton.Bones);


        wrist = this.transform.GetChild(0).GetChild(0);

        //GameObject wristPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube) as GameObject;
        //wristPrefab.transform.parent = wrist;
        //wristPrefab.transform.localScale = 0.05f * Vector3.one;
        //wristPrefab.transform.localPosition = Vector3.zero;
        //wristPrefab.transform.localRotation = Quaternion.identity;

        foreach(OVRSkeleton.BoneId id in fingerTipID)
        {
            GameObject tip = Instantiate(fingerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            FingerTip fingerTip = tip.GetComponent<FingerTip>();
            fingerTip.finger = Finger.indexFinger;

            tip.transform.SetParent(bones[(int)id].Transform);
        }

        //GameObject tip = Instantiate(fingerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        //FingerTip fingerTip = tip.GetComponent<FingerTip>();
        //fingerTip.finger = Finger.indexFinger;

        //tip.transform.parent = wrist.GetChild(1).GetChild(0).GetChild(0).GetChild(2);
        //tip.transform.localPosition = Vector3.zero;
        //tip.transform.localRotation = Quaternion.identity;

        //tip = Instantiate(fingerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        //fingerTip = tip.GetComponent<FingerTip>();
        //fingerTip.finger = Finger.middleFinger;

        //tip.transform.parent = wrist.GetChild(2).GetChild(0).GetChild(0).GetChild(2);
        //tip.transform.localPosition = Vector3.zero;
        //tip.transform.localRotation = Quaternion.identity;


        //indexTip    = wrist.GetChild(1).GetChild(0).GetChild(0).GetChild(2);
        //middleTip   = wrist.GetChild(2).GetChild(0).GetChild(0).GetChild(2);
        //ringTip     = wrist.GetChild(4).GetChild(0).GetChild(0).GetChild(2);
        //pinkyTip    = wrist.GetChild(3).GetChild(0).GetChild(0).GetChild(2);

        fingerTips = new List<GameObject>();

        //GameObject palm = Instantiate(palmPrefab) as GameObject;

        if(handType == OVRHand.Hand.HandLeft)
        {

        }else if(handType == OVRHand.Hand.HandRight)
        {

        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
