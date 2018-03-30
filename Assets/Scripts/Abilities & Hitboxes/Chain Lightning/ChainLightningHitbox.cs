using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainLightningHitbox : Hitbox
{
    List<GameObject> m_EnemiesHit;
    int m_MaxEnemiesHit = 8;
    float m_Speed = 250;
    float m_LifeTime;
    bool m_HasHit = false;
    float m_MaxScale = 100f;
    float m_SphereCheckRadius = 4f;
    GameObject m_Target;
    GameObject m_LastHitEnemy;
    bool fistTime = true;
    List<Third_Person_Camera.CameraOrientedPlane> camRotations;

    public void Initialize(CharacterStats attacker, Ability.AbilityType type, int abilityDamage, float lifeTime, List<GameObject> enemiesHit, GameObject target = null)
    {
        Attacker = attacker;
        Type = type;
        AbilityDamage = abilityDamage;
        m_EnemiesHit = enemiesHit;
        m_LifeTime = lifeTime;
        //somehow add is putting items at the start of the list
        if(enemiesHit.Count > 0)
            m_LastHitEnemy = enemiesHit[enemiesHit.Count - 1];

        if (target != null)
        {
            m_Target = target;
            //we don't need collision after the first guy, we'll check in update if we've connected them
            GetComponent<Collider>().enabled = false;
        }

        if (m_EnemiesHit.Count >= 1)
        {
            m_Speed = 50f;
            //m_Speed *= 0.5f;
        }

        if (m_EnemiesHit.Count >= m_MaxEnemiesHit)
        {
            Destroy(gameObject);
        }

        camRotations = new List<Third_Person_Camera.CameraOrientedPlane>();

        Third_Person_Camera.CameraOrientedPlane plane = new Third_Person_Camera.CameraOrientedPlane(gameObject);
        camRotations.Add(plane);

        foreach (GameObject player in GameManager.playerManager.PlayerList())
        {
            player.GetComponentInChildren<Third_Person_Camera>().Planes.Add(plane);
        }

        #region Play Lightning Sound
        // Play a lightning sound
        GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.CHAIN_LIGHTNING, Attacker.transform, Attacker.transform.position); 
        #endregion
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
        //Animator animator = Attacker.gameObject.GetComponentInChildren<Animator>();
        //if (animator.)

        if (m_LastHitEnemy != null)
        {
            //for some reason nw enemies are being added to the list at the front
            Vector3 pos = m_LastHitEnemy.transform.position;
            pos.y += Attacker.GetComponent<CapsuleCollider>().height * 0.5f;
            gameObject.transform.position = pos;

            Vector3 targetPos = m_Target.transform.position;
            CapsuleCollider cap = m_Target.GetComponent<CapsuleCollider>();
            if (cap != null)
                targetPos.y += cap.height * 0.5f;
            else
                targetPos.y += m_Target.GetComponent<BoxCollider>().size.y * 0.5f;

            Vector3 dir = targetPos - pos;
            Quaternion rot = Quaternion.LookRotation(Vector3.up, dir);

            gameObject.transform.rotation = rot;

            float distanceBetween = Vector3.Distance(pos, targetPos);
            float distanceAway = distanceBetween - transform.localScale.y * 0.1f;

            Vector3 scale = gameObject.transform.localScale;
            scale.y += Time.deltaTime * m_Speed * Mathf.Sign(distanceAway);

            gameObject.transform.localScale = scale;

            if (scale.y >= m_MaxScale)
            {
                if (fistTime)
                {
                    fistTime = false;
                    Destroy(gameObject, m_LifeTime);
                }
            }

            if (!m_HasHit && distanceAway < 0.05f)
            {
                HitEnemyAndCreateNewSegment(m_Target);
            }
        }
        else if (m_EnemiesHit.Count > 0)
        {
            //if we lost our target
            Destroy(gameObject);
        }
        else
        {
            Vector3 scale = gameObject.transform.localScale;
            scale.y += Time.deltaTime * m_Speed;

            gameObject.transform.localScale = scale;

            if (scale.y >= m_MaxScale)
            {
                if (fistTime)
                {
                    fistTime = false;
                    Destroy(gameObject, m_LifeTime);
                }
            }
        }

        if (m_HasHit)
        {
            if (fistTime)
            {
                fistTime = false;
                Destroy(gameObject, m_LifeTime);
            }
        }

    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other == GetComponent<Collider>())
        {
            return;
        }
        // check if I hit someone with the same tag as me
        if (other.gameObject.tag != Attacker.tag)
        {
            // make sure what I hit has a characterstats
            if (other.gameObject.GetComponent<CharacterStats>() != null)
            {
                // If I hit the last enemy in m_EnemiesHit then I need to return here so that I don't immidiatly get destroyed
                if (m_EnemiesHit.Count != 0)
                {
                    if (other.gameObject == m_EnemiesHit[m_EnemiesHit.Count - 1])
                    {
                        return;
                    }
                }
                // If I hit an enemy already in my list of enemies hit destroy the gameObject
                int i = 0;
                foreach (GameObject enemy in m_EnemiesHit)
                {
                    if (enemy == other.gameObject)
                    {
                        Destroy(gameObject, m_LifeTime);
                        return;
                    }
                    i++;
                }

                // If I got this far I hit a valid target

                
                if (m_Target == null)
                {
                    m_Target = other.gameObject;
                    m_LastHitEnemy = Attacker.gameObject;
                }

                HitEnemyAndCreateNewSegment(other.gameObject);


            }
        }
    }

    void HitEnemyAndCreateNewSegment(GameObject hitEnemy)
    {
        m_HasHit = true;

        // Damage who I hit
        hitEnemy.GetComponent<CharacterStats>().TakeDamage(Attacker, Type, AbilityDamage);

        //Apply stun        Use knockback and apply 0 force since it does the same thing
        hitEnemy.GetComponent<CharacterStats>().KnockbackCharacter(Vector3.zero, 1.5f, Attacker);

        // add this enemy to my list of enemies hit
        m_EnemiesHit.Add(hitEnemy);
        // reduce the ability damage by a quarter
        AbilityDamage -= (int)(AbilityDamage * 0.25f);

        // Check if there is another valid target in a spherical radius of 4
        Collider[] colliders = Physics.OverlapSphere(hitEnemy.GetComponent<Collider>().ClosestPointOnBounds(transform.position), m_SphereCheckRadius);

        float distance = float.MaxValue;
        GameObject nextTarget = null;

        foreach (Collider collider in colliders)
        {
            // Make sure the collider Im checking is an enemy or the boss
            if (collider.gameObject.tag == "Enemy" || collider.gameObject.tag == "Boss")
            {
                float tempDistance = Vector3.Distance(collider.gameObject.transform.position, hitEnemy.transform.position);
                // if they are closer than the last collider I checked see if I can see them using a raycast
                if (tempDistance < distance)
                {
                    RaycastHit hit;

                    Vector3 targetPos = collider.gameObject.transform.position;
                    CapsuleCollider capsule = m_Target.GetComponent<CapsuleCollider>();
                    if (capsule != null)
                        targetPos.y += capsule.height * 0.5f;
                    else
                        targetPos.y += m_Target.GetComponent<BoxCollider>().size.y * 0.5f;

                    Vector3 lastHitPos = hitEnemy.transform.position;
                    capsule = m_Target.GetComponent<CapsuleCollider>();
                    if (capsule != null)
                        targetPos.y += capsule.height * 0.5f;
                    else
                        targetPos.y += m_Target.GetComponent<BoxCollider>().size.y * 0.5f;

                    Vector3 dir = targetPos - lastHitPos;
                    Ray ray = new Ray(lastHitPos, dir.normalized);
                    if (Physics.Raycast(ray, out hit, m_SphereCheckRadius))
                    {
                        if (hit.transform == collider.gameObject.transform)
                        {
                            bool alreadyInList = false;
                            // double check I'm not going to try and hit someone already in my list
                            foreach (GameObject enemy in m_EnemiesHit)
                            {
                                if (enemy != null)
                                {
                                    if (enemy.GetComponent<Collider>() == collider)
                                    {
                                        alreadyInList = true;
                                    }
                                }
                            }
                            if (!alreadyInList)
                            {
                                // if I can see the enemy In my sphere check, make them my next target
                                distance = tempDistance;
                                nextTarget = collider.gameObject;
                            }
                        }
                    }
                }
            }
        }

        // if we didn't find another target, leave
        if (nextTarget == null)
        {
            return;
        }

        #region Play Lightning Hit Sound
        // Play a lightning hit sound
        // find the closest point on my collider that my attacker is so that the sound is played directionally properly 
        Vector3 contactPoint = hitEnemy.GetComponent<Collider>().ClosestPointOnBounds(transform.position); // roughly where the collision happened

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
        AudioManager.Sounds sound = GameManager.audioManager.GetSoundFromEffect("Chain Lightning Hit", false);
        GameManager.audioManager.PlaySoundAtPosition(sound, tempListener.transform, contactPoint);
        #endregion

        // find the position and rotation to create the next chain lightning hitbox
        // find the angle around the y axis
        Vector3 direction = nextTarget.transform.position - hitEnemy.transform.position;
        float angleY = (Mathf.Atan(direction.x / direction.z) * Mathf.Rad2Deg);
        // find the angle around the x axis
        float radius = Mathf.Sqrt(direction.x * direction.x + direction.y * direction.y + direction.z * direction.z);
        float angleX = Mathf.Asin(direction.y / radius) * Mathf.Rad2Deg;

        Vector3 pos = hitEnemy.transform.position;
        CapsuleCollider cap = hitEnemy.GetComponent<CapsuleCollider>();
        if (cap != null)
            pos.y += cap.height * 0.5f;
        else
            pos.y += hitEnemy.GetComponent<BoxCollider>().size.y * 0.5f;

        // sometimes the angle I get is off by 180, check if this is the case, and adjust accordingly
        Vector3 angleYDirection = new Vector3(Mathf.Sin(Mathf.Deg2Rad * angleY), direction.y, Mathf.Cos(Mathf.Deg2Rad * angleY));

        if ((angleYDirection.x > 0 && direction.x < 0) || (angleYDirection.x < 0 && direction.x > 0))
        {
            angleY += 180;
        }

        Quaternion rot = Quaternion.Euler(angleX, angleY, 0);

        rot = Quaternion.FromToRotation(Vector3.up, direction);

        //Debug.Log(angleY);

        // create the new lightning hitbox
        GameObject newHitbox = (GameObject)Instantiate(Resources.Load("DamageHitboxes/ChainLightningAbilityHitbox"), pos, rot);
        newHitbox.GetComponent<ChainLightningHitbox>().Initialize(Attacker, Type, AbilityDamage, m_LifeTime, m_EnemiesHit, nextTarget);
    }
}
