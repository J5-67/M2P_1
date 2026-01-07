using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Click : MonoBehaviour
{
    [Header("--- Components ---")]
    [SerializeField] private AudioSource audioPlayer;

    [Header("--- Resources ---")]
    [SerializeField] private AudioClip[] clickClips;
    [SerializeField] private AudioClip upgradeClip;

    [Header("--- Visuals (Sprite & Anim) ---")]
    [SerializeField] private Image characterImg;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite clickSprite;
    [SerializeField] private float faceDuration = 1.18f;

    [Header("--- UI Elements ---")]
    [SerializeField] private Button speakiBtn;
    [SerializeField] private Button upgradeBtn;
    [SerializeField] private Text goldText;

    [Header("--- Game Data ---")]
    [SerializeField] private int currentGold = 0;
    [SerializeField] private int clickGold = 1;
    [SerializeField] private int needGold = 10;

    private Vector3 originScale;
    private Coroutine faceCoroutine;

    private void Awake()
    {
        if (audioPlayer == null) TryGetComponent(out audioPlayer);

        originScale = speakiBtn.transform.localScale;
        if (characterImg != null && defaultSprite != null) characterImg.sprite = defaultSprite;

        speakiBtn.onClick.AddListener(ClickSpeaki);
        upgradeBtn.onClick.AddListener(ClickUpgrade);

        UpdateUI();
    }

    public void ClickSpeaki()
    {
        PlayClickSoundCutoff();

        StopCoroutine("Co_JellyBounce");
        StartCoroutine(Co_JellyBounce());

        if (faceCoroutine != null) StopCoroutine(faceCoroutine);
        faceCoroutine = StartCoroutine(Co_FaceExpression());

        currentGold += clickGold;
        UpdateUI();
    }

    public void ClickUpgrade()
    {
        if (currentGold < needGold) return;

        currentGold -= needGold;
        clickGold += 1;
        needGold = Mathf.CeilToInt(needGold * 1.5f);

        if (upgradeClip != null) audioPlayer.PlayOneShot(upgradeClip);

        UpdateUI();
    }

    private void PlayClickSoundCutoff()
    {
        if (clickClips.Length == 0) return;
        audioPlayer.Stop();
        int index = Random.Range(0, clickClips.Length);
        audioPlayer.clip = clickClips[index];
        audioPlayer.Play();
    }

    private IEnumerator Co_JellyBounce()
    {
        speakiBtn.transform.localScale = originScale * 0.9f;
        yield return new WaitForSeconds(0.05f);
        speakiBtn.transform.localScale = originScale;
    }

    private IEnumerator Co_FaceExpression()
    {
        if (clickSprite != null) characterImg.sprite = clickSprite;

        yield return new WaitForSeconds(faceDuration);

        if (defaultSprite != null) characterImg.sprite = defaultSprite;
        faceCoroutine = null;
    }

    private void UpdateUI()
    {
        goldText.text = $"{currentGold} °ñµå\n(UP: {needGold}G)";
        upgradeBtn.interactable = currentGold >= needGold;
    }
}