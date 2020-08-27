using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This class animates all stunconsume instances in a scene.
    /// This allows a single update call to animate hundreds of sprite 
    /// animations.
    /// If the stunconsumes property is empty, it will automatically find and load 
    /// all stunconsume instances in the scene at runtime.
    /// </summary>
    public class StunconsumeController : MonoBehaviour
    {
        [Tooltip("Frames per second at which stunconsumes are animated.")]
        public float frameRate = 12;
        [Tooltip("Instances of stunconsumes which are animated. If empty, stunconsume instances are found and loaded at runtime.")]
        public StunconsumeInstance[] stunconsumes;

        float nextFrameTime = 0;

        [ContextMenu("Find All Stunconsumes")]
        void FindAllStunconsumesInScene()
        {
            stunconsumes = UnityEngine.Object.FindObjectsOfType<StunconsumeInstance>();
        }

        void Awake()
        {
            //if stunconsumes are empty, find all instances.
            //if stunconsumes are not empty, they've been added at editor time.
            if (stunconsumes.Length == 0)
                FindAllStunconsumesInScene();
            //Register all stunconsumes so they can work with this controller.
            for (var i = 0; i < stunconsumes.Length; i++)
            {
                stunconsumes[i].stunconsumeIndex = i;
                stunconsumes[i].controller = this;
            }
        }

        void Update()
        {
            //if it's time for the next frame...
            if (Time.time - nextFrameTime > (1f / frameRate))
            {
                //update all stunconsumes with the next animation frame.
                for (var i = 0; i < stunconsumes.Length; i++)
                {
                    var stunconsume = stunconsumes[i];
                    //if stunconsume is null, it has been disabled and is no longer animated.
                    if (stunconsume != null)
                    {
                        stunconsume._renderer.sprite = stunconsume.sprites[stunconsume.frame];
                        if (stunconsume.collected && stunconsume.frame == stunconsume.sprites.Length - 1)
                        {
                            stunconsume.gameObject.SetActive(false);
                            stunconsumes[i] = null;
                        }
                        else
                        {
                            stunconsume.frame = (stunconsume.frame + 1) % stunconsume.sprites.Length;
                        }
                    }
                }
                //calculate the time of the next frame.
                nextFrameTime += 1f / frameRate;
            }
        }

    }
}