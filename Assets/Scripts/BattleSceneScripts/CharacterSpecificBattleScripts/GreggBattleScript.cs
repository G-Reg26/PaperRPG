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

        player = FindObjectOfType<GiuseppeBattleScripts>().transform;

        cameraController = FindObjectOfType<BattleCameraController>();

        currentState = States.WAITING;

        facingRight = false;
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();

        if (facingRight)
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

    override public void Retreat()
    {        
        if (grounded)
        {
            facingRight = true;
        }
    }

    public IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.5f);

        // move towards enemy
        rb.velocity = Vector3.left * moveSpeed;

        // wait until player is at a certain distance from enemy
        yield return new WaitUntil(() => Vector3.Distance(transform.position, player.position) < 5.0f);

        // halt for 0.5s
        rb.velocity = Vector3.zero;

        cameraController.ZoomToPlayers();

        yield return new WaitForSeconds(1.0f);

        // hop
        rb.velocity = new Vector3(-moveSpeed, hopHeight, rb.velocity.z);
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

                currentCoroutine = StartCoroutine(RetreatBehavior(moveSpeed, false));
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
                    currentCoroutine = StartCoroutine(BumpedInto(moveSpeed));
                }
            }
        }
    }
}
