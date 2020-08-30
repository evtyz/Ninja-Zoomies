using Platformer.Core;
using Platformer.Mechanics;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player character lands after being airborne.
    /// </summary>
    /// <typeparam name="PlayerLanded"></typeparam>
    public class PlayerMoveRight : Simulation.Event<PlayerMoveRight>
    {
        public PlayerController player;

        public override void Execute()
        {
            if (player.audioSource && player.walkRightAudio)
            {
                player.audioSource.clip = player.walkRightAudio;
                player.audioSource.Play();
            }

        }
    }
}