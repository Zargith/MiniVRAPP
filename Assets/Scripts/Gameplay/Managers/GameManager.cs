using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(DayNighCycleManager))]
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
	List<GameObject> _eatenPlayers = new List<GameObject>();
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
	[SerializeField] GameObject witchSavePanel;
	[SerializeField] TextMeshProUGUI witchSaveText;
	[SerializeField] GameObject witchUseKillPotionPanel;


	[Header("Day night cycle")]
	DayNighCycleManager _dayNighCycleManager;


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
		_dayNighCycleManager = GetComponent<DayNighCycleManager>();
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

		if (_availableRoles[rInt].roleGameplayScriptName != null && _availableRoles[rInt].roleGameplayScriptName != "")
			playerManager.gameObject.AddComponent(System.Type.GetType(_availableRoles[rInt].roleGameplayScriptName));

		_availableRoles[rInt].quantity--;
		if (_availableRoles[rInt].quantity == 0)
			_availableRoles.RemoveAt(rInt);

		for (int i = 0; i < otherPlayers.Count; i++) {
			rInt = Random.Range(0, _availableRoles.Count);
			otherPlayers[i].GetComponent<OtherPlayerManager>().SetRole(_availableRoles[rInt]);

			// if (_availableRoles[rInt].roleGameplayScriptName != null && _availableRoles[rInt].roleGameplayScriptName != "")
			// 	otherPlayers[i].gameObject.AddComponent(System.Type.GetType(_availableRoles[rInt].roleGameplayScriptName));

			if (_availableRoles[rInt]._name != "loup-garou")
				_nonWerewolfPlayers.Add(otherPlayers[i]);

			_availableRoles[rInt].quantity--;
			if (_availableRoles[rInt].quantity == 0)
				_availableRoles.RemoveAt(rInt);
		}
	}

	#endregion
	#region Gameplay

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			changePauseState();

		if (!_waitingEndStep && !_playerDied)
			switch (_gameCycleStep) {
				case GameCycle.SetDay:
					_waitingEndStep = true;
					_dayNighCycleManager.changeDayNightCycle(audioSource);
					StartCoroutine(makeAnnouncement("Il fait jour, le village se réveille", true, true));
					break;

				case GameCycle.DeadsAnnouncement:
					_waitingEndStep = true;
					string eatenPlayers = _eatenPlayers.Count == 0 ? "personne n'a été mangé" : $"{_eatenPlayers.Count} personne(s) a/ont été mangée(s) :\n";
					string deads = $"La nuit dernière {eatenPlayers}";
					for (int i = 0; i < _eatenPlayers.Count; i++)
						if (_eatenPlayers[i].name == "Player") {
							StartCoroutine(makeAnnouncement("La nuit dernière vous avez été dévoré par les loups-garou..."));
							playerManager.Die("La nuit dernière vous avez été dévoré par les loups-garou...");
							_playerDied = true;
							return;
						} else {
							foreach (GameObject player in _alivePlayers)
								if (player.name == _eatenPlayers[i].name) {
									deads += $"{_eatenPlayers[i].name} était {_eatenPlayers[i].GetComponent<OtherPlayerManager>().GetRole()._name}\n";
									setPlayerToEliminated(player);
									break;
								}
						}
					StartCoroutine(makeAnnouncement(deads, true, true, false, false, false, false, true));
					checkClassVictory();
					break;

				case GameCycle.VillagersVote:
					_waitingEndStep = true;
					StartCoroutine(makeAnnouncement("Veuillez désigner une personne qui selon vous est un loup-garou", false, false, true));
					break;

				case GameCycle.SetNight:
					_waitingEndStep = true;
					_dayNighCycleManager.changeDayNightCycle(audioSource);
					StartCoroutine(makeAnnouncement("Le village s'endort", true, true));
					break;

				case GameCycle.WerewolvesVote:
					_waitingEndStep = true;
					if (_playerIsWereWolf) {
						StartCoroutine(makeAnnouncement("Les loups-garous se réveillent pour aller manger un(e) villageois(e)\nQui désignez-vous ?", false, false, true));
						_dayNighCycleManager.activateNightPanel(false);
					} else
						finishWerewolvesVote();
					break;

				case GameCycle.WitchTurn:
					_waitingEndStep = true;
					if (playerManager.GetRole()._name == "sorcière") {
						string eatenPlayer = _eatenPlayers[0].name == "Player" ? "vous avez" : $"{_eatenPlayers[0].name} a";
						StartCoroutine(makeAnnouncement($"La sorcière se réveille.\nCette nuit {_eatenPlayers[0].name} été dévoré(e) par les loups-garou\nSouhaitez-vous utiliser votre potion de résurrection ?", false, false, false, true));
						_dayNighCycleManager.activateNightPanel(false);
					} else {
						foreach (GameObject player in _alivePlayers) {
							if (player.GetComponent<InterfacePlayerManager>().GetRole()._name != "sorcière")
								break;

							WitchGameplay witchGameplay;
							player.TryGetComponent<WitchGameplay>(out witchGameplay);
							if (witchGameplay != null) {
								if (!witchGameplay.potionSavedLife() && Random.Range(0, 2) == 0) {
									_eatenPlayers.Clear();
									witchGameplay.useSaveLifePotion();
								}

								if (!witchGameplay.potionKilled() && Random.Range(0, 2) == 0) {
									List<GameObject> selectablePlayers = new List<GameObject>(_alivePlayers);
									selectablePlayers.Remove(player);
									for (int i = 0; i < selectablePlayers.Count; i++)
										if (_eatenPlayers.Count > 0 && selectablePlayers[i] == _eatenPlayers[0]) {
											selectablePlayers.RemoveAt(i);
											break;
										}
									int rInt = Random.Range(0, selectablePlayers.Count);
									_eatenPlayers.Add(selectablePlayers[rInt]);
									witchGameplay.useKillPotion();
								}
							}
						}
						finishWitchTurn();
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
		_eatenPlayers.Add(getEliminatedPlayer());
		resetSelectPlayers();

		if (_eatenPlayers[0].name == "Player") {
			StartCoroutine(makeAnnouncement("Les villageois ont voté pour vous éliminer"));
			playerManager.Die("Le village à voté contre vous...");
			_playerDied = true;
			return;
		} else {
			foreach (GameObject player in _alivePlayers)
				if (player.name == _eatenPlayers[0].name) {
					setPlayerToEliminated(player);
					break;
				}
			StartCoroutine(makeAnnouncement($"Le village a voté pour éliminer {_eatenPlayers[0].name}\nIl/elle était {_eatenPlayers[0].GetComponent<OtherPlayerManager>().GetRole()._name}", true, true, false, false, false, false, true));
			// _eatenPlayers.Clear();
		}
		checkClassVictory();
	}

	public void finishWerewolvesVote()
	{
		setVisibleSelectButtons(false);
		_dayNighCycleManager.activateNightPanel(true);

		if (!_playerIsWereWolf)
			assignWerewolfVotes();
		_eatenPlayers.Add(getEliminatedPlayer());
		resetSelectPlayers();

		if (_playerIsWereWolf)
			StartCoroutine(makeAnnouncement("Les loups-garou se rendorment", true, true));
		else
			StartCoroutine(makeAnnouncement("Les loups-garous se sont réveillés pour dévorer un(e) villageois(e)", true, true));
		_dayNighCycleManager.activateNightPanel(true);
	}

	public void witchSavePlayer(bool doSave)
	{
		if (doSave) {
			_eatenPlayers.Clear();
			playerManager.gameObject.GetComponent<WitchGameplay>().useSaveLifePotion();
		}

		setWitchSaveLivePanel(false);
		StartCoroutine(makeAnnouncement("Souhaitez vous utiliser votre potion d'empoisonnement ?", false, false, false, false, true));
	}

	public void witchUseKillPotion(bool doUse)
	{
		setWitchKillPanel(false);
		setWitchUseKillPotionPanel(false);
		if (doUse)
			setWitchKillPanel(true);
		else
			finishWitchTurn();
	}

	public void witchKillPlayer(bool doKill)
	{
		setWitchUseKillPotionPanel(false);
		if (doKill) {
			_eatenPlayers.Add(playerManager.gameObject);
			playerManager.gameObject.GetComponent<WitchGameplay>().useKillPotion();
		}

		finishWitchTurn();
	}

	public void finishWitchTurn()
	{
		setWitchKillPanel(false);
		GameObject eliminatedPlayer = getEliminatedPlayer();
		if (eliminatedPlayer)
			_eatenPlayers.Add(eliminatedPlayer);
		resetSelectPlayers();
		if (_alivePlayers.Find(player => player.GetComponent<InterfacePlayerManager>().GetRole()._name == "sorcière")) {
			StartCoroutine(makeAnnouncement("La sorcière se rendort", true, true));
			return;
		}

		if (_gameCycleStep == GameCycle.WitchTurn)
			_gameCycleStep = GameCycle.SetDay;
		else
			_gameCycleStep++;
		_waitingEndStep = !_waitingEndStep;
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
			if (getRemainingPlayerWithRole(role._name) == _alivePlayers.Count) {
				StartCoroutine(makeAnnouncement($"La/le/les {role._name}/{role.namePlurial} a/ont gagné !"));
				setEndGame($"La/le/les {role._name}/{role.namePlurial} a/ont gagné !");
				return;
			}

			WinCondition[] winConditions = role.winConditions;
			foreach (WinCondition condition in winConditions) {
				switch (condition.winConditionType) {
					case ( WinConditionType.All ):
						if (condition.winConditionStatus == WinConditionStatus.Dead && getRemainingPlayerWithRole(condition.roleName) == 0) {
							StartCoroutine(makeAnnouncement($"La/le/les {role._name}/{role.namePlurial} a/ont gagné !"));
							setEndGame($"La/le/les {role._name}/{role.namePlurial} a/ont gagné !");
							return;
						} else if (condition.winConditionStatus == WinConditionStatus.Dead && getRemainingPlayerWithRole("loup-garou") > _nonWerewolfPlayers.Count) {
							StartCoroutine(makeAnnouncement($"La/le/les loup(s)-garou a/ont gagné !"));
							setEndGame("La/le/les loup(s)-garou a/ont gagné !");
							return;
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

	void setPlayerToEliminated(GameObject player)
	{
		player.GetComponent<InterfacePlayerManager>().Die();
		_alivePlayers.Remove(player);
		if (player.GetComponent<InterfacePlayerManager>().GetRole()._name != "loup-garou")
			_nonWerewolfPlayers.Remove(player);
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

	void setVisibleSomeSelectButtons(bool visible, List<GameObject> players)
	{
		foreach (GameObject player in players) {
			Transform selectPlayerGO = player.transform.Find("Canvas/SelectPlayer");
			if (selectPlayerGO != null)
				selectPlayerGO.gameObject.SetActive(visible);
		}
	}

	void setWitchSaveLivePanel(bool visible)
	{
		if (visible)
			witchSaveText.text = _eatenPlayers[0].name == "Player" ? "Vous allez être éliminé(e) !\nVoulez-vous vous sauver ?" : $"{_eatenPlayers[0].name} va être éliminé(e) !\nVoulez-vous le/la sauver ?";
		witchSavePanel.SetActive(visible);
	}

	void setWitchUseKillPotionPanel(bool visible)
	{
		witchUseKillPotionPanel.SetActive(visible);
	}

	void setWitchKillPanel(bool visible)
	{
		List<GameObject> selectablePlayers = new List<GameObject>(_alivePlayers);
		if (_eatenPlayers.Count > 0)
			selectablePlayers.Remove(_eatenPlayers[0]);
		GameObject witch = selectablePlayers.Find(player => player.GetComponent<InterfacePlayerManager>().GetRole()._name == "sorcière");
		if (witch)
			selectablePlayers.Remove(witch);
		setVisibleSomeSelectButtons(visible, selectablePlayers);
	}

	void setEndGame(string reason = "")
	{
		_playerDied = true;
		endGameMenu.SetActive(true);
		endGameText.text = reason + "\n\nPartie terminée, merci d'avoir joué !";
	}

	#endregion
	#region Announcement

	IEnumerator makeAnnouncement(string text, bool changeWaitingEndStep = false, bool updateCycleStep = false, bool displayPlayerSelection = false, bool displayWitchSaveSelection = false, bool displayWitchUseKillPotionSelection = false, bool displayKillSelection = false, bool cleanEatenPlayers = false)
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
			_dayNighCycleManager.activateNightPanel(true);
			_firstTurn = false;
		}

		if (updateCycleStep) {
			if (_gameCycleStep == GameCycle.WitchTurn)
				_gameCycleStep = GameCycle.SetDay;
			else
				_gameCycleStep++;
		}

		if (cleanEatenPlayers)
			_eatenPlayers.Clear();

		if (changeWaitingEndStep)
			_waitingEndStep = !_waitingEndStep;

		if (displayPlayerSelection)
			setVisibleSelectButtons(true);

		if (displayWitchSaveSelection)
			setWitchSaveLivePanel(true);

		if (displayWitchUseKillPotionSelection)
			setWitchUseKillPotionPanel(true);

		if (displayKillSelection)
			setWitchKillPanel(true);
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
		_changeRate = ( _endAlpha - _startAlpha ) / _changeTimeSeconds;
		SetAlpha(_startAlpha);
		while (_fading) {
			_timeSoFar += Time.deltaTime;
			if (_timeSoFar > _changeTimeSeconds) {
				_fading = false;
				SetAlpha(_endAlpha);
				yield break;
			} else
				SetAlpha(announceText.color.a + ( _changeRate * Time.deltaTime ));

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
		ovrManager.RecenterPose();
	}

	#endregion

}
