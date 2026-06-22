using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game2D.Combat
{
    public sealed class AttackHitbox2D : MonoBehaviour
    {
        [SerializeField] private Collider2D hitboxCollider;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private bool disableColliderOnAwake = true;

        private readonly HashSet<IDamageable> hitTargets = new();
        private DamagePayload currentPayload;
        private Coroutine activeRoutine;

        private void Reset()
        {
            hitboxCollider = GetComponent<Collider2D>();
        }

        private void Awake()
        {
            if (hitboxCollider == null)
                hitboxCollider = GetComponent<Collider2D>();

            if (hitboxCollider != null)
                hitboxCollider.isTrigger = true;

            if (disableColliderOnAwake)
                SetActive(false);
        }

        public void Activate(DamagePayload payload, float duration)
        {
            currentPayload = payload;
            hitTargets.Clear();

            if (activeRoutine != null)
                StopCoroutine(activeRoutine);

            activeRoutine = StartCoroutine(ActiveRoutine(duration));
        }

        public void Deactivate()
        {
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
                activeRoutine = null;
            }

            SetActive(false);
        }

        private IEnumerator ActiveRoutine(float duration)
        {
            SetActive(true);
            yield return new WaitForSeconds(Mathf.Max(0.01f, duration));
            SetActive(false);
            activeRoutine = null;
        }

        private void SetActive(bool active)
        {
            if (hitboxCollider != null)
                hitboxCollider.enabled = active;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryHit(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryHit(other);
        }

        private void TryHit(Collider2D other)
        {
            if (!IsInTargetLayer(other.gameObject.layer))
                return;

            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null || hitTargets.Contains(damageable))
                return;

            hitTargets.Add(damageable);
            damageable.TakeDamage(currentPayload);
        }

        private bool IsInTargetLayer(int layer)
        {
            return (targetLayers.value & (1 << layer)) != 0;
        }
    }
}
