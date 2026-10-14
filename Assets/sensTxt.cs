using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class sensTxt : MonoBehaviour
{
    [SerializeField] TMP_InputField input;
    [SerializeField] Toggle pcMode;
    [SerializeField] PlayerMovement shifty;

    private void Start()
    {
        input.text = shifty.MouseSensitivity.ToString();
        pcMode.isOn = shifty.MouseControls;
    }

    public void TogglePCMode()
    {
        shifty.MouseControls = pcMode.isOn;
    }

    public void OnClick()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void UpdateSens()
    {
        try
        {
            long newSens = Convert.ToInt64(input.text);
            shifty.MouseSensitivity = newSens;
        }
        catch (Exception e)
        {
            Debug.LogError($"Invalid sens: {e.Message}");
        }
        Cursor.lockState = CursorLockMode.Locked;
    }
}
