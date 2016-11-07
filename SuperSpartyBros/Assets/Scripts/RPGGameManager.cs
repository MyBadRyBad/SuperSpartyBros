using UnityEngine;
using System.Collections;
using UnityEngine.UI; // include UI namespace so can reference UI elements
using UnityEngine.SceneManagement; // include so we can load new scenes

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

	public PlayerControllerRPG playerController;
	public GameObject enemy1;
	public GameObject enemy2;

	// private variables
	GameObject _player;
	Vector3 _spawnLocation;

	// set things up here
	void Awake () {
		// setup reference to game manager
		if (gm_rpg == null)
			gm_rpg = this.GetComponent<RPGGameManager>();

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
	}

	// setup all the variables, the UI, and provide errors if things not setup properly.
	void setupDefaults() {
		// setup reference to player
		if (_player == null)
			_player = GameObject.FindGameObjectWithTag("Player");

		if (_player==null)
			Debug.LogError("Player not found in Game Manager");

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

		if (playerController.currentHealth <= 0) {
			playerController.canMove = false;
			playerController._didTriggerDeath = true;
			StartCoroutine (LoadGameOver ());
		}
	}

	public void DamageEnemy1(float damageAmount, bool ignoreShield) {
		EnemyRPGAI controller = enemy1.GetComponent<EnemyRPGAI> ();
		controller.DamageEnemy (damageAmount, ignoreShield);
		Debug.Log ("Did damage enemy");
		if (!enemy2 && controller.currentHealth <= 0) {
			GlobalControl.Instance.UpdateEnemyStunAtIndex (GlobalControl.Instance.currentEnemyIndex);
			Debug.Log ("Did destroy enemy");
			playerController.canMove = false;
			playerController._didTriggerVictory = true;

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