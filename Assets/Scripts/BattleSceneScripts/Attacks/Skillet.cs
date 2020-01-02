using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skillet : Attack
{
    private GiuseppeBattleScript player;

    private float timer;
    private int i;

    private bool hit;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    public void Reset()
    {
        i = 0;
        timer = 0.0f;

        hit = false;

        player = null;
    }

    public void Update()
    {
        if (player != null)
        {
            if (!hit)
            {
                // player has let the stick loose
                if (Input.GetAxis("Horizontal") >= 0.0f)
                {
                    if (i > 0 && i <= player.cubes.Length)
                    {
                        hit = true;
                        return;
                    }
                }
                // player has not let the stick loose and is too late
                else
                {
                    if (i > player.cubes.Length)
                    {
                        foreach (MeshRenderer cube in player.cubes)
                        {
                            cube.material.color = Color.red;
                        }

                        hit = true;
                        return;
                    }
                }

                timer += Time.deltaTime;

                if (timer >= 0.5f)
                {
                    if (i < player.cubes.Length - 1)
                    {
                        player.cubes[i].material.color = Color.red;
                    }
                    else if (i == player.cubes.Length - 1)
                    {
                        foreach (MeshRenderer mr in player.cubes)
                        {
                            mr.material.color = Color.green;
                        }
                    }

                    i++;

                    timer = 0.0f;
                }

            }
        }
    }

    override public IEnumerator Behavior(DefaultBattleScript entity, Transform target)
    {
        // Reel back to charge up
        entity.GetRigidbody().velocity = Vector3.right * entity.moveSpeed;

        cameraController.ZoomToEnemies();

        yield return new WaitUntil(() => Vector3.Distance(entity.transform.position, target.position) < 1.5f);

        entity.GetRigidbody().velocity = Vector3.zero;

        player = entity.GetComponent<GiuseppeBattleScript>();

        player.cubesContainer.SetActive(true);

        player.GetAnimator().Play("PlayerReelBack");

        yield return new WaitUntil(() => hit);

        entity.GetAnimator().Play("PlayerSkilletHit");

        yield return new WaitForSeconds(0.15f);

        entity.GetAnimator().Play("PlayerIdle");

        player.cubesContainer.SetActive(false);

        for (int i = 0; i < player.cubes.Length; i++)
        {
            player.cubes[i].material.color = Color.white;
        }

        Reset();

        entity.currentState = DefaultBattleScript.States.RETREAT;

        StartCoroutine(entity.RetreatBehavior(true));
    }
}
