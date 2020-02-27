using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiuseppeController : MonoBehaviour
{
    private Vector3 target;
    private bool facingRight;
    public float moveSpeed;

    public float groundCheckRadius;
    public LayerMask whatIsGround;
    public bool grounded;

    public Transform sprite;
    public Transform feet;
    public Animator anim;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        target = sprite.forward;
        facingRight = true;
    }

    private void LateUpdate()
    {
        anim.SetFloat("Velocity", Mathf.Abs(rb.velocity.x) + Mathf.Abs(rb.velocity.z));
        anim.SetBool("Grounded", grounded);
    }

    // Update is called once per frame
    void Update()
    {
        grounded = Physics.CheckSphere(feet.transform.position, groundCheckRadius, whatIsGround);

        // look at target
        Vector3 dir = Vector3.RotateTowards(sprite.forward, target, 15 * Time.deltaTime, 0.0f);

        sprite.rotation = Quaternion.LookRotation(dir);

        // right input
        if (Input.GetAxisRaw("Horizontal") > 0.0f)
        {
            // change target
            if (!facingRight)
            {
                target = -target;
                facingRight = true;
            }
        }
        // left input
        else if (Input.GetAxisRaw("Horizontal") < 0.0f)
        {
            // change target
            if (facingRight)
            {
                target = -target;
                facingRight = false;
            }
        }

        //Tracks input to keep moving the player
        rb.velocity = new Vector3(Input.GetAxis("Horizontal") * moveSpeed, rb.velocity.y, Input.GetAxis("Vertical") * moveSpeed * 1.50f);

        if (Input.GetButtonDown("Jump") && grounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 10.0f, rb.velocity.z);
        }
        
    }
}
