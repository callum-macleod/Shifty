using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerMovement : MonoBehaviour
{
    /*[SerializeField]*/ int speed = 5;
    /*[SerializeField]*/ float acceleration = 5f;
    /*[SerializeField]*/ float deceleration = 1f;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] InputActionReference move;
    [SerializeField] InputActionReference test;
    Vector2 inputDir = Vector2.zero;


    float momentum = 0f;
    bool moving = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    void FixedUpdate()
    {
        Vector2 newSpeed = inputDir != Vector2.zero
            ? (rb.linearVelocity) * 0.25f + (speed * inputDir) * 0.75f  // acceleration
            : (rb.linearVelocity) * 0.75f + Vector2.zero;               // deceleration

        rb.linearVelocity = newSpeed;

        if (rb.linearVelocity.magnitude <= 0.01)
            rb.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        inputDir = move.action.ReadValue<Vector2>();
    }

    private void OnEnable()
    {

    }
}
