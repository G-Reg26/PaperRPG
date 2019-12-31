using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : Attack
{
    private GiuseppeBattleScript player;

    private bool waitForInput;
    private bool canDoubleHop;

    override public void Start()
    {
        base.Start();

        waitForInput = false;
    }

    public void Update()
    {
        if (player != null)
        {
            switch (player.currentState) {
                case GiuseppeBattleScript.States.ATTACKING:
                    if (Input.GetButtonDown("Jump") && waitForInput)
                    {
                        if (canDoubleHop)
                        {
                            player.SetDoubleHop(true);
                            canDoubleHop = false;
                        }

                        player = null;
                        canDoubleHop = false;
                        waitForInput = false;
                    }
                    break;
                case GiuseppeBattleScript.States.RETREAT:
                    player = null;
                    canDoubleHop = false;
                    waitForInput = false;
                    break;
            }
        }
    }

    override public IEnumerator Behavior(GiuseppeBattleScript player, Transform enemy)
    {
        this.player = player;

        // move towards enemy
        player.GetRigidbody().velocity = Vector3.right * player.moveSpeed;

        // wait until player is at a certain distance from enemy
        yield return new WaitUntil(() => Vector3.Distance(player.transform.position, enemy.position) < 5.0f);

        // halt for 0.5s
        player.GetRigidbody().velocity = Vector3.zero;

        cameraController.ZoomToEnemies();

        player.GetAnimator().Play("PlayerSquat");

        yield return new WaitForSeconds(1.0f);

        // hop
        player.GetRigidbody().velocity = new Vector3(player.moveSpeed, player.hopHeight, player.GetRigidbody().velocity.z);

        waitForInput = true;

        // wait until player is directly above enemy
        yield return new WaitUntil(() => Vector3.Distance(player.transform.position, enemy.position) < 2.5f);

        Debug.Log(Vector3.Distance(player.transform.position, enemy.position));

        canDoubleHop = waitForInput ? true : false;
    }
}
