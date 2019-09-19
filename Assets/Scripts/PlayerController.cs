using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float moveSpeed;
    public float hopHeight;
    public bool attack;
    public bool attackTwo;
    public float n;
    public ParticleSystem dust;

    public Transform enemy;

    private Coroutine currentCoroutine;
    private Rigidbody rb;
    private Vector3 dustLocalPosition;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        dustLocalPosition = dust.transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
		if (attack && currentCoroutine == null)
        {
            currentCoroutine = StartCoroutine(Attack());
        }

        if (attackTwo && currentCoroutine == null)
        {
            currentCoroutine = StartCoroutine(AttackTwo());
        }
    }

    public IEnumerator Attack()
    {
        // move towards enemy
        rb.velocity = Vector3.right * moveSpeed;

        // wait until player is at a certain distance from enemy
        yield return new WaitUntil(() => Vector3.Distance(transform.position, enemy.position) <= 5.0f);

        // halt for 0.5s
        rb.velocity = Vector3.zero;

        yield return new WaitForSeconds(0.5f);

        // hop
        rb.velocity = new Vector3(moveSpeed, hopHeight, rb.velocity.z);

        // wait until player is directly above enemy
        yield return new WaitUntil(() => Mathf.Abs(transform.position.x - enemy.position.x) < 0.1f);

        rb.velocity = new Vector3(0.0f, rb.velocity.y, rb.velocity.z);
    }

    public IEnumerator AttackTwo()
    {
        // Reel back to charge up
        rb.velocity = -Vector3.right * moveSpeed / 5.0f;

        //Increase in size while charging up

        
        while (Mathf.Abs(transform.position.x - enemy.position.x) < 12.0f)
        {
            n += 0.5f * Time.deltaTime;
            transform.localScale = new Vector3(n*1.0f, n*1.0f, n*1.0f);
            yield return null;
        }

        //The point where it should stop charging and start running
        yield return new WaitUntil(() => Mathf.Abs(transform.position.x - enemy.position.x) >= 12.0f);
        dust.Play();
        
        // move towards enemy
        rb.velocity = Vector3.right * moveSpeed * 2.0f;
        while (n > 1)
        {
            n -= 1.0f * Time.deltaTime;
            transform.localScale = new Vector3(n * 1.0f, n * 1.0f, n * 1.0f);
            yield return null;
        }

        // halt for 0.5s
        //rb.velocity = Vector3.zero;

    }

    public IEnumerator Retreat()
    {
        // hop off
        rb.velocity = new Vector3(-moveSpeed, hopHeight / 2, rb.velocity.z);

        n = 1;
        transform.localScale = new Vector3(n * 1.0f, n * 1.0f, n * 1.0f);

        //Wait until back in the reset position
        yield return new WaitUntil(() => Vector3.Distance(transform.position, enemy.position) >= 10.0f);

        // halt for 0.5s
        rb.velocity = Vector3.zero;

        attack = false;
        attackTwo = false;
        currentCoroutine = null;
        dust.transform.parent = transform;
        dust.transform.localPosition = dustLocalPosition;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //On collision trigger the squish effect
        if (collision.gameObject.tag == "Enemy")
        {
            dust.Stop();
            dust.transform.parent = null;

            //Cancel current coroutine if one is active
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }

            currentCoroutine = StartCoroutine(Retreat());
        }
    }
}
