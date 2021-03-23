﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ReturnTitle : MonoBehaviour
{
    bool isActive;
    // Start is called before the first frame update
    void Start()
    {
        isActive = false;
        StartCoroutine(Wait());
    }
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(3);
        isActive = true;
    }
    // Update is called once per frame
    void Update()
    {
        if(isActive)
        {
            if (Input.anyKeyDown)
            {
                Debug.Log("hi");
                StartCoroutine(Close());
            }
        }
    }
    IEnumerator Close()
    {
        GetComponent<Animator>().SetTrigger("end");
        yield return new WaitForSeconds(1.5f);
        if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings);
        }
    }
}
