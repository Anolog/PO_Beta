using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [SerializeField]
    Slider m_MusicSlider;

    enum PlaylistMode
    {
        InOrder,
        Shuffle,
        Random,
        RandomNoRepeat
    }

    public static float FADE_IN_TIMER = 1.0f;
    public static float FADE_OUT_TIMER = 2.0f;

    public List<AudioClip> MusicTracks = new List<AudioClip>();
    public List<AudioClip> ShuffledTracks = new List<AudioClip>();

    [SerializeField]
    private AudioSource m_AudioSource;


    private float m_FadeIn;
    private float m_FadeOut;
    private PlaylistMode m_PlaylistMode = PlaylistMode.InOrder;

    private bool m_FadeOutStarted = false;
    private bool m_FadeInSong = true;
    private bool m_FadeOutSong = true;

    public float MusicVolume = 0.5f;

    public AudioSource AudioSource
    {
        get
        {
            return m_AudioSource;
        }

        set
        {
            m_AudioSource = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        m_FadeIn = FADE_IN_TIMER;
        m_FadeOut = FADE_OUT_TIMER;
        //PlayPlayList(MusicTracks, m_PlaylistMode);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_MusicSlider == null)
        {
            GameObject musicSlider = GameObject.Find("MusicVolSlider");
            if (musicSlider != null)
            {
                m_MusicSlider = musicSlider.GetComponent<Slider>();
            }
        }

        if (m_MusicSlider != null)
        {
            MusicVolume = m_MusicSlider.value;
        }
    }

    void PlayPlayList(List<AudioClip> musicTracks, PlaylistMode playlistMode)
    {
        bool repeatMusic = false;
        m_PlaylistMode = playlistMode;

        if (m_PlaylistMode == PlaylistMode.Shuffle)
        {
            CreateShufflePlaylist(musicTracks);
        }

        if (m_PlaylistMode == PlaylistMode.Random)
        {
            repeatMusic = true;
            StartCoroutine(PlayInRandomOrder(musicTracks, repeatMusic));
        }

        if (m_PlaylistMode == PlaylistMode.RandomNoRepeat)
        {
            repeatMusic = false;
            StartCoroutine(PlayInRandomOrder(musicTracks, repeatMusic));
        }

        if (m_PlaylistMode == PlaylistMode.InOrder)
        {
            StartCoroutine(PlayInOrder(musicTracks, m_FadeInSong, m_FadeOutSong));
        }
    }

    void CreateShufflePlaylist(List<AudioClip> musicTracks)
    {
        List<int> tempOrder = new List<int>();

        for (int i = 0; i < musicTracks.Count; i++)
        {
            tempOrder.Add(i);
        }

        for (int j = 0; j < tempOrder.Count; j++)
        {
            int lastIndex = tempOrder.Count - 1 - j;
            int rand = Random.Range(0, lastIndex);
            int bucket = tempOrder[lastIndex];

            tempOrder[lastIndex] = tempOrder[rand];
            tempOrder[rand] = bucket;
        }

        for (int k = 0; k < tempOrder.Count; k++)
        {
            ShuffledTracks.Add(musicTracks[tempOrder[k]]);
        }

        StartCoroutine(PlayInOrder(ShuffledTracks, m_FadeInSong, m_FadeOutSong));
    }

    List<AudioClip> CreateRandomPlaylist(List<AudioClip> playlist)
    {
        List<AudioClip> temp = new List<AudioClip>();

        //Pick random
        for (int i = 0; i < playlist.Count; i++)
        {
            temp.Add(playlist[Random.Range(0, playlist.Count)]);
        }

        return temp;
    }


    IEnumerator PlayInOrder(List<AudioClip> playList, bool fadeInSong, bool fadeOutSong)
    {
        int counter = 0;

        while (playList.Count >= counter)
        {

            if (m_AudioSource.isPlaying == false || m_AudioSource == null)
            {
                PlayMusic(playList[counter], fadeInSong, false);

            }

            else if (m_AudioSource.isPlaying == true)
            {
                if (m_FadeOutSong == true)
                {
                    if (m_AudioSource.time >= m_AudioSource.clip.length - m_FadeOut && m_FadeOutStarted == false)
                    {
                        m_FadeOutStarted = true;
                        StartCoroutine(FadeOut());
                    }
                }

                //Check if track is done
                if (m_AudioSource.time >= m_AudioSource.clip.length - 1)
                {
                    counter++;
                }
            }

            yield return null;
        }
    }

    IEnumerator PlayInRandomOrder(List<AudioClip> playList, bool repeat)
    {

        if (repeat == true)
        {
            while (repeat)
            {
                StartCoroutine(PlayInOrder(CreateRandomPlaylist(playList), m_FadeInSong, m_FadeOutSong));
                yield return null;
            }

        }

        else if (repeat == false)
        {
            //Create a random playlist
            StartCoroutine(PlayInOrder(CreateRandomPlaylist(playList), m_FadeInSong, m_FadeOutSong));
        }

        yield return null;


    }

    //This function is more of a start playing the song, and if you have fade in enabled then fade it.
    public void PlayMusic(AudioClip song, bool fadeIn, bool onTheFlySwitch)
    {
        //Meant more for playlists.
        if (onTheFlySwitch == false)
        {
            if (fadeIn == true)
            {
                m_AudioSource.Stop();
                m_AudioSource.clip = song;
                m_AudioSource.Play();
                StartCoroutine(FadeIn());
            }

            //Quick and easy
            else
            {
                //Reset just to check
                m_AudioSource.volume = MusicVolume;

                //Stop if anything playing
                m_AudioSource.Stop();
                m_AudioSource.clip = song;
                m_AudioSource.Play();

            }
        }

        else if (onTheFlySwitch == true)
        {
            if (fadeIn == true)
            {
                if (m_AudioSource.isPlaying)
                {
                    //m_FadeOutStarted = true;
                    StartCoroutine(FadeOutSong(song));
                }

                else
                {
                    m_AudioSource.clip = song;
                    m_AudioSource.Play();
                    m_AudioSource.loop = true;
                    StartCoroutine(FadeIn());
                }

            }

            else if (fadeIn == false)
            {
                if (m_AudioSource.isPlaying)
                {
                    m_FadeOutStarted = true;
                    StartCoroutine(FadeOutSong(song));
                }

            }
        }
    }

    void PlayRandomtrack(List<AudioClip> tracks)
    {
        if (m_AudioSource.isPlaying == false)
        {
            PlayMusic(tracks[Random.Range(0, tracks.Count)], m_FadeInSong, false);
        }
    }

    void StopAllMusic()
    {
        m_AudioSource.Stop();
    }

    IEnumerator FadeIn()
    {
        if (m_FadeIn >= 0.0f)
        {
            float lerpHelper = 0.0f;

            while (lerpHelper <= 1.0f && m_AudioSource.isPlaying == true)
            {
                //Uses the audio sources time as Time.Time essentially, because we are going through it, not the game time.
                lerpHelper = Mathf.InverseLerp(0.0f, m_FadeIn, m_AudioSource.time);
                m_AudioSource.volume = Mathf.Lerp(0.0f, MusicVolume, lerpHelper);
                //AudioListener.volume = m_AudioSource.volume;

                //Break out of coroutine
                yield return null;
            }

            m_AudioSource.volume = MusicVolume;
        }
    }

    public IEnumerator FadeOutSong(AudioClip song)
    {
            
        if (m_FadeOut >= 0.0f)
        {
            float lerpHelper = m_FadeOut;

            while (lerpHelper <= m_FadeOut && m_AudioSource.isPlaying == true)
            {
                lerpHelper -= Time.deltaTime * m_FadeOut;
                float temp = Mathf.Lerp(0.0f, MusicVolume, lerpHelper);

                //AudioListener.volume = temp;
                //m_AudioSource.volume = AudioListener.volume;
                m_AudioSource.volume = temp;


               /// Debug.Log("Audio Volume In Fade Out: " + temp + " Listener Vol: " + m_AudioSource.volume);

                if (m_AudioSource.volume <= 0.1f)
                {
                    m_AudioSource.volume = MusicVolume;
                    m_AudioSource.clip = song;
                    m_AudioSource.Play();
                    m_AudioSource.loop = true;
                    StartCoroutine(FadeIn());

                    yield break;
                }

                yield return null;
            }


        }


        
    }

    public IEnumerator FadeOut()
    {
/*
        if (m_FadeOut >= 0.0f)
        {
            float startTime = m_AudioSource.clip.length - m_FadeOut;
            float lerpHelper = 0.0f;

            while (lerpHelper < m_FadeOut && m_AudioSource.isPlaying)
            {
                lerpHelper = Mathf.InverseLerp(startTime, m_AudioSource.clip.length, m_AudioSource.time);
                m_AudioSource.volume = Mathf.Lerp(MusicVolume, 0f, lerpHelper);

                Debug.Log(lerpHelper);


                yield return null;
            }

            m_AudioSource.volume = 0f;
        }
*/
        
        if (m_FadeOut >= 0.0f)
        {
            float lerpHelper = m_FadeOut;

            while (lerpHelper <= m_FadeOut && m_AudioSource.isPlaying == true)
            {
                //lerpHelper = Mathf.Lerp(0.0f, m_FadeOut, m_AudioSource.time);
                lerpHelper -= Time.deltaTime * m_FadeOut;
                float temp = Mathf.Lerp(0.0f, MusicVolume, lerpHelper);

                m_AudioSource.volume = temp;
                

                //Debug.Log("Audio Volume In Fade Out: " + m_AudioSource.volume);


                yield return null;
            }

            m_AudioSource.volume = MusicVolume;
            
        }

        if (m_AudioSource.volume <= 0.0f)
        {
           m_FadeOutStarted = false;
        }


    }

}
