using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{

    [SerializeField] GameObject menu;
    [SerializeField] Button restartButton;
    [SerializeField] Button quitButton;
    [SerializeField] Button continueButton;
    [SerializeField] BuildingPlacer buildingPlacer;
    // Start is called before the first frame update
    private void Awake()
    {
        ResumeGame();
    }
    void Start()
    {
        restartButton.onClick.AddListener(RestartScene);
        quitButton.onClick.AddListener(QuitGame);
        continueButton.onClick.AddListener(() => { 
            menu.SetActive(false);
        });
    }
    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    private void RestartScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
    private void PauseGame()
    {
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }
    private void ResumeGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !menu.activeInHierarchy)  // Restart when 'R' key is pressed
        {
            menu.SetActive(true);
            buildingPlacer.CancelPlacement();
            PauseGame();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) & menu.activeInHierarchy)
        {
            ResumeGame();
            menu.SetActive(false);
        }
    }
}
