using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when a player collides with a token.
    /// </summary>
    /// <typeparam name="PlayerCollision"></typeparam>
    public class PlayerStunCollision : Simulation.Event<PlayerStunCollision>
    {
        public PlayerController player;

        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            if (player.stun())
            {
                if (player.audioSource && player.zapAudio)
                {
                    player.audioSource.PlayOneShot(player.zapAudio);
                }
            }
        }
    }
}