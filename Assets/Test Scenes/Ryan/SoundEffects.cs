using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffects : MonoBehaviour {

    [Range (1, 10)]
    public float MaxVolumeDistance = 2;

    [Range(10, 50)]
    public float ZeroVolumeDistance = 25;

    public float MinSoundLevel = 0.01f;

    public List<AudioClip> Clips;

    Dictionary<Sounds, AudioClip> SoundLibrary;

    public enum Sounds
    {
        STEREO_TEST,
        MONO_TEST
    }

	// Use this for initialization
	void Awake () {

        SoundLibrary = new Dictionary<Sounds, AudioClip>();

        for(int i = 0; i < Clips.Count; i++)
        {
            SoundLibrary.Add((Sounds)i, Clips[i]);
        }
	}

    ////Used for testing, can be deleted once were 100% this works
    //void Update()
    //{

    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        //m_MonoSource.Play();
    //        float xPos = Input.mousePosition.x / (float)Screen.width;
    //        float zPos = Input.mousePosition.y / (float)Screen.height;

    //        PlaySoundAtPosition(Sounds.MONO_TEST, gameObject.transform, GameObject.Find("Sphere").transform.position);
    //    }
    //}

    public void PlaySoundAtPosition(Sounds sound, Transform listener, Vector3 soundPos)
    {
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
        Debug.Log(soundPos.ToString() + " " + listener.position.ToString());

        if (distance < ZeroVolumeDistance)
            AudioSource.PlayClipAtPoint(SoundLibrary[sound], adjustedPos, volume);
    }
}
