using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChangeSceneButton : MonoBehaviour {

    public void OnMouseClick(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }
}
