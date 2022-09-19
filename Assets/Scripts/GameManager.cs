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
    GameCycle _gameCycleStep = GameCycle.WerewolvesVote;
    bool _firstTurn = true;
    bool _waitingEndStep = true;
    string _eatenPlayerName = "";


    [Header("UI")]
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject announceGO;
    [SerializeField] TextMeshProUGUI announceText;
    bool _gamePaused = false;
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
        _waitingEndStep = true;
        StartCoroutine(makeAnnouncement("Les villageois(es) du village de thiercelieux s'endorment paisiblement", true));
    }

    void assignRoles()
    {
        System.Random rand = new System.Random();
        int rInt = rand.Next(0, roles.Count);
        playerManager.SetRole(_availableRoles[rInt]);

        _availableRoles[rInt].quantity--;
        if (_availableRoles[rInt].quantity == 0)
            _availableRoles.RemoveAt(rInt);

        for (int i = 0; i < otherPlayers.Count; i++) {
            rand = new System.Random();
            rInt = rand.Next(0, _availableRoles.Count);
            otherPlayers[i].GetComponent<OtherPlayerManager>().SetRole(_availableRoles[rInt]);

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
                    // StartCoroutine(makeAnnouncement("La nuit dernière X personnes ont été mangées par les loups-garou\nVous devez maintenant choisir une personne du village à éliminer")); // TODO dire qui a été tué et faire une animation de mort (besoin d'une autre step ?)
                    StartCoroutine(makeAnnouncement("La nuit dernière X personnes ont été mangées par les loups-garou\nVous devez maintenant choisir une personne du village à éliminer", true, true));
                    // TODO vérifier conditions de victoire et de mort du joueur
                    break;

                case GameCycle.VillagersVote:
                    _waitingEndStep = true;
                    // StartCoroutine(makeAnnouncement("Les villageois doivent maintenant voter pour éliminer une personne")); // TODO ajouter (TODO en VR) le choix de la personne à éliminer (penser au Maire si besoin ou random parmis les plus haut votes égaux)
                    StartCoroutine(makeAnnouncement("Veuillez désigner une personnes à éliminer", true, true));
                    break;

                case GameCycle.SetNight:
                    _waitingEndStep = true;
                    changeDayNightCycle();
                    StartCoroutine(makeAnnouncement("Le village s'endort", true, true));
                    break;

                case GameCycle.WerewolvesVote:
                    _waitingEndStep = true;
                    // StartCoroutine(makeAnnouncement("Les loups garous doivent maintenant voter pour éliminer une personne")); // TODO ajouter (TODO en VR) le choix de la personne à éliminer (penser au Maire si besoin ou random parmis les plus haut votes égaux)
                    StartCoroutine(makeAnnouncement("Les loups-garous se réveillent pour aller manger un villageois\nQui désignez vous ?", true, true));
                    break;
            }
    }

    public void finishVillagersVote()
    {
        // TODO get les résultats du vote et éliminer la personne
        StartCoroutine(makeAnnouncement($"Les villageois ont voté pour éliminer <insérer nom/n° du joueur>", true, true));
        // TODO vérifier conditions de victoire et de mort du joueur
    }

    public void finishWerewolvesVote()
    {
        // TODO get les résultats du vote et stocker le résultat
        StartCoroutine(makeAnnouncement("Les loups-garou se rendorment", true, true));
    }


#endregion
#region Announcement

    IEnumerator makeAnnouncement(string text, bool changeWaitingEndStep = false, bool updateCycleStep = false)
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

        if (_firstTurn && playerManager.GetRole()._name != "loup-garou") {
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
            if (playerManager.GetRole()._name != "loup-garou")
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