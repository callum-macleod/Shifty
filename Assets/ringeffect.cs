using Unity.VisualScripting;
using UnityEngine;

public class ringeffect : MonoBehaviour
{
    float startTime;
    float duration = 0.3f;
    float size = 0;
    float maxSize = 0.75f;

    bool isGrowingStage = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        startTime = Time.time;
        transform.localScale = Vector3.one * 0.01f;
    }

    // Update is called once per frame
    void Update()
    { 
        if (isGrowingStage)
        {
            size += 1 / duration * Time.deltaTime;
            transform.localScale = Vector3.one * Mathf.Lerp(0, maxSize, EaseInOutSine(size));

            if (Time.time - startTime > duration) isGrowingStage = false;
        }
        else
        {
            size -= 7 / duration * Time.deltaTime;
            transform.localScale = Vector3.one * Mathf.Lerp(0, maxSize, EaseOut(size));

            if (size <= 0.5f) Destroy(gameObject);
        }
    }

    float EaseInOutSine(float x)
    {
        return -(Mathf.Cos(Mathf.PI * x) - 1) / 2;
    }

    float EaseIn(float x) => Mathf.Pow(x, 2);

    float EaseOut(float x)
    {
        return 1 - Mathf.Pow(1 - x, 2);
    }

    float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;

        return 1 + c3* Mathf.Pow(x - 1, 3) + c1* Mathf.Pow(x - 1, 2);

    }
}
