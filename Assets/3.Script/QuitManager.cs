using UnityEngine;
using UnityEngine.UI;

public class QuitManager : MonoBehaviour
{
    public static QuitManager Instance { get; private set; }

    [Header("--- Audio Settings ---")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _normalQuitClip;
    [SerializeField] private AudioClip _endingQuitClip;

    [Header("--- Visual Settings ---")]
    [Tooltip("종료 시 화면이 어두워지는 시간")]
    [SerializeField] private float _fadeDuration = 2.0f;

    private bool _hasSeenEnding = false;
    private bool _isQuitting = false;

    private CanvasGroup _fadeCanvasGroup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            CreateFadeUI();
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Application.wantsToQuit += HandleQuitRequest;
    }

    private void CreateFadeUI()
    {
        GameObject canvasObj = new GameObject("QuitFadeCanvas");
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject imageObj = new GameObject("BlackPanel");
        imageObj.transform.SetParent(canvasObj.transform, false);

        Image img = imageObj.AddComponent<Image>();
        img.color = Color.black;
        img.raycastTarget = false;

        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        _fadeCanvasGroup = imageObj.AddComponent<CanvasGroup>();
        _fadeCanvasGroup.alpha = 0f;
        _fadeCanvasGroup.blocksRaycasts = false;
    }

    public void SetEndingFlag()
    {
        _hasSeenEnding = true;
    }

    private bool HandleQuitRequest()
    {
        if (_isQuitting) return true;

        QuitSequence();
        return false;
    }

    private async void QuitSequence()
    {
        _isQuitting = true;

        AudioClip clipToPlay = _hasSeenEnding ? _endingQuitClip : _normalQuitClip;
        if (clipToPlay != null)
        {
            _audioSource.PlayOneShot(clipToPlay);
        }

        if (_fadeCanvasGroup != null)
        {
            _fadeCanvasGroup.blocksRaycasts = true;
            float timer = 0f;

            float waitTime = (clipToPlay != null) ? Mathf.Max(clipToPlay.length, _fadeDuration) : _fadeDuration;

            while (timer < waitTime)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / _fadeDuration);
                _fadeCanvasGroup.alpha = progress;

                await Awaitable.NextFrameAsync();
            }
            _fadeCanvasGroup.alpha = 1f;
        }

        QuitGame();
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        Application.wantsToQuit -= HandleQuitRequest;
    }
}