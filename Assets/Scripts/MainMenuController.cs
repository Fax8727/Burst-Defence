using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject helpPanel;

    void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.Instance.mainMenuBGM);
        }
    }

    public void OnStartClick()
    {
        AudioManager.Instance?.PlayClickSound();

        SceneManager.LoadScene("GameLevel");
    }

    public void OnExitClick()
    {
        AudioManager.Instance?.PlayClickSound();

        Debug.Log("게임 종료!");
        Application.Quit();
    }

    public void OnHelpClick()
    {
        AudioManager.Instance?.PlayClickSound();

        if (helpPanel != null)
        {
            helpPanel.SetActive(true);
        }
    }


    public void OnCloseHelpClick()
    {
        AudioManager.Instance?.PlayClickSound();

        if (helpPanel != null)
        {
            helpPanel.SetActive(false);
        }
    }
}