using UnityEngine;
using UnityEngine.Events;

public enum TypeOfSound
{
    Def = 0,
    Cool = 1,
    Cooler = 2,
    Coolest = 3
}

[System.Serializable]
public class SoundEvent : UnityEvent<TypeOfSound> { }

public class SoundManager : MonoBehaviour
{
    [Header("Bubble Sounds")]
    [SerializeField] private AudioClip[] bubbleSoundsDef;
    [SerializeField] private AudioClip[] bubbleSoundsCool;
    [SerializeField] private AudioClip[] bubbleSoundsCooler;
    [SerializeField] private AudioClip[] bubbleSoundsCoolest;

    [Header("UI & Game Sounds")]
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip whooshSound;
    [SerializeField] private AudioClip warningSound;

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    private AudioClip[][] _bubbleSoundCategories;

    private void Awake()
    {
        _bubbleSoundCategories = new AudioClip[][]
        {
            bubbleSoundsDef,
            bubbleSoundsCool,
            bubbleSoundsCooler,
            bubbleSoundsCoolest
        };
    }

    public void PlayBubbleSoundStatic(int typeIndex)
    {
        if(typeIndex >= 0 && typeIndex < _bubbleSoundCategories.Length)
        {
            PlayBubbleFromCategory(_bubbleSoundCategories[typeIndex]);
        }
    }

    public void PlayBubbleSound(TypeOfSound type)
    {
        int index = (int)type;
        if(index >= 0 && index < _bubbleSoundCategories.Length)
        {
            PlayBubbleFromCategory(_bubbleSoundCategories[index]);
        }
    }

    private void PlayBubbleFromCategory(AudioClip[] clips)
    {
        if(clips == null || clips.Length == 0 || audioSource == null) return;

        int randomIndex = Random.Range(0, clips.Length);
        AudioClip clip = clips[randomIndex];

        if(clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayWinSound() => PlayClip(winSound);
    public void PlayLoseSound() => PlayClip(loseSound);
    public void PlayClickSound() => PlayClip(clickSound);
    public void PlayWhooshSound() => PlayClip(whooshSound);
    public void PlayWarningSound() => PlayClip(warningSound);

    private void PlayClip(AudioClip clip)
    {
        if(clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}