using UnityEngine;
using UnityEngine.InputSystem;

public class chevron : MonoBehaviour
{
    [SerializeField] InputActionReference aim;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] PlayerMovement player;
    float visualMaxDistance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        visualMaxDistance = sprite.transform.localPosition.y;
    }

    // Update is called once per frame
    void Update()
    {
        sprite.enabled = aim.action.inProgress || player.MouseControls;

        if (player.MouseControls)
        {
            transform.up = player.AimState;
            sprite.transform.localPosition = new Vector3(0, player.AimState.magnitude * visualMaxDistance, 0);
            print(player.AimState.magnitude * visualMaxDistance);
        }
        else if (aim.action.ReadValue<Vector2>() != Vector2.zero)
        {
            transform.up = aim.action.ReadValue<Vector2>();
        }


    }
}
