using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

public class DialogueSystem : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public CanvasGroup interactPanel;
    public CanvasGroup dialoguePanel;

    [Header("Settings")]
    public float animateSpeed = 0.1f;

    public bool interactPanelActive { get; private set; } = false;
    public bool dialoguePanelActive { get; private set; } = false;
    bool gotActualText = false;

    public void ToggleInteractPanel(bool val)
    {
        // if (val && !interactPanel.gameObject.activeInHierarchy) interactPanel.gameObject.SetActive(true);
        interactPanel.DOFade(val ? 1.0f : 0.0f, 0.3f)
        .OnComplete(() =>
        {
            // if (!val) interactPanel.gameObject.SetActive(false);
        });
        interactPanelActive = val;
    }

    public void ToggleDialoguePanel(bool val)
    {
        // if (val && !dialoguePanel.gameObject.activeInHierarchy) dialoguePanel.gameObject.SetActive(true);
        dialoguePanel.DOFade(val ? 1.0f : 0.0f, 0.3f)
        .OnComplete(() =>
        {
            // if (!val) dialoguePanel.gameObject.SetActive(false);
        });
        dialoguePanelActive = val;
    }

    public void AnimateDots()
    {
        StartCoroutine(AnimateDotsCo());
    }
    public void AnimateText(string text)
    {
        gotActualText = true;
        dialogueText.text = "";
        StartCoroutine(AnimateTextCo(text));
    }

    private IEnumerator AnimateDotsCo()
    {
        while (!gotActualText)
        {
            yield return StartCoroutine(AnimateTextCo("..."));
            yield return new WaitForSeconds(1.0f);
            if (gotActualText) yield break;
            dialogueText.text = "";
        }
    }

    private IEnumerator AnimateTextCo(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            dialogueText.text += text[i];
            yield return new WaitForSeconds(animateSpeed);
        }
        gotActualText = false;
    }
}