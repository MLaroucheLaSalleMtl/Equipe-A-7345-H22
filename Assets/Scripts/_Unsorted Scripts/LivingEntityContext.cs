using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LivingEntityContext : MonoBehaviour
{
    // SECTION - Field =========================================================
    [Header("Health")]
    [SerializeField] private FloatReference maxHP;
    [SerializeField] private FloatReference currentHP;

    [Header("Animator")]
    [SerializeField] private Animator anim;
    [SerializeField] private string deathAnimStr;
    [SerializeField] private string takeDmgAnimStr;

    [Header("Events")]
    [SerializeField] private UnityEvent onTakeDamageEvents;
    [SerializeField] private UnityEvent onDeathEvents;


    // SECTION - Property =========================================================
    public bool IsDead { get => currentHP.Value <= 0.0f; }


    // SECTION - Method - Unity Specific =========================================================
    private void Start()
    {
        FullHeal();
    }


    // SECTION - Method - Context Specific =========================================================
    public void FullHeal()
    {
        currentHP.Value = maxHP.Value;
    }

    public void TakeDamage(float damage)
    {
        if (currentHP.Value > 0.0f)
        {
            currentHP.Value -= damage;

            // On Death
            if (IsDead)
            {
                OnDeathBaseHandler(); // Placed here to avoid manual storing in event
                if (onDeathEvents != null)
                    onDeathEvents.Invoke();
            }
            // On Simple Damage
            else
            {
                OnTakeDamageBaseHandler(); // Placed here to avoid manual storing in event
                if (onTakeDamageEvents != null)
                    onTakeDamageEvents.Invoke();
            }
        }
    }


    // SECTION - Method - Utility Specific =========================================================
    private void OnDeathBaseHandler() 
    {
        // Animator
        if (anim != null && deathAnimStr != "")
            anim.SetTrigger(deathAnimStr);

        // Extend default behaviours on death here
    }

    private void OnTakeDamageBaseHandler()
    {
        // Animator
        if (anim != null && takeDmgAnimStr != "")
            anim.SetTrigger(takeDmgAnimStr);

        // Extend default behaviours on take damage here
    }

    private void AEDestroyGameObject_AtEndAnim() // Animator Event
    {
        transform.parent.GetComponent<Room>().FinishRoom();
        Destroy(gameObject);
    }
}
