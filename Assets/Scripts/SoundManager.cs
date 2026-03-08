using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private const string PLAYER_PREFS_SOUND_EFFECT_VOLUMN = "SoundEffectVolumn";

    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    private float volumn = 1f;

    private void Awake()
    {
        Instance = this;

        volumn = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECT_VOLUMN, 1f);
    }
    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFail += DeliveryManager_OnRecipeFail;
        CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
        //Player.Instance.OnPickedSomething += Player_OnPickedSomething;
        Player.OnAnyPickedSomething += Player_OnPickedSomething;
        BaseCounter.OnAnyObjectPlacecdHere += BaseCounter_OnAnyObjectPlacecdHere;
        TrashCounter.OnAnyObjectTrashed += TrashCounter_OnAnyObjectTrashed;
    }

    private void TrashCounter_OnAnyObjectTrashed(object sender, System.EventArgs e)
    {
        TrashCounter trashCounter = sender as TrashCounter;
        PlaySound(audioClipRefsSO.trash, trashCounter.transform.position);
    }

    private void BaseCounter_OnAnyObjectPlacecdHere(object sender, System.EventArgs e)
    {
        // 为何要在counter上播放音效，直接在Main Camera的位置播放音效不久好了吗
        BaseCounter counter = sender as BaseCounter;
        PlaySound(audioClipRefsSO.objectDrop, counter.transform.position);
    }

    private void Player_OnPickedSomething(object sender, System.EventArgs e)
    {
        Player player = sender as Player;
        PlaySound(audioClipRefsSO.objectPickup, player.transform.position);
    }

    private void CuttingCounter_OnAnyCut(object sender, System.EventArgs e)
    {
        CuttingCounter cuttingCounter = sender as CuttingCounter;
        PlaySound(audioClipRefsSO.chop, cuttingCounter.transform.position);
    }

    private void DeliveryManager_OnRecipeFail(object sender, System.EventArgs e)
    {
        PlaySound(audioClipRefsSO.deliveryFail, DeliveryCounter.Instance.transform.position);
    }

    private void DeliveryManager_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        PlaySound(audioClipRefsSO.deliverySuccess, DeliveryCounter.Instance.transform.position);
    }

    public void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volumnMultiplier = 1f)
    {
        PlaySound(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volumnMultiplier);
    }
    public void PlaySound(AudioClip audioClip, Vector3 position, float volumnMultiplier = 1f)
    {
        AudioSource.PlayClipAtPoint(audioClip, position, volumn * volumnMultiplier);
    }
    public void PlayFootstepSound(Vector3 position, float volumnMultiplier)
    {
        PlaySound(audioClipRefsSO.footStep, position, volumnMultiplier);
    }
    public void PlayCountdownSound(float volumnMultiplier = 1f)
    {
        PlaySound(audioClipRefsSO.warning, Vector3.zero, volumnMultiplier);
    }
    public void ChangeVolumn()
    {
        volumn += 0.1f;
        if (volumn > 1f)
        {
            volumn = 0;
        }
        PlayerPrefs.SetFloat(PLAYER_PREFS_SOUND_EFFECT_VOLUMN, volumn);
        PlayerPrefs.Save();
    }
    public float GetVolumn() => volumn;
    public void PlayWarningSound(Vector3 position, float volumnMutiplier)
    {
        PlaySound(audioClipRefsSO.warning, position, volumnMutiplier);
    }
}
