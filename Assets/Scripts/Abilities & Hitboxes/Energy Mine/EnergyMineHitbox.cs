using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyMineHitbox : Hitbox
{
    /*
        private float lifeTime = 0.5f;

        protected void Update()
        {
            lifeTime -= Time.deltaTime;

            if (lifeTime <= 0)
                DestroyImmediate(gameObject);
        }
        */

    public Color StartLight;
    public Color StartMid;
    public Color StartDark;

    public Color EndLight;
    public Color EndMid;
    public Color EndDark;

    public override void Initialize(CharacterStats attacker, Ability.AbilityType type, int abilityDamage, float lifeTime)
    {
        base.Initialize(attacker, type, abilityDamage, lifeTime);
        StartCoroutine(ExplodeCoroutine(lifeTime - 0.1f));
    }

    IEnumerator ExplodeCoroutine(float duration)
    {
        float elapsedTime = 0;
        float inverseDuration = 1 / duration;

        Material mat = gameObject.GetComponentInChildren<Renderer>().material;

        while(elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float scale = 1.7f * Mathf.Pow(elapsedTime * inverseDuration, 10) + 0.1f * Mathf.Sin(25 * Mathf.PI * elapsedTime) + 0.3f;
            transform.localScale = new Vector3(scale, scale, scale);

            float interpVal = (scale - 0.2f) / 1.9f;

            mat.SetColor("_WhiteColor", Color.Lerp(StartLight, EndLight, interpVal));
            mat.SetColor("_GreyColor", Color.Lerp(StartMid, EndMid, interpVal));
            mat.SetColor("_BlackColor", Color.Lerp(StartMid, EndMid, interpVal));

            yield return null;
        }

        gameObject.GetComponentInChildren<SphereCollider>().enabled = true;

        yield return null;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        //Remember to change this for when it is done testing as the player
        if (other.gameObject.tag == "Player" && Attacker.gameObject.tag == "Boss")
        {
            PlayerStats stats = other.gameObject.GetComponent<PlayerStats>();

            if (stats != null)
            {
                RaycastHit hit;
                Vector3 dir = other.gameObject.transform.position - gameObject.transform.position;
                Ray ray = new Ray(gameObject.transform.position, dir);
                if(Physics.Raycast(ray, out hit, 2))
                {
                    stats.TakeDamage(Attacker, Type, AbilityDamage);
                    stats.Rumble(0.4f, 0.7f, 0.75f);
                }
            }
        }  
    }
}
