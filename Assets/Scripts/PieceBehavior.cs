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
    public Collider col;
    GameObject player;

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
        player = GameObject.FindGameObjectWithTag("MainCamera");
    }

    void Update()
    {
        if (isMatch)
        {
            Match();
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
        isMatch = false;
        transform.parent = null;
        isAbsorbed = true;

        speed = 0f;
        float lerpTime = 0f;
        float absorbSpeed = 0.5f;

        while (true)
        {
            lerpTime += Time.deltaTime * absorbSpeed;

            if (lerpTime >= 1f)
            {
                player.GetComponent<PlayerBehaviorNetworking>().CmdDestoryCollider(this.gameObject);
                //col.isTrigger = false;
                //col.enabled = false;
                //this.enabled = false;
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
