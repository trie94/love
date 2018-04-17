using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using UnityEngine.Networking;

public class PieceBehavior : NetworkBehaviour
{
    float range = 5f;
    float speed = 2f;

    Vector3 startPos;
    Vector3 targetPos;
    Vector3 fixPos;
    Vector3 anchor;


    bool isMatch;
    bool isSnapped;
    bool startFollowing;

    float lerpSpeed = 0.2f;

    GameObject net;
    GameObject player;
    NetworkIdentity playerId;
    public GameObject matchedPiece;
    public GameObject matchedGrid;

    public void SetIsMatch(bool _isMatch)
    {
        isMatch = _isMatch;
    }

    public bool GetIsMatch()
    {
        return isMatch;
    }

    void Start()
    {
        anchor = GameSingleton.instance.anchor;
        startPos = transform.position;
        targetPos = Random.insideUnitSphere * range;
        net = GameObject.Find("Net");
    }

    void Update()
    {

    }

    public void Float()
    {
        transform.RotateAround(anchor, Vector3.up, Time.deltaTime * speed);
    }

    public void Match()
    {
        speed = 0f;
    }
}
