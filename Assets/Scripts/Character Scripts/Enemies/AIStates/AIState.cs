using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum StateType
{
    Chase,
    Attack,
    Kite,
    SwitchTarget,
    Teleport,
    Wander,
    Wait,
    Heal,
    Knockback,
    EnergyMine,
    Default,
}


public abstract class AIState
{
    public AIState(EnemyAI owner)
    {
        Owner = owner;
        transform = owner.transform;
        Target = owner.Target;
        Agent = owner.Agent;
    }

    public abstract void Update();
    public abstract void Deactivate();
    public abstract void Activate();

    public EnemyAI Owner;
    public StateType Type;
    protected Transform transform;
    public GameObject Target;
    protected NavMeshAgent Agent;
}
