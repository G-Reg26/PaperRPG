using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public enum States
    {
        WAITING,
        ATTACKING,
        RETREAT
    }

    public States currentState;

    public Attack[] attacks;

    public float moveSpeed;
    public float hopHeight;
    public float squashNStretchSpeed;

    public float maxScale;

    public GameObject cubesContainer;
    public MeshRenderer[] cubes;

    [SerializeField]
    private LayerMask whatIsGround;
    [SerializeField]
    private float groundCheckRadius;

    private Coroutine currentCoroutine;

    private Transform enemy;

    private Transform sprite;
    private Transform feet;
    private ParticleSystem dust;

    private Vector3 initPos;
    private Vector3 dustLocalPosition;

    private Rigidbody rb;

    private Animator anim;

    private float initY;

    private float squashDegree;    
    private float SNSpeed;

    private bool grounded;
    private bool facingRight;
    private bool doubleHop;

    // Use this for initialization
    void Start ()
    {
        enemy = FindObjectOfType<Enemy>().transform;

        sprite = GetComponentInChildren<SpriteRenderer>().transform;
        feet = transform.Find("Feet");
        dust = GetComponentInChildren<ParticleSystem>();

        dustLocalPosition = dust.transform.localPosition;

        initPos = transform.position;
        initY = sprite.localPosition.y;

        rb = GetComponent<Rigidbody>();

        anim = sprite.GetComponent<Animator>();

        cubes = cubesContainer.GetComponentsInChildren<MeshRenderer>();

        cubesContainer.SetActive(false);

        currentState = States.WAITING;

        facingRight = true;
        doubleHop = false;
    }

    private void Reset()
    {
        squashDegree = 0.0f;

        SNSpeed = squashNStretchSpeed;

        sprite.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        sprite.localPosition = new Vector3(sprite.localPosition.x, initY, sprite.localPosition.z);

        currentState = States.WAITING;
    }

    private void LateUpdate()
    {
        anim.SetFloat("VelX", Mathf.Abs(rb.velocity.x));
        anim.SetBool("Grounded", grounded);
    }

    // Update is called once per frame
    void Update()
    {
        grounded = Physics.CheckSphere(feet.position, groundCheckRadius, whatIsGround);

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
                float offset = SinFunc(squashDegree, 2.0f, 0.05f);

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
                break;
        }
    }

    float SinFunc(float x, float freq, float amp)
    {
        float theta = (freq * x) + (Mathf.PI / 2.0f);

        return (Mathf.Sin(theta) * amp) - amp;
    }

    public void PlayDustParticles()
    {
        dust.Play();
    }

    // getters
    public Rigidbody GetRigidbody()
    {
        return rb;
    }

    public Animator GetAnimator()
    {
        return anim;
    }

    // setters
    public void SetDoubleHop(bool doubleHop)
    {
        this.doubleHop = doubleHop;
    }

    // state behaviors
    public void Attack(int index)
    {
        Reset();

        currentState = States.ATTACKING;

        currentCoroutine = StartCoroutine(attacks[index].Behavior(this, enemy));
    }

    public void Retreat()
    {
        currentState = States.RETREAT;

        currentCoroutine = StartCoroutine(RetreatBehavior());
    }

    // coroutines
    public IEnumerator RetreatBehavior()
    {
        rb.velocity = new Vector3(-moveSpeed, rb.velocity.y, rb.velocity.z);

        FindObjectOfType<BattleCameraController>().ToInitialPoint();

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
                // hop off
                rb.velocity = new Vector3(-moveSpeed, hopHeight / 2, rb.velocity.z);

                Retreat();
            }
        }
    }
}
