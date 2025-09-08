using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private AudioSource sfxPreviewSource;

    private void Start()
    {
        // Cargar valores guardados (default 0.75f si no existen)
        float musicValue = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfxValue = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        musicSlider.value = musicValue;
        sfxSlider.value = sfxValue;

        SetMusicVolume(musicValue);
        SetSFXVolume(sfxValue);

        // Escuchar cambios en los sliders
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float value)
    {
        // Convertir a decibelios y guardar
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        // Convertir a decibelios y guardar
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat("SFXVolume", value);

        // Reproducir un sonido de prueba al mover el slider
        if (sfxPreviewSource != null && sfxPreviewSource.clip != null)
        {
            if (!sfxPreviewSource.isPlaying)
            {
                sfxPreviewSource.PlayOneShot(sfxPreviewSource.clip);
            }
        }
    }
}
