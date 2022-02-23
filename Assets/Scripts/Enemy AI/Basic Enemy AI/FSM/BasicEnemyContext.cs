using UnityEngine;
using Pathfinding; // Aaron Granberg A*

// Reminder for setting dynamic scan of navmesh
// private NavGraph[] navGraph;
// navGraph = astarPath.graphs;
// AstartPath astarPath.Scan(navGraph);

public class BasicEnemyContext : MonoBehaviour
{
    // SECTION - Field ===================================================================
    #region REGION - HIDDEN - State
    private IEnemyState currState;
    private IEnemyState oldState;
    #endregion


    #region REGION - HIDDEN - Entity Specific
    private LivingEntityContext myLivingEntity;
    private Transform mySpriteTransform;
    #endregion


    #region REGION - HIDDEN - AStar Specific
    private AIPath myAIPath;                           // Movement, rotation, End Reached Distance, etc.
    private AIDestinationSetter myAIDestinationSetter; // Pathfinding Target
    private float maxSpeed;
    #endregion


    #region REGION - HIDDEN - Animator Strings

    // Animator
    private Animator anim;

    // Parameters
    private readonly string animParam_OnDeath = "OnDeath";
    private readonly string animParam_OnHit = "OnHit";
    private readonly string animParam_OnAtkRoaming = "OnAttackRoaming";
    private readonly string animParam_OnAtkAggressive = "OnAttackAggressive";
    private readonly string animParam_maxSpeed = "maxSpeed";
    private readonly string animParam_animAngle = "animAngle";

    // Animation States
    private readonly string animState_Iddle = "Iddle";
    private readonly string animState_OnAwake = "OnAwake";
    private readonly string animState_OnMoveBlendTree = "BlendTree _ Movement";
    private readonly string animState_OnAtkRoaming = "OnAttackRoaming";
    private readonly string animState_OnAtkAggressive = "OnAttackAgressive";
    private readonly string animState_OnDeath = "OnDeath";

    #endregion


    [Header("    ======= On Start Specifications =======\n")]
    [SerializeField] private BasicEnemy_States myStartingState = BasicEnemy_States.ROAMING;
    [SerializeField] private bool startAtMaxSpeed = true;

    [Space(10)]
    [Header("    ========== Roaming State ==========\n")]
    [Header("Weapon Manager")]
    [SerializeField] private WeaponManager rWeaponManager;

    [Header("Animator")]
    [SerializeField] private bool toAOnAtkExit = false;
    [Tooltip("Animation event must be set manually")]
    [SerializeField] private bool rAnimExecuteAtk = false;
    private AbstractMovementBehaviour r_MoveBehaviour;
    private AbstractAttackBehaviour r_AtkBehaviour;

    [Space(10)]
    [Header("    ========= Aggressive State =========\n")]
    [Header("Weapon Manager")]
    [SerializeField] private WeaponManager aWeaponManager;

    [Header("Animator")]
    [SerializeField] private bool toROnAtkExit = false;
    [Tooltip("Animation event must be set manually")]
    [SerializeField] private bool aAnimExecuteAtk = false;
    private AbstractMovementBehaviour a_MoveBehaviour;
    private AbstractAttackBehaviour a_AtkBehaviour;


    // SECTION - Property ===================================================================
    #region REGION - PROPERTY
    // General
    public LivingEntityContext MyLivingEntity { get => myLivingEntity; set => myLivingEntity = value; }
    public Animator Anim { get => anim; set => anim = value; }

    // AI
    public AIPath MyAIPath { get => myAIPath; set => myAIPath = value; }

    // Roaming State
    public WeaponManager RWeaponManager { get => rWeaponManager; set => rWeaponManager = value; }
    public bool ToAOnAtkExit { get => toAOnAtkExit; }
    public bool RAnimExecuteAtk { get => rAnimExecuteAtk; }
    public AbstractMovementBehaviour R_MoveBehaviour { get => r_MoveBehaviour; }
    public AbstractAttackBehaviour R_AtkBehaviour { get => r_AtkBehaviour; }

    // Aggressive State
    public WeaponManager AWeaponManager { get => aWeaponManager; set => aWeaponManager = value; }
    public bool ToROnAtkExit { get => toROnAtkExit; }
    public bool AAnimExecuteAtk { get => aAnimExecuteAtk; }
    public AbstractMovementBehaviour A_MoveBehaviour { get => a_MoveBehaviour; }
    public AbstractAttackBehaviour A_AtkBehaviour { get => a_AtkBehaviour; }
    #endregion


    // SECTION - Method - Unity Specific ===================================================================
    private void Start()
    {
        // Get Set Components & Variables
        GetSetHiddensHandler();
        // Set State Machine
        FirstStateHandler();
    }

    private void FixedUpdate()
    {
        if (oldState != currState)
        {
            oldState = currState;
            OnStateEnter();
        }

        OnStateUpdate();
        OnStateExit();
    }


    // SECTION - Method - State Specific ===================================================================
    public void OnStateEnter()
    {
        currState.OnStateEnter(this);
    }

    public void OnStateUpdate()
    {
        currState.OnStateUpdate(this);
    }

    public void OnStateExit()
    {
        currState = currState.OnStateExit(this);
    }


    // SECTION - Method - Utility ===================================================================
    #region REGION - On Start Handlers
    private void FirstStateHandler()
    {
        switch(myStartingState)
        {
            case BasicEnemy_States.ROAMING:
                SetFiniteStateMachine(BasicEnemy_States.ROAMING);
                break;
            case BasicEnemy_States.AGGRESSIVE:
                SetFiniteStateMachine(BasicEnemy_States.AGGRESSIVE);
                break;
            default: Debug.Log($"An error as occured at [FirstStateHandler()] of [EnemyContext.cs] from enemy: {gameObject.name}"); break;
        }

        oldState = currState;

        // endReachedDistance
        Debug.Log("FINISH ENDREACHEDDISTANCE IMPLEMENTATION HERE");
        myAIPath.endReachedDistance = 0.64f; // ARBITRARY DISTANCE, DELETE WHEN LINE BELLOW IS IMPLEMENTED
        //myAIPath.endReachedDistance = (myStartingState == BasicEnemy_States.ROAMING) ? myRoamingWeaponHolder.weapon.distance : myAggressiveWeaponHolder.weapon.distance; ;
    }
    
    private void GetSetHiddensHandler()
    {
        // AI ========================================
        // Get Components
        MyAIPath = GetComponentInChildren<AIPath>();
        myAIDestinationSetter = GetComponentInChildren<AIDestinationSetter>();

        // Set Variables
        maxSpeed = myAIPath.maxSpeed;


        if (myAIDestinationSetter.target == null)
            myAIDestinationSetter.target = GameManager.instance.PlayerTransformRef;

        if (!startAtMaxSpeed)
            SetSpeed(0.0f);

        // Miscellaneous ========================================
        // Get Components
        myLivingEntity = GetComponentInChildren<LivingEntityContext>();
        mySpriteTransform = GetComponentInChildren<SpriteRenderer>().transform;
        anim = GetComponentInChildren<Animator>();

        r_MoveBehaviour = transform.GetChild(1).GetComponentInChildren<AbstractMovementBehaviour>();
        r_AtkBehaviour = transform.GetChild(1).GetComponentInChildren<AbstractAttackBehaviour>();

        a_MoveBehaviour = transform.GetChild(2).GetComponentInChildren<AbstractMovementBehaviour>();
        a_AtkBehaviour = transform.GetChild(2).GetComponentInChildren<AbstractAttackBehaviour>();
    }
    #endregion

    #region REGION - Default Behaviours
    public void OnDefaultAttackBehaviour()
    {
        SetSpeed(0.0f);
    }

    public void OnDefaultMoveBehaviour()
    {
        anim.SetFloat(animParam_maxSpeed, myAIPath.maxSpeed);
    }

    private float lastIndex = 0;
    public void OnDefaultSetMoveAnim()
    {
        float angle = StaticEnemyAnimHandler.GetAngle(GameManager.instance.PlayerTransformRef, transform);
        anim.SetFloat(animParam_animAngle, StaticEnemyAnimHandler.GetIndex(angle, lastIndex));
        StaticEnemyAnimHandler.SetSpriteFlip(mySpriteTransform, angle);
    }

    public void SetTargetAsPlayer()
    {
        if (!myAIDestinationSetter.target == GameManager.instance.PlayerTransformRef)
            myAIDestinationSetter.target = GameManager.instance.PlayerTransformRef;
    }

    public void SetTarget(Transform newTarget)
    {
        myAIDestinationSetter.target = newTarget;
    }
    #endregion

    #region REGION - Utility
    public void SetSpeedAsDefault() // Note : Also used as animator event
    {
        MyAIPath.maxSpeed = maxSpeed;
    }

    public void SetSpeed(float newSpeed)
    {
        MyAIPath.maxSpeed = newSpeed;
    }

    public void SetFiniteStateMachine(BasicEnemy_States transitionTo)
    {
        switch (transitionTo)
        {
            case BasicEnemy_States.ROAMING:
                if (!(currState is EnemyStateRoaming))
                    currState = new EnemyStateRoaming();
                break;
            case BasicEnemy_States.AGGRESSIVE:
                if (!(currState is EnemyStateAgressive))
                    currState = new EnemyStateAgressive();
                break;
        }
    }
    public void ToggleState()
    {
        if (currState is EnemyStateRoaming)
            SetFiniteStateMachine(BasicEnemy_States.AGGRESSIVE);
        else
            SetFiniteStateMachine(BasicEnemy_States.ROAMING);
    }

    public void SetAnimTrigger(BasicEnemy_AnimTriggers trigger)
    {
        switch (trigger)
        {
            case BasicEnemy_AnimTriggers.DEATH:
                anim.SetTrigger(animParam_OnDeath);
                break;
            case BasicEnemy_AnimTriggers.ONHIT:
                anim.SetTrigger(animParam_OnHit);
                break;
            case BasicEnemy_AnimTriggers.ROAMINGATTACK:
                anim.SetTrigger(animParam_OnAtkRoaming);
                break;
            case BasicEnemy_AnimTriggers.AGGRESSIVEATTACK:
                anim.SetTrigger(animParam_OnAtkAggressive);
                break;
            default: Debug.Log($"An error as occured at [SetAnimTrigger()] of [EnemyContext.cs] from enemy: {gameObject.name}"); break;
        }
    }

    public bool IsInAnimationState(BasicEnemy_AnimationStates checkAnimation)
    {
        switch (checkAnimation)
        {
            case BasicEnemy_AnimationStates.IDDLE:
                return anim.GetCurrentAnimatorStateInfo(0).IsName(animState_Iddle);

            case BasicEnemy_AnimationStates.ONAWAKE:
                return anim.GetCurrentAnimatorStateInfo(0).IsName(animState_OnAwake);

            case BasicEnemy_AnimationStates.MOVEMENT:
                return anim.GetCurrentAnimatorStateInfo(0).IsName(animState_OnMoveBlendTree);

            case BasicEnemy_AnimationStates.ROAMINGATTACK:
                return anim.GetCurrentAnimatorStateInfo(0).IsName(animState_OnAtkRoaming);

            case BasicEnemy_AnimationStates.AGGRESSIVEATTACK:
                return anim.GetCurrentAnimatorStateInfo(0).IsName(animState_OnAtkAggressive);

            case BasicEnemy_AnimationStates.DEAD:
                return anim.GetCurrentAnimatorStateInfo(0).IsName(animState_OnDeath);

            default: Debug.Log($"An error as occured at [IsInAnimationState()] of [EnemyContext.cs] from enemy: {gameObject.name}"); break;
        }

        return false;
    }

    public float GetCurrentAnimStateLength()
    {
        return anim.GetCurrentAnimatorStateInfo(0).length;
    }

    private void AE_ExecuteRoamingAttack() // Animator Event
    {
        r_AtkBehaviour.Execute();
    }

    private void AE_ExecuteAggressiveAttack() // Animator Event
    {
        A_AtkBehaviour.Execute();
    }

    #endregion
}
