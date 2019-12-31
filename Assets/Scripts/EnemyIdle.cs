using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EnemyIdle : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 target;
    private Vector3 home;
    private Quaternion InitRotation;

    public float Distance;
    public float MoveSpeed;

    private Coroutine CurrentCoroutine;

    private bool noticeChef;
    private bool CurrentState;
    private bool PreviousState;
    private bool LookingAtChef;

    public Transform ChefLocal;
    private Transform EnemyLocal;
    public Transform sprite;
    public Transform feet;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        EnemyLocal = GetComponent<Transform>();
        Distance = 0;
        noticeChef = false;
        CurrentState = false;
        PreviousState = false;
        LookingAtChef = false;
        home = new Vector3(transform.position.x, 1.0f, transform.position.z);
        InitRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        Distance = Vector3.Distance(ChefLocal.localPosition, EnemyLocal.localPosition);

        if (Distance < 2)
        {
            noticeChef = true;
            PreviousState = CurrentState;
            CurrentState = true;
        }
        else
        {
            noticeChef = false;
            PreviousState = CurrentState;
            CurrentState = false;
        }

        if (StateChange(CurrentState, PreviousState) && noticeChef)
        {
            if (CurrentCoroutine != null)
                StopCoroutine(CurrentCoroutine);
            CurrentCoroutine = StartCoroutine(ChaseChef());
        }
        else if (StateChange(CurrentState, PreviousState) && !noticeChef)
        {
            if (CurrentCoroutine != null)
                StopCoroutine(CurrentCoroutine);
            CurrentCoroutine = StartCoroutine(GoHome());
        }

    }

    public bool StateChange(bool State1, bool State2)
    {
        if (State1 != State2)
        {
            return true;
        }
        return false;
    }

    public IEnumerator IdleWalking()
    {
        while (!noticeChef)
        {

            yield return null;
        }

        CurrentCoroutine = null;
    }

    public IEnumerator ChaseChef()
    {
        while (noticeChef)
        {
            Vector3 ChefGrounded = new Vector3(ChefLocal.localPosition.x, transform.position.y, ChefLocal.localPosition.z);
            transform.LookAt(ChefGrounded);
            transform.position += transform.forward * MoveSpeed * Time.deltaTime;

            yield return null;
        }

        CurrentCoroutine = null;
    }

    public IEnumerator GoHome()
    {
        while (Vector3.Distance(home, transform.position) > 0.1)
        {
            Vector3 RestingPosition = new Vector3(home.x, transform.position.y, home.z);
            transform.LookAt(RestingPosition);
            transform.position += transform.forward * (MoveSpeed / 1) * Time.deltaTime;

            yield return null;
        }

        CurrentCoroutine = null;
        CurrentCoroutine = StartCoroutine(IdleWalking());
    }

}