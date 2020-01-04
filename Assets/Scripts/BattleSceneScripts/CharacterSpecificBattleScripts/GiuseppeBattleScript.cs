using System.Collections;
using UnityEngine;

public class GiuseppeBattleScript : DefaultBattleScript {
    
    public GameObject cubesContainer;
    public MeshRenderer[] cubes;

    public Transform target;

    private ParticleSystem dust;
    private Vector3 dustLocalPosition;

    private bool doubleHop;

    // Use this for initialization
    void Start ()
    {
        base.Start();

        target = FindObjectOfType<GreggBattleScript>().transform;

        dust = GetComponentInChildren<ParticleSystem>();

        dustLocalPosition = dust.transform.localPosition;

        cubes = cubesContainer.GetComponentsInChildren<MeshRenderer>();

        cubesContainer.SetActive(false);

        facingRight = true;
        doubleHop = false;
    }

    override public void Reset()
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

        if (currentState != States.ATTACKING)
        {
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
    override public void Attack(int index)
    {
        base.Attack(index);

        currentCoroutine = StartCoroutine(attacks[index].Behavior(this, target));
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
                collision.gameObject.GetComponent<BattleStats>().health -= attacks[currentAttackIndex].damage;

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

                    currentCoroutine = StartCoroutine(RetreatBehavior(true));

                    collision.gameObject.layer = LayerMask.NameToLayer("Enemy");
                }
            }
            else
            {
                currentState = States.ATTACKED;

                Vector3 collisionNormal = collision.contacts[0].normal;
                //Cancel current coroutine if one is active
                if (currentCoroutine != null)
                {
                    StopCoroutine(currentCoroutine);
                    currentCoroutine = null;
                }

                if (Vector3.Dot(collisionNormal, -Vector3.up) > 0.9f)
                {
                    hopPoof.transform.position = new Vector3(collision.contacts[0].point.x, collision.contacts[0].point.y, -0.1f);
                    hopPoof.Play();

                    currentCoroutine = StartCoroutine(SquashNStretch());
                }
                else
                {
                    currentCoroutine = StartCoroutine(BumpedInto(collision.contacts[0].normal));
                }
            }
        }
    }
}
