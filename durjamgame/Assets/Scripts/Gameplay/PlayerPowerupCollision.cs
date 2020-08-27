using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when a player collides with a powerup.
    /// </summary>
    /// <typeparam name="PlayerCollision"></typeparam>
    public class PlayerPowerupCollision : Simulation.Event<PlayerPowerupCollision>
    {
        public PlayerController player;
        public PowerupInstance powerup;

        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            player.getPowerup();
            AudioSource.PlayClipAtPoint(powerup.powerupCollectAudio, powerup.transform.position);
        }
    }
}