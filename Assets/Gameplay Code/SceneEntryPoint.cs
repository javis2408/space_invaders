using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SceneEntryPoint : MonoBehaviour, LevelProvider, LevelDelegate {
	[Header("Character")]
	[SerializeField] private CharacterMovement _characterMovement;
	[SerializeField] private CharacterMovementSettings _characterMovementSettings;

	[SerializeField] private CharacterShoot _characterShoot;
	
	[Header("Level")]
	[SerializeField] private LevelData _levelData;
	[SerializeField] private LevelBounds _levelBounds;

	[Header("Defaults")]
	[SerializeField] private BulletAsset _defaultBulletAsset;

	[Header("Screens")]
	[Header("Start")]
	[SerializeField] private GameObject _startScreen;
	[SerializeField] private ShootingButton _startButton;

	[Header("End")]
	[SerializeField] private GameObject _endScreen;
	[SerializeField] private ShootingButton _restartButton;
	[SerializeField] private ShootingButton _mainMenuButton;
	
	private InputManager _inputManager;
	private LevelProvider _levelProvider;

	private Level _level;
	
	private void Awake() {
		Init();
	}

	private async Task Init() {
		_inputManager = new InputManager();

		if (_characterMovement == null) {
			Debug.LogError("CharacterMovement is null");
			return;
		}
		_characterMovement.Init(_inputManager, _characterMovementSettings, _levelBounds);

		if (_characterShoot == null) {
			Debug.LogError("CharacterShoot is null");
			return;
		}
		await _characterShoot.Init(_inputManager, _defaultBulletAsset, _levelBounds);

		_startScreen.SetActive(true);

		_startButton.Init(Started);
		_restartButton.Init(Restarted);
		_mainMenuButton.Init(MainMenu);

		Debug.Log($"<color=\"green\"> Game Initialized </color>");
	}

	private async void Started() {
		_startScreen.SetActive(false);
		
		if (_level == null) {
			_level = new Level();
			_levelProvider = this;
			_level.@delegate = this;
		}
		await _level.Init(_levelData, _levelProvider);
		Debug.Log($"<color=\"cyan\"> Game Started </color>");
	}

	private async void Restarted() {
		_endScreen.SetActive(false);
		
		await _level.Init(_levelData, _levelProvider);
		Debug.Log($"<color=\"cyan\"> Game Started </color>");
	}

	private void MainMenu() {
		_endScreen.SetActive(false);
		_startScreen.SetActive(true);
		
		Debug.Log($"<color=\"red\"> Game Restarted </color>");
	}

	Action LevelProvider.LevelUpdate { get; set; }
	private void Update() {
		_levelProvider?.LevelUpdate?.Invoke();
	}

	void LevelDelegate.EnemiesKilled() {
		Debug.Log($"<color=\"red\"> Game Finished </color>");
		_endScreen.SetActive(true);
		_level.Fini();
	}
}