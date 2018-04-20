using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class Spawner : NetworkBehaviour
{
    [SerializeField] GameObject[] piecesPrefab;
    [SerializeField] int piecesNum;

    [SerializeField] GameObject wallPrefab;

    [SerializeField] float height = 1f;

    bool didSpawn;

    [SerializeField] GameObject[] spawnablesPrefab;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip spawnSound;

    Vector3 anchor;

    void Start()
    {
        if (!isServer)
        {
            this.enabled = false;
            return;
        }

        anchor = transform.position;
        GameSingleton.instance.anchor = anchor;
        CmdSpawnPieces();
        CmdSpawnWall();
        CmdSpawnables();
    }

    [Command]
    void CmdSpawnPieces()
    {
        // spawn pieces
        for (int i = 0; i < piecesNum; i++)
        {
            float yRange = Random.Range(0, 3.5f);
            float xRange = Random.Range(-3f, 3f);
            float zRange = Random.Range(-3f, 3f);
            int index = i % piecesPrefab.Length;
            GameObject pieces = Instantiate(piecesPrefab[index], anchor + new Vector3(xRange, yRange, zRange), Random.rotation);

            NetworkServer.Spawn(pieces);
            // store piece list
            GameSingleton.instance.spawnedPieces.Add(pieces);
            Debug.Log("spawn");
            // store piece num info
            GameSingleton.instance.PieceNum(piecesNum);
            // allow snapping interaction
            GameSingleton.instance.AllowSnap(true);
        }
    }

    [Command]
    void CmdSpawnWall()
    {
        GameObject wall = Instantiate(wallPrefab, anchor + new Vector3(0f, 0f, 2f), Quaternion.identity);
        NetworkServer.Spawn(wall);

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(spawnSound);
        }

        Debug.Log("wall");
    }

    [Command]
    void CmdSpawnables()
    {
        for (int i = 0; i < spawnablesPrefab.Length; i++)
        {
            GameObject spawnables = Instantiate(spawnablesPrefab[i]);
            NetworkServer.Spawn(spawnables);
        }
    }
}
