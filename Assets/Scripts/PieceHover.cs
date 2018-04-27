using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceHover : MonoBehaviour {

    // blink
    [SerializeField]
    Shader glow;

    Renderer rend;
    [SerializeField]
    float lerpSpeed;
    public bool isBlinking;

    // shiver
    [SerializeField]
    float shiverSpeed;
    public bool isShivering;

    PieceBehavior pieceBehavior;

    void Start()
    {
        rend = GetComponent<Renderer>();
        pieceBehavior = GetComponent<PieceBehavior>();
        rend.material.SetFloat("_RimPower", 1.0f);
    }

    public void Hover()
    {
        StartCoroutine(Glow());
        StartCoroutine(Shiver());
        isBlinking = true;
        isShivering = true;
    }

    public void NotHover()
    {
        isBlinking = false;
        isShivering = false;
        Debug.Log("not hover");
    }

    IEnumerator Glow()
    {
        float lerpTime = 0f;
        float minRimPow = 1.0f;
        float maxRimPow = 6.0f;
        float curRimPow = 6.0f;

        while (true)
        {
            lerpTime += Time.deltaTime * lerpSpeed;
            curRimPow = Mathf.Lerp(minRimPow, maxRimPow, lerpTime);
            rend.material.SetFloat("_RimPower", curRimPow);
            Debug.Log("glow");

            if (lerpTime >= 1f)
            {
                float temp = maxRimPow;
                maxRimPow = minRimPow;
                minRimPow = temp;
                lerpTime = 0f;
            }

            if (!isBlinking)
            {
                Debug.Log("glow break");
                rend.material.SetFloat("_RimPower", 1.0f);
                yield break;
            }

            yield return null;
        }
    }

    IEnumerator Shiver()
    {
        float lerpTime = 0f;
        float randomRange = Random.Range(-0.03f, 0.03f);
        Vector3 startPos = transform.position;
        Vector3 targetPos = transform.position + new Vector3(randomRange, randomRange, randomRange);

        while (true)
        {
            lerpTime += Time.deltaTime * shiverSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, lerpTime);
            Debug.Log("shiver");

            if (lerpTime >= 1f)
            {
                Vector3 temp = startPos;
                startPos = targetPos;
                targetPos = temp;
                randomRange = Random.Range(-0.03f, 0.03f);

                lerpTime = 0f;
            }

            if (!isShivering)
            {
                Debug.Log("shivering break");
                yield break;
            }

            yield return null;
        }
    }
}
