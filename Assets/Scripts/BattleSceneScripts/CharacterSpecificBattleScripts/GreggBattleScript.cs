using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreggBattleScript : DefaultBattleScript
{
    public Transform selectPoint;

    private Transform player;

    private BattleCameraController cameraController;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        player = FindObjectOfType<GiuseppeBattleScript>().transform;

        cameraController = FindObjectOfType<BattleCameraController>();

        facingRight = false;
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();

        anim.SetInteger("State", (int)currentState);

        if (currentState != States.ATTACKING)
        {
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
    }

    override public void Retreat()
    {        
        if (grounded)
        {
            facingRight = true;
        }
    }

    override public void Attack(int index)
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
                Hit(collision.gameObject, false, true);
            }
            else
            {
                Hurt(collision.contacts[0].normal, collision.contacts[0].point);
            }
        }
    }
}
