using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum States
    {
        EXPANDING,
        SELECTING,
        SHRINKING,
        WAITING,
        STATES
    }

    public States currentState;

    public PlayerController player;

    public GameObject[] attacks;

    public Transform menu;
    public RectTransform content;
    public ScrollRect scrollRect;
    public Font font;
    public Image knife;

    public float menuScaleSpeed;
    public float fontSize;

    private Vector2Int range;

    private bool scroll;
    private int i;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>();

        menu.transform.localScale = new Vector3(0.0f, 0.0f, 1.0f);

        currentState = States.EXPANDING;

        SetMenuItems();

        range = new Vector2Int(0, attacks.Length - 1);

        i = range.x;

        StartCoroutine(Wait());
    }

    void SetMenuItems()
    {
        attacks = new GameObject[player.attacks.Length];

        float width = content.parent.parent.GetComponent<RectTransform>().rect.width;

        Debug.Log(width);

        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (fontSize + 10.0f) * player.attacks.Length);

        for (int i = 0; i < player.attacks.Length; i++)
        {
            attacks[i] = new GameObject("" + i, typeof(RectTransform));
            attacks[i].AddComponent<Text>();
            attacks[i].transform.parent = content;

            RectTransform rt = attacks[i].GetComponent<RectTransform>();

            rt.localPosition = new Vector3(0.0f, -(fontSize + 10.0f) * (i + 2), 0.0f);

            rt.anchorMin = new Vector2(0.0f, 1.0f);
            rt.anchorMax = new Vector2(0.0f, 1.0f);
            rt.pivot = Vector2.zero;

            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, fontSize + 10.0f);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rt.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            Text text = attacks[i].GetComponent<Text>();

            text.color = Color.black;
            text.font = font;
            text.fontSize = (int)fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.supportRichText = true;
            text.alignByGeometry = true;
            text.text = player.attacks[i].GetType().Name;
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case States.SELECTING:
                if (Input.GetAxis("Vertical") > 0.0f)
                {
                    if (scroll)
                    {
                        i = i - 1 < range.x ? range.y : i - 1;
                        scroll = false;
                    }
                }
                else if (Input.GetAxis("Vertical") < 0.0f)
                {
                    if (scroll)
                    {
                        i = i + 1 > range.y ? range.x : i + 1;
                        scroll = false;
                    }
                }
                else
                {
                    scroll = true;
                }

                if (Input.GetButtonDown("Jump"))
                {
                    currentState = States.SHRINKING;

                    StartCoroutine(ShrinkMenu(i));

                    i = range.x;

                    break;
                }

                Vector3 target = new Vector3(knife.rectTransform.position.x, attacks[i].GetComponent<Text>().rectTransform.position.y + 0.1f, knife.rectTransform.position.z);

                knife.rectTransform.position = Vector3.Lerp(knife.rectTransform.position, target, 15.0f * Time.deltaTime);
                break;
            case States.WAITING:
                if (player.currentState == PlayerController.States.WAITING)
                {
                    currentState = States.EXPANDING;

                    StartCoroutine(ExpandMenu());
                }
                break;
        }
    }

    public IEnumerator Wait()
    {
        yield return new WaitForSeconds(1.0f);

        StartCoroutine(ExpandMenu());
    }

    public IEnumerator ExpandMenu()
    {
        while (menu.localScale.x < 1.0f)
        {
            float n = menu.localScale.x + (menuScaleSpeed * Time.deltaTime);
            menu.localScale = new Vector3(n, n, 1.0f);

            yield return null;

            scrollRect.verticalScrollbar.value = 1.0f;
        }

        menu.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        currentState = States.SELECTING;
    }

    public IEnumerator ShrinkMenu(int i)
    {
        while (menu.localScale.x > 0.0f)
        {
            float n = menu.localScale.x - (menuScaleSpeed * Time.deltaTime);
            menu.localScale = new Vector3(n, n, 1.0f);

            yield return null;
        }

        menu.localScale = new Vector3(0.0f, 0.0f, 0.0f);

        currentState = States.WAITING;

        player.Attack(i);
    }
}
