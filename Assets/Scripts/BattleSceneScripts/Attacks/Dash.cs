using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : Attack
{
    public override void Start()
    {
        base.Start();
    }

    override public IEnumerator Behavior(GiuseppeBattleScript player, Transform enemy)
    {
        player.GetAnimator().speed = 0.5f;

        // Reel back to charge up
        player.GetRigidbody().velocity = -Vector3.right * player.moveSpeed / 5.0f;

        //Increase in size while charging up
        while (player.transform.localScale.x < player.maxScale)
        {
            float n = player.transform.localScale.x + (0.5f * Time.deltaTime);

            player.transform.localScale = new Vector3(n, n, n);

            yield return null;
        }

        cameraController.ZoomToEnemies();

        player.PlayDustParticles();

        // move towards enemy
        player.GetRigidbody().velocity = Vector3.right * player.moveSpeed * 2.0f;

        player.GetAnimator().speed = 3.0f;

        while (player.transform.localScale.x > 1)
        {
            float n = player.transform.localScale.x - (1.0f * Time.deltaTime);

            player.transform.localScale = new Vector3(n, n, n);

            yield return null;
        }

        player.GetAnimator().speed = 1.0f;
    }
}
