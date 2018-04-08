using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class InputManager : MonoBehaviour {

    [SerializeField]
    GameObject cubePrefab;
    GameObject cube;

    int cubeNum;
    bool offset;

    [SerializeField]
    float zDistance;

    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    AudioClip spawnSound;

	void Start ()
    {
        Debug.Log("start");
	}
	
	void Update ()
    {
        //foreach (var t in Input.touches)
        //{
        //    if (t.phase != TouchPhase.Began)
        //    {
        //        Debug.Log("either moving or...something");
        //        continue;
        //        Debug.Log("actually touching");
        //        Ray ray = Camera.main.ScreenPointToRay(t.position);
        //        RaycastHit hitInfo;

        //        if (Physics.Raycast(ray, out hitInfo))
        //        {
        //            GameObject cube = Instantiate(cubePrefab, hitInfo.point + Vector3.up, Quaternion.identity);
        //            cube.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();
        //            cubeNum++;
        //        }
        //    }
        //}

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Debug.Log("touch");

            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo) && !offset)
            {
                Debug.Log("touch.position: " + touch.position);
                cube = Instantiate(cubePrefab, hitInfo.point, Quaternion.identity);
                cubeNum++;
                offset = true;
                if (!audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(spawnSound);
                }
                StartCoroutine(WaitForTouch());
            }
        }
	}
    
    void OnDisable()
    {
        Debug.Log("cube number: "+ cubeNum);
    }

    IEnumerator WaitForTouch()
    {
        yield return new WaitForSeconds(1f);
        offset = false;
        yield break;
    }
}
