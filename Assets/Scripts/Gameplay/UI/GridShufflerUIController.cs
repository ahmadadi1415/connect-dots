using UnityEngine;
using UnityEngine.UI;

public class GridShufflerUIController : MonoBehaviour {
    [SerializeField] private Button _shuffleButton;

    private void Start() {
        _shuffleButton.onClick.RemoveAllListeners();

        _shuffleButton.onClick.AddListener(OnShuffleButtonClicked);
    }

    private void OnShuffleButtonClicked()
    {
        EventManager.Publish<OnGridShuffledMessage>(new());
    }
}