using UnityEngine;
using System.Collections;
using UnityEngine.UI; // include UI namespace so can reference UI elements
using UnityEngine.SceneManagement; // include so we can load new scenes
using UnityStandardAssets.CrossPlatformInput;

public class RPGGameManager : MonoBehaviour {

	// static reference to game manager so can be called from other scripts directly (not just through gameobject component)
	public static RPGGameManager gm_rpg;

	public string gameOverScene;

	// UI elements to control
	public GameObject UIGamePaused;
	public Image attackCooldown;
	public Image defendCooldown;
	public Image magicCooldown;
	public Image chargeCooldown;

	public Image leftArrow;
	public Image rightArrow;
	public Image upArrow;
	public Image downArrow;

	public PlayerControllerRPG playerController;
	public GameObject enemy1;
	public GameObject enemy2;

	public GameObject playerSlash;
	public GameObject enemy1Slash;
	private Animator _playerSlashAnimator;
	private Animator _enemy1SlashAnimator;

	// private variables
	GameObject _player;
	Vector3 _spawnLocation;
	private bool _didDestroyEnemy1 = false;
	private bool _didDestroyEnemy2 = false;

	// audio sources
	private AudioSource _audio;
	public AudioClip victoryClip;
	public AudioClip defeatClip;

	// set things up here
	void Awake () {
		// setup reference to game manager
		if (gm_rpg == null)
			gm_rpg = this.GetComponent<RPGGameManager>();


		_audio = GetComponent<AudioSource> ();
		if (_audio==null) { // if AudioSource is missing
			Debug.LogWarning("AudioSource component missing from this gameobject. Adding one.");
			// let's just add the AudioSource component dynamically
			_audio = gameObject.AddComponent<AudioSource>();
		}
		_audio.loop = true;

		// setup all the variables, the UI, and provide errors if things not setup properly.
		setupDefaults();
	
	}

	// game loop
	void Update() {
		// if ESC pressed then pause the game
		if (Input.GetKeyDown(KeyCode.Escape)) {
			if (Time.timeScale > 0f) {
				UIGamePaused.SetActive(true); // this brings up the pause UI
				Time.timeScale = 0f; // this pauses the game action
			} else {
				Time.timeScale = 1f; // this unpauses the game action (ie. back to normal)
				UIGamePaused.SetActive(false); // remove the pause UI
			}
		}


		float vx = CrossPlatformInputManager.GetAxis ("Horizontal");
		float vy = CrossPlatformInputManager.GetAxis ("Vertical");

		// left button 
		if (vx <= 0) {
			leftArrow.color = new Color (0.235f, 0.745f, 0.824f, vx * -1);
		} 

		// right button
		if (vx >= 0) {
			rightArrow.color = new Color (0.235f, 0.745f, 0.824f, vx);
		}
		 
		// down button
		if (vy <= 0) {
			downArrow.color = new Color (0.235f, 0.745f, 0.824f, vy * -1);
		}

		if (vy >= 0) {
			upArrow.color = new Color (0.235f, 0.745f, 0.824f, vy);
		}


	}

	// setup all the variables, the UI, and provide errors if things not setup properly.
	void setupDefaults() {
		// setup reference to player
		if (_player == null)
			_player = GameObject.FindGameObjectWithTag("Player");

		if (_player==null)
			Debug.LogError("Player not found in Game Manager");

		if (playerSlash) {
			_playerSlashAnimator = playerSlash.GetComponent<Animator> ();
		}

		if (enemy1Slash) {
			_enemy1SlashAnimator = enemy1Slash.GetComponent<Animator> ();
		}

		// get initial _spawnLocation based on initial position of player
		_spawnLocation = _player.transform.position;

		// get the UI ready for the game
		refreshGUI();
	}
		

	// refresh all the GUI elements
	void refreshGUI() {

	}

	public void UpdateAttackCooldownProgress(float progress) {
		attackCooldown.fillAmount = progress;

		if (progress >= 1.0f) {
			attackCooldown.color = new Color (0.235f, 0.745f, 0.824f, 1.0f);
		} else {
			attackCooldown.color = Color.white;
		}
	}

	public void UpdateDefendCooldownProgress(float progress) {
		defendCooldown.fillAmount = progress;

		if (progress >= 1.0f) {
			defendCooldown.color = new Color (0.235f, 0.745f, 0.824f, 1.0f);
		} else {
			defendCooldown.color = Color.white;
		}

	}

	public void UpdateMagicCooldownProgress(float progress) {
		magicCooldown.fillAmount = progress;

		if (progress >= 1.0f) {
			magicCooldown.color = new Color (0.235f, 0.745f, 0.824f, 1.0f);
		} else {
			magicCooldown.color = Color.white;
		}
	}

	public void UpdateChargeCooldownProgress(float progress) {
		chargeCooldown.fillAmount = progress;
	}


	// damage
	public void DamagePlayer(float damageAmount, bool ignoreShield) {
	//	PlayerControllerRPG controller = _player.GetComponent<PlayerControllerRPG> ();
		playerController.DamagePlayer (damageAmount, ignoreShield);

		if (_playerSlashAnimator) {
			_playerSlashAnimator.Play ("LeftSlash", 0, 0);
		}

		if (playerController.currentHealth <= 0) {
			playerController.canMove = false;
			playerController._didTriggerDeath = true;

			_audio.loop = false;
			_audio.Stop ();
			_audio.PlayOneShot (defeatClip);

			StartCoroutine (LoadGameOver ());
		}
	}

	public void DamageEnemy1(float damageAmount, bool ignoreShield) {
		EnemyRPGAI controller = enemy1.GetComponent<EnemyRPGAI> ();
		controller.DamageEnemy (damageAmount, ignoreShield);

		if (_enemy1SlashAnimator) {
			_enemy1SlashAnimator.Play ("RightSlash", 0, 0);
		}


		if (!enemy2 && controller.currentHealth <= 0 && !_didDestroyEnemy1) {
			_didDestroyEnemy1 = true;
			GlobalControl.Instance.UpdateEnemyStunAtIndex (GlobalControl.Instance.currentEnemyIndex);
			Debug.Log ("Did destroy enemy");
			playerController.canMove = false;
			playerController._didTriggerVictory = true;

			_audio.loop = false;
			_audio.Stop ();
			_audio.PlayOneShot (victoryClip);

			StartCoroutine (LoadLevel ());
		}
	}

	public void DamageEnemy2(float damageAmount, bool ignoreShield) {
		EnemyRPGAI controller = enemy2.GetComponent<EnemyRPGAI> ();
		controller.DamageEnemy (damageAmount, ignoreShield);
	}

	IEnumerator LoadLevel() {
		yield return new WaitForSeconds (3.0f);
		SceneManager.LoadScene (GlobalControl.Instance.mainLevel);
	}

	IEnumerator LoadGameOver() {
		yield return new WaitForSeconds (3.0f);
		SceneManager.LoadScene ("GameLose");
	}
}