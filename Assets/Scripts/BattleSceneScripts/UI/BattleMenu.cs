using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleMenu : MonoBehaviour
{
    public enum States
    {
        IDLE,
        EXPANDING,
        SELECTING,
        TARGETING,
        SHRINKING,
        STATES
    }

    public States currentState;
    
    public Font font;

    public int fontSize;

    public float padding;
    public float menuScaleSpeed;
    public float knifeSpeed;

    private GiuseppeBattleScript player;
    private BattleSceneManager manager;

    private List<GreggBattleScript> greggs;

    private Coroutine currentCoroutine;

    private GameObject[] attacks;

    private ScrollRect scrollRect;
    private RectTransform content;
    private Image knife;

    private Vector2Int menuRange;
    private int menuItem;

    private Vector2Int greggRange;
    private int gregg;

    private Vector3 initKnifePos;

    private bool canScroll;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponentInParent<GiuseppeBattleScript>();
        manager = FindObjectOfType<BattleSceneManager>();

        greggs = new List<GreggBattleScript>();

        foreach (GreggBattleScript gregg in FindObjectsOfType<GreggBattleScript>())
        {
            greggs.Add(gregg);
        }

        currentCoroutine = null;
        
        transform.localScale = new Vector3(0.0f, 0.0f, 1.0f);

        scrollRect = GetComponentInChildren<ScrollRect>();
        content = scrollRect.viewport.GetComponentInChildren<RectTransform>();
        knife = scrollRect.transform.Find("Knife").GetComponent<Image>();

        SetMenuItems();

        menuRange = new Vector2Int(0, attacks.Length - 1);
        menuItem = menuRange.x;

        greggRange = new Vector2Int(0, greggs.Count - 1);
        gregg = greggRange.x;

        canScroll = true;

        initKnifePos = knife.transform.position;
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
        float angle = 0.0f;

        switch (currentState)
        {
            case States.EXPANDING:
                if (currentCoroutine == null)
                {
                    currentCoroutine = StartCoroutine(ExpandMenu());
                }
                break;
            case States.SELECTING:
                if (Input.GetAxisRaw("Vertical") == 0.0f)
                {
                    canScroll = true;
                }
                else
                {
                    if (canScroll)
                    {
                        // go up or wrap around to the bottom
                        if (Input.GetAxis("Vertical") > 0.0f)
                        {
                            menuItem = menuItem - 1 < menuRange.x ? menuRange.y : menuItem - 1;
                            canScroll = false;
                        }
                        // go down or wrap around to the top
                        else if (Input.GetAxis("Vertical") < 0.0f)
                        {
                            menuItem = menuItem + 1 > menuRange.y ? menuRange.x : menuItem + 1;
                            canScroll = false;
                        }
                    }
                }

                // select menu item
                if (Input.GetButtonDown("Jump"))
                {
                    currentState = States.TARGETING;
                    break;
                }

                // move knife to allign with current menu item
                Vector3 target = new Vector3(initKnifePos.x - 0.5f, attacks[menuItem].GetComponent<Text>().transform.position.y, initKnifePos.z);

                knife.transform.position = Vector3.Lerp(knife.transform.position, target, knifeSpeed * Time.deltaTime);

                angle = Vector3.SignedAngle(knife.transform.right, Vector3.right, knife.transform.forward);

                knife.transform.rotation *= Quaternion.AngleAxis(angle * knifeSpeed * Time.deltaTime, knife.transform.forward);
                break;
            case States.TARGETING:
                if (Input.GetAxisRaw("Horizontal") == 0.0f)
                {
                    canScroll = true;
                }
                else
                {
                    if (canScroll)
                    {
                        // go up or wrap around to the bottom
                        if (Input.GetAxis("Horizontal") > 0.0f)
                        {
                            gregg = gregg + 1 > greggRange.y ? greggRange.x : gregg + 1;
                            canScroll = false;
                        }
                        // go down or wrap around to the top
                        else if (Input.GetAxis("Horizontal") < 0.0f)
                        {
                            gregg = gregg - 1 < greggRange.x ? greggRange.y : gregg - 1;
                            canScroll = false;
                        }
                    }
                }

                if (Input.GetButtonDown("Cancel"))
                {
                    currentState = States.SELECTING;
                    break;
                }

                // select menu item
                if (Input.GetButtonDown("Jump"))
                {
                    currentState = States.SHRINKING;
                    break;
                }

                knife.transform.position = Vector3.Lerp(knife.transform.position, greggs[gregg].selectPoint.position, knifeSpeed * Time.deltaTime);

                angle = Vector3.SignedAngle(knife.transform.right, Vector3.down, knife.transform.forward);

                knife.transform.rotation *= Quaternion.AngleAxis(angle * knifeSpeed * Time.deltaTime, knife.transform.forward);

                break;
            case States.SHRINKING:
                if (currentCoroutine == null)
                {
                    player.target = greggs[gregg].transform;
                    currentCoroutine = StartCoroutine(ShrinkMenu(menuItem));

                    menuItem = menuRange.x;
                    gregg = greggRange.x;
                }
                break;
        }
    }

    public IEnumerator ExpandMenu()
    {
        while (transform.localScale.x < 1.0f)
        {
            float n = transform.localScale.x + (menuScaleSpeed * Time.deltaTime);
            transform.localScale = new Vector3(n, n, 1.0f);

            yield return null;

            scrollRect.verticalScrollbar.value = 1.0f;
        }

        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        currentState = States.SELECTING;
        currentCoroutine = null;
    }

    public IEnumerator ShrinkMenu(int i)
    {
        while (transform.localScale.x > 0.0f)
        {
            float n = transform.localScale.x - (menuScaleSpeed * Time.deltaTime);
            transform.localScale = new Vector3(n, n, 1.0f);

            yield return null;
        }

        transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);

        currentState = States.IDLE;
        currentCoroutine = null;

        knife.transform.position = initKnifePos;
        knife.transform.rotation = Quaternion.identity;

        player.Attack(i);
    }
}
