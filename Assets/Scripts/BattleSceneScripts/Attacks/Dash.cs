using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : Attack
{
    public override void Start()
    {
        base.Start();
    }

    override public IEnumerator Behavior(DefaultBattleScript entity, Transform target)
    {
        target.gameObject.layer = LayerMask.NameToLayer("Battle");

        entity.GetAnimator().speed = 0.5f;

        Vector3 dir = (target.position - entity.transform.position).normalized;

        dir.y = 0.0f;

        // Reel back to charge up
        entity.GetRigidbody().velocity = -dir * entity.moveSpeed / 5.0f;

        //Increase in size while charging up
        while (entity.transform.localScale.x < entity.maxScale)
        {
            float n = entity.transform.localScale.x + (0.5f * Time.deltaTime);

            entity.transform.localScale = new Vector3(n, n, n);

            yield return null;
        }

        cameraController.ZoomToEnemies();

        if (entity.GetComponent<GiuseppeBattleScript>() != null)
        {
            entity.GetComponent<GiuseppeBattleScript>().PlayDustParticles();
        }

        // move towards enemy
        entity.GetRigidbody().velocity = dir * entity.moveSpeed * 2.0f;

        entity.GetAnimator().speed = 3.0f;

        while (entity.transform.localScale.x > 1)
        {
            float n = entity.transform.localScale.x - (1.0f * Time.deltaTime);

            entity.transform.localScale = new Vector3(n, n, n);

            yield return null;
        }

        entity.GetAnimator().speed = 1.0f;
    }
}
