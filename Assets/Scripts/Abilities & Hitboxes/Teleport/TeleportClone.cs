using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportClone : Hitbox
{

    Material mat;

    public override void Initialize(CharacterStats attacker, Ability.AbilityType type, int abilityDamage, float lifetime)
    {
        base.Initialize(attacker, type, abilityDamage, lifetime);

        mat = gameObject.GetComponentInChildren<Renderer>().material;

        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float startTime = Time.time;

        while (Time.time < startTime + 1.5)
        {
            float val = (Time.time - startTime);

            if (val <= 0.75f)
                mat.SetFloat("_FadeTime", 1.34f * val);
            else
                mat.SetFloat("_FadeTime", 1.34f * (1 - (val- 0.5f)));


            yield return null;
        }

        yield return null;
    }
}
