using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameSingleton : NetworkBehaviour {

    public static GameSingleton instance;

    [SyncVar]
    public bool allowSnap;
    [SyncVar]
    public int pieceNum;
    [SyncVar]
    public Vector3 anchor;
    [SyncVar]
    public float playTime = 0f;
    [SyncVar]
    public int totalScore = 0;
    [SyncVar]
    public bool isPieceAbsorbed;
    [SyncVar]
    public GameObject targetGrid;

    public List<GameObject> wallGrids = new List<GameObject>();

    public List<GameObject> targetGrids = new List<GameObject>();

    public List<GameObject> spawnedPieces = new List<GameObject>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("more than one singleton.");
        }
    }

	void Start()
	{
		if (spawnedPieces == null)
		{
			spawnedPieces.Clear();
		}

        if (wallGrids == null)
        {
            wallGrids.Clear();
        }

        targetGrid = null;
	}

    public void AllowSnap(bool _allowSnap)
    {
        allowSnap = _allowSnap;
    }

	public void PieceNum(int _pieceNum)
	{
		pieceNum = _pieceNum;
	}

    public void Anchor(Vector3 _anchor)
    {
        anchor = _anchor;
    }

    public void AddScore()
    {
        totalScore++;
    }

    public void AddTargetGrid(GameObject _targetGrid)
    {
        targetGrids.Add(_targetGrid);
    }

    public void SetTargetGrid(GameObject _targetGrid)
    {
        targetGrid = _targetGrid;
    }

    public void SetIsPieceAbsorbed(bool _isPieceAbsorbed)
    {
        isPieceAbsorbed = _isPieceAbsorbed;
    }

    public string PrintScore()
    {
        return totalScore.ToString();
    }

    public void CountTime()
    {
        playTime += Time.deltaTime;
    }

    public string PrintTime()
    {
        return playTime.ToString();
    }
}
