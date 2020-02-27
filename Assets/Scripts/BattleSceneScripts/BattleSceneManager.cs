using System.Collections;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class BattleSceneManager : MonoBehaviour
{
    public enum States
    {
        WAITING,
        BATTLE_IN_SESSION,
        END
    }

    public States currentState;

    public List<DefaultBattleScript> entities;
    public List<GiuseppeBattleScript> players;
    public List<GreggBattleScript> enemies;

    public int activeEntity;

    private Coroutine currentCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        currentState = States.WAITING;

        entities = new List<DefaultBattleScript>();

        players = FindObjectsOfType<GiuseppeBattleScript>().ToList();

        players = players.OrderBy(x => x.transform.position.x).ToList();

        enemies = FindObjectsOfType<GreggBattleScript>().ToList();

        enemies = enemies.OrderByDescending(x => x.transform.position.x).ToList();

        foreach (DefaultBattleScript player in players)
        {
            entities.Add(player);
        }

        foreach (DefaultBattleScript enemy in enemies)
        {
            entities.Add(enemy);
        }

        activeEntity = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (activeEntity == entities.Count || activeEntity < 0)
        {
            activeEntity = 0;
        }

        if (enemies.Count == 0 && currentState != States.END)
        {
            currentState = States.END;
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
            default:
                break;
        }
    }

    public void RemoveEntity(DefaultBattleScript entity)
    {
        entities.Remove(entity);

        if (entity.GetComponent<GreggBattleScript>())
        {
            enemies.Remove(entity.GetComponent<GreggBattleScript>());
        }
        else
        {
            players.Remove(entity.GetComponent<GiuseppeBattleScript>());
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
