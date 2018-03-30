using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Command
{
    static float xRotationMouseSensitivity = 120.0f;
    static float yRotationMouseSensitivity = 1.8f;
    static float xRotationRightStickSensitivity = 1.5f;
    static float yRotationRightStickSensitivity = 0.02f;
    static float chargeRotationMulitplier = 0.25f;

    //Pending implementation of character object
    //Character* m_Character;
    public static void MoveTowards(GameObject player, Vector3 targetPosition)
    {
        // FOR AI
    }

    public static void Move(Controllers controller, Vector2 direction)
    {
        if (!GameManager.playerManager.Players.ContainsKey(controller))
        {
            return;
        }
        GameObject player = GameManager.playerManager.Players[controller];
        Rigidbody body = player.GetComponent<Rigidbody>();
        CharacterStats stats = player.GetComponent<CharacterStats>();

        if (!stats.isControllable ||
            stats.usingGroundShock ||
            stats.usingChainLightning ||
            stats.usingHeal ||
            stats.usingProtectMe)
        {
            return;
        }

        float maxSpeed = stats.MovementSpeed;

        if (direction.y < 0)
            maxSpeed *= 0.5f;

        Vector3 velocity = body.velocity;
        velocity.y = 0;
        float speed = velocity.magnitude;

        float desiredSpeed = speed + stats.Acceleration * Time.fixedDeltaTime;
        desiredSpeed = Mathf.Clamp(desiredSpeed, 0, maxSpeed);

        Vector3 direction3D = Vector3.Normalize(direction.x * body.transform.right + direction.y * body.transform.forward);
        Vector3 desiredVelocity = direction3D * desiredSpeed;

        Vector3 velocityDifference = desiredVelocity - velocity;
        velocityDifference = Vector3.ClampMagnitude(velocityDifference, stats.Acceleration * Time.fixedDeltaTime);

        body.AddForce(velocityDifference, ForceMode.VelocityChange);
    }

    public static void RotateTowards(GameObject player, Quaternion targetRotation)
    {
        // FOR AI
    }

    public static void RotateController(Controllers controller, Vector2 rotation)
    {
        if (!GameManager.playerManager.Players.ContainsKey(controller))
        {
            return;
        }

        PlayerManager playerManager = GameManager.playerManager;
        GameObject player = playerManager.Players[controller];

        CharacterStats stats = player.GetComponent<CharacterStats>();
        if (!stats.isControllable)
        {
            return;
        }

        float horizontalRotation = rotation.x * xRotationRightStickSensitivity * playerManager.InvertedAxis[controller].x * playerManager.SensitivityMultiplier[controller];
        float verticalRotation = rotation.y * yRotationRightStickSensitivity * playerManager.InvertedAxis[controller].y * playerManager.SensitivityMultiplier[controller];
        if (player.GetComponent<CharacterStats>().usingCharge)
        {
            horizontalRotation *= chargeRotationMulitplier;
        }

        Vector3 rot = player.transform.eulerAngles;

        rot.y += horizontalRotation * player.GetComponent<CharacterStats>().AngularSpeed * Time.fixedDeltaTime;

        player.transform.rotation = (Quaternion.Euler(rot.x, rot.y, rot.z));


        if (player.GetComponent<CharacterStats>().usingCharge)
        {
            verticalRotation *= chargeRotationMulitplier;
        }

        Third_Person_Camera cam = player.GetComponent<PlayerStats>().Camera.GetComponent<Third_Person_Camera>();

        float rotate = cam.VerticalRotation;

        rotate += verticalRotation * player.GetComponent<PlayerStats>().AngularSpeed * Time.fixedDeltaTime;
        rotate = Mathf.Clamp(rotate, -1, 1);

        cam.VerticalRotation = rotate;
    }

    public static void RotateMouse(Controllers controller, Vector2 rotation)
    {
        if (!GameManager.playerManager.Players.ContainsKey(controller))
        {
            return;
        }

        PlayerManager playerManager = GameManager.playerManager;
        GameObject player = playerManager.Players[controller];
        CharacterStats stats = player.GetComponent<CharacterStats>();
        if (!stats.isControllable)
        {
            return;
        }

        float horizontalRotation = -rotation.x * xRotationMouseSensitivity * playerManager.InvertedAxis[controller].x * playerManager.SensitivityMultiplier[controller]; 
        float verticalRotation = -rotation.y * yRotationMouseSensitivity * playerManager.InvertedAxis[controller].y * playerManager.SensitivityMultiplier[controller]; 
        if (player.GetComponent<CharacterStats>().usingCharge)
        {
            horizontalRotation *= chargeRotationMulitplier;
        }

        Vector3 rot = player.GetComponent<Rigidbody>().rotation.eulerAngles;

        rot.y -= horizontalRotation * Time.fixedDeltaTime;

        // Have to rotate the rigidbody not the transform
        player.GetComponent<Rigidbody>().MoveRotation(Quaternion.Euler(rot.x, rot.y, rot.z));
    
        if (player.GetComponent<CharacterStats>().usingCharge)
        {
            verticalRotation *= chargeRotationMulitplier;
        }
        Third_Person_Camera cam = player.GetComponent<PlayerStats>().Camera.GetComponent<Third_Person_Camera>();

        float rotate = cam.VerticalRotation;

        rotate -= verticalRotation * Time.fixedDeltaTime;
        rotate = Mathf.Clamp(rotate, -1, 1);

        cam.VerticalRotation = rotate;
    }

    public static void Jump(Controllers controller)
    {
        if (!GameManager.playerManager.Players.ContainsKey(controller))
        {
            return;
        }

        GameObject player = GameManager.playerManager.Players[controller];
        //Debug.Log(player.GetComponent<CharacterStats>().Grounded.ToString());
        PlayerStats stats = player.GetComponent<PlayerStats>();

        if (!stats.isControllable ||
            stats.usingChainLightning ||
            stats.usingHeal ||
            stats.usingProtectMe)
        {
            return;
        }

        if (stats.Grounded)
        {
            stats.StartCoroutine(JumpCoroutine(stats, 0.3f));
            GameManager.audioManager.PlaySound(AudioManager.Sounds.PLAYER_JUMP);
        }

    }

    static IEnumerator JumpCoroutine(PlayerStats stats, float buildUpDuration)
    {
        stats.Grounded = false;
        stats.gameObject.GetComponentInChildren<Animator>().SetBool("Jump", true);

        stats.m_CollisionUpdateTimer = stats.CollisionUpdateTime + Time.time + buildUpDuration;

        yield return new WaitForSeconds(buildUpDuration);

        Rigidbody body = stats.gameObject.GetComponent<Rigidbody>();
        PhysicMaterial material = stats.gameObject.GetComponent<CapsuleCollider>().material;
        UnityEngine.AI.NavMeshAgent agent = stats.gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();

        agent.enabled = false;

        stats.Landing = true;

        if (body.mass < 1000)
        {
            body.mass = 1000;
        }
        material.frictionCombine = PhysicMaterialCombine.Minimum;
        material.staticFriction = 0.0f;
        material.dynamicFriction = 0.0f;
        body.AddForce(new Vector3(0, CharacterStats.JUMP_IMPULSE, 0), ForceMode.Impulse);

        stats.AddToJumps();

        yield return null;
    }

    public static void UseAbility1(Controllers controller)
    {
        if (!GameManager.playerManager.Players.ContainsKey(controller))
        {
            return;
        }

        GameObject player = GameManager.playerManager.Players[controller];
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats.isControllable && !stats.isCastLocked && !stats.isDowned)
        {
            stats.Abilities[1].Use();
        }
    }

    public static void UseAbility2(Controllers controller)
    {
        if (!GameManager.playerManager.Players.ContainsKey(controller))
        {
            return;
        }

        GameObject player = GameManager.playerManager.Players[controller];
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats.isControllable && !stats.isCastLocked && !stats.isDowned)
        {
            stats.Abilities[2].Use();
        }
    }

    public static void UseAbility3(Controllers controller)
    {
        if (!GameManager.playerManager.Players.ContainsKey(controller))
        {
            return;
        }

        GameObject player = GameManager.playerManager.Players[controller];
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats.isControllable && !stats.isCastLocked && !stats.isDowned)
        {
            stats.Abilities[3].Use();
        }
    }

    public static void UseAbility4(Controllers controller)
    {
        if (!GameManager.playerManager.Players.ContainsKey(controller))
        {
            return;
        }

        GameObject player = GameManager.playerManager.Players[controller];
        CharacterStats stats = player.GetComponent<CharacterStats>();
        if (stats.isControllable && !stats.isCastLocked && !stats.isDowned)
        {
            stats.Abilities[4].Use();
        }
    }

    public static void UseAttackAbility(Controllers controller)
    {
        if (!GameManager.playerManager.Players.ContainsKey(controller))
        {
            return;
        }

        GameObject player = GameManager.playerManager.Players[controller];
        CharacterStats stats = player.GetComponent<CharacterStats>();
        if (stats.isControllable && !stats.isCastLocked && !stats.isDowned)
        {
            stats.Abilities[0].Use();
        }
    }

    public static void UseRevive(Controllers controller)
    {
        if (!GameManager.playerManager.Players.ContainsKey(controller))
        {
            return;
        }

        GameObject player = GameManager.playerManager.Players[controller];
        PlayerStats stats = player.GetComponent<PlayerStats>();
        GameObject playerUI = player.transform.Find("Player_UI_Prefab").gameObject;

        if (controller != Controllers.KEYBOARD_MOUSE)
        {

            //Set the button to be down if hit
            if (GameManager.controllerInput.GetController(controller).GetButton(GAMEPAD_BUTTONS.LEFT_BUMPER, BUTTON_STATES.PRESSED))
            {
                playerUI.GetComponent<PlayerUI>().RevivePrompt.transform.Find("ReviveLeftBumper").GetComponent<UnityEngine.UI.Image>().sprite = 
                playerUI.GetComponent<PlayerUI>().m_LeftBumperImage[1];
            }

            /*
            else if (GameManager.controllerInput.GetController(controller).GetButton(GAMEPAD_BUTTONS.LEFT_BUMPER, BUTTON_STATES.UP))
            {
                playerUI.GetComponent<PlayerUI>().RevivePrompt.transform.Find("ReviveLeftBumper").GetComponent<UnityEngine.UI.Image>().sprite = 
                playerUI.GetComponent<PlayerUI>().m_LeftBumperImage[0];
            }
            */

            //Set the button to be down if hit
            if (GameManager.controllerInput.GetController(controller).GetButton(GAMEPAD_BUTTONS.RIGHT_BUMPER, BUTTON_STATES.PRESSED))
            {
                playerUI.GetComponent<PlayerUI>().RevivePrompt.transform.Find("ReviveRightBumper").GetComponent<UnityEngine.UI.Image>().sprite =
                playerUI.GetComponent<PlayerUI>().m_RightBumperImage[1];
            }

            /*
            else if (GameManager.controllerInput.GetController(controller).GetButton(GAMEPAD_BUTTONS.RIGHT_BUMPER, BUTTON_STATES.UP))
            {
                playerUI.GetComponent<PlayerUI>().RevivePrompt.transform.Find("ReviveRightBumper").GetComponent<UnityEngine.UI.Image>().sprite = 
                playerUI.GetComponent<PlayerUI>().m_RightBumperImage[0];
            }
            */

        }

        else
        {
            if (playerUI.GetComponent<PlayerUI>().RevivePrompt.transform.Find("ReviveRightBumper").GetComponent<UnityEngine.UI.Image>().sprite == 
                playerUI.GetComponent<PlayerUI>().m_RightBumperImage[0])
            {
                playerUI.GetComponent<PlayerUI>().RevivePrompt.transform.Find("ReviveRightBumper").GetComponent<UnityEngine.UI.Image>().sprite =
                playerUI.GetComponent<PlayerUI>().m_RightBumperImage[1];

                playerUI.GetComponent<PlayerUI>().RevivePrompt.transform.Find("ReviveLeftBumper").GetComponent<UnityEngine.UI.Image>().sprite =
                playerUI.GetComponent<PlayerUI>().m_LeftBumperImage[1];
            }

            else if (playerUI.GetComponent<PlayerUI>().RevivePrompt.transform.Find("ReviveRightBumper").GetComponent<UnityEngine.UI.Image>().sprite ==
                     playerUI.GetComponent<PlayerUI>().m_RightBumperImage[1])
            {
                playerUI.GetComponent<PlayerUI>().RevivePrompt.transform.Find("ReviveRightBumper").GetComponent<UnityEngine.UI.Image>().sprite =
                playerUI.GetComponent<PlayerUI>().m_RightBumperImage[0];

                playerUI.GetComponent<PlayerUI>().RevivePrompt.transform.Find("ReviveLeftBumper").GetComponent<UnityEngine.UI.Image>().sprite =
                playerUI.GetComponent<PlayerUI>().m_LeftBumperImage[0];
            }
        }

        stats.Revive.Use();
    }

    public static void HandlePause(Controllers controller)
    {
        if (!GameManager.playerManager.Players.ContainsKey(controller))
        {
            return;
        }

        Game game = GameObject.Find("Game").GetComponent<Game>();
        Canvas pauseUI = game.PauseMenuUI;

        if (!game.IsPaused)
        {
            game.RemoveInput();
            Pause(pauseUI);
        }
        else
        {
            UnPause(pauseUI);
            game.SetUpInput();
        }
    }

    public static void SkipCutScene(Controllers controller)
    {
        GameObject.Find("Game").GetComponent<Game>().SwitchToInGame();
    }

    public static void Pause(Canvas pauseUI)
    {

        pauseUI.gameObject.SetActive(true);

        foreach (GameObject player in GameManager.playerManager.Players.Values)
        {
            player.GetComponent<PlayerStats>().isControllable = false;
        }

    }

    public static void UnPause(Canvas pauseUI)
    {
        Time.timeScale = 1;
        pauseUI.gameObject.SetActive(false);

        foreach (GameObject player in GameManager.playerManager.Players.Values)
        {
            player.GetComponent<PlayerStats>().isControllable = true;
        }
    }

    public static void KillEnemy(GameObject enemy)
    {
        CharacterStats enemyStats = enemy.GetComponent<CharacterStats>();
        WaveSpawner spawner = enemyStats.Game.GetComponent<WaveSpawner>();
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        Rigidbody body = enemy.GetComponent<Rigidbody>();

        enemyAI.Agent.enabled = false;

        GameObject tempListener = GameManager.playerManager.PlayerList()[0];
        float dist = float.MaxValue;
        foreach (GameObject player in GameManager.playerManager.PlayerList())
        {
            if (Vector3.Distance(player.transform.position, enemy.transform.position) < dist)
            {
                dist = Vector3.Distance(player.transform.position, enemy.transform.position);
                tempListener = player;
            }
        }

        if (enemyAI is MeleeEnemyAI)
        {
            GameManager.audioManager.PlaySoundAtPosition(GameManager.audioManager.GetSoundFromEffect("Plant Death", false), tempListener.transform, tempListener.transform.position);
        }
        else if (enemyAI is RangedEnemyAI)
        {
            GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.RANGED_DIE, tempListener.transform, tempListener.transform.position);
        }
        else
        {
            GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.SUPPORT_DIE, tempListener.transform, tempListener.transform.position);
        }

        //body.AddForce(Vector3.zero, ForceMode.VelocityChange);
        body.useGravity = false;
        enemy.GetComponent<Collider>().enabled = false;
        spawner.Enemies.Remove(enemy);
        spawner.PointsInScene -= enemyStats.ScorePointValue;
        enemyStats.Game.GetComponent<Game>().SetPointsLeft(enemyStats.Game.GetComponent<Game>().GetPointsLeft() - (int)enemyStats.ScorePointValue);
        if (enemyAI.Target != null)
        {
            enemyAI.Target.GetComponent<PlayerStats>().RemoveFromFollowers(enemyStats);
        }

        enemyAI.Animator.SetTrigger("Die");
        if (enemy.GetComponent<RangedEnemyAI>() != null)
        {
            enemy.transform.Find("Mesh_RangedEnemy").Find("Mesh_Effect").gameObject.SetActive(false);
            enemy.transform.Find("Mesh_RangedEnemy").Find("Projectile").gameObject.SetActive(false);
            enemyAI.Agent.enabled = false;
        }
        enemyAI.isDead = true;

        Object.Destroy(enemy, 1.5f);
    }

    public static void KillBoss(GameObject boss)
    {
        CharacterStats bossStats = boss.GetComponent<CharacterStats>();
        if (bossStats == null)
        {
            return;
        }
        Game game = bossStats.Game.GetComponent<Game>();
        boss.GetComponent<Rigidbody>().AddForce(Vector3.zero, ForceMode.VelocityChange);
        boss.GetComponent<Collider>().enabled = false;


        GameObject tempListener = GameManager.playerManager.PlayerList()[0];
        float dist = float.MaxValue;
        foreach (GameObject player in GameManager.playerManager.PlayerList())
        {
            if (Vector3.Distance(player.transform.position, boss.transform.position) < dist)
            {
                dist = Vector3.Distance(player.transform.position, boss.transform.position);
                tempListener = player;
            }
        }
        GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.BOSS_DIE, tempListener.transform, tempListener.transform.position);

        bossStats.Game.GetComponent<WaveSpawner>().Enemies.Remove(boss);
        game.SetPointsLeft(bossStats.Game.GetComponent<Game>().GetPointsLeft() - (int)bossStats.ScorePointValue);
        game.IsBossDead = true;
        game.DeadTimer += Time.time;
        boss.GetComponent<EnemyAI>().Target.GetComponent<PlayerStats>().RemoveFromFollowers(bossStats);
        boss.GetComponent<EnemyAI>().Animator.SetTrigger("Die");
        boss.GetComponent<EnemyAI>().Agent.enabled = false;
        foreach (GameObject player in GameManager.playerManager.PlayerList())
        {
            PlayerUI UI = player.transform.Find("Player_UI_Prefab").GetComponent<PlayerUI>();
            UI.BossHealth.SetActive(false);
        }

        Object.Destroy(boss, 2.4f);
    }


    public static float RAYCAST_DOWN_AMOUNT = 6.0f;

    public static void DownPlayer(GameObject player)
    {
        // make myself kinematic
        player.gameObject.GetComponent<Rigidbody>().isKinematic = true;

        Vector3 playerPos = player.transform.position;

        //Raycast to make sure it spawns on the ground when used
        Ray raycast = new Ray(playerPos + Vector3.up, Vector3.down * RAYCAST_DOWN_AMOUNT);
        RaycastHit hitInfo = new RaycastHit();

        if (Physics.Raycast(raycast, out hitInfo))
        {
            if (hitInfo.transform.tag == "Platform" || hitInfo.transform.tag == "Wall")
            {
                //playerPos -= new Vector3(0, hitInfo.distance - 1, 0);
                player.GetComponent<PlayerStats>().StartCoroutine(FallToGround(hitInfo.distance - 1, 7, player));
            }
        }

        player.transform.position = playerPos;

        // play a hurting sound
        GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.PLAYER_DOWN, player.transform, player.transform.position);
        // set isDowned to true
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        playerStats.isDowned = true;

        player.GetComponent<CharacterStats>().usingCharge = false;

        playerStats.AddToDowns(1);

        player.GetComponentInChildren<PlayerUI>().RevivePrompt.SetActive(false);

        playerStats.ScorePointsEarned = (int)(playerStats.ScorePointsEarned * 0.75f);

        // untag the player so enemies know not to try and find this player
        player.tag = "DownedPlayer";
        // Change layer for layer masking
        player.layer = LayerMask.NameToLayer("DownedPlayer");

        // create the downed player hitbox
        Vector3 pos = player.transform.position;
        pos.x += (player.GetComponent<CapsuleCollider>().height / 2) * player.transform.forward.x;
        pos.z += (player.GetComponent<CapsuleCollider>().height / 2) * player.transform.forward.z;
        Quaternion rot = player.transform.rotation;
        playerStats.SetDownedHitbox((GameObject)Object.Instantiate(Resources.Load("DownedPlayerHitbox"), pos, rot));
        playerStats.GetDownedHitbox().GetComponent<DownedPlayerHitbox>().Initialize(playerStats);

        // change my colour for now so I know I have died
        //Transform mesh = player.transform.Find("Mesh_Player").transform.Find("Body");
        //mesh.GetComponent<Renderer>().material.color = new Color(0.08f, 0.11f, 0.12f, 1);

        // animate me going down
        Animator animator = player.GetComponentInChildren<Animator>();
        animator.SetBool("Down", true);
        animator.SetLayerWeight(animator.GetLayerIndex("UpperBodyLayer"), 0);
        animator.SetLayerWeight(animator.GetLayerIndex("UpperBodyWithPelvisLayer"), 0);

        playerStats.RemoveAllFollowers();
    }

    static IEnumerator FallToGround(float distance, float speed, GameObject player)
    {
        float distanceLeft = distance;
        Vector3 playerPos = player.transform.position;

        while (distanceLeft > 0)
        {
            playerPos.y -= Time.deltaTime * speed;

            distanceLeft -= Time.deltaTime * speed;

            player.transform.position = playerPos;

            yield return null;
        }

        yield return null;
    }

    public static void RevivePlayer(GameObject player, PlayerStats Revivor)
    {
        // play a healing sound
        GameManager.audioManager.PlaySoundAtPosition(AudioManager.Sounds.PLAYER_REVIVE, player.transform, player.transform.position);

        // set isDowned to false
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        playerStats.isDowned = false;

        // tag the player as player
        player.tag = "Player";
        // Change layer for layer masking
        player.layer = LayerMask.NameToLayer("AlivePlayer");

        // change my colour back to normal
        //Transform mesh = player.transform.Find("Mesh_Player").transform.Find("Body");
        //mesh.GetComponent<Renderer>().material.color = playerStats.PlayerColour;

        // let my animator know I'm not down anymore
        player.GetComponentInChildren<Animator>().SetBool("Down", false);

        // Give me my health and shield back
        playerStats.Health = playerStats.MaxHealth;
        playerStats.Shield = playerStats.MaxShield;

        // make myself unkinematic
        player.gameObject.GetComponent<Rigidbody>().isKinematic = false;

        Revivor.AddToRevives(1);
    }
}
