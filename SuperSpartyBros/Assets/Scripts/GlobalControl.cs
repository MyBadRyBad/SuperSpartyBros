using UnityEngine;
using System.Collections;

public class GlobalControl : MonoBehaviour {

	public static GlobalControl Instance;

	public string mainLevel;
	public PlayerData playerData;
	public EnemyData[] enemyData;
	public MovingPlatformData[] platformsData;
	public CoinData[] coinData;

	public int currentEnemyIndex = -1;

	void Awake ()   
	{
		if (Instance == null)
		{
			DontDestroyOnLoad(gameObject);
			Instance = this;
		}
		else if (Instance != this)
		{
			Destroy (gameObject);
		}
	}

	public void UpdateEnemyStunAtIndex(int index) {
		EnemyData enemyData = Instance.enemyData [index];
		enemyData.isStunned = true;
	}

	public void ResetGlobalControl() {
		mainLevel = "";
		playerData = null;
		enemyData = null;
		platformsData = null;
		coinData = null;
	}
}
