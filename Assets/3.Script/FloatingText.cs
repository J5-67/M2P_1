using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;

public class FloatingText : MonoBehaviour
{
    [Header("--- Settings ---")]
    [SerializeField] private float moveSpeed = 100f;
    [SerializeField] private float lifeTime = 1.0f;
    [SerializeField] private Vector3 offset = new Vector3(0, 50, 0);

    private Text targetText;
    private RectTransform rectTransform;
    private Color originColor;
    private float timer;
    private IObjectPool<FloatingText> pool;

    private void Awake()
    {
        TryGetComponent(out targetText);
        TryGetComponent(out rectTransform);
        originColor = targetText.color;
    }

    public void Init(IObjectPool<FloatingText> _pool, Vector3 spawnPos, int goldAmount)
    {
        pool = _pool;
        transform.position = spawnPos + offset;
        targetText.text = $"+{goldAmount}";

        targetText.color = originColor;
        timer = 0f;

        float randomScale = Random.Range(0.9f, 1.2f);
        transform.localScale = Vector3.one * randomScale;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        rectTransform.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;

        float alpha = Mathf.Lerp(originColor.a, 0f, timer / lifeTime);
        targetText.color = new Color(originColor.r, originColor.g, originColor.b, alpha);

        if (timer >= lifeTime)
        {
            pool.Release(this);
        }
    }
}