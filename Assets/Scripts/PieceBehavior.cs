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
    Collider col;

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
        col = GetComponent<Collider>();
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
    }

    IEnumerator Absorb()
    {
        transform.parent = null;
        isMatch = false;
        isAbsorbed = true;

        speed = 0f;
        float lerpTime = 0f;
        float absorbSpeed = 0.5f;

        while (true)
        {
            lerpTime += Time.deltaTime * absorbSpeed;

            if (lerpTime >= 1f)
            {
                col.isTrigger = false;
                col.enabled = false;
                //transform.rotation = Quaternion.identity;
                GameSingleton.instance.AddScore();
                this.enabled = false;
                yield break;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, matchedGrid.transform.position, lerpTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, lerpTime);
            }
            yield return null;
        }
    }
}
