using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneWithString(string scene)
    {
        InputMaster.instance.NewInput();
        SceneManager.LoadScene(scene);
    }

    public void ReloadScene()
    {
        InputMaster.instance.NewInput();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}