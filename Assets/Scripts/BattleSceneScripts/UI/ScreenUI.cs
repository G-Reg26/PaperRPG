using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenUI : MonoBehaviour
{
    public GameObject healthBox;

    public Vector2 leftCorner;
    public Vector2 rightCorner;

    public float offset;

    private BattleSceneManager manager;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitForManager());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator WaitForManager()
    {
        yield return new WaitUntil(() => FindObjectOfType<BattleSceneManager>() != null);

        manager = FindObjectOfType<BattleSceneManager>();

        foreach (DefaultBattleScript player in manager.players)
        {
            GameObject temp = Instantiate(healthBox, transform);

            RectTransform rectTransform = temp.GetComponent<RectTransform>();

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;

            rectTransform.anchoredPosition = leftCorner;

            leftCorner.x += offset;

            temp.GetComponentInChildren<HealthText>().stats = player.GetComponent<BattleStats>();
        }

        foreach (DefaultBattleScript enemy in manager.enemies)
        {
            GameObject temp = Instantiate(healthBox, transform);

            RectTransform rectTransform = temp.GetComponent<RectTransform>();

            rectTransform.anchorMin = new Vector2(1.0f, 0.0f);
            rectTransform.anchorMax = new Vector2(1.0f, 0.0f);

            rectTransform.anchoredPosition = rightCorner;

            rightCorner.x -= offset;

            temp.GetComponentInChildren<HealthText>().stats = enemy.GetComponent<BattleStats>();
        }
    }
}
