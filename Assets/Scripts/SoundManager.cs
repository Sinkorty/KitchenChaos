using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFail += DeliveryManager_OnRecipeFail;
    }

    private void DeliveryManager_OnRecipeFail(object sender, System.EventArgs e)
    {

    }

    private void DeliveryManager_OnRecipeSuccess(object sender, System.EventArgs e)
    {

    }

    private void PlaySound(AudioClip audioClip,Vector3 position, float volumn=1f)
    {
        AudioSource.PlayClipAtPoint(audioClip, position, volumn);
    }
}
