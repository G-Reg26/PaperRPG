using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skillet : Attack
{
    private GiuseppeBattleScript player;

    private float timer;
    private float shakeTimer;

    private int i;

    private bool hit;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        Reset();
    }

    public void Reset()
    {
        i = 0;
        timer = 0.0f;
        shakeTimer = 0.0f;

        hit = false;

        player = null;
    }

    public void Update()
    {
        if (player != null)
        {
            if (i >= player.eggTimerFrames.Length - 1)
            {
                shakeTimer += Time.deltaTime;

                if (shakeTimer > 0.05f)
                {
                    if (player.eggTimer.transform.rotation == Quaternion.identity || player.eggTimer.transform.rotation.eulerAngles.z > 350.0f)
                    {
                        player.eggTimer.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 5.0f));
                    }
                    else if (player.eggTimer.transform.rotation.eulerAngles.z < 10.0f)
                    {
                        player.eggTimer.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 355.0f));
                    }

                    shakeTimer = 0.0f;
                }
            }
            else
            {
                if (player.eggTimer.transform.localScale.x > 1.0f)
                {
                    player.eggTimer.transform.localScale -= Vector3.one * 2.0f * Time.deltaTime;
                }
                else
                {
                    player.eggTimer.transform.localScale = Vector3.one;
                }
            }

            if (!hit)
            {
                if (i <= player.eggTimerFrames.Length)
                {
                    if (Input.GetAxis("Horizontal") >= 0.0f)
                    {
                        if (i > 0)
                        {
                            hit = true;
                            return;
                        }
                    }

                    timer += Time.deltaTime;

                    if (timer >= 0.5f)
                    {
                        i++;

                        player.eggTimer.gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

                        if (i < player.eggTimerFrames.Length)
                        {
                            player.eggTimer.sprite = player.eggTimerFrames[i];
                        }
                        else
                        {
                            player.eggTimer.color = Color.red;

                            hit = true;
                            return;
                        }

                        timer = 0.0f;
                    }
                }
            }
        }
    }

    override public IEnumerator Behavior(DefaultBattleScript entity, Transform target)
    {
        target.gameObject.layer = LayerMask.NameToLayer("Battle");

        Vector3 dir = (target.position - entity.transform.position).normalized;

        dir.y = 0.0f;

        entity.GetRigidbody().velocity = dir * entity.moveSpeed;

        cameraController.ZoomToEnemies();

        yield return new WaitUntil(() => Vector3.Distance(entity.transform.position, target.position) < 1.5f);

        entity.GetRigidbody().velocity = Vector3.zero;

        player = entity.GetComponent<GiuseppeBattleScript>();

        player.eggTimer.enabled = true;

        player.GetAnimator().Play("PlayerReelBack");

        yield return new WaitUntil(() => hit);

        entity.GetAnimator().Play("PlayerSkilletHit");

        player.Hit(target.gameObject, true, false);
        target.GetComponent<DefaultBattleScript>().Hurt(-Vector3.up, target.position);

        yield return new WaitForSeconds(0.15f);

        entity.GetAnimator().Play("PlayerIdle");

        player.eggTimer.enabled = false;
        player.eggTimer.sprite = player.eggTimerFrames[0];
        player.eggTimer.color = Color.white;

        player.eggTimer.transform.rotation = Quaternion.identity;
        player.eggTimer.transform.localScale = Vector3.one;

        Reset();

        entity.currentState = DefaultBattleScript.States.RETREAT;

        StartCoroutine(entity.RetreatBehavior(true));
    }
}
