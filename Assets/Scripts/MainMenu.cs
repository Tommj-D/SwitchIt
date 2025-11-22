using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private SceneController sceneController;

    public void Play()
    {
        sceneController.LoadScene("GameScene");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
