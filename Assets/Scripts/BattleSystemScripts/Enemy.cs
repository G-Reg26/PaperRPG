using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public enum States
    {
        IDLE,
        ATTACKED,
        STATES
    };

    public States currentState;

    public float moveSpeed;
    public float hopHeight;
    public float squashNStretchSpeed;

    private Coroutine currentCoroutine;

    private Transform player;

    private Transform sprite;

    private Vector3 initPos;

    private Rigidbody rb;

    private float initY;

    private float squashDegree;
    private float SNSpeed;

	// Use this for initialization
	void Start ()
    {
        player = FindObjectOfType<PlayerController>().transform;

        sprite = GetComponentInChildren<SpriteRenderer>().transform;

        initPos = transform.position;
        initY = sprite.localPosition.y;

        rb = GetComponent<Rigidbody>();

        Reset();
    }

    private void Reset()
    {
        squashDegree = 0.0f;

        SNSpeed = squashNStretchSpeed;

        sprite.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        sprite.localPosition = new Vector3(sprite.localPosition.x, initY, sprite.localPosition.z);

        currentState = States.IDLE;
    }

    // Update is called once per frame
    void Update ()
    {
        switch (currentState)
        {
            case States.IDLE:
                float offset = SinFunc(squashDegree, 2.0f, 0.05f);

                sprite.localScale = new Vector3(1.0f - (offset / 2.0f), 1.0f + offset, 1.0f);
                sprite.localPosition = new Vector3(sprite.localPosition.x, initY + offset, sprite.localPosition.z);

                squashDegree += Time.deltaTime;
                break;
            default:
                break;
        }   
    }

    float SinFunc(float x, float freq, float amp)
    {
        float theta = (freq * x) + (Mathf.PI / 2.0f);

        return (Mathf.Sin(theta) * amp) - amp;
    }

    public IEnumerator SquashNStretch()
    {
        //While loop for Squish Animation
        while (SNSpeed > 0.0f)
        {
            float offset = SinFunc(squashDegree, 6.0f, 0.1f);

            sprite.localScale = new Vector3(1.0f, 1.0f + offset, 1.0f);
            sprite.transform.localPosition = new Vector3(sprite.localPosition.x, initY + offset, sprite.localPosition.z);
            
            squashDegree += Time.deltaTime * SNSpeed;

            SNSpeed -= Mathf.Pow(Time.deltaTime, 0.5f);

            SNSpeed = Mathf.Clamp(SNSpeed, 0.0f, 10.0f);

            yield return null;
        }

        //Post squish returning to something close to reset state
        while (sprite.localScale.y < 0.95f)
        {
            float offset = SinFunc(squashDegree, 6.0f, 0.1f);

            sprite.localScale = new Vector3(1.0f, 1.0f + offset, 1.0f);
            sprite.transform.localPosition = new Vector3(sprite.localPosition.x, initY + offset, sprite.localPosition.z);

            squashDegree += Time.deltaTime;

            yield return null;
        }

        //Properly reset
        Reset();
    }

    public IEnumerator BumpedInto()
    {
        rb.velocity = new Vector3(moveSpeed, hopHeight, rb.velocity.z);

        float n = moveSpeed;

        while (n > 0)
        {
            rb.velocity = new Vector3(n, rb.velocity.y, rb.velocity.z);

            n -= 5.0f * Time.deltaTime;

            yield return null;
        }

        // Reel back to charge up
        rb.velocity = -Vector3.right * moveSpeed;

        //Wait until back in the reset position
        yield return new WaitUntil(() => Vector3.Distance(transform.position, initPos) < moveSpeed * 0.02f);

        transform.position = initPos;

        rb.velocity = Vector3.zero;

        Reset();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //On collision trigger the squish effect
        if (collision.gameObject.tag == "Player")
        {
            Reset();

            currentState = States.ATTACKED;

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
                currentCoroutine = StartCoroutine(BumpedInto());
            }
        }
    }
}
