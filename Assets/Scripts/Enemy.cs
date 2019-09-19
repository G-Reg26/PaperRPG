using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public Transform mesh;
    public Transform player;
    public Coroutine currentCoroutine;

    public float moveSpeed;
    public float initSNSpeed;
    private Rigidbody rb;
    public float hopHeight;

    //Controls degree of squish
    private float SquashDegree;

    //Controls speed of squishing eff
    [SerializeField]
    private float SNSpeed;

	// Use this for initialization
	void Start () {
        //Reset mesh values to an original state
        Reset();

        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        
    }

    private void Reset()
    {
        //Reset values for before enemy is hit
        SquashDegree = Mathf.PI / 2.0f;

        SNSpeed = initSNSpeed;

        mesh.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    public IEnumerator SquashNStretch()
    {
        //While loop for Squish Animation
        while (SNSpeed > 0.0f)
        {
            mesh.localScale = new Vector3(1.0f, (Mathf.Sin(SquashDegree * 6.0f) / 10.0f) + 0.9f, 1.0f);

            SNSpeed = Mathf.Clamp(SNSpeed, 0.0f, 10.0f);

            SquashDegree += Time.deltaTime * SNSpeed;

            SNSpeed -= Mathf.Pow(Time.deltaTime, 0.5f);

            yield return null;
        }

        //Post squish returning to something close to reset state
        while (mesh.localScale.y < 0.95f)
        {
            mesh.localScale = new Vector3(1.0f, (Mathf.Sin(SquashDegree * 6.0f) / 10.0f) + 0.9f, 1.0f);

            SquashDegree += Time.deltaTime;

            yield return null;
        }

        //Properly reset
        Reset();
    }

    public IEnumerator bumpedInto()
    {
        rb.velocity = new Vector3(moveSpeed, hopHeight, rb.velocity.z);

        while (moveSpeed > 0)
        {
            rb.velocity = new Vector3(moveSpeed, rb.velocity.y, rb.velocity.z);

            moveSpeed -= 5.0f * Time.deltaTime;

            yield return null;
        }

        // Reel back to charge up
        rb.velocity = -Vector3.right * moveSpeed / 5.0f;

        //Wait until back in the reset position
        yield return new WaitUntil(() => Vector3.Distance(transform.position, player.position) >= 10.0f);

    }

    private void OnCollisionEnter(Collision collision)
    {
        //On collision trigger the squish effect
        if (collision.gameObject.tag == "Player")
        {
            Vector3 collisionNormal =  collision.contacts[0].normal;
            //Cancel current coroutine if one is active
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }

            if (Vector3.Dot(collisionNormal, -Vector3.up) == 1.0f)
            {
                currentCoroutine = StartCoroutine(SquashNStretch());
            }
            else if (Vector3.Dot(collisionNormal, Vector3.right) == 1.0f)
            {
                currentCoroutine = StartCoroutine(bumpedInto());
            }
        }
    }
}
