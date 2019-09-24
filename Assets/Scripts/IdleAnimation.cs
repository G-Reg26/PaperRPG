using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleAnimation : MonoBehaviour
{
    private float n;
    private float initY;

    // Start is called before the first frame update
    void Start()
    {
        n = 0.0f;
        initY = transform.localPosition.y;
    }

    // Update is called once per frame
    void Update()
    {
        float offset = sinFunc(n, 2.0f, 0.05f);

        transform.localScale = new Vector3(1.0f - (offset / 2.0f), 1.0f + offset, 1.0f);
        transform.localPosition = new Vector3(transform.localPosition.x, initY + (offset / 2.0f), transform.localPosition.z);

        n += Time.deltaTime;
    }

    float sinFunc(float x, float freq, float amp)
    {
        float theta = (freq * x) + (Mathf.PI / 2.0f);

        return (Mathf.Sin(theta) * amp) - amp;
    }
}
