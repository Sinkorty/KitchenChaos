using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private const string PLAYER_PREFS_MUSIC_VOLUMN = "MusicVolumn";
    public static MusicManager Instance { get; private set; }

    private AudioSource audioSource;

    private float volumn = 1f;

    private void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        volumn = PlayerPrefs.GetFloat(PLAYER_PREFS_MUSIC_VOLUMN, 1f);
    }

    public void ChangeVolumn()
    {
        volumn += 0.1f;
        if (volumn > 1f)
        {
            volumn = 0;
        }
        PlayerPrefs.SetFloat(PLAYER_PREFS_MUSIC_VOLUMN, volumn);
        PlayerPrefs.Save();     
    }
    public float GetVolumn() => volumn;
}
