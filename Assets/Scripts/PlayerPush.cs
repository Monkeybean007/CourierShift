using UnityEngine;
using TMPro; // Required for TextMeshPro

public class PlayerPush : MonoBehaviour
{
    [Header("Push Settings")]
    public float pushRange = 2f; // How close player needs to be
    public LayerMask pushableLayer; // Layer for obstacles
    public TextMeshProUGUI pushMessageText; // Use this for UI text under Canvas

    private PushableObstacle targetObstacle;

    void Update()
    {
        CheckForObstacle();

        // Push the obstacle when pressing F
        if (targetObstacle != null && Input.GetKeyDown(KeyCode.F))
        {
            Vector3 pushDir = transform.forward;
            targetObstacle.Push(pushDir);
        }
    }

    void CheckForObstacle()
    {
        // Raycast from player to detect pushable objects
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pushRange, pushableLayer))
        {
            targetObstacle = hit.collider.GetComponent<PushableObstacle>();
            if (targetObstacle != null)
            {
                // Show the message
                if (pushMessageText != null)
                    pushMessageText.gameObject.SetActive(true);
                return;
            }
        }

        targetObstacle = null;

        // Hide the message if no obstacle detected
        if (pushMessageText != null)
            pushMessageText.gameObject.SetActive(false);
    }
}
