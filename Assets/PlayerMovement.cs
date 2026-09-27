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
    float movementLockoutMinSpeed = 5f;

    [SerializeField] Rigidbody2D rb;

    [SerializeField] InputActionReference move;
    [SerializeField] InputActionReference blink;
    [SerializeField] InputActionReference sk8;

    [SerializeField] GameObject ringEffect;
    [SerializeField] GameObject ball;
    [SerializeField] GameObject arrow;

    void FixedUpdate()
    {
        Vector2 newSpeed = Vector2.zero;
        if (movementLockout)                newSpeed += MovementLockoutMovement();

        if (!sk8ing)
        {
            if (moveInput != Vector2.zero) newSpeed += BasicAcceleration(!movementLockout ? 1 : Time.time - movementLockoutStartTime);
            else if (!movementLockout)      newSpeed += BasicDeceleration();
        }
        else                                newSpeed += Sk8Movement();

        rb.linearVelocity += newSpeed;

        // stop smoothly
        if (!sk8ing && rb.linearVelocity.magnitude <= 0.01)
            rb.linearVelocity = Vector2.zero;

        //print(rb.linearVelocity.magnitude);
    }
    void Update()
    {
        moveInput = move.action.ReadValue<Vector2>();
    }


    // new fixed update idea
    // each movement method returns a vector as it's contribution to the new speed
    // then they are added together at the end and applied to the rb

    Vector2 BasicAcceleration(float effectiveness = 1f)
    {
        Vector2 desiredSpeed = (rb.linearVelocity) * 0.25f + (speed * moveInput.normalized) * .75f; // acceleration
        Vector2 diff = desiredSpeed - rb.linearVelocity;

        return diff * effectiveness;
    }
    Vector2 BasicDeceleration(float effectiveness = 1f)
    {
        Vector2 desiredSpeed = (rb.linearVelocity) * 0.75f;
        Vector2 diff = desiredSpeed - rb.linearVelocity;

        return diff * effectiveness;
    }

    Vector2 Sk8Movement()
    {
        Vector2 newSpeed = Vector2.zero;

        if (moveInput != Vector2.zero)
        {
            float slerpStrengthModifier = (Vector2.Dot(rb.linearVelocity.normalized, moveInput.normalized) + 2) / 2;  // slerp softer when inputting opposite direction to current momentum
            Vector2 newDir = Vector3.Slerp(rb.linearVelocity.normalized, moveInput.normalized, slerpStrength * slerpStrengthModifier * Time.fixedDeltaTime);
            newSpeed += newDir * rb.linearVelocity.magnitude;

            if (rb.linearVelocity.magnitude < maxSk8Velocity)
            {
                if (rb.linearVelocity.magnitude < minSk8Velocity)
                    newSpeed += moveInput.normalized * 20f * Time.fixedDeltaTime;

                newSpeed += rb.linearVelocity * .25f * Time.fixedDeltaTime;
            }

            newSpeed = newSpeed - rb.linearVelocity;
        }

        return newSpeed;
    }

    Vector2 MovementLockoutMovement()
    {
        movementLockout = rb.linearVelocity.magnitude > movementLockoutMinSpeed;

        if (!movementLockout) return Vector2.zero;

        float frictionStrength = 0.5f * (1 + Mathf.Clamp01(EaseOutCubic(Time.time - movementLockoutStartTime)));
        print(frictionStrength);
        Vector2 newSpeed = -1f * frictionStrength * rb.linearVelocity.normalized;

        return newSpeed;
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
        if (!sk8ing) return;

        sk8ing = false;
        arrow.SetActive(false);
        ball.SetActive(true);

        StartMovementLockout();
    }

    bool StartMovementLockout()
    {
        movementLockoutStartTime = Time.time;
        return movementLockout = rb.linearVelocity.magnitude > movementLockoutMinSpeed;
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

    float EaseInCubic(float x) => Mathf.Pow(x, 3);

}