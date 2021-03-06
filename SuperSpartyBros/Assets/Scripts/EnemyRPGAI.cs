﻿using UnityEngine;
using System.Collections;

public class EnemyRPGAI : MonoBehaviour {

	AudioSource _audio;
	public AudioClip slashSFX;
	public AudioClip blockSFX;
	public AudioClip hitSFX;

	public float attackDamage = 5.0f;

	public float attackDelayMin = 5.0f;
	public float attackDelayMax = 10.0f;
	private float _attackTimer = 0.0f;
	private bool _isAttacking = false;

	public float defendDelayMin = 3.0f;
	public float defendDelayMax = 10.0f;
	private float _defendTimer = 0.0f;
	private bool _isDefending = false;

	public float maxHealth = 20.0f;
	public float currentHealth = 20.0f;
	public TextMesh healthTextMesh;
	public GameObject battleTextIndicatorPrefab;

	private Animator _animator;

	void Awake() {
		_audio = GetComponent<AudioSource> ();
		if (_audio==null) { // if AudioSource is missing
			Debug.LogWarning("AudioSource component missing from this gameobject. Adding one.");
			// let's just add the AudioSource component dynamically
			_audio = gameObject.AddComponent<AudioSource>();
		}
	}

	// Use this for initialization
	void Start () {
		_animator = GetComponent<Animator>();
		if (_animator==null) // if Animator is missing
			Debug.LogError("Animator component missing from this gameobject");


		_attackTimer = Time.time + Random.Range (attackDelayMin, attackDelayMax);
		_defendTimer = Time.time + Random.Range (defendDelayMin, defendDelayMax);

		currentHealth = maxHealth;
		healthTextMesh.text = currentHealth.ToString ("f0") + " / " + maxHealth.ToString("f0");
	}
	
	// Update is called once per frame
	void Update () {
		if (!_isAttacking && !_isDefending && currentHealth > 0) {
			if (_attackTimer <= Time.time) {
				Attack ();
			} else if (_defendTimer <= Time.time) {
				Defend ();
			}
		}
	}

	void Attack() {
		_attackTimer = Time.time + Random.Range (attackDelayMin, attackDelayMax);
		_animator.Play ("EnemyAttack", 0, 0);
		_audio.PlayOneShot (slashSFX);
		_isAttacking = true;
		Invoke ("EndAttack", 1.0f);
	}

	void EndAttack() {
		_isAttacking = false;

		RPGGameManager.gm_rpg.DamagePlayer(attackDamage, false);

		_animator.Play ("EnemyIdle", 0, 0);
	}

	void Defend() {
		_defendTimer = Time.time + Random.Range (defendDelayMin, defendDelayMax);
		_animator.Play ("EnemyDefend", 0, 0);

		_isDefending = true;
		Invoke ("EndDefend", 1.0f);
	}

	void EndDefend() {
		
		_isDefending = false;
		_animator.Play ("EnemyIdle", 0, 0);
	}

	// damage Enemy
	public void DamageEnemy(float damageAmount, bool ignoreShield) {
		if (!_isDefending || ignoreShield) {
			currentHealth -= damageAmount;

			// show battleindicator
			if (battleTextIndicatorPrefab) {
				Vector3 position = new Vector3 (transform.position.x, transform.position.y + 1.0f, transform.position.z);
				GameObject obj = (GameObject)Instantiate (battleTextIndicatorPrefab, position, Quaternion.identity);
				BattleTextIndicator battleText = obj.GetComponent<BattleTextIndicator> ();
				battleText.textColor = new Color (1.0f, 0.385f, 0.385f, 1.0f);
				battleText.text = "-" + damageAmount.ToString ("f0");
			}

			healthTextMesh.text = currentHealth.ToString ("f0") + " / " + maxHealth.ToString ("f0");

			if (currentHealth <= 0) {
				currentHealth = 0.0f;
				healthTextMesh.text = currentHealth.ToString ("f0") + " / " + maxHealth.ToString ("f0");

				_animator.Play ("EnemyStunned", 0, 0);
			}

			_audio.PlayOneShot (hitSFX);

		} else {
			Vector3 position = new Vector3 (transform.position.x, transform.position.y + 1.0f, transform.position.z);
			GameObject obj = (GameObject)Instantiate (battleTextIndicatorPrefab, position, Quaternion.identity);
			BattleTextIndicator battleText = obj.GetComponent<BattleTextIndicator> ();
			battleText.textColor = new Color (0.65f, 0.65f, 0.65f, 1.0f);
			battleText.text = "Blocked";

			_audio.PlayOneShot (blockSFX);
		}

	}
}
