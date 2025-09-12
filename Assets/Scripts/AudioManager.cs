using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioClip gameOverMusic;
    public AudioClip mainMusic;
    public AudioSource mainSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            if (mainSource == null)
            {
                mainSource = gameObject.AddComponent<AudioSource>();
                mainSource.loop = true;
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic()
    {
        if (mainMusic != null)
        {
            mainSource.Stop();
            mainSource.clip = mainMusic;
            mainSource.loop = true;
            mainSource.Play();
        }
    }

    public void PlayGameOver()
    {
        StartCoroutine(PlayGameOverAndReturn());
    }

    private IEnumerator PlayGameOverAndReturn()
    {
        mainSource.Stop();
        mainSource.loop = false;
        mainSource.clip = gameOverMusic;
        mainSource.Play();

        yield return new WaitForSeconds(gameOverMusic.length);

      //  PlayMusic(); // phát lại nhạc nền
    }
}
