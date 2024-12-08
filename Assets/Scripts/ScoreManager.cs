using TMPro;
using UnityEngine;

public sealed class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set;}

    private int score;

    public int Score
    {
        get => score;
        
        set
        {
            if (score == value)
            {
                return;
            }

            score = value;

            scoreText.SetText($"{score}");
        }
    }

    [SerializeField] private TextMeshProUGUI scoreText;

    private void Awake() => Instance = this;
}
