using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine.Utility;
using Cinemachine;
using System;
using UnityEngine.Serialization;
using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;

namespace Platformer.Gameplay
{
    // Custom implementation of Cinemachine Virtual Camera
    public class FollowCamera : CinemachineVirtualCamera
    {
        // Update is called once per frame
        protected override void Update()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            GameObject maxPlayer = players[0];
            Debug.Log(players.Length);
            foreach (var player in players)
            {
                if (maxPlayer is null)
                {
                    maxPlayer = player;
                }
                else
                {
                    if (player.transform.position.x > maxPlayer.transform.position.x)
                    {
                        maxPlayer = player;
                    }
                }
            }
            m_Follow = maxPlayer.transform;
            m_LookAt = maxPlayer.transform;
            base.Update();
        }
    }
}

