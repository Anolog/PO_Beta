using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundShockHitbox : Hitbox {

    const int FORCE_MULTIPLER = 1000;

    float Speed = 35;

    Quaternion InitialRotation;

    GameObject particles;

    Material mat;

    public void Start()
    {
        InitialRotation = transform.rotation;

        mat = gameObject.GetComponentInChildren<Renderer>().material;

        particles = transform.Find("Particle System").gameObject;
    }

    public void Update ()
    {
        // first find which helper (front or back) to use based on which direction the ramp (if there is a ramp) is going
        //get helper pos
        Vector3 frontHelperPos = transform.Find("GroundShockCollider").Find("GroundShockHelperFront").transform.position;
        //Vector3 backHelperPos = transform.Find("Mesh_Ground_Shock_Hitbox_With2Helpers").Find("GroundShockHelperBack").transform.position;
        // raycast down with both of them to see which ray is shorter

        // create a layer mask
        int fadingGeometry = LayerMask.NameToLayer("FadingGeometry");
        int nonFadingGeometry = LayerMask.NameToLayer("NonFadingGeometry");

        int layerMaskA = 1 << fadingGeometry;
        int layerMaskB = 1 << nonFadingGeometry;

        int layerMask = layerMaskA | layerMaskB;

        Vector3 scale = gameObject.transform.localScale;
        scale.x += Time.deltaTime * Speed;
        scale.z += Time.deltaTime * Speed;
        scale.y += Time.deltaTime * Speed;

        gameObject.transform.localScale = scale;

        //scale.x = 1 / scale.x;
        //scale.y = 1 / scale.y;
        //scale.z = 1 / scale.z;

        //particles.transform.localScale = scale;

        //raycast
        RaycastHit hit;
        if (Physics.Raycast(frontHelperPos, -transform.up, out hit, 0.5f, layerMask))
        {
            transform.rotation = InitialRotation;

            //get normal form raycast hit
            Vector3 normal = hit.normal;
            float height = hit.distance;

            Vector3 pos = gameObject.transform.position;
            pos.y -= height - 0.16f;
            gameObject.transform.position = new Vector3(pos.x, pos.y, pos.z);
            GetComponent<Rigidbody>().useGravity = false;
        }
        else
        {
            GetComponent<Rigidbody>().useGravity = true;
        }

        if (scale.x >= 20)
        {
            DestroyObject(gameObject);
        }

    }

protected override void OnTriggerEnter(Collider other)
    {
        //added for testing, can be removed but is also kinda fun
        if (other.gameObject.tag != Attacker.tag)
        {
            if (other.gameObject.GetComponent<CharacterStats>() != null)
            {
                other.gameObject.GetComponent<CharacterStats>().TakeDamage(Attacker, Type, AbilityDamage);
                other.gameObject.GetComponent<CharacterStats>().KnockbackCharacter(gameObject.transform.forward * FORCE_MULTIPLER, 1, Attacker);
                
                #region Play Hit Sound
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
        }
        //base.OnTriggerEnter(other);
    }
}
