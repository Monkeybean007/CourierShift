using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushableObstacle : MonoBehaviour
{
    private Rigidbody rb;
    public float basePushForce = 5f; // force applied by player

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Scale mass based on object size so bigger = harder to push
        Vector3 size = transform.localScale;
        rb.mass = Mathf.Max(1f, size.x * size.y * size.z);

        rb.linearDamping = 1f;
        rb.angularDamping = 1f;
    }

    public void Push(Vector3 direction)
    {
        float force = basePushForce / rb.mass;
        rb.AddForce(direction.normalized * force, ForceMode.Impulse);
    }
}
