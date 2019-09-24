using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private Vector3 target;
    private bool facingRight;

    // Start is called before the first frame update
    void Start()
    {
        target = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        // look at target
        Vector3 dir = Vector3.RotateTowards(transform.forward, target, 10 * Time.deltaTime, 0.0f);

        transform.rotation = Quaternion.LookRotation(dir);

        // right input
        if (Input.GetAxis("Horizontal") > 0.0f)
        {
            // change target
            if (!facingRight)
            {
                target = -target;
                facingRight = true;
            }
        }
        // left input
        else if (Input.GetAxis("Horizontal") < 0.0f)
        {
            // change target
            if (facingRight)
            {
                target = -target;
                facingRight = false;
            }
        }
    }
}
