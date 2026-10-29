using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EnergyMeter : MonoBehaviour
{
    public float Energy { get; private set; } = 1f;
    float energyRegenRate = 0.5f;
    float blinkCost = 0.3f;
    float sk8Cost = 0.1f;
    float minEnergyToStartSk8 = 0.1f;

    float currentDuration = float.MaxValue;
    float blinkCooldown = 1f;
    int blinkCharges = 2;
    [SerializeField] Image blinkMeter;


    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Image>().fillAmount = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (Energy <= 1)
        {
            GetComponent<Image>().fillAmount = Energy;
        }

        TickBlinkCd();

        blinkMeter.fillAmount = 0.5f * (blinkCharges + currentDuration);
        print(blinkCharges);
    }

    void TickBlinkCd()
    {
        if (blinkCharges < 2)
        {
            if (currentDuration > blinkCooldown)
                blinkCharges++;
            else
            {
                currentDuration += Time.deltaTime;
                return;
            }

            if (blinkCharges < 2)  // if still < 2, start another cooldown
                currentDuration = 0;
        }
    }


    public void RegenEnergy()
    {
        if (Energy <= 1)
            Energy += energyRegenRate * Time.deltaTime;
    }

    public bool TryBlink()
    {
        if (blinkCharges > 0)
        {
            blinkCharges--;
            if (currentDuration >= blinkCooldown) // start cooldown if there isn't already one
                currentDuration = 0;

            return true;
        }

        return false;
    }

    public bool CanStartSk8()
    {
        return Energy > minEnergyToStartSk8;
    }

    public bool ExpendSk8()
    {
        if (Energy >= sk8Cost * Time.deltaTime)
        {
            Energy -= sk8Cost * Time.deltaTime;
            return true;
        }

        return false;
    }
}
