using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultBattleScript : MonoBehaviour
{
    public enum States
    {
        READY,
        ATTACKING,
        RETREAT,
        ATTACKED,
        WAITING
    }

    public States currentState;

    public float moveSpeed;
    public float hopHeight;
    public float maxScale;
    public float squashNStretchSpeed;

    public Attack[] attacks;

    public int currentAttackIndex;

    public ParticleSystem hopPoof;

    [SerializeField]
    protected LayerMask whatIsGround;
    [SerializeField]
    protected float groundCheckRadius;

    protected Coroutine currentCoroutine;

    protected Transform sprite;
    protected Transform feet;

    protected LayerMask initLayer;

    protected Vector3 initPos;

    protected Rigidbody rb;

    protected Animator anim;

    protected float initY;

    protected float squashDegree;
    protected float SNSpeed;

    protected bool grounded;
    protected bool facingRight;

    // Start is called before the first frame update
    protected void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>().transform;
        feet = transform.Find("Feet");

        initPos = transform.position;
        initY = sprite.localPosition.y;

        rb = GetComponent<Rigidbody>();

        anim = sprite.GetComponent<Animator>();

        currentState = States.WAITING;

        initLayer = gameObject.layer;

        SNSpeed = squashNStretchSpeed;
    }

    virtual public void Reset()
    {
        squashDegree = 0.0f;

        SNSpeed = squashNStretchSpeed;

        sprite.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        sprite.localPosition = new Vector3(sprite.localPosition.x, initY, sprite.localPosition.z);

        currentState = States.WAITING;

        gameObject.layer = initLayer;
    }

    // Update is called once per frame
    protected void Update()
    {
        grounded = Physics.CheckSphere(feet.position, groundCheckRadius, whatIsGround);

        switch (currentState)
        {
            case States.RETREAT:
                Retreat();
                break;
            case States.ATTACKING:
                gameObject.layer = LayerMask.NameToLayer("Battle");
                break;
            case States.ATTACKED:
                break;
            default:
                float offset = SinFunc(squashDegree, 2.0f, 0.05f);

                sprite.localScale = new Vector3(1.0f - (offset / 2.0f), 1.0f + offset, 1.0f);
                sprite.localPosition = new Vector3(sprite.localPosition.x, initY + offset, sprite.localPosition.z);

                squashDegree += Time.deltaTime;
                break;
        }
    }

    protected float SinFunc(float x, float freq, float amp)
    {
        float theta = (freq * x) + (Mathf.PI / 2.0f);

        return (Mathf.Sin(theta) * amp) - amp;
    }

    virtual public void Attack(int index)
    {
        Reset();

        currentAttackIndex = index;

        currentState = States.ATTACKING;
    }

    virtual public void Retreat()
    {
        
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

    public IEnumerator RetreatBehavior(bool faceRight)
    {
        Vector3 dir = (initPos - transform.position).normalized;

        dir.y = 0.0f;

        rb.velocity = new Vector3(moveSpeed * dir.x, rb.velocity.y, moveSpeed * dir.z);

        FindObjectOfType<BattleCameraController>().ToInitialPoint();

        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        //Wait until back in the reset position
        yield return new WaitUntil(() => Vector3.Distance(transform.position, initPos) < 0.1f);

        transform.position = initPos;

        // halt for 0.5s
        rb.velocity = Vector3.zero;

        currentState = States.WAITING;

        facingRight = faceRight;

        currentCoroutine = null;

        Reset();
    }

    public IEnumerator SquashNStretch()
    {
        transform.position = initPos;

        rb.velocity = Vector3.zero;

        //While loop for Squish Animation
        while (SNSpeed > 0.0f)
        {
            float offset = SinFunc(squashDegree, 6.0f, 0.1f);

            sprite.localScale = new Vector3(1.0f, 1.0f + offset, 1.0f);
            sprite.transform.localPosition = new Vector3(sprite.localPosition.x, initY + offset, sprite.localPosition.z);

            squashDegree += Time.deltaTime * SNSpeed;

            SNSpeed -= Mathf.Pow(Time.deltaTime, 0.5f);

            SNSpeed = Mathf.Clamp(SNSpeed, 0.0f, 10.0f);

            yield return null;
        }

        //Post squish returning to something close to reset state
        while (sprite.localScale.y < 0.95f)
        {
            float offset = SinFunc(squashDegree, 6.0f, 0.1f);

            sprite.localScale = new Vector3(1.0f, 1.0f + offset, 1.0f);
            sprite.transform.localPosition = new Vector3(sprite.localPosition.x, initY + offset, sprite.localPosition.z);

            squashDegree += Time.deltaTime;

            yield return null;
        }

        //Properly reset
        Reset();
    }

    public IEnumerator BumpedInto(Vector3 dir)
    {
        rb.velocity = new Vector3(dir.x * moveSpeed / 2.0f, hopHeight / 2.0f, dir.z * moveSpeed / 2.0f);

        yield return new WaitForSeconds(0.2f);

        yield return new WaitUntil(() => grounded);

        Vector3 n = -rb.velocity;

        // Reel back to charge up
        rb.velocity = -dir * moveSpeed;

        //Wait until back in the reset position
        yield return new WaitUntil(() => Vector3.Distance(transform.position, initPos) <  0.1f);

        transform.position = initPos;

        rb.velocity = Vector3.zero;

        Reset();
    }
}
