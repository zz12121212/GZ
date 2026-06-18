using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiseDoor : MonoBehaviour
{
    public GameObject Door;
    private bool isMoving = false;
    public Vector3 targetPosition;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player" && !isMoving)
        {
            StartCoroutine(MoveDoor());
        }
    }

    IEnumerator MoveDoor()
    {
        isMoving = true;
        Vector3 startPos = gameObject.transform.position;
        Vector3 endPos = targetPosition;
        float duration = 0.3f;
        float elapsed = 0f;

        Door.transform.position = startPos;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Door.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        Door.transform.position = endPos;
        isMoving = false;
    }
}

