﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // include so we can load new scenes
using UnityStandardAssets.CrossPlatformInput;

public class PlayerControllerRPG : MonoBehaviour {

	// store references to components on the gameObject
	Transform _transform;
	Rigidbody2D _rigidbody;
	public Animator _animator;
	AudioSource _audio;

	public AudioSource magicChargeAudioSource;
	public AudioClip slashSFX;
	public AudioClip blockSFX;
	public AudioClip hitSFX;

	public ParticleSystem dashParticleSystem;
	public ParticleSystem dodgeParticleSystem;
	public ParticleSystem magicParticleSystem;
	public Transform playerSpawnLocation;
	public Transform magicSpawnLocation;
	public Transform attackStopLocation;
	public Transform battleIndicatorLocation;

	// player health
	public float currentHealth = 500.0f;
	private float _defaultMaxHealth = 100.0f;
	public TextMesh healthTextMesh;
	public GameObject battleTextIndicatorPrefab;

	// player Damage
	public float attackDamage = 10.0f;

	//[HideInInspector]
	public bool canMove = true;

	// fire ball
	public GameObject fireballPrefab;
	private GameObject _currentFireball;

	// attack cooldowns
	public float attackCooldown = 1.0f;
	private float _attackTimer = 0.0f;

	[HideInInspector]
	public bool isUsingAttack = false;

	// shield cooldowns
	public float shieldCooldown = 2.0f;
	private float _shieldTimer = 0.0f;

	[HideInInspector]
	public bool isUsingShield = false;

	// shield magic cooldowns
	public float magicCooldown = 5.0f;
//	private float _magicTimer = 0.0f;

	[HideInInspector]
	public bool isUsingMagic = false;

	public float magicChargeRate = 10.0f;
	[HideInInspector]
	public float magicCharge = 0.0f;

	// shield dodge cooldowns
	public float dodgeCooldown = 1.0f;
//	private float _dodgeTimer = 0.0f;

	[HideInInspector]
	public bool isUsingDodge = false;

	[HideInInspector]
	public bool isUsingCharge = false;

	// TODO: fix animator - currently using workaround public variables to trigger animation
	public bool _didTriggerVictory = false;
	public bool _didTriggerDeath = false;

	// Use this for initialization
	void Awake () {
		// get a reference to the components we are going to be changing and store a reference for efficiency purposes
		_transform = GetComponent<Transform> ();

		_rigidbody = GetComponent<Rigidbody2D> ();
		if (_rigidbody==null) // if Rigidbody is missing
			Debug.LogError("Rigidbody2D component missing from this gameobject"); 

		_animator = GetComponent<Animator>();
		if (_animator==null) // if Animator is missing
			Debug.LogError("Animator component missing from this gameobject");

		_audio = GetComponent<AudioSource> ();
		if (_audio==null) { // if AudioSource is missing
			Debug.LogWarning("AudioSource component missing from this gameobject. Adding one.");
			// let's just add the AudioSource component dynamically
			_audio = gameObject.AddComponent<AudioSource>();
		}
			
		canMove = true;
		if (GlobalControl.Instance != null && GlobalControl.Instance.playerData != null) {
			currentHealth = GlobalControl.Instance.playerData.playerHP;
			healthTextMesh.text = currentHealth.ToString ("f0") + " / " + GlobalControl.Instance.playerData.playerMAXHP.ToString ("f0");
		} else {
			currentHealth = 100.0f;
			healthTextMesh.text = currentHealth.ToString ("f0") + " / " + _defaultMaxHealth.ToString("f0");
		}
		// disable particles for now
	/*	EnableDashParticleSystem(false);
		EnableMagicParticleSystem (false);
		EnableDodgeParticleSystem (false); */

	}
	
	// Update is called once per frame
	void Update () {
		if (canMove) {
			ExecutePlayerControls ();
		} else if (_didTriggerVictory) {
			_didTriggerVictory = false;
			_animator.Play ("SpartyBattleVictory", 0, 0);
		} else if (_didTriggerDeath) {
			_didTriggerDeath = false;
			_animator.Play ("SpartyBattleDeath", 0, 0);
		}

	}
		
	void ExecutePlayerControls() {
		float vx = CrossPlatformInputManager.GetAxis ("Horizontal");
		float vy = CrossPlatformInputManager.GetAxis ("Vertical");

		if (!isUsingMagic) {
			if (vx == 1 && _attackTimer <= Time.time) {
				ExecuteAttack ();
				EnableMagicParticleSystem (false);
			} else if (vx == -1) {
				ExecuteCharge ();
			} else {
				if (magicChargeAudioSource.isPlaying && !isUsingMagic) {
					magicChargeAudioSource.Stop ();
				}
				EnableMagicParticleSystem (false);
			}
			if (vy == -1 && _shieldTimer <= Time.time) {
				ExecuteShield ();
				EnableMagicParticleSystem (false);
			} else if (vy == 1 && magicCharge >= 100.0f) {
				ExecuteMagic ();
			} 
		}

		updateCooldownUI ();
	}
		

	public void updateCooldownUI() {
		float cooldownAttack = Mathf.Min(1 - (( _attackTimer - Time.time) / attackCooldown), 1.0f);
		float cooldownDefend = Mathf.Min(1 - (( _shieldTimer - Time.time) / shieldCooldown), 1.0f);
		float cooldownMagicCharge = Mathf.Min (magicCharge / 100, 1.0f);

		RPGGameManager.gm_rpg.UpdateAttackCooldownProgress (cooldownAttack);
		RPGGameManager.gm_rpg.UpdateDefendCooldownProgress (cooldownDefend);
		RPGGameManager.gm_rpg.UpdateMagicCooldownProgress (cooldownMagicCharge);

	}

	// attack actions
	public void ExecuteAttack() {
		_attackTimer = Time.time + attackCooldown;
		_animator.Play ("SpartyAttack", 0, 0);
		EnableDashParticleSystem (false);
		_audio.PlayOneShot (slashSFX);

		StartCoroutine (EndAttack ());

	}

	IEnumerator EndAttack() {
		yield return new WaitForSeconds (1.0f);

		if (RPGGameManager.gm_rpg.enemy1) {
			RPGGameManager.gm_rpg.DamageEnemy1 (attackDamage, false);
		} else if (RPGGameManager.gm_rpg.enemy2) {
			RPGGameManager.gm_rpg.DamageEnemy2 (attackDamage, false);
		}

		gameObject.transform.position = playerSpawnLocation.transform.position;
		_animator.Play ("SpartyBattleIdle", 0, 0);
	}
		
	// magic actions
	void ExecuteMagic() {
		Debug.Log ("Do Magic");
		magicChargeAudioSource.Play ();

	//	_magicTimer = Time.time + magicCooldown;
		magicCharge = 0.0f;
		isUsingMagic = true;

		_animator.Play ("SpartyMagic", 0, 0);
		_currentFireball = (GameObject)Instantiate (fireballPrefab, magicSpawnLocation.position, Quaternion.identity);
		_currentFireball.transform.parent = gameObject.transform;
		_currentFireball.transform.localPosition = magicSpawnLocation.localPosition;

		FireballCharge fireballCharge = _currentFireball.GetComponent<FireballCharge> ();
		fireballCharge.parentTransform = gameObject.transform;

		if (fireballCharge) {
			Invoke ("EndMagic", fireballCharge.chargeTime);
		} else {
			Invoke ("EndMagic", 4.0f);
		}

		EnableMagicParticleSystem (true);
	}
		
	public void EndMagic() {
		_animator.Play ("SpartyBattleIdle", 0, 0);
		isUsingMagic = false;

		magicChargeAudioSource.Stop ();

		EnableMagicParticleSystem (false);
	}

	// charge actions
	public void ExecuteCharge() {
		if (!magicChargeAudioSource.isPlaying) {
			magicChargeAudioSource.Play ();
		}

		magicCharge += magicChargeRate * Time.deltaTime;
		EnableMagicParticleSystem(true);
	}

	// dodge actions
	void ExecuteDodge() {
		Debug.Log ("Do Dodge");
		_animator.Play ("SpartyDodge", 0, 0);
	//	_dodgeTimer = Time.time + dodgeCooldown;
		isUsingDodge = true;
	}

	void EndDodge() {
		gameObject.transform.position = playerSpawnLocation.transform.position;
		_animator.Play ("SpartyBattleIdle", 0, 0);
	}


	void ExecuteShield() {
		Debug.Log ("Do Shield");

		_shieldTimer = Time.time + shieldCooldown;
		isUsingShield = true;

		_animator.Play ("SpartyDefend", 0, 0);

		StartCoroutine (EndShield ());
		Invoke ("EndShield", 1.0f);
	}

	IEnumerator EndShield() {
		yield return new WaitForSeconds (1.0f);

		isUsingShield = false;
		_animator.Play ("SpartyBattleIdle", 0, 0);
	}
		
	// damage player
	public void DamagePlayer(float damageAmount, bool ignoreShield) {
		if (!isUsingShield || ignoreShield) {
			currentHealth -= damageAmount;

			// show battleindicator
			if (battleTextIndicatorPrefab) {
				Vector3 position = new Vector3 (transform.position.x, transform.position.y + 1.0f, transform.position.z);
				GameObject obj = (GameObject)Instantiate (battleTextIndicatorPrefab, position, Quaternion.identity);
				obj.transform.parent = gameObject.transform;
				obj.transform.localPosition = battleIndicatorLocation.localPosition;
				BattleTextIndicator battleText = obj.GetComponent<BattleTextIndicator> ();
				battleText.textColor = new Color (1.0f, 0.385f, 0.385f, 1.0f);
				battleText.text = "-" + damageAmount.ToString ("f0");
			}

			// update health text mesh
			if (GlobalControl.Instance != null && GlobalControl.Instance.playerData != null) {
				GlobalControl.Instance.playerData.playerHP -= damageAmount;
				healthTextMesh.text = currentHealth.ToString ("f0") + " / " + GlobalControl.Instance.playerData.playerMAXHP.ToString ("f0");
			} else {
				healthTextMesh.text = currentHealth.ToString ("f0") + " / " + _defaultMaxHealth.ToString ("f0");
			}
				
			_audio.PlayOneShot (hitSFX);


		} else { // did Block
			// show battleindicator
			if (battleTextIndicatorPrefab) {
				Vector3 position = new Vector3 (transform.position.x, transform.position.y + 1.0f, transform.position.z);
				GameObject obj = (GameObject)Instantiate (battleTextIndicatorPrefab, position, Quaternion.identity);
				obj.transform.parent = gameObject.transform;
				obj.transform.localPosition = battleIndicatorLocation.localPosition;
				BattleTextIndicator battleText = obj.GetComponent<BattleTextIndicator> ();
				battleText.textColor = new Color (0.65f, 0.65f, 0.65f, 1.0f);
				battleText.text = "Blocked";
			}

			_audio.PlayOneShot (blockSFX);
		}

	}
		
	// particle systems
	public void EnableMagicParticleSystem(bool play) {
		if (magicParticleSystem) {
			if (play) {
				if (!magicParticleSystem.emission.enabled) {
					var emission = magicParticleSystem.emission;
					emission.enabled = true;
				}

				magicParticleSystem.Play ();
			} else {
				if (magicParticleSystem.emission.enabled) {
					var emission = magicParticleSystem.emission;
					emission.enabled = false;
				}

				magicParticleSystem.Stop ();
			}
		}
	}

	public void EnableDashParticleSystem(bool play) {
		if (dashParticleSystem) {
			if (play) {
				if (!dashParticleSystem.emission.enabled) {
					var emission = dashParticleSystem.emission;
					emission.enabled = true;
				}

				dashParticleSystem.Play ();
			} else {
				if (dashParticleSystem.emission.enabled) {
					var emission = dashParticleSystem.emission;
					emission.enabled = false;
				}

				dashParticleSystem.Stop ();
			}
		}
	}

	public void EnableDodgeParticleSystem (bool play) {
		if (dodgeParticleSystem) {
			if (play) {
				if (!dodgeParticleSystem.emission.enabled) {
					var emission = dodgeParticleSystem.emission;
					emission.enabled = true;
				}

				dodgeParticleSystem.Play ();
			} else {
				if (dodgeParticleSystem.emission.enabled) {
					var emission = dodgeParticleSystem.emission;
					emission.enabled = false;
				}

				dodgeParticleSystem.Stop ();
			}
		}
	}

}
