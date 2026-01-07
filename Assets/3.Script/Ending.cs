using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EndingDirector : MonoBehaviour
{
    [Header("--- Components ---")]
    [SerializeField] private AudioSource audioPlayer;
    [SerializeField] private Image fadePanel;

    [Header("--- Resources ---")]
    [SerializeField] private AudioClip endingBGM;

    private void Awake()
    {
        if (audioPlayer == null) TryGetComponent(out audioPlayer);
    }

    private void Start()
    {
        if (QuitManager.Instance != null)
        {
            QuitManager.Instance.SetEndingFlag();
        }

        if (audioPlayer != null && endingBGM != null)
        {
            audioPlayer.clip = endingBGM;
            audioPlayer.Play();
        }

        StartCoroutine(Co_FadeIn());
    }

    private IEnumerator Co_FadeIn()
    {
        if (fadePanel == null) yield break;

        fadePanel.gameObject.SetActive(true);
        float duration = 2.0f;
        float timer = 0f;

        Color startColor = Color.black;
        Color endColor = new Color(0, 0, 0, 0);

        fadePanel.color = startColor;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            fadePanel.color = Color.Lerp(startColor, endColor, timer / duration);
            yield return null;
        }

        fadePanel.color = endColor;
        fadePanel.gameObject.SetActive(false);
    }
}