using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLauncher : MonoBehaviour
{
    public static SceneLauncher Instance { get; private set; }
    [SerializeField] private GameObject _splashScreen;
    public static readonly string SCENE_MAINMENU = "MainMenu";
    public static readonly string SCENE_GAMEPLAY = "Gameplay";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _splashScreen.SetActive(false);
        }
    }

    private void OnEnable()
    {
        EventManager.Subscribe<OnChangeSceneMessage>(OnChangeScene);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<OnChangeSceneMessage>(OnChangeScene);
    }

    private void OnChangeScene(OnChangeSceneMessage message)
    {
        LoadScene(message.TargetSceneName);
    }

    private void LoadScene(string targetSceneName)
    {
        _splashScreen.SetActive(true);

        AsyncOperation loadedScene = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);
        loadedScene.completed += (asyncOperation) => _splashScreen.SetActive(false);
    }

    public void Exit()
    {
        Debug.Log("Exit Game");
        Application.Quit();
    }
}