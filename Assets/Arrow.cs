using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;

    // Update is called once per frame
    void Update()
    {
        if (rb.linearVelocity.magnitude > 0.1f)
            transform.up = rb.linearVelocity.normalized;
    }
}
