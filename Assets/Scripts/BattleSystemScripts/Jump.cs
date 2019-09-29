using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : Attack
{
    private PlayerController player;

    public bool waitForInput;
    public bool canDoubleHop;

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
                case PlayerController.States.ATTACKING:
                    if (Input.GetButtonDown("Jump") && waitForInput)
                    {
                        if (canDoubleHop)
                        {
                            player.doubleHop = true;
                            canDoubleHop = false;
                        }

                        player = null;
                        canDoubleHop = false;
                        waitForInput = false;
                    }
                    break;
                case PlayerController.States.RETREAT:
                    player = null;
                    canDoubleHop = false;
                    waitForInput = false;
                    break;
            }
        }
    }

    override public IEnumerator Behavior(PlayerController player, Transform enemy)
    {
        this.player = player;

        // move towards enemy
        player.rb.velocity = Vector3.right * player.moveSpeed;

        // wait until player is at a certain distance from enemy
        yield return new WaitUntil(() => Vector3.Distance(player.transform.position, enemy.position) < 5.0f);

        // halt for 0.5s
        player.rb.velocity = Vector3.zero;

        cameraController.ZoomToEnemies();

        player.anim.Play("PlayerSquat");

        yield return new WaitForSeconds(1.0f);

        // hop
        player.rb.velocity = new Vector3(player.moveSpeed, player.hopHeight, player.rb.velocity.z);

        waitForInput = true;

        // wait until player is directly above enemy
        yield return new WaitUntil(() => Vector3.Distance(player.transform.position, enemy.position) < 2.5f);

        Debug.Log(Vector3.Distance(player.transform.position, enemy.position));

        canDoubleHop = waitForInput ? true : false;
    }
}
