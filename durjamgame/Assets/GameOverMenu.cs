using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Platformer.Mechanics;

public class GameOverMenu : MonoBehaviour {

   public void RestartGame()
   {
       SceneManager.LoadScene(0); // loads start screen and game
   }

   public void QuitGame ()
   {
     Application.Quit();
   }

   public static GameObject[] gameObjects;

   public void Awake()
   {
    gameObjects = new GameObject[] {GameObject.Find("Player1EndScreen"), GameObject.Find("Player2EndScreen"), GameObject.Find("Player3EndScreen"), GameObject.Find("Player4EndScreen")};
   }

   public void Update()
   {
    //gameObjects = new GameObject[] {GameObject.Find("Player1EndScreen"), GameObject.Find("Player2EndScreen"), GameObject.Find("Player3EndScreen"), GameObject.Find("Player4EndScreen")};
    foreach (var gameObject in gameObjects)
    {
      gameObject.SetActive(false);
    }
    gameObjects[GameController.winningPlayer - 1].SetActive(true);
   }

}
