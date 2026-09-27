using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerMovement : MonoBehaviour
{
    int speed = 5;
    int slerpStrength = 5;
    Vector2 moveInput = Vector2.zero;
    float blinkDistance = 3f;
    bool sk8ing = false;
    float maxSk8Velocity = 30;
    float minSk8Velocity = 10;

    bool movementLockout = false;
    float movementLockoutStartTime = 0f;

    [SerializeField] Rigidbody2D rb;

    [SerializeField] InputActionReference move;
    [SerializeField] InputActionReference blink;
    [SerializeField] InputActionReference sk8;

    [SerializeField] GameObject ringEffect;
    [SerializeField] GameObject ball;
    [SerializeField] GameObject arrow;

    void FixedUpdate()
    {
        if (movementLockout)
        {
            MovementLockoutMovement();
            return;
        }

        if (!sk8ing)    BasicMovement();
        else            Sk8Movement();
    }


    // new fixed update idea
    // each movement method returns a vector as it's contribution to the new speed
    // then they are added together at the end and applied to the rb

    void BasicMovement()
    {
        Vector2 newSpeed = moveInput != Vector2.zero && !movementLockout
            ? (rb.linearVelocity) * 0.25f + (speed * moveInput) * 0.75f // acceleration
            : (rb.linearVelocity) * 0.75f;                              // deceleration

        rb.linearVelocity = newSpeed;

        if (rb.linearVelocity.magnitude <= 0.01)
            rb.linearVelocity = Vector2.zero;
    }

    void Sk8Movement()
    {
        if (moveInput != Vector2.zero)
        {
            float slerpStrengthModifier = (Vector2.Dot(rb.linearVelocity.normalized, moveInput.normalized) + 2) / 2;  // slerp softer when inputting opposite direction to current momentum
            Vector2 newDir = Vector3.Slerp(rb.linearVelocity.normalized, moveInput.normalized, slerpStrength * slerpStrengthModifier * Time.fixedDeltaTime);
            rb.linearVelocity = newDir * rb.linearVelocity.magnitude;

            if (rb.linearVelocity.magnitude < maxSk8Velocity)
            {
                if (rb.linearVelocity.magnitude < minSk8Velocity)
                    rb.linearVelocity += moveInput.normalized * 20f * Time.fixedDeltaTime;

                rb.linearVelocity *= 1 + .25f * Time.fixedDeltaTime;
            }
        }
    }


    void Update()
    {
        moveInput = move.action.ReadValue<Vector2>();
    }

    void DoBlink(InputAction.CallbackContext ctx)
    {
        Instantiate(ringEffect, transform.position, Quaternion.identity);
        transform.position += (Vector3) moveInput.normalized * blinkDistance;
    }
    void DoSk8(InputAction.CallbackContext ctx)
    {
        sk8ing = true;
        ball.SetActive(false);
        arrow.SetActive(true);
    }
    void EndSk8(InputAction.CallbackContext ctx)
    {
        sk8ing = false;
        arrow.SetActive(false);
        ball.SetActive(true);

        movementLockout = rb.linearVelocity.magnitude > 5;
    }

    bool StartMovementLockout()
    {
        movementLockoutStartTime = Time.time;
        return movementLockout = rb.linearVelocity.magnitude > 5;
    }

    void MovementLockoutMovement()
    {
        if (!StartMovementLockout()) return;

        float frictionStrength = 1 + Mathf.Clamp01(EaseOutCubic(1 - (Time.time - movementLockoutStartTime)));
        rb.linearVelocity -= frictionStrength * rb.linearVelocity.normalized;

        //BasicMovement();
    }

    private void OnEnable()
    {
        blink.action.started += DoBlink;
        sk8.action.started += DoSk8;
        sk8.action.canceled += EndSk8;
    }

    float EaseOutCubic(float x)
    {
        return 1 - Mathf.Pow(1 - x, 3);
    }

}