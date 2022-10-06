using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField] List<Role> roles;
    List<Role> _availableRoles = new List<Role>();
    [SerializeField] PlayerManager playerManager;
    [SerializeField] List<GameObject> otherPlayers = new List<GameObject>();
    List<GameObject> _alivePlayers = new List<GameObject>();
    List<GameObject> _nonWerewolfPlayers = new List<GameObject>();
    GameCycle _gameCycleStep = GameCycle.WerewolvesVote;
    bool _firstTurn = true;
    bool _waitingEndStep = true;
    bool _playerIsWereWolf = false;
    GameObject _eatenPlayer = null;
    [SerializeField] Transform playerTransform;
    Transform _playerDefaultTransform;
    [SerializeField] OVRManager ovrManager;
    [SerializeField] Camera playerMainCamera;

    [Header("UI")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject endGameMenu;
    [SerializeField] TextMeshProUGUI endGameText;
    [SerializeField] GameObject announceGO;
    [SerializeField] TextMeshProUGUI announceText;
    bool _gamePaused = false;
    bool _playerDied = false;
    float _changeTimeSeconds = 2.5f;
    float _startAlpha = 0;
    float _endAlpha = 1;
    float _changeRate = 0;
    float _timeSoFar = 0;
    bool _fading = false;


    [Header("Day night cycle")]
    [SerializeField] Material daySkybox;
    [SerializeField] Material nightSkybox;
    [SerializeField] Skybox skybox;
    [SerializeField] GameObject campfireFire;
    [SerializeField] GameObject sunlight;
    [SerializeField] GameObject nightPanel;
    [SerializeField] AudioClip dayAmbiance;
    [SerializeField] AudioClip nightAmbiance;
    bool _dayNightCycle = true; // false = day, true = night

    public static GameManager Instance { get; private set; }
    private void Awake() 
    {
        if (Instance != null && Instance != this) 
            Destroy(this);
        else
            Instance = this;
    }

#region Preparation

    void Start()
    {
        _playerDefaultTransform = new GameObject().transform;
        _playerDefaultTransform.position = playerTransform.position;
        _playerDefaultTransform.rotation = playerTransform.rotation;

        for (int i = 0; i < roles.Count; i++)
            _availableRoles.Add(new Role(roles[i]));

        assignRoles();
        _alivePlayers = otherPlayers;
        _alivePlayers.Add(playerManager.gameObject);
        _waitingEndStep = true;
        StartCoroutine(makeAnnouncement("Le village de thiercelieux s'endort paisiblement", true));
    }

    void assignRoles()
    {
        int rInt = Random.Range(0, roles.Count);
        playerManager.SetRole(_availableRoles[rInt]);
        _playerIsWereWolf = _availableRoles[rInt]._name == "loup-garou";
        if (!_playerIsWereWolf)
            _nonWerewolfPlayers.Add(playerManager.gameObject);

        _availableRoles[rInt].quantity--;
        if (_availableRoles[rInt].quantity == 0)
            _availableRoles.RemoveAt(rInt);

        for (int i = 0; i < otherPlayers.Count; i++) {
            rInt = Random.Range(0, _availableRoles.Count);
            otherPlayers[i].GetComponent<OtherPlayerManager>().SetRole(_availableRoles[rInt]);
            if (_availableRoles[rInt]._name != "loup-garou")
                _nonWerewolfPlayers.Add(otherPlayers[i]);

            _availableRoles[rInt].quantity--;
            if (_availableRoles[rInt].quantity == 0)
                _availableRoles.RemoveAt(rInt);
        }
    }

#endregion
#region Gameplay

    // TODO - Pause : changer l'interface 2D pour une inteface VR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            changePauseState();

        if (!_waitingEndStep && !_playerDied)
            switch (_gameCycleStep) {
                case GameCycle.SetDay:
                    _waitingEndStep = true;
                    changeDayNightCycle();
                    StartCoroutine(makeAnnouncement("Il fait jour, le village se réveille", true, true));
                    break;

                case GameCycle.DeadsAnnouncement:
                    _waitingEndStep = true;
                    if (_eatenPlayer.name == "Player") {
                        StartCoroutine(makeAnnouncement("La nuit dernière vous avez été dévoré par les loups-garou..."));
                        playerManager.Die("La nuit dernière vous avez été dévoré par les loups-garou...");
                        _playerDied = true;
                    } else {
                        foreach (GameObject player in _alivePlayers)
                            if (player.name == _eatenPlayer.name) {
                                player.GetComponent<OtherPlayerManager>().Die();
                                _alivePlayers.Remove(player);
                                break;
                            }
                        StartCoroutine(makeAnnouncement($"La nuit dernière {_eatenPlayer.name} a été mangé(e) par les loups-garou\nIl/elle était {_eatenPlayer.GetComponent<OtherPlayerManager>().GetRole()._name}", true, true));
                    }
                    checkClassVictory();
                    // TODO vérifier conditions de victoire et de mort du joueur mort
                    break;

                case GameCycle.VillagersVote:
                    _waitingEndStep = true;
                    // TODO ajouter (TODO en VR) le choix de la personne à éliminer
                    StartCoroutine(makeAnnouncement("Veuillez désigner une personne qui selon vous est un loup-garou", false, false, true));
                    break;

                case GameCycle.SetNight:
                    _waitingEndStep = true;
                    changeDayNightCycle();
                    StartCoroutine(makeAnnouncement("Le village s'endort", true, true));
                    break;

                case GameCycle.WerewolvesVote:
                    _waitingEndStep = true;
                    // TODO ajouter (TODO en VR) le choix de la personne à éliminer
                    if (_playerIsWereWolf) {
                        StartCoroutine(makeAnnouncement("Les loups-garous se réveillent pour aller manger un(e) villageois(e)\nQui désignez-vous ?", false, false, true));
                        nightPanel.SetActive(false);
                    } else
                        finishWerewolvesVote();
                    break;
            }
    }

    public GameCycle getGameCycleStep()
    {
        return _gameCycleStep;
    }

    public void finishVillagersVote()
    {
        setVisibleSelectButtons(false);

        assignNPCVotes();
        _eatenPlayer = getEliminatedPlayer();
        resetSelectPlayers();

        if (_eatenPlayer.name == "Player") {
            StartCoroutine(makeAnnouncement("Les villageois ont voté pour vous éliminer"));
            playerManager.Die("Le village à voté contre vous...");
            _playerDied = true;
            return;
        } else {
            StartCoroutine(makeAnnouncement($"Les villageois ont voté pour éliminer {_eatenPlayer.name}\nIl/elle était {_eatenPlayer.GetComponent<OtherPlayerManager>().GetRole()._name}", true, true));
            foreach (GameObject player in _alivePlayers)
                if (player.name == _eatenPlayer.name) {
                    player.GetComponent<OtherPlayerManager>().Die();
                    _alivePlayers.Remove(player);
                    break;
                }
        }
        checkClassVictory();
        // TODO vérifier conditions de victoire et de mort du joueur mort
    }

    public void finishWerewolvesVote()
    {
        setVisibleSelectButtons(false);
        nightPanel.SetActive(true);

        if (!_playerIsWereWolf)
            assignWerewolfVotes();
        _eatenPlayer = getEliminatedPlayer();
        resetSelectPlayers();

        if (_playerIsWereWolf)
            StartCoroutine(makeAnnouncement("Les loups-garou se rendorment", true, true));
        else
            StartCoroutine(makeAnnouncement("Les loups-garous se sont réveillés pour dévorer un(e) villageois(e)", true, true));
        nightPanel.SetActive(true);
    }

    int getRemainingPlayerWithRole(string roleName)
    {
        int remainingPlayers = 0;
        foreach (GameObject player in _alivePlayers)
            if (player.GetComponent<InterfacePlayerManager>().GetRole()._name == roleName)
                remainingPlayers++;
        return remainingPlayers;
    }

    void assignNPCVotes()
    {
        int remainingVotes = _alivePlayers.Count - 1;
        for (int i = 0; i < remainingVotes; i++) {
            int rInt = Random.Range(0, _alivePlayers.Count);
           _alivePlayers[rInt].GetComponent<SelectPlayer>().addVote();
        }
    }

    void assignWerewolfVotes()
    {
        int remainingVotes = getRemainingPlayerWithRole("loup-garou");
        if (_playerIsWereWolf)
            remainingVotes--;

        for (int i = 0; i < remainingVotes; i++) {
            int rInt = Random.Range(0, _nonWerewolfPlayers.Count);
            _nonWerewolfPlayers[rInt].GetComponent<SelectPlayer>().addVote();
        }
    }

    void checkClassVictory()
    {
        foreach (Role role in roles) {
            WinCondition[] winConditions = role.winConditions;
            foreach (WinCondition condition in winConditions) {
                switch (condition.winConditionType) {
                    case (WinConditionType.All):
                        if (condition.winConditionStatus == WinConditionStatus.Dead && getRemainingPlayerWithRole(condition.roleName) == 0) {
                            StartCoroutine(makeAnnouncement($"Les {role.namePlurial} ont gagné !"));
                            setEndGame($"Les {role.namePlurial} ont gagné !");
                        } else if (condition.winConditionStatus == WinConditionStatus.Dead && getRemainingPlayerWithRole("loup-garou") > getRemainingPlayerWithRole("villageois(e)")) {
                            StartCoroutine(makeAnnouncement($"Les loups-garou ont gagné !"));
                            setEndGame("Les loups-garou ont gagné !");
                        }
                        break;
                }
            }
        }
    }

    GameObject getEliminatedPlayer()
    {
        GameObject eliminatedPlayer = null;
        foreach (GameObject player in _alivePlayers) {
            if (!eliminatedPlayer)
                eliminatedPlayer = player;
            else if (player.GetComponentInChildren<SelectPlayer>().get() > eliminatedPlayer.GetComponentInChildren<SelectPlayer>().get())
                eliminatedPlayer = player;
        }
        return eliminatedPlayer;
    }

    void resetSelectPlayers()
    {
        foreach (GameObject player in _alivePlayers)
            player.GetComponent<SelectPlayer>().reset();
    }

#endregion
#region UI

    void setVisibleSelectButtons(bool visible)
    {
        foreach (GameObject player in _alivePlayers) {
            Transform selectPlayerGO = player.transform.Find("Canvas/SelectPlayer");
            if (selectPlayerGO != null) {
                if (_playerIsWereWolf && player.GetComponent<OtherPlayerManager>().GetRole()._name == "loup-garou")
                    continue;
                else
                    selectPlayerGO.gameObject.SetActive(visible);
            }
        }
    }

    void setEndGame(string reason = "")
    {
        _playerDied = true;
        endGameMenu.SetActive(true);
        endGameText.text = reason + "\n\nPartie terminée, merci d'avoir joué !";
    }

#endregion
#region Announcement

    IEnumerator makeAnnouncement(string text, bool changeWaitingEndStep = false, bool updateCycleStep = false, bool displaySelection = false)
    {
        string[] texts = text.Split("\n");

        announceGO.SetActive(true);
        announceText.text = "";
        for (int i = 0; i < texts.Length; i++) {
            announceText.text = texts[i];
            FadeIn();
            yield return new WaitForSeconds(3);
        }
        FadeOut();
        yield return new WaitForSeconds(2.5f);
        announceGO.SetActive(false);

        if (_firstTurn && !_playerIsWereWolf) {
            nightPanel.SetActive(true);
            _firstTurn = false;
        }

        if (updateCycleStep) {
            if (_gameCycleStep == GameCycle.WerewolvesVote)
                _gameCycleStep = GameCycle.SetDay;
            else
                _gameCycleStep++;
        }

        if (changeWaitingEndStep)
            _waitingEndStep = !_waitingEndStep;

        if (displaySelection)
            setVisibleSelectButtons(true);
    }

    public void FadeIn()
    {
        _startAlpha = 0;
        _endAlpha = 1;
        _timeSoFar = 0;
        _fading = true;
        StartCoroutine(FadeCoroutine());
    }

    public void FadeOut()
    {
        _startAlpha = 1;
        _endAlpha = 0;
        _timeSoFar = 0;
        _fading = true;
        StartCoroutine(FadeCoroutine());
    }

    IEnumerator FadeCoroutine()
    {
        _changeRate = (_endAlpha - _startAlpha) / _changeTimeSeconds;
        SetAlpha(_startAlpha);
        while (_fading) {
            _timeSoFar += Time.deltaTime;
            if (_timeSoFar > _changeTimeSeconds) {
                _fading = false;
                SetAlpha(_endAlpha);
                yield break;
            } else
                SetAlpha(announceText.color.a + (_changeRate * Time.deltaTime));

            yield return null;
        }
    }

    public void SetAlpha(float alpha)
    {
        Color c = announceText.color;
        c.a = Mathf.Clamp(alpha, 0, 1);
        announceText.color = c;
    }

#endregion
#region Pause/Quit/ResetPosition

    void changePauseState()
    {
        if (_playerDied)
            return;

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

    public void PauseButtonPressed(InputAction.CallbackContext context)
    {
        if (context.performed)
            changePauseState();
    }

    public void ResetPositionButtonHolded(InputAction.CallbackContext context)
    {
        if (context.performed)
            ResetPosition();
    }

    [ContextMenu("Reset position")]
    public void ResetPosition()
    {
        Debug.Log("button pressed");
        // UnityEngine.XR.InputTracking.Recenter();
        // UnityEngine.XR.XRInputSubsystem.TryRecenter();
        ovrManager.RecenterPose();

        // playerTransform.transform.position = _playerDefaultTransform.position;
        // playerTransform.transform.rotation = _playerDefaultTransform.rotation;

        // playerTransform.transform.Rotate(0, _playerDefaultTransform.rotation.eulerAngles.y - playerTransform.rotation.eulerAngles.y, 0);
        // playerTransform.transform.position += _playerDefaultTransform.position - playerTransform.position;

        // playerMainCamera.transform.Rotate(0, _playerDefaultTransform.rotation.eulerAngles.y - playerMainCamera.transform.rotation.eulerAngles.y, 0);
        // playerMainCamera.transform.position += _playerDefaultTransform.position - playerMainCamera.transform.position;
    }


    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

#endregion
#region DayNightCycle

    void changeDayNightCycle()
    {
        _dayNightCycle = !_dayNightCycle;
        if (_dayNightCycle) {
            skybox.material = nightSkybox;
            campfireFire.SetActive(true);
            sunlight.SetActive(false);
            nightPanel.SetActive(true);
            audioSource.clip = nightAmbiance;
            audioSource.Play();
        } else {
            skybox.material = daySkybox;
            campfireFire.SetActive(false);
            sunlight.SetActive(true);
            nightPanel.SetActive(false);
            audioSource.clip = dayAmbiance;
            audioSource.Play();
        }
    }
}
#endregion