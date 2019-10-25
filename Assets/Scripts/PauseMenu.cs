using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public bool isPaused;

    public GameObject pauseMenu;
    private GameObject[] players;

	// Use this for initialization
	void Start ()
    {
        pauseMenu.SetActive(false);
        players = GameObject.FindGameObjectsWithTag("Player");
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseCheck();
        }
	}

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        isPaused = true;
        
        foreach (GameObject player in players)
        {
            player.GetComponent<SpriteRenderer>().sortingOrder = 0;
        }
        
        Cursor.visible = true;
        Time.timeScale = 0;
    }

    public void QuitGame()
    {
        SceneManager.LoadScene("GameMode Selection");
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        isPaused = false;

        foreach (GameObject player in players)
        {
            player.GetComponent<SpriteRenderer>().sortingOrder = 1;
        }

        Cursor.visible = true;
        Time.timeScale = 1;
    }

    private void PauseCheck()
    {
        if (isPaused)
        {
            ResumeGame();
        }

        else if (!isPaused)
        {
            PauseGame();
        }
    }
}
