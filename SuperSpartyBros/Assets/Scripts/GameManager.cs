using UnityEngine;
using System.Collections;
using UnityEngine.UI; // include UI namespace so can reference UI elements
using UnityEngine.SceneManagement; // include so we can load new scenes

public class GameManager : MonoBehaviour {

	// static reference to game manager so can be called from other scripts directly (not just through gameobject component)
	public static GameManager gm;

	// levels to move to on victory and lose
	public string currentLevel;
	public string levelAfterVictory;
	public string levelAfterGameOver;

	public string levelBattle;

	// game performance
	public int score = 0;
	public int highscore = 0;
	public int startLives = 3;
	public int lives = 3;
	public float playerHP = 100.0f;
	public float playerMAXHP = 100.0f;

	// UI elements to control
	public Text UIScore;
	public Text UIHighScore;
	public Text UILevel;
	public Text PlayerHPText;
	public GameObject[] UIExtraLives;
	public GameObject UIGamePaused;

	// reference
	public GameObject[] enemies;
	public GameObject[] platforms;
	public GameObject[] coins;

	// private variables
	GameObject _player;
	Vector3 _spawnLocation;

	// set things up here
	void Awake () {
		// setup reference to game manager
		if (gm == null)
			gm = this.GetComponent<GameManager>();

		// setup all the variables, the UI, and provide errors if things not setup properly.
		setupDefaults();

		Debug.Log ("did awake");
	}

	void Start() {
		Debug.Log ("Did Start");

		Debug.Log ("currentLevel: " + GlobalControl.Instance.mainLevel);
		if (!GlobalControl.Instance.mainLevel.Equals (currentLevel)) {
			SetupGlobalControls ();				
		} else {
			RefreshWithGlobalControls ();
		}

		PlayerHPText.text = "HP: " + GlobalControl.Instance.playerData.playerHP + "/ " + GlobalControl.Instance.playerData.playerMAXHP;

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

		// if levels not specified, default to current level
		if (levelAfterVictory=="") {
			Debug.LogWarning("levelAfterVictory not specified, defaulted to current level");
			levelAfterVictory = Application.loadedLevelName;
		}
		
		if (levelAfterGameOver=="") {
			Debug.LogWarning("levelAfterGameOver not specified, defaulted to current level");
			levelAfterGameOver = Application.loadedLevelName;
		}

		// friendly error messages
		if (UIScore==null)
			Debug.LogError ("Need to set UIScore on Game Manager.");
		
		if (UIHighScore==null)
			Debug.LogError ("Need to set UIHighScore on Game Manager.");
		
		if (UILevel==null)
			Debug.LogError ("Need to set UILevel on Game Manager.");
		
		if (UIGamePaused==null)
			Debug.LogError ("Need to set UIGamePaused on Game Manager.");
		
		// get stored player prefs
		refreshPlayerState();

		// get the UI ready for the game
		refreshGUI();
	}

	// get stored Player Prefs if they exist, otherwise go with defaults set on gameObject
	void refreshPlayerState() {
		lives = PlayerPrefManager.GetLives();

		// special case if lives <= 0 then must be testing in editor, so reset the player prefs
		if (lives <= 0) {
			PlayerPrefManager.ResetPlayerState(startLives,false);
			lives = PlayerPrefManager.GetLives();
		}
		score = PlayerPrefManager.GetScore();
		highscore = PlayerPrefManager.GetHighscore();

		// save that this level has been accessed so the MainMenu can enable it
		PlayerPrefManager.UnlockLevel();
	}

	// refresh all the GUI elements
	void refreshGUI() {
		// set the text elements of the UI
		UIScore.text = "Score: "+score.ToString();
		UIHighScore.text = "Highscore: "+highscore.ToString ();
		UILevel.text = Application.loadedLevelName;
		
		// turn on the appropriate number of life indicators in the UI based on the number of lives left
		for(int i=0;i<UIExtraLives.Length;i++) {
			if (i<(lives-1)) { // show one less than the number of lives since you only typically show lifes after the current life in UI
				UIExtraLives[i].SetActive(true);
			} else {
				UIExtraLives[i].SetActive(false);
			}
		}
	}

	// public function to add points and update the gui and highscore player prefs accordingly
	public void AddPoints(int amount)
	{
		// increase score
		score+=amount;

		// update UI
		UIScore.text = "Score: "+score.ToString();

		// if score>highscore then update the highscore UI too
		if (score>highscore) {
			highscore = score;
			UIHighScore.text = "Highscore: "+score.ToString();
		}
	}

	// public function to remove player life and reset game accordingly
	public void ResetGame() {
		// remove life and update GUI
		lives--;
		refreshGUI();

		if (lives<=0) { // no more lives
			// save the current player prefs before going to GameOver
			PlayerPrefManager.SavePlayerState(score,highscore,lives);

			// load the gameOver screen
			SceneManager.LoadScene(levelAfterGameOver);
		} else { // tell the player to respawn
			_player.GetComponent<CharacterController2D>().Respawn(_spawnLocation);
		}
	}

	// public function for level complete
	public void LevelCompete() {
		// save the current player prefs before moving to the next level
		PlayerPrefManager.SavePlayerState(score,highscore,lives);

		// use a coroutine to allow the player to get fanfare before moving to next level
		StartCoroutine(LoadNextLevel());
	}

	// load the nextLevel after delay
	IEnumerator LoadNextLevel() {
		yield return new WaitForSeconds(3.5f);
		SceneManager.LoadScene(levelAfterVictory);
	}

	public void LoadBattle() {
		SceneManager.LoadScene (levelBattle);
	}
		
	void RefreshWithGlobalControls() {
		_player.transform.position = GlobalControl.Instance.playerData.currentPlayerPosition;

		for (int index = 0; index < GlobalControl.Instance.enemyData.Length; index++) {
			EnemyData enemyData = GlobalControl.Instance.enemyData [index];
			GameObject enemy = enemies [index];
			Enemy enemyBehavior = enemy.GetComponent<Enemy> ();

			// is a moving Enemy
			if (!enemyBehavior) {
				Transform childEnemy = enemy.transform.GetChild (0);
				enemyBehavior = childEnemy.gameObject.GetComponent<Enemy> ();
				childEnemy.position = enemyData.childPosition;
				enemy.transform.position = enemyData.currentPosition;

			} else {
				enemy.transform.position = enemyData.currentPosition;
			}
				
			if (enemyData.isStunned) {
				enemyBehavior.Stunned ();
			}

		/*	if (enemyData.movingPlatformIndex >= 0) {
				SetObjectToChildOfPlatform (enemy, platforms [enemyData.movingPlatformIndex]);
			} */
				
		}

		for (int index = 0; index < GlobalControl.Instance.coinData.Length; index++) {
			CoinData coinData = GlobalControl.Instance.coinData [index];
			GameObject coin = coins [index];
			coin.SetActive (coinData.doesExist);
		}

		for (int index = 0; index < GlobalControl.Instance.platformsData.Length; index++) {
			MovingPlatformData platformData = GlobalControl.Instance.platformsData [index];
			GameObject platform = platforms [index];
			platform.transform.position = platformData.currentPosition;
	
		}
	}

	public void UpdateGlobalControls() {
		GlobalControl.Instance.playerData.movingPlatformIndex = IndexOfMovingPlatform (_player);
		GlobalControl.Instance.playerData.currentPlayerPosition = _player.transform.position;

		for (int index = 0; index < enemies.Length; index++) {
			EnemyData enemyData = GlobalControl.Instance.enemyData [index];
			GameObject enemy = enemies [index];
			Enemy enemyBehavior = enemy.GetComponent<Enemy> ();

			// is a moving Enemy
			if (!enemyBehavior) {
				Transform childEnemy = enemy.transform.GetChild (0);
				enemyBehavior = childEnemy.gameObject.GetComponent<Enemy> ();

				enemyData.isStunned = enemyBehavior.isStunned;
				enemyData.childPosition = childEnemy.position;
				enemyData.currentPosition = enemy.transform.position;
				enemyData.isMovingEnemy = true;
			} else {
				enemyData.isStunned = enemyBehavior.isStunned;
				enemyData.currentPosition = enemy.transform.position;
				enemyData.isMovingEnemy = false;
			}
		}
			
	/*	for (int index = 0; index < coins.Length; index++) {
			CoinData coinData = GlobalControl.Instance.coinData [index];
			GameObject coin = coins [index];
			coin.SetActive (coinData.doesExist);
		} */
			
		for (int index = 0; index < platforms.Length; index++) {
			GameObject platform = platforms [index];
			MovingPlatformData platformData = GlobalControl.Instance.platformsData [index];
			platformData.currentPosition = platform.transform.position;

			PlatformMover platformMover = platform.GetComponent<PlatformMover> ();
			platformMover.myWaypointIndex = platformData.waypointIndex;
		}
	}

	public void UpdateGlobalControlsEnemyIndex(GameObject enemy) {	

		for (int index = 0; index < enemies.Length; index++) {
			GameObject storedObject = enemies [index];
			GameObject currentObject; 
			if (enemy.transform.parent == null) {
				currentObject = enemy;
			} else {
				currentObject = enemy.transform.parent.gameObject;
			}

			if (storedObject.Equals (currentObject)) {
				GlobalControl.Instance.currentEnemyIndex = index;

				Debug.Log ("enemyIndex: " + index);
			}
		}
	}

	void SetupGlobalControls() {
		GlobalControl.Instance.mainLevel = currentLevel;

		GlobalControl.Instance.playerData = new PlayerData();
		GlobalControl.Instance.enemyData = new EnemyData[enemies.Length];
		GlobalControl.Instance.coinData = new CoinData[coins.Length];
		GlobalControl.Instance.platformsData = new MovingPlatformData[platforms.Length];

		GlobalControl.Instance.playerData.playerHP = playerHP;
		GlobalControl.Instance.playerData.playerMAXHP = playerMAXHP;
		GlobalControl.Instance.playerData.movingPlatformIndex = IndexOfMovingPlatform (_player);

		for (int index = 0; index < enemies.Length; index++) {
			GameObject enemy = enemies [index];
			EnemyData enemyData = new EnemyData ();
			enemyData.currentPosition = enemy.transform.position;
			enemyData.isStunned = false;

			GlobalControl.Instance.enemyData [index] = enemyData;
		}

		for (int index = 0; index < coins.Length; index++) {
			CoinData coinData = new CoinData ();
			coinData.doesExist = true;

			GlobalControl.Instance.coinData [index] = coinData;
		}

		for (int index = 0; index < platforms.Length; index++) {
			GameObject platform = platforms [index];
			MovingPlatformData platformData = new MovingPlatformData ();
			platformData.currentPosition = platform.transform.position;

			GlobalControl.Instance.platformsData [index] = platformData;
		}
	}

	public int IndexOfMovingPlatform(GameObject obj) {
		if (obj.transform.parent && 
			obj.transform.parent.parent && 
			obj.transform.parent.parent.gameObject.GetComponent<PlatformMover>()) {
			for (int index = 0; index < platforms.Length; index++) {
				if (obj.transform.parent.parent.gameObject == platforms [index]) {
					Debug.Log ("Found Platform");
					return index;
				}
			}
		}

		return -1;
	}

	public bool SetObjectToChildOfPlatform(GameObject obj, GameObject movingPlatform) {
		Transform childPlatform = movingPlatform.transform.GetChild (0);

		if (childPlatform) {
			obj.transform.parent = childPlatform;
			return true;
		} else {
			return false;
		}
	}
}
