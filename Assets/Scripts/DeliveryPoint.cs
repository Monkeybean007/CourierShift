using UnityEngine;

public class DeliveryPoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Package pkg = other.GetComponent<Package>();
        if (pkg != null)
        {
            // Add score
            GameManager.Instance.AddScore(pkg.payout);
            Destroy(other.gameObject);
        }
    }
}
