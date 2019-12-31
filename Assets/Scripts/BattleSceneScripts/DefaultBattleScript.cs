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
    public float squashNStretchSpeed;

    [SerializeField]
    protected LayerMask whatIsGround;
    [SerializeField]
    protected float groundCheckRadius;

    protected Coroutine currentCoroutine;

    protected Transform sprite;
    protected Transform feet;

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
    }

    protected void Reset()
    {
        squashDegree = 0.0f;

        SNSpeed = squashNStretchSpeed;

        sprite.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        sprite.localPosition = new Vector3(sprite.localPosition.x, initY, sprite.localPosition.z);

        currentState = States.WAITING;
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

    public IEnumerator RetreatBehavior(float retreatSpeed, bool faceRight)
    {
        rb.velocity = new Vector3(retreatSpeed, rb.velocity.y, rb.velocity.z);

        FindObjectOfType<BattleCameraController>().ToInitialPoint();

        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        //Wait until back in the reset position
        yield return new WaitUntil(() => Vector3.Distance(transform.position, initPos) < 0.05f);

        transform.position = initPos;

        // halt for 0.5s
        rb.velocity = Vector3.zero;

        currentState = States.WAITING;

        facingRight = faceRight;

        currentCoroutine = null;
    }

    public IEnumerator SquashNStretch()
    {
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

    public IEnumerator BumpedInto(float bumpSpeed)
    {
        rb.velocity = new Vector3(bumpSpeed, hopHeight, rb.velocity.z);

        float n = bumpSpeed;

        while (n > 0)
        {
            rb.velocity = new Vector3(n, rb.velocity.y, rb.velocity.z);

            if (moveSpeed > 0.0f)
            {
                n -= 5.0f * Time.deltaTime;
            }
            else
            {
                n += 5.0f * Time.deltaTime;
            }

            yield return null;
        }

        // Reel back to charge up
        rb.velocity = Vector3.right * -bumpSpeed;

        //Wait until back in the reset position
        yield return new WaitUntil(() => Vector3.Distance(transform.position, initPos) < moveSpeed * 0.02f);

        transform.position = initPos;

        rb.velocity = Vector3.zero;

        Reset();
    }
}
