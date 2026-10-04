using System.Drawing;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Principal;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerMovement : MonoBehaviour
{
    int speed = 5;
    Vector2 mvmtInput = Vector2.zero;

    bool blinking = false;
    bool sk8Blinking = false;
    float blinkSpeed = 50f;
    float blinkDuration = 0.1f;
    float blinkStartTime;
    float blinkSk8CancelEfficiency = 0.75f;
    Vector2? blinkPredeterminedEndpoint;
    float blinkOverWallGrace = 5f;  // adds a certain amount of distance onto a blink to make it go over a wall

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

    float abilityLockoutMinSpeed = 0.5f;
    bool abilityLockout = false;
    float abilityLockoutStartTime = 0f;
    float abilityLockoutDuration = 0.5f;

    Vector3 lastFrameVelocity = Vector3.zero;


    [SerializeField] Rigidbody2D rb;

    [SerializeField] InputActionReference move;
    [SerializeField] InputActionReference blink;
    [SerializeField] InputActionReference sk8;

    [SerializeField] GameObject ringEffect;
    [SerializeField] SpriteRenderer ballWhite;
    [SerializeField] SpriteRenderer ballBlack;
    [SerializeField] SpriteRenderer ballShadow;
    [SerializeField] SpriteRenderer arrowBlack;
    [SerializeField] GameObject ball;
    [SerializeField] GameObject arrow;

    void FixedUpdate()
    {
        OnFixedUpdate();

        lastFrameVelocity = rb.linearVelocity;
    }
    void Update()
    {
        mvmtInput = move.action.ReadValue<Vector2>();
    }


    void OnFixedUpdate()
    {
        // continue movement lockout?
        bool lockout = mvmtLockout || abilityLockout;
        if (mvmtLockout)
            mvmtLockout = Time.time < mvmtLockoutStartTime + mvmtLockoutDuration && rb.linearVelocity.magnitude > mvmtLockoutMinSpeed;
        if (abilityLockout) 
            abilityLockout = Time.time < abilityLockoutStartTime + abilityLockoutDuration && rb.linearVelocity.magnitude > abilityLockoutMinSpeed;
            
        if (lockout && !abilityLockout) LateSk8Check();


        // continue blinking?
        if (blinking)
        {
            if (blinkPredeterminedEndpoint != null && ((Vector2)transform.position - blinkPredeterminedEndpoint.Value).magnitude > 0.5f) return;
            if (Time.time < blinkStartTime + blinkDuration && Time.time > blinkDuration) return;
            else EndBlink();
        }




        // calculate changes in speed
        Vector2 newSpeed = Vector2.zero;
        if (forcedSkidding) newSpeed += ForcedSkidMovement();

        if (!sk8ing)
        {
            if (mvmtInput != Vector2.zero) newSpeed += BasicAcceleration(!mvmtLockout ? 1 : Time.time - mvmtLockoutStartTime);
            else if (!mvmtLockout && !forcedSkidding) newSpeed += BasicDeceleration();
        }
        else if (!mvmtLockout && !forcedSkidding) newSpeed += Sk8Movement();

        rb.linearVelocity += newSpeed;




        // stop smoothly
        if (!sk8ing && rb.linearVelocity.magnitude <= 0.01)
            rb.linearVelocity = Vector2.zero;


        // print speed for fun
        //print(rb.linearVelocity.magnitude);
    }

    Vector2 BasicAcceleration(float effectiveness = 1f)
    {
        //print(effectiveness);
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
        if (abilityLockout) return;
        if (mvmtLockout) mvmtLockout = false;

        Instantiate(ringEffect, transform.position, Quaternion.identity);
        rb.linearVelocity = mvmtInput.normalized * blinkSpeed;
        blinking = true;
        blinkStartTime = Time.time;


        if (sk8.action.inProgress)
        {
            sk8Blinking = true;
            arrowBlack.enabled = false;
        }
        else
        {
            ballBlack.enabled = false;
            ballShadow.enabled = false;

            Vector2 p1 = (Vector2)transform.position + mvmtInput.normalized * blinkSpeed * blinkDuration;
            Vector2[] points = new Vector2[]
            {
                p1,
                p1 + 0.25f * blinkOverWallGrace * mvmtInput.normalized,
                p1 + 0.5f * blinkOverWallGrace * mvmtInput.normalized,
                p1 + 0.75f * blinkOverWallGrace * mvmtInput.normalized,
            };

            foreach (Vector2 point in points)
            {
                Collider2D collider = Physics2D.OverlapCircle(point, transform.localScale.x * 0.5f, Utils.LayerToLayerMask(Layers.Wall));
                //Instantiate(ringEffect, point, Quaternion.identity);
                if (collider == null)
                {
                    blinkPredeterminedEndpoint = point;
                    rb.excludeLayers = 255;
                    break;
                }
            }
        }
    }

    void EndBlink()
    {
        ballBlack.enabled = true;
        arrowBlack.enabled = true;
        ballShadow.enabled = true;

        blinking = false;
        bool wasSk8Blinking = sk8Blinking = false;

        if (!mvmtLockout && !sk8ing && !sk8.action.inProgress)
        {
            rb.linearVelocity = speed * 2 * mvmtInput.normalized;
        }
        else
        {
            rb.linearVelocity = rb.linearVelocity * blinkSk8CancelEfficiency;
            if (!wasSk8Blinking) DoSk8();
        }
        if (blinkPredeterminedEndpoint != null)
        {
            blinkPredeterminedEndpoint = null;
            rb.excludeLayers = 0;
        }
    }

    void DoSk8Req(InputAction.CallbackContext ctx) => DoSk8();
    void DoSk8()
    {
        if (blinking || abilityLockout) return;
        if (mvmtLockout) mvmtLockout = false;

        sk8ing = true;
        ball.SetActive(false);
        arrow.SetActive(true);
    }

    void LateSk8Check()
    {
        if (!sk8ing && sk8.action.inProgress) DoSk8();
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
    void DoAbilityLockout()
    {
        abilityLockoutStartTime = Time.time;
        abilityLockout = true;
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
            if ((blinking && blinkPredeterminedEndpoint == null) || sk8ing)
            {
                if (blinking && !(sk8ing || sk8.action.inProgress))
                    rb.linearVelocity = Vector3.Reflect(lastFrameVelocity, collision.GetContact(0).normal) / 3f;
                else    
                    rb.linearVelocity = Vector3.Reflect(lastFrameVelocity, collision.GetContact(0).normal);

                EndSk8(false);
                DoMvmtLockout();
                DoAbilityLockout();
                EndBlink();
            }

        }
    }
}