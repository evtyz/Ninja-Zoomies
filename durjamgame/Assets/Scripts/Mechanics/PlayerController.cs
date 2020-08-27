using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;

public class PlayerController : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 400f;							// Amount of force added when the player jumps.
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement
	[SerializeField] private bool m_AirControl = false;							// Whether or not a player can steer while jumping;
	[SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character
	[SerializeField] private Transform m_GroundCheck;							// A position marking where to check if the player is grounded.

	const float k_GroundedRadius = .1f; // Radius of the overlap circle to determine if grounded
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

	private int stunTimeLeft = 0;
	public int stunTime;

	SpriteRenderer spriteRenderer;

    public AudioSource audioSource;

    public float runSpeed = 40f;

	float horizontalMove = 0f;
	bool jump = false;

	private double energy = 0;
	public double energyDecayRate;
	private double bonusForce = 0;
	private int powerupFuel;
	public int powerupDuration;

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
		Destroy(gameObject);
	}

	public void stun()
	{
		stunTimeLeft = stunTime;
	}

    public void getToken()
    {
		energy += 1;
    }

    public void getPowerup()
    {
		int powerupKey = Random.Range(1, System.Enum.GetNames(typeof(Powerup)).Length);
		powerup = (Powerup)powerupKey;
		powerupFuel = powerupDuration;
    }

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
		spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
	}


	private void FixedUpdate()
	{
		if (stunTimeLeft != 0)
		{
			stunTimeLeft -= 1;
		}


		if (energy >= energyDecayRate)
		{
			energy -= energyDecayRate;
		}
		else
		{
			energy = 0;
		}

		gravityModifier = 1;
		numExtraJumps = 0;
		speedMultiplier = 1;

		if (powerupFuel != 0)
		{
			powerupFuel -= 1;
		}
		else
		{
			powerup = Powerup.None;
		}

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

		bonusForce = 2 * Mathf.Log((float)energy + 1, 2);

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
		if (stunTimeLeft == 0)
		{
			horizontalMove = Input.GetAxisRaw(horizontalAxis) * (runSpeed + (float)bonusForce) * ((float)speedMultiplier);

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
