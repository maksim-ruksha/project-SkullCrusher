using System;
using Damages;
using Damages.Enums;
using UnityEngine;
using Violence;

namespace Debug
{
    public class DamagePoint : MonoBehaviour
    {

        public float radius = 0.25f;
        public float amount = 1.5f;
        public DamageType type;
        public ViolenceableObject violenceableObject;


        private void Update()
        {
            if (gameObject.activeInHierarchy)
            {
                violenceableObject.AddDamage(transform.position, new Damage(amount, type), radius);
                gameObject.SetActive(false);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}