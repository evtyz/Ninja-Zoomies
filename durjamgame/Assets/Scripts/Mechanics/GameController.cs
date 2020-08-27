using Platformer.Core;
using Platformer.Model;
using UnityEngine;
using Platformer.Mechanics;
using System;
using System.Collections.Generic;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This class exposes the the game model in the inspector, and ticks the
    /// simulation.
    /// </summary> 
    public class GameController : MonoBehaviour
    {
        public static GameController Instance { get; private set; }

        //This model field is public and can be therefore be modified in the 
        //inspector.
        //The reference actually comes from the InstanceRegister, and is shared
        //through the simulation and events. Unity will deserialize over this
        //shared reference when the scene loads, allowing the model to be
        //conveniently configured inside the inspector.
        public PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        private bool gameRunning = true;

        public int gameOverThreshold;

        void OnEnable()
        {
            Instance = this;
        }

        void OnDisable()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (Instance == this) Simulation.Tick();

            if (gameRunning)
            {
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

                if (players.Length == 1)
                {
                    Win(players[0].GetComponent<PlayerController>());
                }
                else
                {
                    foreach(GameObject player in players)
                    {
                        if (player.transform.position.x > gameOverThreshold)
                        {
                            Win(player.GetComponent<PlayerController>());
                            break;
                        }
                    }
                }
            }
        }

        void Win(PlayerController player)
        {
            // TODO: make game over screen
            Debug.Log("Player " + player.ID + " wins!");
            gameRunning = false;
        }
    }
}