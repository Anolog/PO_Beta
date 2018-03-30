using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    [SerializeField]
    public Slider m_SoundFXSlider;

    public float m_Volume;
    public float m_MusicVolume;

    AudioSource m_CurrentMusic;
    AudioSource m_NewMusic;

    bool m_MusicFading = false;

    class Effect
    {
        public Effect(string name, params Sounds[] sounds)
        {
            for (int i = 0; i < sounds.Length; i++)
            {
                m_SoundFX.Add(sounds[i]);
            }

            Name = name;
            m_LastSound = sounds.Length;
        }

        List<Sounds> m_SoundFX = new List<Sounds>();
        public string Name;
        int m_LastSound;

        public Sounds GetRandomSound()
        {
            if (m_SoundFX.Count == 0)
            {
                Debug.Log(Name + " doesn't have any sounds in it");
                return ((Sounds)0);
            }

            int index = Random.Range(0, m_SoundFX.Count);
            m_LastSound = index;

            //Debug.Log(m_SoundFX[index]);

            return m_SoundFX[index];
        }

        public Sounds GetRandomNoRepSound()
        {
            if (m_SoundFX.Count == 1)
            {
                Debug.Log(Name + " will repeat since it only has one sound");
                return m_SoundFX[0];
            }

            Sounds lastSound = (Sounds)m_LastSound;

            Sounds sound = GetRandomSound();
            while (sound == lastSound)
            {
                sound = GetRandomSound();
            }

            return sound;
        }
    }

    [Range(1, 10)]
    public float MaxVolumeDistance = 2;

    [Range(10, 50)]
    public float ZeroVolumeDistance = 25;

    public float MinSoundLevel = 0.01f;

    public List<AudioClip> Clips;

    Dictionary<Sounds, AudioClip> SoundLibrary;
    Dictionary<string, Effect> SoundEffects;

    public enum Sounds
    {
        STEREO_TEST,
        ERROR_SOUND,
        VORTEX,
        HEAL,
        KNOCKBACK,
        WEAPON_SMASH_GROUND,
        ENRAGE,
        BOW_1,
        BOW_2,
        BOW_3,
        SWORD_1,
        SWORD_2,
        SWORD_3,
        PLANT_ATTACK_1,
        PLANT_ATTACK_2,
        TELEPORT,
        TELEPORT_CHANNEL,
        ROCK_ATTACK_HIT,
        CHARGE,
        PLAYER_SHIELD_DAMAGE_1,
        CHARGE_HIT,
        CHAIN_LIGHTNING_HIT_1,
        CHAIN_LIGHTNING_HIT_2,
        CHAIN_LIGHTNING,
        PLANT_HIT_1,
        PLANT_HIT_2,
        PLANT_HIT_3,
        ROCK_THROWER_HIT_1,
        ROCK_THROWER_HIT_2,
        ROCK_THROWER_HIT_3,
        SUPPORT_HIT_1,
        SUPPORT_HIT_2,
        SUPPORT_HIT_3,
        BOSS_HIT_1,
        BOSS_HIT_2,
        BOSS_HIT_3,
        BOSS_ATTACK_1,
        BOSS_ATTACK_2,
        BOSS_ATTACK_3,
        GROUND_SHOCK,
        WEAPON_SMASH,
        PLAYER_HIT_1,
        PLAYER_HIT_2,
        PLAYER_HIT_3,
        DECORATION_HIT_1,
        DECORATION_HIT_2,
        DECORATION_HIT_3,
        PROTECT_ME,
        ENERGY_BOMB,
        ENERGY_BOMB_EXPLOSION,
        ENERGY_MINE,
        MENU_SELECT,
        MENU_CONFIRM,
        MENU_BACK,
        MENU_SLIDER,
        ABILITY_STILL_ON_CD,
        BOW_USE,
        PLAYER_JUMP,
        PLAYER_LAND,
        RECIEVE_HEALING,
        SWORD_SWING,
        HEALTH_PICKUP,
        PLAYER_SHIELD_DAMAGE_2,
        TARGET_HEAL,
        BOULDER_THROW,
        PLAYER_REVIVE,
        PLAYER_DOWN,
        PLANT_DIE_1,
        PLANT_DIE_2,
        SUPPORT_DIE,
        BOSS_DIE,
        INVINC_PICKUP,
        DAMAGE_PICKUP,
        COOL_DOWN_PICKUP,
        POINTS_PICKUP,
        SPEED_PICKUP,
        RANGED_DIE,
        REVIVING_LOW,
        REVIVING_MED,
        REVIVING_HIGH,
        MUSIC_MAIN_MENU,
        MUSIC_LOADOUT,
        MUSIC_IN_GAME,
        MUSIC_BOSS,
        MUSIC_POST_GAME,
        CUTSCENE_AUDIO,
    }

    void SetVolume()
    {

    }

    // Use this for initialization
    void Awake()
    {
        //m_Volume = 0.5f;

        SoundLibrary = new Dictionary<Sounds, AudioClip>();
        SoundEffects = new Dictionary<string, Effect>();

        m_NewMusic = gameObject.AddComponent<AudioSource>();
        m_CurrentMusic = gameObject.AddComponent<AudioSource>();
        m_NewMusic.loop = true;
        m_CurrentMusic.loop = true;

        for (int i = 0; i < Clips.Count; i++)
        {
            if (Clips[i] == null)
                continue;

            SoundLibrary.Add((Sounds)i, Clips[i]);
        }

        Effect bowFire = new Effect("Bow Fire", Sounds.BOW_1, Sounds.BOW_2, Sounds.BOW_3);
        SoundEffects.Add(bowFire.Name, bowFire);

        Effect swordSwing = new Effect("Sword Swing", Sounds.SWORD_1, Sounds.SWORD_2, Sounds.SWORD_3);
        SoundEffects.Add(swordSwing.Name, swordSwing);

        Effect plantAttack = new Effect("Plant Attack", Sounds.PLANT_ATTACK_1, Sounds.PLANT_ATTACK_2);
        SoundEffects.Add(plantAttack.Name, plantAttack);

        Effect chainLightningHit = new Effect("Chain Lightning Hit", Sounds.CHAIN_LIGHTNING_HIT_1, Sounds.CHAIN_LIGHTNING_HIT_2);
        SoundEffects.Add(chainLightningHit.Name, chainLightningHit);

        Effect plantHit = new Effect("Plant Hit", Sounds.PLANT_HIT_1, Sounds.PLANT_HIT_2, Sounds.PLANT_HIT_3);
        SoundEffects.Add(plantHit.Name, plantHit);

        Effect rockThrowerHit = new Effect("Rock Thrower Hit", Sounds.ROCK_THROWER_HIT_1, Sounds.ROCK_THROWER_HIT_2, Sounds.ROCK_THROWER_HIT_3);
        SoundEffects.Add(rockThrowerHit.Name, rockThrowerHit);

        Effect supportHit = new Effect("Support Hit", Sounds.SUPPORT_HIT_1, Sounds.SUPPORT_HIT_2, Sounds.SUPPORT_HIT_3);
        SoundEffects.Add(supportHit.Name, supportHit);

        Effect bossHit = new Effect("Boss Hit", Sounds.BOSS_HIT_1, Sounds.BOSS_HIT_2, Sounds.BOSS_HIT_3);
        SoundEffects.Add(bossHit.Name, bossHit);

        Effect bossAttack = new Effect("Boss Attack", Sounds.BOSS_ATTACK_1, Sounds.BOSS_ATTACK_2, Sounds.BOSS_ATTACK_3);
        SoundEffects.Add(bossAttack.Name, bossAttack);

        Effect playerHit = new Effect("Player Hit", Sounds.PLAYER_HIT_1, Sounds.PLAYER_HIT_2, Sounds.PLAYER_HIT_3);
        SoundEffects.Add(playerHit.Name, playerHit);

        Effect decorationHit = new Effect("Decoration Hit", Sounds.DECORATION_HIT_1, Sounds.DECORATION_HIT_2, Sounds.DECORATION_HIT_3);
        SoundEffects.Add(decorationHit.Name, decorationHit);

        Effect shieldHit = new Effect("Shield Hit", Sounds.PLAYER_SHIELD_DAMAGE_1, Sounds.PLAYER_SHIELD_DAMAGE_2);
        SoundEffects.Add(shieldHit.Name, shieldHit);

        Effect plantDeath = new Effect("Plant Death", Sounds.PLANT_DIE_1, Sounds.PLANT_DIE_2);
        SoundEffects.Add(plantDeath.Name, plantDeath);
    }

    public Sounds GetSoundFromEffect(string effectName, bool noRepeat)
    {
        if (!SoundEffects.ContainsKey(effectName))
        {
            Debug.Log("Effect " + effectName + " doesn't exist");
            return (Sounds)0;
        }

        if (noRepeat)
            return SoundEffects[effectName].GetRandomNoRepSound();
        else
            return SoundEffects[effectName].GetRandomSound();
    }

    public void PlaySoundAtPosition(Sounds sound, Transform listener, Vector3 soundPos)
    {
        // If I don't have the sound I want to play: leave
        if (!SoundLibrary.ContainsKey(sound))
        {
            Debug.Log("There is no audio clip " + sound.ToString() + ". Consider adding it");
            return;
        }

        Vector3 pos = soundPos - listener.position;

        float distance = pos.magnitude;

        Vector3 unitDirection = pos.normalized;

        //if dot with right is positive, angle is to the right of forward, otherwise opposite
        float angleSign = Mathf.Sign(Vector3.Dot(transform.right, unitDirection));
        float directionDot = Vector3.Dot(transform.forward, unitDirection);

        float soundAngle = Mathf.PI * 0.5f - (Mathf.Acos(directionDot) * angleSign);

        Vector3 adjustedPos = new Vector3(Mathf.Cos(soundAngle), soundPos.y, Mathf.Sin(soundAngle));

        //attenuation equation of the form 1/a^x
        float a = Mathf.Pow(1 / MinSoundLevel, 1 / (ZeroVolumeDistance - MaxVolumeDistance));

        float volume = Mathf.Clamp01(1 / Mathf.Pow(a, distance - MaxVolumeDistance));

        if (m_SoundFXSlider == null)
        {
            GameObject obj = GameObject.Find("Game").GetComponent<Game>().PauseMenuUI.transform.Find("PauseMenu").GetComponent<PauseMenuBehaviour>().m_SettingsMenu.GetComponent<OptionsMenuBehaviour>().m_AudioScreen.GetComponent<AudioMenuBehaviour>().m_SFXSlider;

            if (obj != null)
            {
                m_SoundFXSlider = obj.GetComponentInChildren<Slider>();
            }
        }

        volume *= m_Volume;

        if (distance < ZeroVolumeDistance)
            AudioSource.PlayClipAtPoint(SoundLibrary[sound], adjustedPos, volume);
    }

    public AudioSource PlaySoundAtPositionAndMaybeStopLater(GameObject obj, Sounds sound, Transform listener, Vector3 soundPos)
    {
        // If I don't have the sound I want to play: leave
        if (!SoundLibrary.ContainsKey(sound))
        {
            sound = Sounds.ERROR_SOUND;
        }

        AudioSource source = obj.GetComponent<AudioSource>();

        if (source == null)
            source = obj.AddComponent<AudioSource>();

        Vector3 pos = soundPos - listener.position;

        float distance = pos.magnitude;

        Vector3 unitDirection = pos.normalized;

        //if dot with right is positive, angle is to the right of forward, otherwise opposite
        float angleSign = Mathf.Sign(Vector3.Dot(transform.right, unitDirection));
        float directionDot = Vector3.Dot(transform.forward, unitDirection);

        float soundAngle = Mathf.PI * 0.5f - (Mathf.Acos(directionDot) * angleSign);

        Vector3 adjustedPos = new Vector3(Mathf.Cos(soundAngle), soundPos.y, Mathf.Sin(soundAngle));

        //attenuation equation of the form 1/a^x
        float a = Mathf.Pow(1 / MinSoundLevel, 1 / (ZeroVolumeDistance - MaxVolumeDistance));

        float volume = Mathf.Clamp01(1 / Mathf.Pow(a, distance - MaxVolumeDistance));

        if (m_SoundFXSlider == null)
        {
            GameObject sobj = GameObject.Find("Game").GetComponent<Game>().PauseMenuUI.transform.Find("PauseMenu").GetComponent<PauseMenuBehaviour>().m_SettingsMenu.GetComponent<OptionsMenuBehaviour>().m_AudioScreen.GetComponent<AudioMenuBehaviour>().m_SFXSlider;

            if (sobj != null)
            {
                m_SoundFXSlider = obj.GetComponentInChildren<Slider>();
            }
        }

        volume *= m_Volume;          

        source.clip = SoundLibrary[sound];
        source.volume = volume;

        float angle = Mathf.Atan2(adjustedPos.z, adjustedPos.x);
        source.panStereo = Mathf.Cos(angle);
        source.Play();

        return source;
    }

    public void PlaySound(Sounds sound)
    {
        if (SoundLibrary.ContainsKey(sound))
        {
            AudioSource.PlayClipAtPoint(SoundLibrary[sound], Vector3.zero, m_Volume);
        }
    }

    public void SetMusicVolume(float volume)
    {
        m_MusicVolume = volume;
        m_CurrentMusic.volume = volume;
        m_NewMusic.volume = volume;
    }

    public void StopCurrentMusic()
    {
        m_CurrentMusic.Stop();
    }

    public void PlayMusic(Sounds sound, float fadeInDuration)
    {
        m_NewMusic.clip = SoundLibrary[sound];

        if (!m_MusicFading)
        {
            StartCoroutine(FadeOutCoroutine(fadeInDuration));
        }
    }

    public void FadeMusic(float fadeOutDuration)
    {
        if (!m_MusicFading)
        {
            StartCoroutine(FadeOutCoroutine(fadeOutDuration));
        }
    }

    IEnumerator FadeOutCoroutine(float duration)
    {
        m_MusicFading = true;

        //the reference to current music may get swapped on us, but it's fade in that is responsible for swapping them
        AudioSource audioRef = m_CurrentMusic;

        float startTime = Time.unscaledTime;

        while(Time.unscaledTime < startTime + duration)
        {
            float val = (Time.unscaledTime - startTime) / duration;
            audioRef.volume = m_MusicVolume * (1 - val);

            yield return null;
        }

        audioRef.volume = 0;
        audioRef.Stop();
        
        m_MusicFading = false;

        StartCoroutine(FadeInCoroutine(duration));

        yield return null;
    }

    IEnumerator FadeInCoroutine(float duration)
    {
        m_NewMusic.Play();

        float startTime = Time.unscaledTime;

        while (Time.unscaledTime < startTime + duration)
        {
            float val = (Time.unscaledTime - startTime) / duration;
            m_NewMusic.volume = m_MusicVolume * val;

            yield return null;
        }

        m_NewMusic.volume = m_MusicVolume;

        AudioSource temp = m_CurrentMusic;
        m_CurrentMusic = m_NewMusic;
        m_NewMusic = temp;
        yield return null;
    }
}