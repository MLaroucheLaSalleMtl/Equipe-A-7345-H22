using UnityEngine;
using UnityEngine.Events;

public class PlayerContext : MonoBehaviour
{
    // SECTION - Field ===================================================================
    private IPlayerState currState;
    private IPlayerState oldState;

    [Header("Living Entity")]
    [SerializeField] private LivingEntityContext livingEntityContext;

    [Header("Raycast")]
    [SerializeField] private float distanceGround = 0.55f;
    [SerializeField] private float distanceInteractible = 0.75f;
    private bool isDebugOn = false;

    [Header("Weapons")]
    // CURRENT WEAPON & MORE GO HERE
    [SerializeField] private WeaponsInventorySO weapons;
    [SerializeField] private WeaponManager meleeWeapon;
    [SerializeField] private WeaponManager mainWeapon;
    [SerializeField] private WeaponManager secondaryWeapon;
    [SerializeField] private TransformSO playerTransform;
    [SerializeField] private PositionRotationSO lastSpawnPositionRotation;

    [Header("Rigidbody & Colliders")]
    [SerializeField] private Rigidbody rb;

    [Header("Scriptables")]
    [SerializeField] private PlayerInputSO input;

    [Header("Animator")]
    [SerializeField] private Animator anim;

    [Header("Canvases")]
    [SerializeField] private InteractCanvasHandler interactCanvasHandler;

    [Header("Events")]
    [SerializeField] private UnityEvent mainWeaponHasChanged;


    // SECTION - Property ===================================================================
    #region REGION - PROPERTY
    public LivingEntityContext LivingEntityContext { get => livingEntityContext; set => livingEntityContext = value; }

    public float DistanceGround { get => distanceGround; }
    public float DistanceInteractible { get => distanceInteractible; }
    public bool IsDebugOn { get => isDebugOn; set => isDebugOn = value; }

    public WeaponsInventorySO Weapons { get => weapons; set => weapons = value; }
    public WeaponManager MeleeWeapon { get => meleeWeapon; set => meleeWeapon = value; }
    public WeaponManager MainWeapon { get => mainWeapon; set => mainWeapon = value; }
    public WeaponManager SecondaryWeapon { get => secondaryWeapon; set => secondaryWeapon = value; }
    public TransformSO PlayerTransform { get => playerTransform; set => playerTransform = value; }
    public PositionRotationSO LastSpawnPositionRotation { get => lastSpawnPositionRotation; set => lastSpawnPositionRotation = value; }

    public Rigidbody Rb { get => rb; set => rb = value; }

    public PlayerInputSO Input { get => input; set => input = value; }

    public Animator Anim { get => anim; set => anim = value; }
    
    public UnityEvent MainWeaponHasChanged { get => mainWeaponHasChanged; set => mainWeaponHasChanged = value; }

    public InteractCanvasHandler InteractCanvasHandler { get => interactCanvasHandler; set => interactCanvasHandler = value; }
    #endregion

    private void Awake()
    {
        playerTransform.Transform = transform;
    }

    // SECTION - Method - Unity ===================================================================
    private void Start()
    {
        currState = new PlayerStateGrounded();
        oldState = currState;

        // TO BE DELETED
        // livingEntityContext.FullHeal();
        // TO BE MOVED
        meleeWeapon.Weapon = weapons.EquippedMeleeWeapon;
        mainWeapon.Weapon = weapons.EquippedMainWeapon;
        secondaryWeapon.Weapon = weapons.EquippedSecondaryWeapon;
    }

    private void Update()
    {
        if (oldState != currState)
        {
            oldState = currState;
            OnStateEnter();
        }

        OnStateUpdate();
        OnStateExit();  
    }


    // SECTION - Method - State Machine ===================================================================
    public void OnStateEnter()
    {
        currState.OnStateEnter(this);
    }

    public void OnStateUpdate()
    {
        // Added to have player position known to everything
        currState.OnStateUpdate(this);
    }

    public void OnStateExit()
    {
        currState = currState.OnStateExit(this);
    }


    // SECTION - Method - Utility ===================================================================
    // NOTE : Decouple in its own script?
    public RaycastHit TryRayCastGround() // Only purpose is to aleviate eye bleeding
    {
        return StaticRayCaster.IsLineCastTouching(transform.position, -transform.up, DistanceGround, GameManager.instance.groundMask, IsDebugOn);
    }

    public RaycastHit TryRayCastRespawn() // Only purpose is to aleviate eye bleeding
    {
        return StaticRayCaster.IsLineCastTouching(transform.position, -transform.up, DistanceGround, GameManager.instance.respawnMask, IsDebugOn);
    }

    public RaycastHit TryRayCastInteractable() // Only purpose is to aleviate eye bleeding
    {
        return StaticRayCaster.IsLineCastTouching(transform.position, transform.forward, distanceInteractible, GameManager.instance.interactableMask, isDebugOn);
    }

    #region REGION - Movement
    public void OnDefaultMovement(float stateDependantModifier = 1.0f)
    {
        float moveX = input.DirX * input.MoveFactor.Value;
        float moveZ = input.DirZ * input.MoveFactor.Value;

        Vector3 movement = (transform.right * moveX +
                            transform.up * rb.velocity.y +
                            transform.forward * moveZ) *
                            stateDependantModifier;

        rb.velocity = movement;
    }

    public void OnDefaultLook()
    {
        float lookY = input.LookY * input.MouseSensitivity.Value;

        Vector3 rotationValues = Vector3.up * lookY;

        transform.Rotate(rotationValues);
    }
    #endregion

    #region REGION - Weapon
    public void OnDefaultFireWeaponMelee()
    {
        if (input.FireMeleeWeapon)
        {
            if (!meleeWeapon.Weapon.CanFireContinuously)
            {
                input.FireMeleeWeapon = false;
            }

            meleeWeapon.TriggerWeapon();
        }
    }

    public void OnDefaultFireWeaponMain()
    {
        if (input.FireMainWeapon)
        {
            if (!mainWeapon.Weapon.CanBeCharged)
            {
                if (!mainWeapon.Weapon.CanFireContinuously || mainWeapon.Weapon.CurrentClip == 0)
                {
                    input.FireMainWeapon = false;
                }

                mainWeapon.TriggerWeapon();
            }
            else
            {

            }
        }
    }

    public void OnDefaultFireWeaponSecondary()
    {
        if (input.FireSecondaryWeapon)
        {
            input.FireSecondaryWeapon = false;

            secondaryWeapon.TriggerWeapon();
        }
    }

    public void OnDefaultWeaponChange()
    {
        if (input.WeaponOne)            // WEAPON ONE
        {
            input.WeaponOne = false;

            // EVENT GO HERE
            mainWeapon.ResetReload();

            weapons.EquippedMainWeapon = weapons.CarriedMainWeapons[0];
            mainWeapon.UpdateWeapon(); // weapons.EquippedMainWeapon
            // weaponHolder.MainWeapon = weapons.EquippedMainWeapon;
            StaticDebugger.SimpleDebugger(isDebugOn, $"MAIN WEAPON CHANGED TO ... {weapons.EquippedMainWeapon.WeaponName}");
            mainWeaponHasChanged.Invoke();
        }
        else if (input.WeaponTwo)       // WEAPON TWO
        {
            input.WeaponTwo = false;

            mainWeapon.ResetReload();

            // EVENT GO HERE
            if (weapons.CarriedMainWeapons.Count > 1)
            {
                weapons.EquippedMainWeapon = weapons.CarriedMainWeapons[1];
                mainWeapon.UpdateWeapon(); // weapons.EquippedMainWeapon
                // weaponHolder.MainWeapon = weapons.EquippedMainWeapon;
                StaticDebugger.SimpleDebugger(IsDebugOn, $"MAIN WEAPON CHANGED TO ... {weapons.EquippedMainWeapon.WeaponName}");
                mainWeaponHasChanged.Invoke();
            }
        }
        else if (input.WeaponScrollBackward)       // WEAPON SCROLL <=
        {
            input.WeaponScrollBackward = false;

            mainWeapon.ResetReload();

            // EVENT GO HERE
            if (weapons.CarriedMainWeapons.Count > 1)
            {
                var index = weapons.CarriedMainWeapons.IndexOf(weapons.EquippedMainWeapon) - 1;
                if (index < 0)
                    index = weapons.CarriedMainWeapons.Count - 1;
                weapons.EquippedMainWeapon = weapons.CarriedMainWeapons[index];
                mainWeapon.UpdateWeapon(); // weapons.EquippedMainWeapon 
                // weaponHolder.MainWeapon = weapons.EquippedMainWeapon;
                StaticDebugger.SimpleDebugger(IsDebugOn, $"MAIN WEAPON CHANGED TO ... {weapons.EquippedMainWeapon.WeaponName}");
                mainWeaponHasChanged.Invoke();
            }
        }
        else if (input.WeaponScrollForward)       // WEAPON SCROLL =>
        {
            input.WeaponScrollForward = false;

            mainWeapon.ResetReload();

            // EVENT GO HERE
            if (weapons.CarriedMainWeapons.Count > 1)
            {
                var index = weapons.CarriedMainWeapons.IndexOf(weapons.EquippedMainWeapon) + 1;
                if (index > weapons.CarriedMainWeapons.Count - 1)
                    index = 0;
                weapons.EquippedMainWeapon = weapons.CarriedMainWeapons[index];
                mainWeapon.UpdateWeapon(); // weapons.EquippedMainWeapon
                // weaponHolder.MainWeapon = weapons.EquippedMainWeapon;
                StaticDebugger.SimpleDebugger(IsDebugOn, $"MAIN WEAPON CHANGED TO ... {weapons.EquippedMainWeapon.WeaponName}");
                mainWeaponHasChanged.Invoke();
            }
        }
    }

    public void OnWeaponReload()
    {
        if (input.Reload)
        {
            input.Reload = false;

            // EVENT GO HERE
            mainWeapon.ReloadWeapon();
        }
    }
    #endregion

    #region REGION - Misc
    public void OnDefaultInteract(RaycastHit hit)
    {
        if (input.Interact)
        {
            input.Interact = false;

            if (hit.transform != null)
            {
                Debug.Log($"hit name is : {hit.transform.name}");
                Interactable interactable = hit.transform.GetComponentInChildren<Interactable>();

                // Canvas visual cue
                interactCanvasHandler.SetVisualCue(interactable.IsInteractable);

                // Event launch
                interactable.OnInteraction();
            }
        }
    }

    public void OnDefaultShowMap()
    {
        if (input.ShowMap)
        {
            input.ShowMap = false;

            // EVENT GO HERE
            FindObjectOfType<Minimap>().ToggleMap();
        }
    }
    #endregion
}