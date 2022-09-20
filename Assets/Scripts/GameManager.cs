using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    string _eatenPlayerName = "";


    [Header("UI")]
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject announceGO;
    [SerializeField] TextMeshProUGUI announceText;
    bool _gamePaused = false;
    bool _playerDied = false;
    float changeTimeSeconds = 2.5f;
    float startAlpha = 0;
    float endAlpha = 1;
    float changeRate = 0;
    float timeSoFar = 0;
    bool fading = false;


    [Header("Day night cycle")]
    [SerializeField] Material daySkybox;
    [SerializeField] Material nightSkybox;
    [SerializeField] Skybox skybox;
    [SerializeField] GameObject campfireFire;
    [SerializeField] GameObject sunlight;
    [SerializeField] GameObject nightPanel;
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
        for (int i = 0; i < roles.Count; i++)
            _availableRoles.Add(new Role(roles[i]));

        assignRoles();
        _alivePlayers = otherPlayers;
        _alivePlayers.Add(playerManager.gameObject);
        _waitingEndStep = true;
        StartCoroutine(makeAnnouncement("Les villageois(es) du village de thiercelieux s'endorment paisiblement", true));
    }

    void assignRoles()
    {
        System.Random rand = new System.Random();
        int rInt = rand.Next(0, roles.Count);
        playerManager.SetRole(_availableRoles[rInt]);
        _playerIsWereWolf = _availableRoles[rInt]._name == "loup-garou";
        if (!_playerIsWereWolf)
            _nonWerewolfPlayers.Add(playerManager.gameObject);

        _availableRoles[rInt].quantity--;
        if (_availableRoles[rInt].quantity == 0)
            _availableRoles.RemoveAt(rInt);

        for (int i = 0; i < otherPlayers.Count; i++) {
            rand = new System.Random();
            rInt = rand.Next(0, _availableRoles.Count);
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

        if (!_waitingEndStep)
            switch (_gameCycleStep) {
                case GameCycle.SetDay:
                    _waitingEndStep = true;
                    changeDayNightCycle();
                    StartCoroutine(makeAnnouncement("Il fait jour, le village se réveille", true, true));
                    break;

                case GameCycle.DeadsAnnouncement:
                    _waitingEndStep = true;
                    // TODO dire qui a été tué et faire une animation de mort (besoin d'une autre step ?)
                    if (_eatenPlayerName == "Player") {
                        StartCoroutine(makeAnnouncement($"La nuit dernière vous avez été mangé par les loups-garou"));
                        playerManager.Die();
                        _playerDied = true;
                    } else {
                        // int[] indexToRemove = new int[0];
                        foreach (GameObject player in _alivePlayers)
                            if (player.name == _eatenPlayerName) {
                                player.GetComponent<OtherPlayerManager>().Die();
                                _alivePlayers.Remove(player);
                                break;
                            }
                        StartCoroutine(makeAnnouncement($"La nuit dernière {_eatenPlayerName} a été mangé(e) par les loups-garou\nVous devez maintenant choisir une personne du village à éliminer", true, true));
                    }
                    // TODO vérifier conditions de victoire et de mort du joueur
                    break;

                case GameCycle.VillagersVote:
                    _waitingEndStep = true;
                    // TODO ajouter (TODO en VR) le choix de la personne à éliminer
                    StartCoroutine(makeAnnouncement("Veuillez désigner une personne qui selon vous est un loup-garou", false, false, true));
                    // setVisibleSelectButtons(true);
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
                        StartCoroutine(makeAnnouncement("Les loups-garous se réveillent pour aller manger un villageois\nQui désignez-vous ?", false, false, true));
                        // setVisibleSelectButtons(true);
                    } else {
                        StartCoroutine(makeAnnouncement("Les loups-garous se réveillent pour aller manger un villageois"));
                        finishWerewolvesVote();
                    }
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
        _eatenPlayerName = getEliminatedPlayerName();
        resetSelectPlayers();

        if (_eatenPlayerName == "Player")
            StartCoroutine(makeAnnouncement("Les villageois ont voté pour vous éliminer", true, true));
        else
            StartCoroutine(makeAnnouncement($"Les villageois ont voté pour éliminer {_eatenPlayerName}", true, true));
        // TODO vérifier conditions de victoire et de mort du joueur
    }

    public void finishWerewolvesVote()
    {
        nightPanel.SetActive(true);
        setVisibleSelectButtons(false);

        assignWerewolfVotes();
        _eatenPlayerName = getEliminatedPlayerName();
        resetSelectPlayers();

        StartCoroutine(makeAnnouncement("Les loups-garou se rendorment", true, true));
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
            Debug.LogError("====================================");
            System.Random rand = new System.Random();
            int rInt = rand.Next(0, _alivePlayers.Count);
            Debug.LogError($"{i} {_alivePlayers[rInt].name} {remainingVotes}");
            _alivePlayers[rInt].GetComponent<SelectPlayer>().select();
        }
    }

    void assignWerewolfVotes()
    {
        int remainingVotes = getRemainingPlayerWithRole("loup-garou");
        if (_playerIsWereWolf)
            remainingVotes--;

        // for (int i = 0; i < remainingVotes; i++) {
            // Debug.LogError("====================================");
            System.Random rand = new System.Random();
            int rInt = rand.Next(0, _nonWerewolfPlayers.Count);
            // Debug.LogError($"{i} {_nonWerewolfPlayers[rInt].name} {remainingVotes}");
            _nonWerewolfPlayers[rInt].GetComponent<SelectPlayer>().select();
        // }
    }

    string getEliminatedPlayerName()
    {
        GameObject eliminatedPlayer = null;
        foreach (GameObject player in _alivePlayers) {
            if (!eliminatedPlayer)
                eliminatedPlayer = player;
            else if (player.GetComponentInChildren<SelectPlayer>().get() > eliminatedPlayer.GetComponentInChildren<SelectPlayer>().get())
                eliminatedPlayer = player;
        }
        return eliminatedPlayer.name;
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
            if (selectPlayerGO != null)
                selectPlayerGO.gameObject.SetActive(visible);
        }
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
            yield return new WaitForSeconds(5);
        }
        FadeOut();
        yield return new WaitForSeconds(3);
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
        startAlpha = 0;
        endAlpha = 1;
        timeSoFar = 0;
        fading = true;
        StartCoroutine(FadeCoroutine());
    }

    public void FadeOut()
    {
        startAlpha = 1;
        endAlpha = 0;
        timeSoFar = 0;
        fading = true;
        StartCoroutine(FadeCoroutine());
    }

    IEnumerator FadeCoroutine()
    {
        changeRate = (endAlpha - startAlpha) / changeTimeSeconds;
        SetAlpha(startAlpha);
        while (fading) {
            timeSoFar += Time.deltaTime;
            if (timeSoFar > changeTimeSeconds) {
                fading = false;
                SetAlpha(endAlpha);
                yield break;
            } else
                SetAlpha(announceText.color.a + (changeRate * Time.deltaTime));

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
#region Pause/Quit

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
        } else {
            skybox.material = daySkybox;
            campfireFire.SetActive(false);
            sunlight.SetActive(true);
            nightPanel.SetActive(false);
        }
    }
}
#endregion