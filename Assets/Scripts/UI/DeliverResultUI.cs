using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliverResultUI : MonoBehaviour
{
    private const string ANIMATOR_POPUP = "Popup";

    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI messageText;
    private Animator animator;

    [SerializeField] private Color successColor;
    [SerializeField] private Color failColor;

    [SerializeField] private Sprite successSprite;
    [SerializeField] private Sprite failSprite;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFail += DeliveryManager_OnRecipeFail;

        gameObject.SetActive(false);
    }

    private void DeliveryManager_OnRecipeFail(object sender, System.EventArgs e)
    {
        gameObject.SetActive(true);
        backgroundImage.color = failColor;
        iconImage.sprite = failSprite;
        messageText.text = "DELIVERY\nFAILED";
        animator.SetTrigger(ANIMATOR_POPUP);
    }

    private void DeliveryManager_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        gameObject.SetActive(true);
        backgroundImage.color = successColor;
        iconImage.sprite = successSprite;
        messageText.text = "DELIVERY\nSUCCESS";
        animator.SetTrigger(ANIMATOR_POPUP);
    }
}
