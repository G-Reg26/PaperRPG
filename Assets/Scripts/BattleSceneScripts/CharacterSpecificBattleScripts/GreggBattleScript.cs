using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreggBattleScript : DefaultBattleScript
{   
    private Transform player;

    private BattleCameraController cameraController;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        player = FindObjectOfType<GiuseppeBattleScript>().transform;

        cameraController = FindObjectOfType<BattleCameraController>();

        currentState = States.WAITING;

        facingRight = false;
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();

        if (currentState != States.ATTACKING)
        {
            if (facingRight && currentState != States.ATTACKING)
            {
                Vector3 target = Vector3.RotateTowards(sprite.forward, -Vector3.forward, 15.0f * Time.deltaTime, 0.0f);

                sprite.rotation = Quaternion.LookRotation(target);
            }
            else
            {
                Vector3 target = Vector3.RotateTowards(sprite.forward, Vector3.forward, 15.0f * Time.deltaTime, 0.0f);

                sprite.rotation = Quaternion.LookRotation(target);
            }
        }
    }

    override public void Retreat()
    {        
        if (grounded)
        {
            facingRight = true;
        }
    }

    public void Attack(int index)
    {
        base.Attack(index);

        currentCoroutine = StartCoroutine(attacks[index].Behavior(this, player));
    }

    private void OnCollisionEnter(Collision collision)
    {
        //On collision trigger the squish effect
        if (collision.gameObject.tag == "Player")
        {
            if (currentState == States.ATTACKING)
            {
                //Cancel current coroutine if one is active
                if (currentCoroutine != null)
                {
                    StopCoroutine(currentCoroutine);
                    currentCoroutine = null;
                }

                // hop off
                rb.velocity = new Vector3(-moveSpeed, hopHeight / 2, rb.velocity.z);

                currentState = States.RETREAT;

                currentCoroutine = StartCoroutine(RetreatBehavior(false));
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

                if (Vector3.Dot(collisionNormal, -Vector3.up) > 0.9f)
                {
                    hopPoof.transform.position = new Vector3(collision.contacts[0].point.x, collision.contacts[0].point.y, -0.1f);
                    hopPoof.Play();

                    currentCoroutine = StartCoroutine(SquashNStretch());
                }
                else if (Vector3.Dot(collisionNormal, Vector3.right) > 0.9f)
                {
                    currentCoroutine = StartCoroutine(BumpedInto());
                }
            }
        }
    }
}
