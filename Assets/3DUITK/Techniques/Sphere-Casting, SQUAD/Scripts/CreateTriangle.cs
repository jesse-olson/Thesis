using UnityEngine;

public class CreateTriangle : MonoBehaviour {
    private static float cos45 = Mathf.Cos(Mathf.PI / 4);
    readonly Vector3[] vertices = {
        new Vector3(0,0,0),
        new Vector3( cos45, cos45, 0),
        new Vector3(-cos45, cos45, 0)
    };
    readonly int[] triangles = { 0, 1, 2 };

    public Material material;
    public GameObject cameraHead;

    // Use this for initialization
    void Start () {
        GameObject newTriangle = new GameObject();

        MeshFilter   _filter = newTriangle.AddComponent<MeshFilter>();
        MeshRenderer _renderer = newTriangle.AddComponent<MeshRenderer>();
        MeshCollider _collider = newTriangle.AddComponent<MeshCollider>();

        _renderer.material = material;
        _collider.convex = true;

        Mesh mesh = new Mesh()
        {
            vertices = vertices,
            triangles = triangles
        };

        _filter.mesh = mesh;

        GameObject triangle = Create(0, newTriangle);
        triangle.transform.name = "North";

        triangle = Create(1, newTriangle);
        triangle.transform.name = "West";

        triangle = Create(2, newTriangle);
        triangle.transform.name = "South";

        triangle = Create(3, newTriangle);
        triangle.transform.name = "East";

		transform.SetParent(cameraHead.transform);
        transform.localRotation = Quaternion.identity;
		transform.localPosition = new Vector3 (0f, 0f, 1f);
		gameObject.SetActive(false);
    }

    public GameObject Create(int i, GameObject triangle)
    {
        return Instantiate(triangle, Vector3.zero, Quaternion.AngleAxis(45 + i * 90, Vector3.forward), transform) as GameObject;
    }
}
