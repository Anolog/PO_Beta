using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VortexHitbox : Hitbox {

    private GameObject m_Weapon;

    Material mat1;
    Material mat2;

    Collider col;

    public void Initialize(CharacterStats attacker, Ability.AbilityType type, int abilityDamage, float lifeTime, GameObject weapon)
    {
        base.Initialize(attacker, type, abilityDamage, lifeTime);
        m_Weapon = weapon;

        Renderer[] rends = gameObject.GetComponentsInChildren<Renderer>();
        mat1 = rends[0].material;
        mat2 = rends[1].material;

        col = GetComponentInChildren<Collider>();

        StartCoroutine(Contract());
    }

    public void Update()
    {
        Vector3 pos = Attacker.transform.position;
        pos.y += Attacker.gameObject.GetComponent<CapsuleCollider>().height * 0.5f;
        gameObject.transform.position = pos;
    }

    override protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy" && Attacker.gameObject.tag == "Player" ||
            other.gameObject.tag == "Boss" && Attacker.gameObject.tag == "Player" ||
            other.gameObject.tag == "Player" && Attacker.gameObject.tag == "Boss")
        {
            if (other.gameObject.GetComponent<CharacterStats>() != null)
            {
                Vector3 direction = other.gameObject.transform.position - transform.position;
                float dist = direction.magnitude;
                direction.Normalize();
                other.gameObject.GetComponent<CharacterStats>().KnockbackCharacter(-direction * 300 * dist, 1, Attacker);
                Attacker.gameObject.GetComponent<CharacterStats>().AddMass(1000, 1);
                other.gameObject.GetComponent<CharacterStats>().TakeDamage(Attacker, Ability.AbilityType.Melee, AbilityDamage);
            }
        }
    }

    private void OnDestroy()
    {
        Animator animator = Attacker.gameObject.GetComponentInChildren<Animator>();

        animator.SetLayerWeight(animator.GetLayerIndex("UpperBodyLayer"), 0f);

        animator.SetBool("UseVortex", false);

        m_Weapon.SetActive(true);
    }

    IEnumerator Contract()
    {
        float startTime = Time.time;

        bool colEnabled = true;

        while (Time.time < startTime + 1)
        {
            float val = 1f * (Time.time - startTime);

            if (val > 0.3f && colEnabled)
                col.enabled = false;

            float scale = (1 - Mathf.Clamp01(val));
            transform.localScale = new Vector3(scale, scale, scale);
            mat1.SetFloat("_Fill", 1 + 2 * val);// 0.5f + 4.5f * Mathf.Pow(val, 6));
            mat1.SetFloat("_Duration", val * 1.7f * 4.8f);
            mat1.SetFloat("_Dim", 0.2f * val);
            mat1.SetFloat("_Angle", 3 * val);
            mat2.SetFloat("_Fill", 1 + 2 * val);// 0.5f + 4.5f * Mathf.Pow(val, 6));
            mat2.SetFloat("_Duration", val * 1.7f * 4.8f);
            mat2.SetFloat("_Dim", 0.2f * val);
            mat2.SetFloat("_Angle", 3 * val);

            yield return null;
        }

        yield return null;
    }
}
