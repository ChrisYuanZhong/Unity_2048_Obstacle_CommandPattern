using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TileBoard board;
    public CanvasGroup gameOverCanvasGroup;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI hiscoreText;

    public int score { get; set; }

    private void Start()
    {
        NewGame();
    }

    public void NewGame()
    {
        SetScore(0);
        hiscoreText.text = PlayerPrefs.GetInt("hiscore", 0).ToString();

        StartCoroutine(UIFade(gameOverCanvasGroup, 0f, 0f));
        gameOverCanvasGroup.interactable = false;

        board.enabled = true;
        board.NewGame();
    }

    public void GameOver()
    {
        board.enabled = false;
        gameOverCanvasGroup.interactable = true;

        StartCoroutine(UIFade(gameOverCanvasGroup, 1f, 1f));
    }

    private IEnumerator UIFade(CanvasGroup canVasGroup, float to, float delay)
    {
        yield return new WaitForSeconds(delay);
    
        float elapsedTime = 0f;
        float duration = 0.1f;
        float from = canVasGroup.alpha;

        while (elapsedTime < duration)
        {
            canVasGroup.alpha = Mathf.Lerp(from, to, elapsedTime / duration);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        canVasGroup.alpha = to;
    }

    public void IncreaseScore(int amount)
    {
        SetScore(score + amount);
    }

    private void SetScore(int score)
    {
        this.score = score;

        scoreText.text = score.ToString();

        SaveHiscore();
    }

    private void SaveHiscore()
    {
        if (score > PlayerPrefs.GetInt("hiscore", 0))
        {
            PlayerPrefs.SetInt("hiscore", score);
        }
    }
}
