using UnityEngine;

public class Package : MonoBehaviour
{
    public float weight = 1f;
    public int payout = 10;

    void Start()
    {
        gameObject.tag = "Package"; // for pickup
    }
}
