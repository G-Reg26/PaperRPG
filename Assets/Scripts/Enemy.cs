using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public Transform mesh;
    public Coroutine currentCoroutine;

    public float initSNSpeed;

    //Controls degree of squish
    private float SquashDegree;

    //Controls speed of squishing eff
    [SerializeField]
    private float SNSpeed;

	// Use this for initialization
	void Start () {
        //Reset mesh values to an original state
        Reset();

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

    private void OnCollisionEnter(Collision collision)
    {
        
        //On collision trigger the squish effect
        if (collision.gameObject.tag == "Player")
        {
            //Cancel current coroutine if one is active
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }
            
            currentCoroutine = StartCoroutine(SquashNStretch());
        }
    }
}
