using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Principal;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerMovement : MonoBehaviour
{
    int speed = 5;
    Vector2 mvmtInput = Vector2.zero;

    bool blinking = false;
    float blinkSpeed = 50f;
    float blinkDuration = 0.1f;
    float blinkStartTime;

    float blinkSk8CancelEfficiency = 0.75f;

    bool sk8ing = false;
    float maxSk8Velocity = 20;
    float minSk8Velocity = 10;
    int slerpStrength = 5;

    bool forcedSkidding = false;
    float forcedSkidStartTime = 0f;
    float forcedSkidMinSpeed = 5f;

    float mvmtLockoutMinSpeed = 0.5f;
    bool mvmtLockout = false;
    float mvmtLockoutStartTime = 0f;
    float mvmtLockoutDuration = 0.5f;


    [SerializeField] Rigidbody2D rb;

    [SerializeField] InputActionReference move;
    [SerializeField] InputActionReference blink;
    [SerializeField] InputActionReference sk8;

    [SerializeField] GameObject ringEffect;
    [SerializeField] GameObject ball;
    [SerializeField] GameObject arrow;

    void FixedUpdate()
    {
        // continue movement lockout?
        if (mvmtLockout)
            mvmtLockout = Time.time < mvmtLockoutStartTime + mvmtLockoutDuration && rb.linearVelocity.magnitude > mvmtLockoutMinSpeed;
            //mvmtLockout = Time.time < mvmtLockoutStartTime + mvmtLockoutDuration;
        //if (mvmtLockout) return;

        // continue blinking?
        if (blinking && Time.time < blinkStartTime + blinkDuration) return;
        else
        {
            blinking = false;
            if (sk8.action.inProgress && !sk8ing)
            {
                rb.linearVelocity = rb.linearVelocity * blinkSk8CancelEfficiency;
                DoSk8();
            }
        }




        // calculate changes in speed
        Vector2 newSpeed = Vector2.zero;
        if (forcedSkidding)                                 newSpeed += ForcedSkidMovement();

        if (!sk8ing)
        {
            if (mvmtInput != Vector2.zero)                  newSpeed += BasicAcceleration(!mvmtLockout ? 1 : Time.time - mvmtLockoutStartTime);
            else if (!mvmtLockout && !forcedSkidding)       newSpeed += BasicDeceleration();
        }
        else if (!mvmtLockout && !forcedSkidding)           newSpeed += Sk8Movement();

        rb.linearVelocity += newSpeed;




        // stop smoothly
        if (!sk8ing && rb.linearVelocity.magnitude <= 0.01)
            rb.linearVelocity = Vector2.zero;


        // print speed for fun
        //print(rb.linearVelocity.magnitude);
    }
    void Update()
    {
        mvmtInput = move.action.ReadValue<Vector2>();
    }


    Vector2 BasicAcceleration(float effectiveness = 1f)
    {
        print(effectiveness);
        Vector2 desiredSpeed = (rb.linearVelocity) * 0.25f + (speed * mvmtInput.normalized) * .75f; // acceleration
        Vector2 diff = Vector2.ClampMagnitude(desiredSpeed - rb.linearVelocity, 5f);

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

        if (mvmtInput != Vector2.zero)
        {
            // turn
            float slerpStrengthModifier = (Vector2.Dot(rb.linearVelocity.normalized, mvmtInput.normalized) + 2) / 2;  // slerp softer when inputting opposite direction to current momentum
            Vector2 newDir = Vector3.Slerp(rb.linearVelocity.normalized, mvmtInput.normalized, slerpStrength * slerpStrengthModifier * Time.fixedDeltaTime);
            newSpeed += newDir * rb.linearVelocity.magnitude;

            // speed up / slow down
            if (rb.linearVelocity.magnitude < maxSk8Velocity)
            {
                if (rb.linearVelocity.magnitude < minSk8Velocity)
                    newSpeed += mvmtInput.normalized * 20f * Time.fixedDeltaTime;

                newSpeed += rb.linearVelocity * .25f * Time.fixedDeltaTime;
            }
            else
            {
                newSpeed -= rb.linearVelocity * .15f * Time.fixedDeltaTime;
            }

            newSpeed = newSpeed - rb.linearVelocity;
        }

        return newSpeed;
    }

    Vector2 ForcedSkidMovement()
    {
        forcedSkidding = rb.linearVelocity.magnitude > forcedSkidMinSpeed && mvmtLockout;

        if (!forcedSkidding) return Vector2.zero;

        float frictionStrength = 0.5f * (1 + Mathf.Clamp01(EaseOutCubic(Time.time - forcedSkidStartTime)));
        Vector2 newSpeed = -1f * frictionStrength * rb.linearVelocity.normalized;

        return newSpeed;
    }



    void DoBlinkReq(InputAction.CallbackContext ctx) => DoBlink();
    void DoBlink()
    {
        Instantiate(ringEffect, transform.position, Quaternion.identity);
        rb.linearVelocity = mvmtInput.normalized * blinkSpeed;
        blinking = true;
        blinkStartTime = Time.time;
        
        if (sk8ing) EndSk8(false);
    }

    void EndBlink()
    {
        blinking = false;
    }

    void DoSk8Req(InputAction.CallbackContext ctx) => DoSk8();
    void DoSk8()
    {
        if (blinking || mvmtLockout || forcedSkidding) return;
        sk8ing = true;
        ball.SetActive(false);
        arrow.SetActive(true);
    }

    void EndSk8Req(InputAction.CallbackContext ctx) => EndSk8(!blinking);
    void EndSk8(bool forceSkid = true)
    {
        if (!sk8ing) return;

        sk8ing = false;
        arrow.SetActive(false);
        ball.SetActive(true);

        if (forceSkid) DoForcedSkid();
    }

    bool DoForcedSkid()
    {
        forcedSkidStartTime = mvmtLockoutStartTime = Time.time;
        return forcedSkidding = mvmtLockout = rb.linearVelocity.magnitude > forcedSkidMinSpeed;
    }
    void DoMvmtLockout()
    {
        mvmtLockoutStartTime = Time.time;
        mvmtLockout = true;
    }

    private void OnEnable()
    {
        blink.action.started += DoBlinkReq;
        sk8.action.started += DoSk8Req;
        sk8.action.canceled += EndSk8Req;
    }

    float EaseOutCubic(float x)
    {
        return 1 - Mathf.Pow(1 - x, 3);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null) return;
        

        if (collision.collider.gameObject.layer == 6 || collision.collider.gameObject.layer == 7)
        {
            EndSk8(false);
            EndBlink();
            DoMvmtLockout();
        }
    }
}