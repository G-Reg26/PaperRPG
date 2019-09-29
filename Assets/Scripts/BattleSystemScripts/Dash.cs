using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : Attack
{
    public override void Start()
    {
        base.Start();
    }

    override public IEnumerator Behavior(PlayerController player, Transform enemy)
    {
        player.anim.speed = 0.5f;

        // Reel back to charge up
        player.rb.velocity = -Vector3.right * player.moveSpeed / 5.0f;

        //Increase in size while charging up
        while (player.transform.localScale.x < player.maxScale)
        {
            float n = player.transform.localScale.x + (0.5f * Time.deltaTime);

            player.transform.localScale = new Vector3(n, n, n);

            yield return null;
        }

        cameraController.ZoomToEnemies();

        player.dust.Play();

        // move towards enemy
        player.rb.velocity = Vector3.right * player.moveSpeed * 2.0f;

        player.anim.speed = 3.0f;

        while (player.transform.localScale.x > 1)
        {
            float n = player.transform.localScale.x - (1.0f * Time.deltaTime);

            player.transform.localScale = new Vector3(n, n, n);

            yield return null;
        }

        player.anim.speed = 1.0f;
    }
}
