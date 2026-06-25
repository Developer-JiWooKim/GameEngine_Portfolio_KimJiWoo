using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private TextMeshProUGUI _mazeSizeText;
    [SerializeField] private TextMeshProUGUI _resultTimeText;
    [SerializeField] private Button _replayButton;
    [SerializeField] private Button _gameEndButton;

    private void Awake()
    {
        _replayButton.onClick.AddListener(() => GameManager.Instance.Replay());
        _gameEndButton.onClick.AddListener(() => GameManager.Instance.GameExit());
    }

    public void Show(string message, string mazeSizeLabel, string formattedTime)
    {
        _resultText.text = message;
        _mazeSizeText.text = mazeSizeLabel;
        _resultTimeText.text = $"End Time : {formattedTime}";

        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);
}
