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

	public List<GameObject> spawnedPieces = new List<GameObject> ();

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
	}

    public void AllowSnap(bool _allowSnap)
    {
        allowSnap = _allowSnap;
    }

	public void PieceNum(int _pieceNum)
	{
		pieceNum = _pieceNum;
	}

    //public void SnappedPiece(int _snappedPiece)
    //{
      //  snappedPiece = _snappedPiece;
    //}

    //public void IsSnapped(bool _isSnapped)
    //{
      //  isSnapped = _isSnapped;
    //}

    public void Anchor(Vector3 _anchor)
    {
        anchor = _anchor;
    }

    //public void MatchedPiece(int _matchedPiece)
    //{
      //  matchedPiece = _matchedPiece;
    //}

    public void AddScore()
    {
        totalScore++;
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
