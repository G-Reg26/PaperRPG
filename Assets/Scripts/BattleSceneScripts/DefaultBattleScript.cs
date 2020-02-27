﻿using System.Collections;
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
        WAITING,
        DEAD
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

    public Coroutine currentCoroutine;

    protected Transform sprite;
    protected Transform feet;

    protected BattleStats stats;

    public LayerMask initLayer;

    protected Vector3 initPos;

    protected Rigidbody rb;

    protected Animator anim;

    protected float initY;

    protected float squashDegree;
    protected float SNSpeed;

    protected bool grounded;
    protected bool facingRight;
    protected bool doubleHop;

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

        stats = GetComponent<BattleStats>();

        doubleHop = false;
    }

    virtual public void Reset()
    {
        squashDegree = 0.0f;

        SNSpeed = squashNStretchSpeed;

        sprite.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        sprite.localPosition = new Vector3(sprite.localPosition.x, initY, sprite.localPosition.z);

        if (currentState != States.DEAD)
        {
            currentState = States.WAITING;
        }

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

    public void Hurt(Vector3 collisionNormal, Vector3 collisionPoint)
    {

        //Cancel current coroutine if one is active
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        if (Vector3.Dot(collisionNormal, -Vector3.up) > 0.9f)
        {
            hopPoof.transform.position = new Vector3(collisionPoint.x, collisionPoint.y, -0.1f);
            hopPoof.Play();

            currentCoroutine = StartCoroutine(SquashNStretch());
        }
        else
        {
            currentCoroutine = StartCoroutine(BumpedInto(collisionNormal));
        }
    }

    virtual public void Hit(GameObject target, bool facingRight, bool fromCollsion)
    {
        BattleStats stats = target.GetComponent<BattleStats>();

        stats.health -= attacks[currentAttackIndex].damage;

        DefaultBattleScript targetBattleScript = target.gameObject.GetComponent<DefaultBattleScript>();

        if (stats.health == 0)
        {
            doubleHop = false;

            FindObjectOfType<BattleSceneManager>().RemoveEntity(targetBattleScript);

            if (target.gameObject.GetComponent<GreggBattleScript>())
            {
                GreggBattleScript enemy = target.gameObject.GetComponent<GreggBattleScript>();

                foreach (BattleMenu menu in FindObjectsOfType<BattleMenu>())
                {
                    menu.RemoveEnemy(enemy);
                }
            }

            targetBattleScript.currentState = States.DEAD;
        }
        else
        {
            targetBattleScript.currentState = States.ATTACKED;
        }

        if (currentCoroutine != null && fromCollsion)
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
            if (fromCollsion)
            {
                rb.velocity = new Vector3(-moveSpeed, hopHeight / 2, rb.velocity.z);

                currentState = States.RETREAT;

                currentCoroutine = StartCoroutine(RetreatBehavior(facingRight));
            }

            target.gameObject.layer = targetBattleScript.initLayer;
        }
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
        yield return new WaitUntil(() => Vector3.Distance(transform.position, initPos) < 0.1f);

        transform.position = initPos;

        rb.velocity = Vector3.zero;

        Reset();
    }
}
