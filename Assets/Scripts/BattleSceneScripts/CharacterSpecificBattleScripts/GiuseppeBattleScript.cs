using System.Collections;
using UnityEngine;

public class GiuseppeBattleScript : DefaultBattleScript {
    
    //public GameObject cubesContainer;

    public SpriteRenderer eggTimer;
    public Sprite[] eggTimerFrames;

    //public MeshRenderer[] cubes;

    public Transform target;

    private ParticleSystem dust;
    private Vector3 dustLocalPosition;

    // Use this for initialization
    void Start ()
    {
        base.Start();

        target = FindObjectOfType<GreggBattleScript>().transform;

        dust = GetComponentInChildren<ParticleSystem>();

        dustLocalPosition = dust.transform.localPosition;

        eggTimer.enabled = false;

        facingRight = true;
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

    override public void Hit(GameObject target, bool facingRight, bool fromCollision)
    {
        base.Hit(target, facingRight, fromCollision);

        dust.Stop();
        dust.transform.parent = null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //On collision trigger the squish effect
        if (collision.gameObject.tag == "Enemy")
        {
            if (currentState == States.ATTACKING)
            {
                Hit(collision.gameObject, true, true);
            }
            else
            {
                Hurt(collision.contacts[0].normal, collision.contacts[0].point);
            }
        }
    }
}
