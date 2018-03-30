using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyMineAbility : Ability
{
    public static float RECOVERY_TIME = 0.15f;
    public static float CAST_TIME = 0.1f;
    public static float COOL_DOWN_TIME = 5f;
    public static float BASE_DAMAGE = 10.0f;
    public static float LIFE_TIME = 1.25f;
    public static float EXPLOSION_LIFE_TIME = 0.5f;
    public static float RAYCAST_DOWN_AMOUNT = 6.0f;

    public List<GameObject> energyMine = new List<GameObject>();

    private List<Vector3> targetPositions = new List<Vector3>();
    //private Vector3 dropHeight = Vector3.up * 2;

    public EnergyMineAbility(CharacterStats character) 
        : base(character)
    {
        m_Type = AbilityType.Ranged;
        m_CastTime = CAST_TIME;
        m_RecoveryTime = RECOVERY_TIME;
        m_CoolDownTime = COOL_DOWN_TIME;
        m_BaseDamage = BASE_DAMAGE;
        m_DefaultCooldown = COOL_DOWN_TIME;
        Damage = m_BaseDamage + m_Character.Damage;

        m_Lifetime = LIFE_TIME;
    }

    public override void Update()
    {
        base.Update();
        /*
        if (m_Lifetime <= Time.time)
        {
            for (int i = 0; i < energyMine.Count; i++)
            { 
                Hitbox = (GameObject)Object.Instantiate(Resources.Load("DamageHitboxes/EnergyMineHitbox"), energyMine[i].transform);
                Hitbox.GetComponent<EnergyMineHitbox>().Initialize(m_Character, m_Type, (int)m_Damage, EXPLOSION_LIFE_TIME);
            }
        }
        */
    }

    protected override void ActivateEffect()
    {
        #region Play Use Sound
        // play an explosion sound
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
        GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.ENERGY_MINE, tempListener.transform, m_Character.transform.position);
        #endregion


        Quaternion rot = m_Character.transform.rotation;

        //Clear information
        ResetInfo();
        
        //Find all players within the scene
        foreach (GameObject gameObject in GameManager.playerManager.PlayerList())
        {
            targetPositions.Add(gameObject.transform.position);
        }

        //Make sure energymine amount is equal to the targetpositions, this is doubling for some reason
        energyMine.Capacity = targetPositions.Capacity;

        //Set a blank gameobject
        for (int i = 0; i < targetPositions.Count; i++)
        {
            GameObject tempEnergyMine = null;
            energyMine.Add(tempEnergyMine);
        }

        //Spawning them
        for (int i = 0; i < energyMine.Count; i++)
        {
            Ray raycast = new Ray(targetPositions[i], Vector3.down * RAYCAST_DOWN_AMOUNT);
            RaycastHit hitInfo = new RaycastHit();
            //Debug.DrawRay(targetPositions[i], raycast.direction, Color.blue);

            //Debug.Log("Target Pos : " + targetPositions[i]);
           

            if (Physics.Raycast(raycast, out hitInfo))
            {
                if (hitInfo.transform.tag == "Platform" || hitInfo.transform.tag == "Wall")
                {
                    //targetPositions[i] = hitInfo.transform.position + new Vector3(0, 0.1f, 0);
                    targetPositions[i] = targetPositions[i]  - new Vector3(0, hitInfo.distance, 0);
                    //Debug.Log("TP + Raycast : " + targetPositions[i]);
                }
            }

            //we used to spawn an energy mine object
            //energyMine[i] = (GameObject)Object.Instantiate(Resources.Load("EnergyMine/EnergyMinePrefab"), targetPositions[i], rot);
            //energyMine[i].GetComponent<EnergyMineObject>().Initialize(m_Character, m_Type, (int)Damage, m_Lifetime);
            //energyMine[i].GetComponent<Hitbox>().Initialize(m_Character, m_Type, (int)m_Damage, m_Lifetime);

            GameObject hitbox = (GameObject)GameObject.Instantiate(Resources.Load("DamageHitboxes/EnergyMineHitbox"), targetPositions[i], rot);
            hitbox.GetComponent<EnergyMineHitbox>().Initialize(m_Character, m_Type, (int)m_BaseDamage, m_Lifetime);

            m_Character.gameObject.GetComponentInChildren<Animator>().SetTrigger("EnergyMine");

            //we are playing the same sound earlier in this function
            #region Play Explosion Sound
            // play an explosion sound
            // find which player is the listener
            //GameObject audioListener = GameManager.playerManager.PlayerList()[0];
            //float dist = float.MaxValue;
            //foreach (GameObject player in GameManager.playerManager.PlayerList())
            //{
            //    if (Vector3.Distance(player.transform.position, gameObject.transform.position) < dist)
            //    {
            //        dist = Vector3.Distance(player.transform.position, gameObject.transform.position);
            //        audioListener = player;
            //    }
            //}
            //GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.ENERGY_MINE, audioListener.transform, gameObject.transform.position);
            #endregion
        }
    }

    private void ResetInfo()
    {
        energyMine.Clear();
        targetPositions.Clear();
    }
}
