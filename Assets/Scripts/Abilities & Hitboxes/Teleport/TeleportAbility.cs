using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportAbility : Ability
{
    public static float RECOVERY_TIME = 0.15f;
    public static float CAST_TIME = 0.5f;
    public static float BASE_DAMAGE = 0.0f;
    public static float LIFE_TIME = 1.5f;
    private static float TIME_LEFT_TO_SWAP = LIFE_TIME * 0.5f;
    public static float COOL_DOWN_TIME = LIFE_TIME;
    public static float RAYCAST_DOWN_AMOUNT = 20.0f;

    private float m_TimeLeftToSwap = TIME_LEFT_TO_SWAP;
    public GameObject TeleporLocation;
    private bool m_CanTeleport = false;
    private bool m_HasSwapped = true;

    private Vector3 m_LocationToTeleportTo = Vector3.zero;

    private GameObject m_teleportClone;

    public TeleportAbility(CharacterStats character)
        : base(character)
    {
        m_Type = AbilityType.Ranged;
        m_CastTime = CAST_TIME;
        m_RecoveryTime = RECOVERY_TIME;
        m_CoolDownTime = COOL_DOWN_TIME;
        m_BaseDamage = BASE_DAMAGE;
        m_DefaultCooldown = COOL_DOWN_TIME;
        //No damage since it's just a TP.
        Damage = m_BaseDamage;

        //Lifetime is used for the clone
        m_Lifetime = LIFE_TIME;
    }

    public override void Use(GameObject teleportLocation)
    {
        TeleporLocation = teleportLocation;

        base.Use();
    }

    protected override void ActivateEffect()
    {
        //TeleporLocation = m_Character.gameObject.GetComponent<Boss1AI>().Target;

        RaycastHit hit;

        Vector3 dir = -TeleporLocation.transform.forward;
        Ray ray = new Ray(TeleporLocation.transform.position, dir);

        if (Physics.Raycast(ray, out hit, 1.5f, LayerMask.NameToLayer("AlivePlayer")))
        {
            m_CanTeleport = false;
        }

        else
        {
            m_CanTeleport = true;
        }

        if (m_CanTeleport == true)
        {

            #region Play Teleporting Sound
            // Play a teleporting sound
            // find which player is the listener
            GameObject tempListener = GameManager.playerManager.PlayerList()[0];
            float dist = float.MaxValue;
            foreach (GameObject player in GameManager.playerManager.PlayerList())
            {
                if (Vector3.Distance(player.transform.position, m_Character.transform.position) < dist)
                {
                    dist = Vector3.Distance(player.transform.position, m_Character.transform.position);
                    tempListener = player;
                }
            }

            //We don't have a sound for this yet
            GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.TELEPORT_CHANNEL, tempListener.transform, m_Character.transform.position);
            #endregion


            m_Character.gameObject.GetComponent<Boss1AI>().Agent.enabled = false;

            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;

            rot = m_Character.gameObject.transform.rotation;

            m_LocationToTeleportTo = new Vector3(-0.5f * TeleporLocation.transform.forward.x, 0.5f, -0.5f * TeleporLocation.transform.forward.z);

            //Move clone to the teleport location
            pos = TeleporLocation.transform.position + m_LocationToTeleportTo;

            //Raycast to make sure it spawns on the ground when used
            Ray raycast = new Ray(pos + Vector3.up, Vector3.down * RAYCAST_DOWN_AMOUNT);
            RaycastHit hitInfo = new RaycastHit();

            if (Physics.Raycast(raycast, out hitInfo))
            {
                if (hitInfo.transform.tag == "Platform" || hitInfo.transform.tag == "Wall")
                {
                    pos -= new Vector3(0, hitInfo.distance - 1, 0);
                }
            }

            //Create the clone
            m_teleportClone = (GameObject)Object.Instantiate(Resources.Load("Teleport/TeleportClonePrefab"), pos, rot);
            m_teleportClone.GetComponent<Hitbox>().Initialize(m_Character, m_Type, (int)Damage, m_Lifetime);


            //Set back so it doesn't create new gameobject.
            //m_CanTeleport = false;
            m_HasSwapped = false;
            m_TimeLeftToSwap = TIME_LEFT_TO_SWAP;
        }
    }

    public override void Update()
    {
        base.Update();

        if (m_CanTeleport)
        {
            m_TimeLeftToSwap -= Time.deltaTime;
        }

        if (m_TimeLeftToSwap <= 0.0f && m_HasSwapped == false)
        {
            #region Play swapping sound
            // Play a reappearing sound
            // find which player is the listener
            GameObject tempListener = GameManager.playerManager.PlayerList()[0];
            float dist = float.MaxValue;
            foreach (GameObject player in GameManager.playerManager.PlayerList())
            {
                if (Vector3.Distance(player.transform.position, m_Character.transform.position) < dist)
                {
                    dist = Vector3.Distance(player.transform.position, m_Character.transform.position);
                    tempListener = player;
                }
            }
            GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.TELEPORT, tempListener.transform, m_Character.transform.position); 
            #endregion

            //Grab a few values before swap
            Vector3 preSwapLoc = m_Character.transform.position;
            Quaternion preSwapRot = m_Character.transform.rotation;

            //Move the boss to the teleport location
            m_Character.transform.position = m_teleportClone.transform.position;
            m_Character.transform.rotation = m_teleportClone.transform.rotation;

            //Move the clone to the boss location before swap
            m_teleportClone.transform.position = preSwapLoc;
            m_teleportClone.transform.rotation = preSwapRot;

            m_HasSwapped = true;
            m_TimeLeftToSwap = TIME_LEFT_TO_SWAP;

            m_CanTeleport = false;
            m_Character.gameObject.GetComponent<Boss1AI>().Agent.enabled = true;

            //Object.DestroyObject(m_teleportClone);
        }
    }
}
