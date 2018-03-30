using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetHealHitbox : Hitbox
{

    float m_Speed = 50;
    float m_MaxScale = 100f;
    float m_HealSpeed = 5;

    GameObject HealTarget;

    List<Third_Person_Camera.CameraOrientedPlane> camRotations;

    public void Initialize(CharacterStats attacker, Ability.AbilityType type, int abilityDamage, GameObject healTarget, float healSpeed)
    {
        Attacker = attacker;
        Type = type;
        AbilityDamage = abilityDamage;
        HealTarget = healTarget;
        m_HealSpeed = healSpeed;

        // play healing sound
        // find which player is the listener
        //GameObject tempListener = GameManager.playerManager.PlayerList()[0];
        //float dist = float.MaxValue;
        //foreach (GameObject player in GameManager.playerManager.PlayerList())
        //{
        //    if (Vector3.Distance(player.transform.position, gameObject.transform.position) < dist)
        //    {
        //        dist = Vector3.Distance(player.transform.position, gameObject.transform.position);
        //        tempListener = player;
        //    }
        //}
        //GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.MONO_TEST, tempListener.transform, gameObject.transform.position);

        camRotations = new List<Third_Person_Camera.CameraOrientedPlane>();

        Third_Person_Camera.CameraOrientedPlane plane = new Third_Person_Camera.CameraOrientedPlane(gameObject);
        camRotations.Add(plane);

        foreach (GameObject player in GameManager.playerManager.PlayerList())
        {
            player.GetComponentInChildren<Third_Person_Camera>().Planes.Add(plane);
        }
    }

    public void OnDestroy()
    {
        foreach (GameObject player in GameManager.playerManager.PlayerList())
        {
            Third_Person_Camera cam = player.GetComponentInChildren<Third_Person_Camera>();

            for (int i = camRotations.Count - 1; i >= 0; i--)
            { 
                //when game is closing camera might be destroyed before this object
                if (cam != null)
                    cam.Planes.Remove(camRotations[i]);
            }
        }

    }

    public void Update()
    {
        if (Attacker == null)
        {
            Destroy(gameObject);
            return;
        }
        if (Attacker.gameObject.GetComponent<SupportEnemyAI>().HealTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        // make sure that the hitbox is always connecting the two enemies
        Vector3 pos = Attacker.transform.position;
        pos.y += Attacker.GetComponent<CapsuleCollider>().height * 0.5f;
        gameObject.transform.position = pos;

        if (HealTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 healTargetPos = HealTarget.transform.position;
        //healTargetPos.y += HealTarget.GetComponent<CapsuleCollider>().height * 0.5f;

        Vector3 dir = healTargetPos - pos;
        Quaternion rot = Quaternion.LookRotation(Vector3.up, dir);

        gameObject.transform.rotation = rot;

        float dist = Vector3.Distance(healTargetPos, Attacker.transform.position);
        Vector3 scale = gameObject.transform.localScale;

        if (scale.y / 10 < dist)
        {
            scale.y += Time.deltaTime * m_Speed;
            gameObject.transform.localScale = scale;
        }

        if (scale.y / 10 > dist)
        {
            scale.y -= Time.deltaTime * m_Speed;
            gameObject.transform.localScale = scale;
        }

        if (scale.y > m_MaxScale)
        {
            Destroy(gameObject);
        }

        if (HealTarget.GetComponent<CharacterStats>().Health >= HealTarget.GetComponent<CharacterStats>().MaxHealth)
        {
            HealTarget.GetComponent<CharacterStats>().Health = HealTarget.GetComponent<CharacterStats>().MaxHealth;
            HealTarget = null;
            Destroy(gameObject);
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == HealTarget)
        {
            HealTarget.GetComponent<CharacterStats>().Health += Time.deltaTime * m_HealSpeed;
            //Debug.Log(HealTarget.GetComponent<CharacterStats>().Health);

            // Play a healing sound
            // find the closest point on my collider that my attacker is so that the sound is played directionally properly 
            //Vector3 contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position); // roughly where the collision happened

            // find which player is the listener
            //GameObject tempListener = GameManager.playerManager.PlayerList()[0];
            //float dist = float.MaxValue;
            //foreach (GameObject player in GameManager.playerManager.PlayerList())
            //{
            //    if (Vector3.Distance(player.transform.position, contactPoint) < dist)
            //    {
            //        dist = Vector3.Distance(player.transform.position, contactPoint);
            //        tempListener = player;
            //    }
            //}

            //GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.MONO_TEST, tempListener.transform, contactPoint);

        }
    }

    protected void OnTriggerStay(Collider other)
    {
        if (other.gameObject == HealTarget)
        {
            HealTarget.GetComponent<CharacterStats>().Health += Time.deltaTime * m_HealSpeed;
            //Debug.Log(HealTarget.GetComponent<CharacterStats>().Health);
        }
    }
}
