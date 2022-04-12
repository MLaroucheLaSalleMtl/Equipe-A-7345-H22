using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularProjectileAttackBehaviour : AbstractBehaviour
{
    // SECTION - Field ===================================================================
    [Header("Child Specific")]

    [Header("Position & Rotation")]
    [SerializeField] private bool tracksPlayer = false;
    [Tooltip("Should be the transform of the WeaponManager.cs")]
    [SerializeField] private Transform weaponManagerTransform;
    [Tooltip("Determine the rotation AROUND parent")]
    [Min(1.0f)] [SerializeField] private float desiredAngle = 1.0f;
    [Tooltip("Determine the rotation of CURRENT object\nY value should be > 0")]
    [SerializeField] private Vector3 desiredAxis = new Vector3(0f, 1f, 0f);

    [Space(10)]
    [Header("Modifiers")]
    [Tooltip("Parent.forward * projectileSpeedModifier\nOverride the base projectile speed of projectile [PhysicalProjectile.cs]")]
    [SerializeField] private float projectileSpeedModifier = 0.25f;
    [Space(10)]
    [Tooltip("If true, will override desired angle to fit projectiles into a circle's circumference\n 360 / instantiationQuantity")]
    [SerializeField] private bool isFullCircle = false;
    [Space(10)]
    [SerializeField] private Vector3 instantiate_PosModifier = Vector3.zero;
    [SerializeField] private bool instantiate_MultiplyMinusOne = false;
    [Space(10)]
    [SerializeField] private Vector3 loop_PosModifier = Vector3.zero;
    [SerializeField] private bool loop_MultiplyMinusOne = false;


    [Space(10)]
    [Header("Loop values")]
    [Tooltip("Number of time the instantiation will occur")]
    [Min(1)][SerializeField] private int loopQuantity = 1;
    [Min(0.0f)] [SerializeField] private float loopEverySeconds = 0.5f;

    [Space(10)]
    [Min(1)] [SerializeField] private int instantiationQuantity = 1;
    [Min(0.0f)] [SerializeField] private float instantiateEverySeconds = 0.14f;


    private WeaponManager myWeaponManager;
    private Vector3 oldPosition;
    bool defaultTracksPlayer;

    private float toggleModifier = 1.0f;


    // SECTION - Method - Implementation Specific ===================================================================

    public override void Behaviour()
    {
        OnBehaviourStart();

        StartCoroutine(ExecuteBehaviour());

    }

    public override bool ChildSpecificValidations()
    {
        return true;
    }


    // SECTION - Method - Utility Specific===================================================================
    private void OnBehaviourStart()
    {
        if (weaponManagerTransform == null)
            weaponManagerTransform = GetComponentInParent<Transform>();

        if (weaponManagerTransform == null)
            weaponManagerTransform = GetComponent<Transform>();

        myWeaponManager = myContext.GetCurrentWeaponManager();
        oldPosition = weaponManagerTransform.localPosition;
        defaultTracksPlayer = myWeaponManager.TracksPlayer;
    }

    private IEnumerator ExecuteBehaviour()
    {

        if (!myWeaponManager)
        {
            Debug.Log("Error | [WeaponManager.cs] not found");
            yield break;
        }

        myWeaponManager.TracksPlayer = tracksPlayer;

        for (int count = 0; count != loopQuantity; count++)
        {
            for (int i = 0; i < instantiationQuantity; i++)
            {
                RotateWeapon();

                //myWeaponManager.ShootProjectile(myWeaponManager.Weapon);
                //if (i != 0 || !myContext.HasToken) // Weapon manager instantiate the first bullet
                ShootProjectile(myWeaponManager.Weapon);

                ModifyPosition(instantiate_PosModifier, instantiate_MultiplyMinusOne);

                yield return new WaitForSeconds(instantiateEverySeconds);
            }

            ModifyPosition(loop_PosModifier, loop_MultiplyMinusOne);
            yield return new WaitForSeconds(loopEverySeconds);
        }

        // Can use behaviour now
        ResetValues(true);
        yield return new WaitForSeconds(0.25f);
        myContext.CanUseBehaviour = true;
    }

    private void RotateWeapon()
    {
        //Vector3 axis = new Vector3(0, 25, 0);
        //myTransform.Rotate(desiredRotation);
        float trueAngle = (isFullCircle) ? 360 / instantiationQuantity : desiredAngle;
        weaponManagerTransform.RotateAround(weaponManagerTransform.parent.position, desiredAxis, trueAngle);
    }

    private void ModifyPosition(Vector3 toBeAdded, bool isToggle)
    {
        if (toBeAdded != Vector3.zero)
        {
            toggleModifier = (isToggle) ? toggleModifier * -1 : toggleModifier;
            Vector3 newPosition = weaponManagerTransform.localPosition + toBeAdded * toggleModifier;

            weaponManagerTransform.localPosition = newPosition;
        }
    }

    private void ResetValues(bool isAlsoResetTracksPlayer = false)
    {
        if (isAlsoResetTracksPlayer)
            myWeaponManager.TracksPlayer = defaultTracksPlayer;

        weaponManagerTransform.localPosition = oldPosition;
        weaponManagerTransform.localRotation = Quaternion.Euler(Vector3.zero);

        toggleModifier = 1.0f;
    }


    // Copy from [WeaponManager.cs]
    public void ShootProjectile(WeaponSO weapon)
    {
        var newProjectile = Instantiate(weapon.Projectile, transform);
        //newProjectile.MyRigidbody.velocity = Vector3.zero;
        newProjectile.MyRigidbody.velocity = weaponManagerTransform.forward * projectileSpeedModifier;
        newProjectile.transform.parent = null;
    }   
}