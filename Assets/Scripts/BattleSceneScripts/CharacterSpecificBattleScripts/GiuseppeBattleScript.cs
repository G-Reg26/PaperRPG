using System.Collections;
using UnityEngine;

public class GiuseppeBattleScripts : DefaultBattleScript {

    public Attack[] attacks;

    public float maxScale;

    public GameObject cubesContainer;
    public MeshRenderer[] cubes;

    private Transform enemy;

    private ParticleSystem dust;
    private Vector3 dustLocalPosition;

    private bool doubleHop;

    // Use this for initialization
    void Start ()
    {
        base.Start();

        enemy = FindObjectOfType<GreggBattleScript>().transform;

        dust = GetComponentInChildren<ParticleSystem>();

        dustLocalPosition = dust.transform.localPosition;

        cubes = cubesContainer.GetComponentsInChildren<MeshRenderer>();

        cubesContainer.SetActive(false);

        currentState = States.READY;

        facingRight = true;
        doubleHop = false;
    }

    void Reset()
    {
        base.Reset();

        dust.transform.parent = transform;
        dust.transform.localPosition = dustLocalPosition;
    }

    private void LateUpdate()
    {
        anim.SetFloat("VelX", Mathf.Abs(rb.velocity.x));
        anim.SetBool("Grounded", grounded);
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
            
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
    }

    public void PlayDustParticles()
    {
        dust.Play();
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

    override public void Retreat()
    {
        if (grounded)
        {
            facingRight = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //On collision trigger the squish effect
        if (collision.gameObject.tag == "Enemy")
        {
            if (currentState == States.ATTACKING)
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

                    currentState = States.RETREAT;

                    currentCoroutine = StartCoroutine(RetreatBehavior(-moveSpeed, true));
                }
            }
            else
            {
                Reset();

                currentState = States.ATTACKED;

                Vector3 collisionNormal = collision.contacts[0].normal;
                //Cancel current coroutine if one is active
                if (currentCoroutine != null)
                {
                    StopCoroutine(currentCoroutine);
                    currentCoroutine = null;
                }

                if (Vector3.Dot(collisionNormal, -Vector3.up) == 1.0f)
                {
                    currentCoroutine = StartCoroutine(SquashNStretch());
                }
                else if (Vector3.Dot(collisionNormal, Vector3.right) == 1.0f)
                {
                    currentCoroutine = StartCoroutine(BumpedInto(-moveSpeed));
                }
            }
        }
    }
}
