using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Targeting")]
    public Transform player;
    public float detectionRange = 8f;
    public float rotationSpeed = 5f;

    [Header("Shooting")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float fireRate = 1f;
    private float fireTimer;

    [Header("Turret Part (optional, if separate from base)")]
    public Transform turretHead; // rotates independently, leave null to rotate whole object

    private bool playerInRange;

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        playerInRange = distance <= detectionRange;

        if (playerInRange)
        {
            AimAtPlayer();

            fireTimer += Time.deltaTime;
            if (fireTimer >= fireRate)
            {
                Shoot();
                fireTimer = 0f;
            }
        }
    }

    void AimAtPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Transform target = turretHead != null ? turretHead : transform;

        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        target.rotation = Quaternion.Slerp(target.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

   void Shoot()
{
    if (bulletPrefab == null || firePoint == null) return;

    // firePoint.position/rotation already reflects the pivot's rotation
    // since it's a child of the rotated barrel
    GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
}

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}