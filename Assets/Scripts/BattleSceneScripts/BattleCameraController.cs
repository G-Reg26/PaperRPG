using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleCameraController : MonoBehaviour
{
    public Transform playerPoint;
    public Transform enemyPoint;

    public float cameraSpeed;

    private GiuseppeBattleScripts player;

    private Vector3 target;
    private Vector3 initPos;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<GiuseppeBattleScripts>();
        initPos = transform.position;

        ToInitialPoint();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = Vector3.Slerp(transform.position, target, cameraSpeed * Time.deltaTime);
    }

    public void ToInitialPoint()
    {
        target = initPos;
    } 

    public void ZoomToEnemies()
    {
        target = enemyPoint.position;
    }

    public void ZoomToPlayers()
    {
        target = playerPoint.position;
    }
}
