using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    public IndividualScene currentScene;

    public void Awake()
    {
        instance = this;
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}