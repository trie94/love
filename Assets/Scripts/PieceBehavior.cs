using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using UnityEngine.Networking;

public class PieceBehavior : NetworkBehaviour
{
    float speed = 2f;

    Vector3 anchor;
    public GameObject matchedGrid;
    [SerializeField] Collider col;

    Vector3 stopPos;

    bool isMatch;
    public void SetIsMatch(bool _isMatch)
    {
        isMatch = _isMatch;
    }

    public bool GetIsMatch()
    {
        return isMatch;
    }

    bool isSnapped;
    bool startFollowing;

    bool isAbsorbed;
    public void SetIsAbsorbed(bool _isAbsorbed)
    {
        isAbsorbed = _isAbsorbed;
    }

    public bool GetIsAbsorbed()
    {
        return isAbsorbed;
    }


    void Start()
    {
        anchor = GameSingleton.instance.anchor;
    }

    void Update()
    {
        if (isMatch)
        {
            Match();
        }

        if (transform.parent)
        {
            //Stop();
        }

        else
        {
            //Float();
        }
    }

    void Float()
    {
        transform.RotateAround(anchor, Vector3.up, Time.deltaTime * speed);
    }

    void Stop()
    {
        speed = 0f;
    }

    void Match()
    {
        StartCoroutine(Absorb());
        isMatch = false;
    }

    IEnumerator Absorb()
    {
        speed = 0f;
        float lerpTime = 0f;
        float absorbSpeed = 0.5f;

        while (true)
        {
            lerpTime += Time.deltaTime * absorbSpeed;

            if (lerpTime >= 1f)
            {
                isAbsorbed = true;
                transform.parent = null;
                col.enabled = false;
                yield break;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, matchedGrid.transform.position, lerpTime);
            }
            yield return null;
        }
    }
}
