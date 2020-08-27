using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 4;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 6;

        private float bonusBaseVelocity = 0;

        private int numExtraJumps = 0;

        private int jumpsAvailable = 0;

        private float speedMultiplier = 1;

        private int powerupFuel = 0;

        public void getToken()
        {
            bonusBaseVelocity += 1;
        }

        public enum Powerup
        {
            None,
            DoubleJump,
            Speed,
            WeakGravity
        }

        private Powerup currentPowerup = Powerup.None;

        public void getPowerup()
        {
            powerupFuel = 2000;
            int powerupKey = Random.Range(0, 3);
            switch (powerupKey)
            {
                case 0:
                    currentPowerup = Powerup.DoubleJump;
                    break;
                case 1:
                    currentPowerup = Powerup.Speed;
                    break;
                case 2:
                    currentPowerup = Powerup.WeakGravity;
                    break;
            }
        }

        public string horizontalID = "Horizontal";
        public string jumpID = "Jump";

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;
        public float energyDecayPerFrame = (float)0.02;

        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }
        
        void OnBecameInvisible()
        {
            Destroy(gameObject);
        }

        protected override void Update()
        {
            // Default values for powerup stats
            gravityModifier = 1;
            numExtraJumps = 0;
            speedMultiplier = 1;
            Debug.Log(currentPowerup);

            if (powerupFuel == 0)
            {
                currentPowerup = Powerup.None;
            }

            if (currentPowerup != Powerup.None)
            {
                powerupFuel -= 1;
            }

            // Enable special stats based on possible powerup.
            switch (currentPowerup)
            {
                case Powerup.DoubleJump:
                    numExtraJumps = 1;
                    break;
                case Powerup.Speed:
                    speedMultiplier = (float)1.4;
                    break;
                case Powerup.WeakGravity:
                    gravityModifier = (float)0.5;
                    break;
            }

            // Decay energy
            if (bonusBaseVelocity >= energyDecayPerFrame)
            {
                bonusBaseVelocity = bonusBaseVelocity - energyDecayPerFrame;
            }
            else
            {
                bonusBaseVelocity = 0;
            }

            Debug.Log(numExtraJumps);
            Debug.Log(jumpsAvailable);

            if (controlEnabled)
            {
                move.x = Input.GetAxis(horizontalID);
                if (Input.GetButtonDown(jumpID))
                {
                    if (jumpState == JumpState.Grounded)
                    {
                        jumpState = JumpState.PrepareToJump;
                        jumpsAvailable = numExtraJumps;
                    }
                    else if (jumpsAvailable != 0)
                    {
                        jumpsAvailable -= 1;
                        jumpState = JumpState.PrepareToJump;
                    }
                }
                else if (Input.GetButtonUp(jumpID))
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }
            }
            else
            {
                move.x = 0;
            }
            UpdateJumpState();
            base.Update();
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            float bonusVelocity = (float)(2 * Mathf.Log(bonusBaseVelocity + 1, 2));

            if (jump)
            {
                velocity.y = (jumpTakeOffSpeed + bonusVelocity) * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / (maxSpeed + bonusVelocity));

            targetVelocity = move * (speedMultiplier * (maxSpeed + bonusVelocity));
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}