using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class BattleSceneManager : MonoBehaviour
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

    public Transform menu;
    public Font font;
    public int fontSize;
    public float padding;

    public float menuScaleSpeed;

    private GameObject[] attacks;

    private GiuseppeBattleScript player;
    private GreggBattleScript gregg;

    private ScrollRect scrollRect;
    private RectTransform content;
    private Image knife;

    private Vector2Int menuRange;
    private int i;

    private bool scroll;
    [SerializeField]
    private bool playerAttack;

    // Start is called before the first frame update
    void Start()
    {
        currentState = States.EXPANDING;

        player = FindObjectOfType<GiuseppeBattleScript>();
        gregg = FindObjectOfType<GreggBattleScript>();

        // set ui components
        menu.transform.localScale = new Vector3(0.0f, 0.0f, 1.0f);
        scrollRect = menu.GetComponentInChildren<ScrollRect>();
        content = scrollRect.viewport.GetComponentInChildren<RectTransform>();
        knife = scrollRect.transform.Find("Knife").GetComponent<Image>();

        SetMenuItems();

        menuRange = new Vector2Int(0, attacks.Length - 1);
        i = menuRange.x;

        scroll = true;
        playerAttack = true;

        StartCoroutine(Wait());
    }

    void SetMenuItems()
    {
        // create a ui game object for each player attack
        attacks = new GameObject[player.attacks.Length];

        // the width for each object will be the same width as the scroll rect
        float width = scrollRect.GetComponent<RectTransform>().rect.width;

        // set height of the content rect
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (fontSize + padding) * player.attacks.Length);

        for (int i = 0; i < player.attacks.Length; i++)
        {
            // create new ui game object
            attacks[i] = new GameObject("" + i, typeof(RectTransform));
            attacks[i].transform.parent = content;

            // set rect transform fields
            RectTransform rt = attacks[i].GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0.0f, 1.0f);
            rt.anchorMax = new Vector2(0.0f, 1.0f);
            rt.pivot = Vector2.zero;

            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, fontSize);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

            rt.anchoredPosition = new Vector3(0.0f, -((fontSize + padding) * (i + 1)), 0.0f);
            rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0.0f);
            rt.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            // add and set text component in attack game object
            Text text = attacks[i].AddComponent<Text>();

            text.color = Color.black;
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.supportRichText = true;
            text.alignByGeometry = true;
            text.text = player.attacks[i].GetType().Name;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gregg.currentState == DefaultBattleScript.States.WAITING && player.currentState == DefaultBattleScript.States.WAITING)
        {
            playerAttack = !playerAttack;

            if (playerAttack)
            {
                player.currentState = DefaultBattleScript.States.READY;
            }
            else
            {
                gregg.currentState = DefaultBattleScript.States.READY;
            }
        }

        switch (currentState)
        {
            case States.SELECTING:
                // scroll through menu
                // if stick is idle
                if (Input.GetAxisRaw("Vertical") == 0.0f)
                {
                    scroll = true;
                }
                else
                {
                    if (scroll)
                    {
                        // go up or wrap around to the bottom
                        if (Input.GetAxis("Vertical") > 0.0f)
                        {
                            i = i - 1 < menuRange.x ? menuRange.y : i - 1;
                            scroll = false;
                        }
                        // go down or wrap around to the top
                        else if (Input.GetAxis("Vertical") < 0.0f)
                        {
                            i = i + 1 > menuRange.y ? menuRange.x : i + 1;
                            scroll = false;
                        }
                    }
                }

                // select menu item
                if (Input.GetButtonDown("Jump"))
                {
                    currentState = States.SHRINKING;

                    StartCoroutine(ShrinkMenu(i));

                    i = menuRange.x;

                    break;
                }

                // move knife to allign with current menu item
                Vector3 target = new Vector3(knife.rectTransform.anchoredPosition.x, attacks[i].GetComponent<Text>().rectTransform.anchoredPosition.y + 0.1f, 0.0f);

                knife.rectTransform.anchoredPosition = Vector3.Lerp(knife.rectTransform.anchoredPosition, target, 15.0f * Time.deltaTime);
                break;
            case States.WAITING:
                if (player.currentState == DefaultBattleScript.States.READY)
                {
                    currentState = States.EXPANDING;

                    StartCoroutine(ExpandMenu());
                }
                else if (gregg.currentState == DefaultBattleScript.States.READY)
                {
                    gregg.currentState = DefaultBattleScript.States.ATTACKING;

                    gregg.Attack(0);
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
