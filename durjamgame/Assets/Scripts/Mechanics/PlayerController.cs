using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using Platformer.UI;

public class PlayerController : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 400f;							// Amount of force added when the player jumps.
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement
	[SerializeField] private bool m_AirControl = false;							// Whether or not a player can steer while jumping;
	[SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character
	[SerializeField] private Transform m_GroundCheck;							// A position marking where to check if the player is grounded.

	const float k_GroundedRadius = .25f; // Radius of the overlap circle to determine if grounded
	private bool m_Grounded;            // Whether or not the player is grounded.
	private Rigidbody2D m_Rigidbody2D;
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private Vector3 velocity = Vector3.zero;
    public AudioClip jumpAudio;
    public AudioClip respawnAudio;
    public AudioClip ouchAudio;
    public string horizontalAxis;
    public string jumpAxis;
	public int ID;
	internal Animator animator;
	readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

	public OverlayController overlay;

	private float distanceBehindCamera = 0;
	private Transform _camera;

	private const float MIN_DISTANCE_BEHIND_CAMERA = (float)-1.244;
	private const float MAX_DISTANCE_BEHIND_CAMERA = (float)13f;
	private float catchupSpeed = 0;

	public float catchupFactor = 5;

	private double stunTimeLeft = 0;
	public double stunTime;
	private double timeSinceLastStun;
	public double stunInvulnerabilityDuration;

	SpriteRenderer spriteRenderer;

    public AudioSource audioSource;

    public float runSpeed = 40f;

	float horizontalMove = 0f;
	bool jump = false;

	private double energy = 0;
	public double energyDecayRate;
	private double bonusForce = 0;
	private double powerupDurationLeft;
	public double powerupDuration;
	public double energyStrength;

	private double gravityModifier = 1;
	private int numExtraJumps = 0;
	private double speedMultiplier = 1;

	private int numExtraJumpsLeft = 0;

	public double lowGravityPowerupFactor = 0.5;
	public double speedPowerupFactor = 1.5;
	public int extraJumpsPowerupFactor = 1;

	private enum Powerup
	{
		None,
		LowGravity,
		ExtraSpeed,
		DoubleJump
	}

	private Powerup powerup = Powerup.None;

	void OnBecameInvisible()
	{
		overlay.kill();
		Destroy(gameObject);
	}

	public void stun()
	{
		if (timeSinceLastStun < stunInvulnerabilityDuration + stunTime)
		{
			return;
		}

		if (stunTimeLeft < stunTime && stunTimeLeft != 0)
		{
			return;
		}

		animator.SetBool("stunned", true);
		stunTimeLeft = stunTime;
		timeSinceLastStun = 0;
		energy = 0;
	}

    public void getToken()
    {
		energy += 1;
    }

    public void getPowerup()
    {
		int powerupKey = Random.Range(1, System.Enum.GetNames(typeof(Powerup)).Length);
		powerup = (Powerup)powerupKey;
		overlay.setPowerup(powerupKey);
		powerupDurationLeft = powerupDuration;
    }

	private void Awake()
	{
		timeSinceLastStun = stunInvulnerabilityDuration;
		_camera = Camera.main.transform;
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
		spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
	}


	private void FixedUpdate()
	{

		m_Grounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
				m_Grounded = true;
		}

		animator.SetBool("grounded", m_Grounded);
        
        Move(horizontalMove * Time.fixedDeltaTime, jump);
		jump = false;
	}


	public void Move(float move, bool jump)
	{
		//only control the player if grounded or airControl is turned on
		if (m_Grounded || m_AirControl)
		{
			// Move the character by finding the target velocity
			Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
			// And then smoothing it out and applying it to the character
			m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref velocity, m_MovementSmoothing);

			// If the input is moving the player right and the player is facing left...
			if (move > 0 && !m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
			// Otherwise if the input is moving the player left and the player is facing right...
			else if (move < 0 && m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
		}
		// If the player should jump...
		if ((m_Grounded || numExtraJumpsLeft != 0) && jump)
		{
			if ((!m_Grounded) && numExtraJumpsLeft != 0)
			{
				numExtraJumpsLeft -= 1;
			}
		
			// Add a vertical force to the player.
			m_Grounded = false;
			m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0);
			m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce + (float)bonusForce * 10));
		}

		if (m_Grounded)
		{
			numExtraJumpsLeft = numExtraJumps;
		}
		animator.SetBool("grounded", m_Grounded);
		animator.SetFloat("velocityX", Mathf.Abs(horizontalMove));

	}

    public void Update()
    {
		timeSinceLastStun += Time.deltaTime;

		if (stunTimeLeft > 0)
		{
			stunTimeLeft -= Time.deltaTime;
		}
		if (stunTimeLeft < 0)
		{
			animator.SetBool("stunned", false);
			stunTimeLeft = 0;
		}


		if (energy > 0)
		{
			energy -= energyDecayRate * Time.deltaTime;
		}

		if (energy < 0)
		{
			energy = 0;
		}

		if (powerupDurationLeft > 0)
		{
			powerupDurationLeft -= Time.deltaTime;
		}
		if (powerupDurationLeft <= 0)
		{
			powerup = Powerup.None;
			overlay.setPowerup(0);
			powerupDurationLeft = 0;
		}

		gravityModifier = 1;
		numExtraJumps = 0;
		speedMultiplier = 1;

		switch (powerup)
		{
			case Powerup.DoubleJump:
				numExtraJumps = extraJumpsPowerupFactor;
				break;
			case Powerup.ExtraSpeed:
				speedMultiplier = speedPowerupFactor;
				break;
			case Powerup.LowGravity:
				gravityModifier = lowGravityPowerupFactor;
				break;
		}

		m_Rigidbody2D.gravityScale = (float)gravityModifier;

		bonusForce = energyStrength * Mathf.Log((float)energy + 1, 2);

		distanceBehindCamera = _camera.position.x - transform.position.x;
		catchupSpeed = catchupFactor * (distanceBehindCamera - MIN_DISTANCE_BEHIND_CAMERA) / (MAX_DISTANCE_BEHIND_CAMERA - MIN_DISTANCE_BEHIND_CAMERA);

		if (stunTimeLeft == 0)
		{
			horizontalMove = Input.GetAxisRaw(horizontalAxis) * (runSpeed + (float)bonusForce + (float)catchupSpeed) * ((float)speedMultiplier);

			if (Input.GetButtonDown(jumpAxis))
			{
				jump = true;
			}
		}
		else
		{
			horizontalMove = 0;
			jump = false;
		}

    }


	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Flip x axis of local scale for transform
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
}
