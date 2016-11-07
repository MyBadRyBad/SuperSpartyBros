using UnityEngine;
using System.Collections;

public class Level1Controller : MonoBehaviour {

	public static Level1Controller Instance;

	public float playerHP;
	public Vector2 currentPlayerPosition;
	public string mainLevel;
	public bool[] enemiesStunned;



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
}
