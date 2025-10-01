using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Score")]
    public int score;
    public TextMeshProUGUI scoreText;

    [Header("Packages")]
    public int totalPackagesNeeded = 5;   // total packages needed for level
    private int collectedPackages = 0;    // how many packages player has collected

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Existing score function
    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
    }

    // --- NEW: Call this when a package is collected
    public void AddPackage(int payout = 0)
    {
        collectedPackages += 1;

        // Optionally add payout to score
        if (payout > 0)
            AddScore(payout);
        else
            UpdateUI();

        Debug.Log("Package collected! Total = " + collectedPackages);

        if (collectedPackages >= totalPackagesNeeded)
        {
            Debug.Log("All packages collected! You win!");
            // TODO: Trigger victory screen here
        }
    }

    // --- NEW: EnemyAI will use this
    public int GetCollectedPackages()
    {
        return collectedPackages;
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score + " | Packages: " + collectedPackages + "/" + totalPackagesNeeded;
        }
    }
}
