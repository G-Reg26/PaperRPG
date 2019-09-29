using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public enum States
    {
        WAITING,
        ATTACKING,
        RETREAT
    }

    public States currentState;

    public float moveSpeed;
    public float hopHeight;
    public float initSNSpeed;

    public float maxScale;
    public float groundCheckRadius;

    public bool grounded;
    public bool facingRight;
    public bool doubleHop;

    public Attack[] attacks;

    public ParticleSystem dust;

    public Transform sprite;
    public Transform enemy;
    public Transform feet;

    public Rigidbody rb;

    public Animator anim;

    public LayerMask whatIsGround;

    private Coroutine currentCoroutine;

    private Vector3 initPos;
    private Vector3 dustLocalPosition;

    private float initY;

    private float squashDegree;    
    private float SNSpeed;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();

        initPos = transform.position;
        initY = sprite.localPosition.y;

        dustLocalPosition = dust.transform.localPosition;

        currentState = States.WAITING;

        facingRight = true;
        doubleHop = false;
    }

    private void Reset()
    {
        //Reset values for before enemy is hit
        squashDegree = 0.0f;

        SNSpeed = initSNSpeed;

        sprite.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        sprite.localPosition = new Vector3(sprite.localPosition.x, initY, sprite.localPosition.z);

        currentState = States.WAITING;
    }

    // Update is called once per frame
    void Update() {
        grounded = Physics.CheckSphere(feet.position, groundCheckRadius, whatIsGround);

        anim.SetFloat("VelX", Mathf.Abs(rb.velocity.x));
        anim.SetBool("Grounded", grounded);

        if (facingRight)
        {
            Vector3 target = Vector3.RotateTowards(sprite.forward, Vector3.forward, 15.0f * Time.deltaTime, 0.0f);

            sprite.rotation = Quaternion.LookRotation(target);
        }
        else
        {
            Vector3 target = Vector3.RotateTowards(sprite.forward, -Vector3.forward, 15.0f * Time.deltaTime, 0.0f);

            sprite.rotation = Quaternion.LookRotation(target);
        }

        switch (currentState) {
            case States.WAITING:
                float offset = sinFunc(squashDegree, 2.0f, 0.05f);

                sprite.localScale = new Vector3(1.0f - (offset / 2.0f), 1.0f + offset, 1.0f);
                sprite.localPosition = new Vector3(sprite.localPosition.x, initY + offset, sprite.localPosition.z);

                squashDegree += Time.deltaTime;
                break;
            case States.RETREAT:
                if (grounded)
                {
                    facingRight = false;
                }
                break;
            case States.ATTACKING:
                //Debug.Log(Vector3.Distance(transform.position, enemy.position));
                break;
        }
    }

    float sinFunc(float x, float freq, float amp)
    {
        float theta = (freq * x) + (Mathf.PI / 2.0f);

        return (Mathf.Sin(theta) * amp) - amp;
    }

    public void Attack(int index)
    {
        Reset();

        currentState = States.ATTACKING;

        currentCoroutine = StartCoroutine(attacks[index].Behavior(this, enemy));
    }

    public IEnumerator Retreat()
    {
        FindObjectOfType<BattleCameraController>().ToInitialPoint();

        // hop off
        rb.velocity = new Vector3(-moveSpeed, hopHeight / 2, rb.velocity.z);

        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        //Wait until back in the reset position
        yield return new WaitUntil(() => Vector3.Distance(transform.position, initPos) < 0.05f);

        transform.position = initPos;

        // halt for 0.5s
        rb.velocity = Vector3.zero;

        dust.transform.parent = transform;
        dust.transform.localPosition = dustLocalPosition;

        currentState = States.WAITING;

        facingRight = true;

        currentCoroutine = null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //On collision trigger the squish effect
        if (collision.gameObject.tag == "Enemy")
        {
            dust.Stop();
            dust.transform.parent = null;

            //Cancel current coroutine if one is active
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }

            if (doubleHop)
            {
                rb.velocity = new Vector3(0.0f, hopHeight, rb.velocity.z);

                doubleHop = false;
            }
            else
            {
                currentState = States.RETREAT;

                currentCoroutine = StartCoroutine(Retreat());
            }
        }
    }
}
