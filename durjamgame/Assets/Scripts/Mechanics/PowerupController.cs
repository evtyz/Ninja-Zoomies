using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This class animates all powerup instances in a scene.
    /// This allows a single update call to animate hundreds of sprite 
    /// animations.
    /// If the powerups property is empty, it will automatically find and load 
    /// all powerup instances in the scene at runtime.
    /// </summary>
    public class PowerupController : MonoBehaviour
    {
        [Tooltip("Frames per second at which powerups are animated.")]
        public float frameRate = 12;
        [Tooltip("Instances of powerups which are animated. If empty, powerup instances are found and loaded at runtime.")]
        public PowerupInstance[] powerups;

        float nextFrameTime = 0;

        [ContextMenu("Find All Powerups")]
        void FindAllPowerupsInScene()
        {
            powerups = UnityEngine.Object.FindObjectsOfType<PowerupInstance>();
        }

        void Awake()
        {
            //if powerups are empty, find all instances.
            //if powerups are not empty, they've been added at editor time.
            if (powerups.Length == 0)
                FindAllPowerupsInScene();
            //Register all powerups so they can work with this controller.
            for (var i = 0; i < powerups.Length; i++)
            {
                powerups[i].powerupIndex = i;
                powerups[i].controller = this;
            }
        }

        void Update()
        {
            //if it's time for the next frame...
            if (Time.time - nextFrameTime > (1f / frameRate))
            {
                //update all powerups with the next animation frame.
                for (var i = 0; i < powerups.Length; i++)
                {
                    var powerup = powerups[i];
                    //if powerup is null, it has been disabled and is no longer animated.
                    if (powerup != null)
                    {
                        powerup._renderer.sprite = powerup.sprites[powerup.frame];
                        if (powerup.collected && powerup.frame == powerup.sprites.Length - 1)
                        {
                            powerup.gameObject.SetActive(false);
                            powerups[i] = null;
                        }
                        else
                        {
                            powerup.frame = (powerup.frame + 1) % powerup.sprites.Length;
                        }
                    }
                }
                //calculate the time of the next frame.
                nextFrameTime += 1f / frameRate;
            }
        }

    }
}