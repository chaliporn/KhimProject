using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleLogic : MonoBehaviour
{
    public void GoToMain()
    {
        SceneManager.LoadScene("Main");
    }

    public void GoToLearn()
    {
        SceneManager.LoadScene("Learn");
    }
    
}
