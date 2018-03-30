using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownedPlayerHitbox : Hitbox
{

    private PlayerStats m_DowndedPlayer;

    public void Update()
    {
        if (!m_DowndedPlayer.isDowned)
        {
            Destroy(gameObject);
        }

        // stay around the player
        Vector3 pos = m_DowndedPlayer.transform.position;
        pos.x += (m_DowndedPlayer.GetComponent<CapsuleCollider>().height / 2) * m_DowndedPlayer.transform.forward.x;
        pos.z += (m_DowndedPlayer.GetComponent<CapsuleCollider>().height / 2) * m_DowndedPlayer.transform.forward.z;

        transform.position = pos;
    }

    public virtual void Initialize(PlayerStats downedPlayer)
    {
        m_DowndedPlayer = downedPlayer;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerStats stats = other.gameObject.GetComponent<PlayerStats>();
            PlayerUI uiRef = other.gameObject.GetComponentInChildren<PlayerUI>();
            stats.Revive.SetTarget(m_DowndedPlayer.gameObject);
            uiRef.RevivePrompt.SetActive(true);

            //Reset the bumper images
            uiRef.RevivePrompt.transform.Find("ReviveLeftBumper").GetComponent<UnityEngine.UI.Image>().sprite = uiRef.m_LeftBumperImage[0];
            uiRef.RevivePrompt.transform.Find("ReviveRightBumper").GetComponent<UnityEngine.UI.Image>().sprite = uiRef.m_RightBumperImage[0];

        }
    }

    protected void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (other.gameObject.GetComponent<PlayerStats>().Revive.GetTarget() != null)
            {
                return;
            }
            PlayerStats stats = other.gameObject.GetComponent<PlayerStats>();
            stats.Revive.SetTarget(m_DowndedPlayer.gameObject);
            other.gameObject.GetComponentInChildren<PlayerUI>().RevivePrompt.SetActive(true);
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player" || other.gameObject.tag == "DownedPlayer")
        {
            other.gameObject.GetComponent<PlayerStats>().Revive.SetTarget(null);
            other.gameObject.GetComponentInChildren<PlayerUI>().RevivePrompt.SetActive(false);
        }
    }

    private void OnDestroy()
    {
         foreach(GameObject player in GameManager.playerManager.PlayerList())
         {
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats.Revive.GetTarget() == m_DowndedPlayer.gameObject)
            {
                stats.Revive.SetTarget(null);
                player.gameObject.GetComponentInChildren<PlayerUI>().RevivePrompt.SetActive(false);
            }
        }
    }
}
