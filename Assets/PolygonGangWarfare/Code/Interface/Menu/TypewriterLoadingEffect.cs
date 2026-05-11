using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text;
[RequireComponent(typeof(TextMeshProUGUI))]
public class TypewriterLoadingEffect : MonoBehaviour
{
    [Header("Налаштування тексту")]
    [TextArea(3, 10)]
    public string fullText;

    [Header("Налаштування швидкості")]
    public float timePerCharacter = 0.03f;
    public float pauseAfterOk = 0.5f;

    private TextMeshProUGUI tmpText;
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        tmpText.text = "";
    }

    void OnEnable()
    {
        StartLoadingAnimation();
    }

    void OnDisable()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        tmpText.text = "";
        isTyping = false;
    }

    public void StartLoadingAnimation()
    {
        if (isTyping)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(fullText))
        {
            fullText = "> Підключення до камер спостереження... [ОК]\n\n" +
                       "> Аналіз периметра лабораторії... [ОК]\n\n" +
                       "> Заряджання зброї... [ОК]\n\n" +
                       "\n" +
                       "<color=#4FFB00>> ДОЗВІЛ НА ПРОВЕДЕННЯ МІСІЇ ОТРИМАНО.</color>";

        }

        tmpText.text = "";
        typingCoroutine = StartCoroutine(TypeTextCoroutine());
    }

    private IEnumerator TypeTextCoroutine()
    {
        isTyping = true;
        StringBuilder currentTextDisplay = new StringBuilder();

        int currentIndex = 0;
        bool insideTag = false;

        while (currentIndex < fullText.Length)
        {
            char currentChart = fullText[currentIndex];

            if (currentChart == '<') insideTag = true;

            currentTextDisplay.Append(currentChart);

            if (currentChart == '>')
            {
                insideTag = false;
                currentIndex++;
                tmpText.text = currentTextDisplay.ToString();
                continue;
            }

            if (insideTag)
            {
                currentIndex++;
                continue;
            }

            tmpText.text = currentTextDisplay.ToString();
            currentIndex++;

            if (currentChart == ']' && pauseAfterOk > 0)
            {
                yield return new WaitForSecondsRealtime(pauseAfterOk);
            }
            else if (currentChart != ' ' && currentChart != '\n')
            {
                yield return new WaitForSecondsRealtime(timePerCharacter);
            }
        }

        isTyping = false;
        typingCoroutine = null;
    }
}
