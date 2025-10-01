using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Package : MonoBehaviour
{
    public float weight = 1f;
    public int payout = 10;

    void Start()
    {
        gameObject.tag = "Package"; // ensures player/enemies recognize it
        // Make sure the collider is set to "Is Trigger" in the inspector
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddPackage(payout);
            }
            else
            {
                Debug.LogWarning("GameManager.Instance is null when collecting package.");
            }

            // Remove the package from the scene
            Destroy(gameObject);
        }
    }
}
