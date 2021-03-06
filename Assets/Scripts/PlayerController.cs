﻿using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public float maxSpeed = 10f;
	public float jumpForceMagnitude = 100f;
	public bool facingRight = false;

	bool canJump = true;
	bool grounded = false;
	public Transform groundCheck;
	float groundWallRadius = 0.2f;
	public LayerMask whatIsGround;

	bool wallJumpLeft = false;
	bool wallJumpRight = false;
	bool wallJumped = false;
	float wallJumpedTimer = 0.0f;
	public Transform wallCheckRight;
	public Transform wallCheckLeft;
	public LayerMask whatIsWall;
	
	private bool falling = false;
	
	public int score = 0;

	private AudioSource jumpAudio;

	private Animator animator;
	
	public GUIStyle style;
	
	/////////////////////
	/// Unity Functions
	/////////////////////

	void Start() {
		animator = GetComponent<Animator> ();
		animator.SetBool("LightWorld", true);
		Flip ();
		AudioSource[] audios = GetComponents<AudioSource>();
		jumpAudio = audios[0];
	}
	
	void FixedUpdate () {
		MovePlayer ();
	}

	void Update() {
		if (grounded && rigidbody2D.velocity.y == 0) {
			animator.SetBool ("Fall", false);
			falling = false;
		}
		
		if (wallJumped) {
			wallJumpedTimer += Time.deltaTime;
			if (wallJumpedTimer > 0.2f) {
				wallJumped = false;
				wallJumpedTimer = 0.0f;
			}
		}
		if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp ("Jump")) {
			canJump = true;
		}
		if ((Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump")) && canJump) {
			if (grounded) {
				rigidbody2D.AddForce (new Vector2(0, jumpForceMagnitude));
				canJump = false;
				animator.SetBool ("Jump", true);
				animator.SetBool("Fall", false);
				falling = false;
				jumpAudio.Play();
			}
			else if (wallJumpLeft) {
				rigidbody2D.AddForce (new Vector2(jumpForceMagnitude, jumpForceMagnitude));
				canJump = false;
				wallJumped = true;
				animator.SetBool ("Jump", true);
				animator.SetBool("Fall", false);
				falling = false;
				jumpAudio.Play();
			}
			else if (wallJumpRight) {
				rigidbody2D.AddForce (new Vector2(-jumpForceMagnitude, jumpForceMagnitude));
				canJump = false;
				wallJumped = true;
				animator.SetBool ("Jump", true);
				animator.SetBool("Fall", false);
				falling = false;
				jumpAudio.Play();
			}
		}
		
		if (!falling && rigidbody2D.velocity.y <= 0 && !grounded) {
			falling = true;
			animator.SetBool ("Jump", false);
			animator.SetBool("Fall", true);
		}
	}
	
	void OnGUI() {
		GUI.Label (new Rect(200, 10, 100, 20), "Score: " + score, style);
	}

	/////////////////////
	/// Our Functions
	/////////////////////
	private void MovePlayer() {
		float move = Input.GetAxis ("Horizontal");

		if (grounded)
			rigidbody2D.velocity = new Vector2 (move * maxSpeed, rigidbody2D.velocity.y);
		else if (!wallJumped) {
			float velocityChange = Mathf.Clamp(move * maxSpeed / 4 + rigidbody2D.velocity.x, -maxSpeed, maxSpeed);
			rigidbody2D.velocity = new Vector2(velocityChange, rigidbody2D.velocity.y);
		}
		
		Debug.Log (grounded);

		if (move != 0) {
			animator.SetBool("Move", true);
		}
		else {
			animator.SetBool ("Move", false);
		}

		if ((move > 0 && !facingRight) ||
		    (move < 0 && facingRight)) {
			Flip ();
		}

		grounded = Physics2D.OverlapCircle (groundCheck.position, groundWallRadius, whatIsGround);

		wallJumpLeft = false;
		wallJumpRight = false;
		if (!grounded) {
			wallJumpLeft = Physics2D.OverlapCircle (wallCheckLeft.position, groundWallRadius, whatIsWall);
			wallJumpRight = Physics2D.OverlapCircle (wallCheckRight.position, groundWallRadius, whatIsWall);
			if (facingRight) {
				bool temp = wallJumpLeft;
				wallJumpLeft = wallJumpRight;
				wallJumpRight = temp;
			}
		}
	}

	private void Flip() {
		facingRight = !facingRight;

		Vector3 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;
	}
	
	public void WorldStateChange(bool lightWorld) {
	
		animator.SetBool("LightWorld", lightWorld);
		
		if (lightWorld) {
			GetComponent<GunController>().enabled = false;
			GetComponent<MeleeAttackController>().enabled = true;
		}
		else {
			GetComponent<GunController>().enabled = true;
			GetComponent<MeleeAttackController>().enabled = false;
		}
	}
	
	public void DecreaseScore() {
		score = Mathf.Max(0, score - 5);
	}
	
	public void IncreaseScore() {
		score += 10;
	}

}
