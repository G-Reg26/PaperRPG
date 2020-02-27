using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreggController : MonoBehaviour
{
    public enum States
    {
        ROAM,
        CHASE,
        RETURN,
        STATES
    };

    [SerializeField]
    private States currentState;

    private GiuseppeController player;

    private Rigidbody rb;

    [SerializeField]
    private LayerMask whatIsGround;

    [SerializeField]
    private Vector3 target;
    private Vector3 lookDir;
    [SerializeField]
    private Vector2 home;

    public float moveSpeed;
    public float chaseSpeed;
    public float roamRadius;
    public float groundCheckRadius;

    public bool grounded;
    public bool facingRight;

    public Transform sprite;
    public Transform feet;

    // Start is called before the first frame update
    void Start()
    {
        // GET COMPONENTS
        rb = GetComponent<Rigidbody>();

        // FIND OBJECTS
        player = FindObjectOfType<GiuseppeController>();

        // INITIALIZE
        currentState = States.ROAM;

        home = new Vector3(transform.position.x, transform.position.z);

        facingRight = false;
    }

    // Update is called once per frame
    void Update()
    {
        facingRight = rb.velocity.x > 0.0f ? true : false;

        lookDir = facingRight ? -Vector3.forward : Vector3.forward;

        Vector3 dir = Vector3.RotateTowards(sprite.forward, lookDir, 10.0f * Time.deltaTime, 0.0f);

        sprite.rotation = Quaternion.LookRotation(dir);

        if (Physics.CheckSphere(feet.position, groundCheckRadius, whatIsGround))
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }

        float distanceFromPlayer = Vector3.Distance(player.transform.position, transform.localPosition);

        float speed;

        switch (currentState)
        {
            case States.ROAM:
                speed = moveSpeed;

                if (grounded)
                {
                    if (distanceFromPlayer < 2.0f)
                    {
                        currentState = States.CHASE;
                    }
                    else if (target == Vector3.zero || Vector3.Distance(target, transform.position) < 0.25f)
                    {
                        GetRoamPoint();
                    }
                }
                break;
            case States.CHASE:
                speed = chaseSpeed;

                if (distanceFromPlayer > 2.0f)
                {
                    target = new Vector3(home.x, transform.position.y, home.y);

                    currentState = States.RETURN;
                }
                else
                {
                    target = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
                }
                break;
            case States.RETURN:
                speed = moveSpeed;

                if (distanceFromPlayer < 2.0f)
                {
                    target = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);

                    currentState = States.CHASE;
                }
                else if (Vector3.Distance(target, transform.position) < 0.1f)
                {
                    currentState = States.ROAM;
                }
                break;
            default:
                speed = moveSpeed;
                break;
        }

        rb.velocity = (target - transform.position).normalized * speed;
    }

    private void GetRoamPoint()
    {
        Vector2 point = Random.insideUnitCircle;

        target = new Vector3(home.x + (point.x * roamRadius), transform.position.y, home.y + (point.y * roamRadius));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!(Vector3.Dot(Vector3.up, collision.contacts[0].normal) > 0.95f) && currentState == States.ROAM)
        {
            GetRoamPoint();
        }
    }
}