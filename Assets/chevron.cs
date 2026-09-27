using UnityEngine;
using UnityEngine.InputSystem;

public class chevron : MonoBehaviour
{
    [SerializeField] InputActionReference aim;
    [SerializeField] SpriteRenderer sprite;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (aim.action.ReadValue<Vector2>() != Vector2.zero)
        {
            sprite.enabled = aim.action.inProgress;
            transform.up = aim.action.ReadValue<Vector2>();
        }
    }
}
