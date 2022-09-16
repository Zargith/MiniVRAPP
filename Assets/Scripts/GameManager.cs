using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    [SerializeField] PlayerManager playerManager;
    [SerializeField] List<Role> roles;
    List<Role> _availableRoles = new List<Role>();

    bool _gamePaused = false;
    [SerializeField] GameObject pauseMenu;

    public static GameManager Instance { get; private set; }
    private void Awake() 
    {
        if (Instance != null && Instance != this) 
            Destroy(this);
        else
            Instance = this;
    }

    void Start()
    {
        for (int i = 0; i < roles.Count; i++)
            _availableRoles.Add(new Role(roles[i]));

        System.Random rand = new System.Random();
        int rInt = rand.Next(0, roles.Count);
        playerManager.SetRole(_availableRoles[rInt]);

        _availableRoles[rInt].quantity--;
        if (_availableRoles[rInt].quantity == 0)
            _availableRoles.RemoveAt(rInt);
    }

    // TODO - Pause : changer l'interface 2D pour une inteface VR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            changePauseState();
    }

    void changePauseState()
    {
        _gamePaused = !_gamePaused;
        if (_gamePaused)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
        pauseMenu.SetActive(_gamePaused);
    }

    public void PauseGame()
    {
        _gamePaused = true;
        Time.timeScale = 0;
        pauseMenu.SetActive(_gamePaused);
    }

    public void ResumeGame()
    {
        _gamePaused = false;
        Time.timeScale = 1;
        pauseMenu.SetActive(_gamePaused);
    }
}
