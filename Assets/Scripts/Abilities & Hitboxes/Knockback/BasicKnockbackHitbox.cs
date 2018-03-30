using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicKnockbackHitbox : Hitbox {

    const int FORCE_MULTIPLIER = 3000;
    Material mat1;
    Material mat2;

    Collider col;
    protected void Start()
    {
        name = "Knockback";
        Renderer[] rends = gameObject.GetComponentsInChildren<Renderer>();
        mat1 = rends[0].material;
        mat2 = rends[1].material;

        col = GetComponentInChildren<Collider>();

        StartCoroutine(Expand());
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
                direction.Normalize();
                //Debug.Log("Knockback");
                
                other.gameObject.GetComponent<CharacterStats>().TakeDamage(Attacker, Type, AbilityDamage);
                other.gameObject.GetComponent<CharacterStats>().KnockbackCharacter(direction * FORCE_MULTIPLIER, 1, Attacker);
            }

            #region Play knokback sound
            // Play a knockback sound

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
    }

    IEnumerator Expand()
    {
        float startTime = Time.time;
        transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        bool colEnabled = true;

        while (Time.time < startTime + 1.6f)
        {
            float val = (Time.time - startTime);
            float scale = 0.2f + Mathf.Clamp01(3 * val);

            if (scale > 1.19f && colEnabled)
                col.enabled = false;

            transform.localScale = new Vector3(scale, scale, scale);
            mat1.SetFloat("_Fill", 1 + 5 * val);// 0.5f + 4.5f * Mathf.Pow(val, 6));
            mat1.SetFloat("_Duration", Mathf.Pow(val, 0.4f) * 10f);
            mat1.SetFloat("_Dim", 0.2f * Mathf.Cos(val * Mathf.PI * 2) + 0.2f);
            mat2.SetFloat("_Fill", 1 + 5 * val);// 0.5f + 4.5f * Mathf.Pow(val, 6));
            mat2.SetFloat("_Duration", Mathf.Pow(val, 0.4f) * 10f);
            mat2.SetFloat("_Dim", 0.2f * Mathf.Cos(val * Mathf.PI * 2) + 0.2f);
            yield return null;
        }

        yield return null;
    }
}
