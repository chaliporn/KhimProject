using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleLogic : MonoBehaviour
{
    public void GoToMain()
    {
        MainLogic.state = MainState.FreePlay;
        SceneManager.LoadScene("Main");
    }

    public void GoToLearn()
    {
        MainLogic.state = MainState.InTutorial;
        SceneManager.LoadScene("Learn");
    }
    
}
