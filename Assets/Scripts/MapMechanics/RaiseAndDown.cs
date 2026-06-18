using UnityEngine;

public class RaiseAndDown : MonoBehaviour
{
    [Header("∏ﬂ∂»…Ë÷√")]
    public float highY;
    public float lowY;
    public float moveSpeed = 3f;

    private Vector2 targetPos;
    private bool hasPlayer;
    private bool hasMonster;

    void Start()
    {
        targetPos = transform.position;
    }

    void Update()
    {
        if (hasPlayer && hasMonster)
            targetPos.y = lowY;
        else
            targetPos.y = highY;

        transform.position = Vector2.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            hasPlayer = true;
        else if (col.CompareTag("Monster"))
            hasMonster = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
            hasPlayer = false;
        else if (col.CompareTag("Monster"))
            hasMonster = false;
    }
}
