using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class BattleSceneManager : MonoBehaviour
{
    public enum States
    {
        WAITING,
        BATTLE_IN_SESSION
    }

    public States currentState;

    public List<DefaultBattleScript> entities;
    public int activeEntity;

    private Coroutine currentCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        currentState = States.WAITING;

        entities = new List<DefaultBattleScript>();

        foreach (GiuseppeBattleScript player in FindObjectsOfType<GiuseppeBattleScript>())
        {
            entities.Add(player);
        }

        foreach (GreggBattleScript enemy in FindObjectsOfType<GreggBattleScript>())
        {
            entities.Add(enemy);
        }

        activeEntity = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (activeEntity == entities.Count)
        {
            activeEntity = 0;
        }

        switch (currentState)
        {
            case States.WAITING:
                if (currentCoroutine == null)
                {
                    currentCoroutine = StartCoroutine(Wait());
                }
                break;
            case States.BATTLE_IN_SESSION:
                foreach (DefaultBattleScript entity in entities)
                {
                    if (entity.currentState == DefaultBattleScript.States.WAITING)
                    {
                        if (entity == entities[entities.Count - 1])
                        {
                            currentState = States.WAITING;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                break;
        }
    }

    public IEnumerator Wait()
    {
        yield return new WaitForSeconds(1.0f);

        if (entities[activeEntity].GetComponent<GiuseppeBattleScript>() != null)
        {
            BattleMenu menu = entities[activeEntity].GetComponentInChildren<BattleMenu>();

            menu.currentState = BattleMenu.States.EXPANDING;

            yield return new WaitUntil(() => menu.currentState == BattleMenu.States.IDLE);
        }
        else
        {
            entities[activeEntity].Attack(0);
        }

        activeEntity++;

        currentState = States.BATTLE_IN_SESSION;

        currentCoroutine = null;
    }
}
