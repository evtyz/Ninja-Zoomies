using UnityEngine;

namespace Platformer.View
{
    /// <summary>
    /// Used to move a transform relative to the main camera position with a scale factor applied.
    /// This is used to implement parallax scrolling effects on different branches of gameobjects.
    /// </summary>
    public class ParallaxLayer : MonoBehaviour
    {
        /// <summary>
        /// Movement of the layer is scaled by this value.
        /// </summary>
        // public Vector3 movementScale = Vector3.one;
        public float xMovementScale;

        private float originalCameraX;

        Transform _camera;

        void Awake()
        {
            _camera = Camera.main.transform;
            originalCameraX = _camera.position.x;
        }

        void LateUpdate()
        {
            transform.position = new Vector3((_camera.position.x - originalCameraX) * xMovementScale, transform.position.y, transform.position.z);
        }

    }
}
