using Platformer.Core;
using Platformer.Mechanics;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player character lands after being airborne.
    /// </summary>
    /// <typeparam name="PlayerLanded"></typeparam>
    public class PlayerMoveLeft : Simulation.Event<PlayerMoveLeft>
    {
        public PlayerController player;

        public override void Execute()
        {
            if (player.audioSource && player.walkLeftAudio)
            {
                player.audioSource.clip = player.walkLeftAudio;
                player.audioSource.Play();
            }

        }
    }
}