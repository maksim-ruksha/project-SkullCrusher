using System;
using System.Collections.Generic;
using Damages.Classes;
using UnityEngine;
using UnityEngine.Events;

namespace Damages
{
    public class DamageableObject : MonoBehaviour
    {
        public float health = 100.0f;
        public DamageableGroup defaultDamageableGroup;

        public IDamageModificator damageModificator;
        
        public UnityEvent onDamageTookEvent;
        public UnityEvent onDiedEvent;
        
        public delegate void DamageEvent(Damage damage, DamageableObjectPart part);
        // will be used to organize special reactions
        // depending on damage type, amount and damaged part
        // like CUv4's ignition and explosion from laser
        public DamageEvent onDamageTookDamageEvent;
        public DamageEvent onDiedDamageEvent;
        
        private List<DamageableObjectPart> parts;

        private float initialHealth;

        private void Start()
        {
            initialHealth = health;
            AssignParts(transform);
            if (!defaultDamageableGroup)
            {
                UnityEngine.Debug.LogError("No default damageable group was specified", this);
            }
        }

        public void TakeDamage(Damage damage, DamageableObjectPart part)
        {
            if(!IsAlive())
                return;
            
            float finalDamageAmount = damage.amount * part.damageableGroup.damageMultiplier;

            health -= finalDamageAmount;
            onDamageTookEvent.Invoke();
            onDamageTookDamageEvent.Invoke(damage, part);
            
            if (!IsAlive())
            {
                Die();
                onDiedEvent.Invoke();
                onDiedDamageEvent.Invoke(damage, part);
            }
            
        }

        public float GetRangedHealth()
        {
            return health / initialHealth;
        }

        public bool IsAlive()
        {
            return health <= 0.0f;
        }

        public int GetHealth()
        {
            return (int) Math.Round(health + 0.4f);
        }

        public float GetRawHealth()
        {
            return health;
        }

        private void AssignParts(Transform t)
        {
            DamageableObjectPart component = t.GetComponent<DamageableObjectPart>();

            if (component)
            {
                if (!component.damageableGroup)
                {
                    component.damageableGroup = defaultDamageableGroup;
                }

                component.SetMainObject(this);
                parts.Add(component);
            }

            for (int i = 0; i < t.childCount; i++)
            {
                AssignParts(t.GetChild(i));
            }
        }

        private void Die()
        {
            // TODO: make death manager
            // but for now just destroy object
            print(name + " died");
            Destroy(gameObject);
        }
    }
}