using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSmashHitbox : Hitbox
{

    Rigidbody m_AttackerBody;

    Vector3 m_JumpForce = new Vector3(8000, 5000, 8000);

    float m_HeightOffset;
    float m_AngleOffset;

    public void SetupWeaponSmashInitailOffsets(float heightOffset, float angleOffset)
    {
        m_HeightOffset = heightOffset;
        m_AngleOffset = angleOffset;
    }

    public void Start()
    {
        m_AttackerBody = Attacker.gameObject.GetComponent<Rigidbody>();

        Vector3 forward = Attacker.gameObject.transform.forward;

        Vector3 force = m_JumpForce;

        force.x *= forward.x;
        force.z *= forward.z;

        m_AttackerBody.AddForce(force, ForceMode.Impulse);

        Attacker.gameObject.GetComponent<PlayerStats>().m_CollisionUpdateTimer = Attacker.gameObject.GetComponent<PlayerStats>().CollisionUpdateTime + Time.time;

        PhysicMaterial material = Attacker.gameObject.GetComponent<CapsuleCollider>().material;

        material.frictionCombine = PhysicMaterialCombine.Minimum;
        material.staticFriction = 0.0f;
        material.dynamicFriction = 0.0f;
    }

    public void Update()
    {
        // make the hitbox follow
        Vector3 pos;
        Vector3 characterPos = m_AttackerBody.position;

        pos = characterPos;
        pos.y += m_HeightOffset;

        transform.position = pos;

        float speed = 750;

        if (Attacker.Grounded)
        {
            Quaternion rot = Quaternion.Euler(gameObject.transform.rotation.eulerAngles.x + (speed * Time.deltaTime), Attacker.gameObject.transform.rotation.eulerAngles.y, Attacker.gameObject.transform.rotation.eulerAngles.z);
            gameObject.transform.rotation = rot;
            Attacker.gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;
        }
        else
        {
            Quaternion rot = Quaternion.Euler(m_AngleOffset, Attacker.gameObject.transform.rotation.eulerAngles.y, Attacker.gameObject.transform.rotation.eulerAngles.z);
            gameObject.transform.rotation = rot;
        }

        if (gameObject.transform.rotation.eulerAngles.x > 35 && gameObject.transform.rotation.eulerAngles.x < 40)
        {
            #region Play hitting Ground Sound
            // play a weapon hitting ground sound
            GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.WEAPON_SMASH_GROUND, Attacker.transform, Attacker.transform.position);
            #endregion

            ((PlayerStats)Attacker).Rumble(0.2f, 0.4f, 0.5f);

            Destroy(gameObject);
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy" ||
            other.gameObject.tag == "Boss")
        {
            #region Play hitting character sound
            // Play a hit sound
            // find the closest point on my collider that my attacker is so that the sound is played directionally properly 
            Vector3 contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position); // roughly where the collision happened

            // find which player is the listener
            GameObject tempListener = GameManager.playerManager.PlayerList()[0];
            float dist = float.MaxValue;
            foreach (GameObject player in GameManager.playerManager.PlayerList())
            {
                if (Vector3.Distance(player.transform.position, contactPoint) < dist)
                {
                    dist = Vector3.Distance(player.transform.position, contactPoint);
                    tempListener = player;
                }
            }

            GameManager.audioManager.PlaySoundAtPosition(Attacker.GetHitSound(other.gameObject), tempListener.transform, contactPoint); 
            #endregion
        }

        base.OnTriggerEnter(other);
    }

    private void OnDestroy()
    {
        Attacker.usingWeaponSmash = false;

        Animator animator = Attacker.gameObject.GetComponentInChildren<Animator>();

        if (Attacker.Abilities[0].AbilityName == "Sword and Shield")
        {
            animator.SetBool("UseWeaponSmashSword", false);
        }
        else
        {
            animator.SetBool("UseWeaponSmashBow", false);
        }
    }
}
