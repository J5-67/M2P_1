using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class Click : MonoBehaviour
{
    [Header("--- Components ---")]
    [SerializeField] private AudioSource audioPlayer;

    [Header("--- Audio Resources ---")]
    [SerializeField] private AudioClip gameStartClip;
    [SerializeField] private AudioClip[] clickClips;
    [SerializeField] private AudioClip[] upgradeClips;
    [SerializeField] private AudioClip finishComboClip;

    [Header("--- Visuals (Sprite & Anim) ---")]
    [SerializeField] private Image characterImg;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite clickSprite;
    [SerializeField] private Sprite upgradeSprite;
    [SerializeField] private float faceDuration = 0.5f;

    [Header("--- Ending System ---")]
    [SerializeField] private Button pumpkinBtn;
    [SerializeField] private Image fadePanel;
    [SerializeField] private int pumpkinPrice = 9999;
    [SerializeField] private string endingSceneName = "EndingScene";

    [Header("--- Floating Text System (Pooling) ---")]
    [SerializeField] private FloatingText floatingTextPrefab;
    [SerializeField] private Transform textSpawnParent;
    private IObjectPool<FloatingText> textPool;

    [Header("--- UI Elements ---")]
    [SerializeField] private Button speakiBtn;
    [SerializeField] private Button upgradeBtn;
    [SerializeField] private Text goldText;

    [Header("--- Game Data ---")]
    [SerializeField] private int currentGold = 0;
    [SerializeField] private int clickGold = 1;
    [SerializeField] private int needGold = 10;

    private const string KEY_GOLD = "Save_Gold";
    private const string KEY_CLICK_LEVEL = "Save_ClickGold";
    private const string KEY_NEED_GOLD = "Save_NeedGold";
    private const string KEY_UPGRADE_LEVEL = "Save_UpgradeLevel";

    [SerializeField] private float comboTimeout = 0.5f;
    private float lastClickTime;
    private bool isClickingSequence;

    private Vector3 originScale;
    private Coroutine faceCoroutine;

    private void Awake()
    {
        if (audioPlayer == null) TryGetComponent(out audioPlayer);
        originScale = speakiBtn.transform.localScale;

        if (characterImg != null && defaultSprite != null) characterImg.sprite = defaultSprite;

        speakiBtn.onClick.AddListener(ClickSpeaki);
        upgradeBtn.onClick.AddListener(ClickUpgrade);
        pumpkinBtn.onClick.AddListener(ClickBuyPumpkin);

        InitTextPool();
        LoadGameData();
        UpdateUI();
    }

    private void Start()
    {
        PlaySound(gameStartClip);
        if (fadePanel != null) fadePanel.gameObject.SetActive(false);
    }

    private void InitTextPool()
    {
        textPool = new ObjectPool<FloatingText>(
            createFunc: () => Instantiate(floatingTextPrefab, textSpawnParent),
            actionOnGet: (textObj) => textObj.gameObject.SetActive(true),
            actionOnRelease: (textObj) => textObj.gameObject.SetActive(false),
            actionOnDestroy: (textObj) => Destroy(textObj.gameObject),
            defaultCapacity: 20,
            maxSize: 50
        );
    }

    private void OnApplicationPause(bool pause) { if (pause) SaveGameData(); }

    private void OnApplicationQuit() { SaveGameData(); }

    private void Update() { CheckComboFinish(); }

    private void CheckComboFinish()
    {
        if (isClickingSequence)
        {
            if (Time.time - lastClickTime > comboTimeout)
            {
                isClickingSequence = false;
                PlaySound(finishComboClip);
            }
        }
    }

    public void ClickSpeaki()
    {
        PlayClickSoundCutoff();
        lastClickTime = Time.time;
        isClickingSequence = true;

        StopCoroutine("Co_JellyBounce");
        StartCoroutine(Co_JellyBounce());
        ChangeFace(clickSprite);
        SpawnFloatingText(Input.mousePosition);

        currentGold += clickGold;
        UpdateUI();
    }

    public void ClickUpgrade()
    {
        if (currentGold < needGold) return;

        currentGold -= needGold;
        clickGold += 1;
        needGold = Mathf.CeilToInt(needGold * 1.5f);

        if (upgradeClips.Length > 0)
        {
            int index = Random.Range(0, upgradeClips.Length);
            PlaySound(upgradeClips[index]);
        }

        ChangeFace(upgradeSprite);
        SaveGameData();
        UpdateUI();
    }

    public void ClickBuyPumpkin()
    {
        if (currentGold < pumpkinPrice) return;

        currentGold -= pumpkinPrice;
        SaveGameData();
        UpdateUI();

        StartCoroutine(Co_GoToEnding());
    }

    private IEnumerator Co_GoToEnding()
    {
        pumpkinBtn.interactable = false;
        speakiBtn.interactable = false;
        upgradeBtn.interactable = false;

        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            float duration = 2.0f;
            float timer = 0f;
            Color startColor = new Color(0, 0, 0, 0);
            Color endColor = new Color(0, 0, 0, 1);

            while (timer < duration)
            {
                timer += Time.deltaTime;
                fadePanel.color = Color.Lerp(startColor, endColor, timer / duration);
                yield return null;
            }
            fadePanel.color = endColor;
        }

        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(endingSceneName);
    }

    private void ChangeFace(Sprite targetSprite)
    {
        if (targetSprite == null) return;
        if (faceCoroutine != null) StopCoroutine(faceCoroutine);
        faceCoroutine = StartCoroutine(Co_ShowFaceRoutine(targetSprite));
    }

    private IEnumerator Co_ShowFaceRoutine(Sprite targetSprite)
    {
        characterImg.sprite = targetSprite;
        yield return new WaitForSeconds(faceDuration);
        if (defaultSprite != null) characterImg.sprite = defaultSprite;
        faceCoroutine = null;
    }

    private void SpawnFloatingText(Vector3 pos)
    {
        FloatingText textObj = textPool.Get();
        textObj.Init(textPool, pos, clickGold);
    }

    private void SaveGameData()
    {
        PlayerPrefs.SetInt(KEY_GOLD, currentGold);
        PlayerPrefs.SetInt(KEY_CLICK_LEVEL, clickGold);
        PlayerPrefs.SetInt(KEY_NEED_GOLD, needGold);
        PlayerPrefs.SetInt(KEY_UPGRADE_LEVEL, PlayerPrefs.GetInt(KEY_UPGRADE_LEVEL, 0));
        PlayerPrefs.Save();
    }
    private void LoadGameData()
    {
        currentGold = PlayerPrefs.GetInt(KEY_GOLD, 0);
        clickGold = PlayerPrefs.GetInt(KEY_CLICK_LEVEL, 1);
        needGold = PlayerPrefs.GetInt(KEY_NEED_GOLD, 10);
    }
    [ContextMenu("Reset Data")]
    public void ResetGameData() { PlayerPrefs.DeleteAll(); currentGold = 0; clickGold = 1; needGold = 10; UpdateUI(); }

    private void PlayClickSoundCutoff()
    {
        if (clickClips.Length == 0) return;
        audioPlayer.Stop();
        int index = Random.Range(0, clickClips.Length);
        audioPlayer.clip = clickClips[index];
        audioPlayer.Play();
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        audioPlayer.PlayOneShot(clip);
    }

    private IEnumerator Co_JellyBounce()
    {
        speakiBtn.transform.localScale = originScale * 0.9f;
        yield return new WaitForSeconds(0.05f);
        speakiBtn.transform.localScale = originScale;
    }

    private void UpdateUI()
    {
        goldText.text = $"{currentGold} °ñµå\n(UP: {needGold}G)";
        upgradeBtn.interactable = currentGold >= needGold;
        if (pumpkinBtn != null) pumpkinBtn.interactable = currentGold >= pumpkinPrice;
    }
}