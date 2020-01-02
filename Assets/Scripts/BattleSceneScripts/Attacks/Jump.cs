using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : Attack
{
    private DefaultBattleScript entity;

    Vector3 jumpPos;
    Vector3 lookAtPoint;

    private bool waitForInput;
    private bool canDoubleHop;

    override public void Start()
    {
        base.Start();

        waitForInput = false;
    }

    public void Update()
    {
        if (entity != null)
        {
            switch (entity.currentState) {
                case DefaultBattleScript.States.ATTACKING:
                    if (entity.GetComponent<GiuseppeBattleScript>() != null)
                    {
                        if (Input.GetButtonDown("Jump") && waitForInput)
                        {
                            if (canDoubleHop)
                            {
                                entity.GetComponent<GiuseppeBattleScript>().SetDoubleHop(true);
                                canDoubleHop = false;
                            }

                            entity = null;
                            canDoubleHop = false;
                            waitForInput = false;
                        }
                    }
                    else
                    {
                        float angle = Vector3.SignedAngle(lookAtPoint - jumpPos, lookAtPoint - entity.transform.position, entity.transform.forward);

                        entity.transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle);
                    }
                    break;
                case DefaultBattleScript.States.RETREAT:
                    entity.transform.rotation = Quaternion.identity;

                    entity = null;
                    canDoubleHop = false;
                    waitForInput = false;
                    break;
            }
        }
    }

    override public IEnumerator Behavior(DefaultBattleScript entity, Transform target)
    {
        this.entity = entity;

        // move towards enemy
        entity.GetRigidbody().velocity = Vector3.right * entity.moveSpeed;

        // wait until player is at a certain distance from enemy
        yield return new WaitUntil(() => Vector3.Distance(entity.transform.position, target.position) < 4.9f);

        // halt for 0.5s
        entity.GetRigidbody().velocity = Vector3.zero;


        if (entity.GetComponent<GiuseppeBattleScript>() != null)
        {
            cameraController.ZoomToEnemies();

            entity.GetAnimator().Play("PlayerSquat");
        }
        else
        {
            cameraController.ZoomToPlayers();
        }

        yield return new WaitForSeconds(1.0f);

        jumpPos = entity.transform.position;
        lookAtPoint = Vector3.Lerp(entity.transform.position, target.position, 0.5f);

        // hop
        entity.GetRigidbody().velocity = new Vector3(entity.moveSpeed, entity.hopHeight, entity.GetRigidbody().velocity.z);

        waitForInput = true;

        // wait until player is directly above enemy
        yield return new WaitUntil(() => Vector3.Distance(entity.transform.position, target.position) < 2.5f);

        Debug.Log(Vector3.Distance(entity.transform.position, target.position));

        canDoubleHop = waitForInput ? true : false;
    }
}
