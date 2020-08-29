using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverMenu : MonoBehaviour {

   public void RestartGame()
   {
       SceneManager.LoadScene(0); // loads start screen and game
   }

   public void QuitGame ()
   {
     Debug.Log("QUIT");
     Application.Quit();
   }

}
