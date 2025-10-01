using UnityEngine;

public class PlayerNoise : MonoBehaviour
{
    public float noiseInterval = 1f;
    private float noiseTimer = 0f;


void Update()
    {
        // Footstep noise
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            noiseTimer -= Time.deltaTime;
            if (noiseTimer <= 0f)
            {
                MakeNoise();
                noiseTimer = noiseInterval;
            }
        }
    }

    public void MakeNoise()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, 15f);
        foreach (Collider col in enemies)
        {
            EnemyAI enemy = col.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.HearNoise(transform.position);
            }
        }
    }

}
