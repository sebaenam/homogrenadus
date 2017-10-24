﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PlayerMovement : MonoBehaviour {
	public float moveVelocity;
	public float jumpForce;
	public float timeBeforeStopJumping;
	public float jumpingTime;
	public float movementSlowAffectedByExplocion;

	private Rigidbody2D rigid;
	private Vector3 lastDirection;
	private Vector3 lastVelocity;
	private float verticalAxis;
	private float horizontalAxis;
	private bool jump=false;
	private Vector2 explosionForce=Vector2.zero;
	private bool grounded=true;
	private float jumpingSince=0;
	List<GameObject> lastCollisionGameObject = new List<GameObject>();

	PlayerInput input;

	void Awake () {
		input = GetComponent<PlayerInput> ();
		rigid = GetComponent<Rigidbody2D> ();
	}

	void Start(){
	}

	void Update(){
		horizontalAxis = Input.GetAxisRaw (input.Horizontal);
		verticalAxis = Input.GetAxisRaw(input.Vertical);
		if (Input.GetButton (input.Jump)) {jump = true;}else {jump = false;}
		if (Input.GetButtonUp (input.Jump) && jumpingSince!=0.0f) {jumpingSince = jumpingTime;}
	}

	void FixedUpdate(){
		//rigid.AddForce (Vector2.right, ForceMode2D.Force);
		rigid.velocity = new Vector2 (horizontalAxis * moveVelocity, rigid.velocity.y);
		if (horizontalAxis > 0.1f) {
			lastDirection = Vector3.right;
		} else if (horizontalAxis < -0.1f) {
			lastDirection = Vector3.left;
		}
		if (jump) {
			if (jumpingTime>jumpingSince) {
				jumpingSince += Time.deltaTime;
				//rigid.AddForce (Vector2.up * jumpForce, ForceMode2D.Force);
				rigid.velocity = new Vector2(rigid.velocity.x,jumpForce*(jumpingTime-jumpingSince));
			}
		}

		if (!grounded) {
			rigid.velocity = new Vector2 (rigid.velocity.x, rigid.velocity.y + (-rigid.gravityScale*rigid.mass));
		}
		if (explosionForce != Vector2.zero) {
			rigid.velocity = rigid.velocity / movementSlowAffectedByExplocion + explosionForce;
		}
	}

	public IEnumerator AddExplosionForce(Vector2 direction, float timeExploding){
		float currentTime = 0;
		while (currentTime <= timeExploding) {

			explosionForce = direction;
			currentTime += Time.fixedDeltaTime;
			yield return null;
		}
		explosionForce = Vector2.zero;
	}

	void OnCollisionEnter2D(Collision2D col){
		if (col.gameObject.tag.Equals ("Ground")) {
			grounded = true;
			lastCollisionGameObject.Add(col.gameObject);
			if (gameObject.activeSelf) {
				StopCoroutine ("ExitGroundJumpChance");
				jumpingSince=0;
			}
		}
	}

	void OnCollisionExit2D(Collision2D col){
		if (col.gameObject.tag.Equals ("Ground")) {
			lastCollisionGameObject.Remove (col.gameObject);
			if(lastCollisionGameObject.Count!=0){
				return;
			}
			grounded = false;
			if (gameObject.activeSelf && jumpingSince!=0.0f) {
				StartCoroutine (ExitGroundJumpChance (timeBeforeStopJumping));
			}
		}
	}
	IEnumerator ExitGroundJumpChance(float time){
		yield return new WaitForSeconds (time);
		if (gameObject.activeSelf && jumpingSince != 0.0f) {
			jumpingSince = jumpingTime;
		}
	}



	//Getters
	public Vector3 LastDirection {
		get {
			return this.lastDirection;
		}
	}
	public float VerticalAxis {
		get {
			return this.verticalAxis;
		}
	}
	
	public float HorizontalAxis {
		get {
			return this.horizontalAxis;
		}
	}
}
