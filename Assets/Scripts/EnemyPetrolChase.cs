using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyPatrolChase : MonoBehaviour
{
    public Transform[] patrolPoints;
    public Transform player;

    public float patrolSpeed = 2.5f;
    public float chaseSpeed = 4.5f;
    public float detectionRange = 8f;
    public float loseSightRange = 12f;
    public float catchDistance = 1.5f;

    [Header("Wall Avoidance & Unstuck")]
    public float wallCheckDistance = 0.85f;
    public float stuckCheckTime = 0.8f;

    [Header("Safe Zone Boundary")]
    public bool restrictToPatrolZone = true;
    public float zonePadding = 0.5f;

    private int currentPointIndex = 0;
    private bool isChasing = false;

    private float minX, maxX, minZ, maxZ;
    private bool hasBounds = false;

    private Vector3 lastCheckedPos;
    private float stuckTimer = 0f;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        lastCheckedPos = transform.position;
        CalculatePatrolBounds();
    }

    void CalculatePatrolBounds()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        minX = patrolPoints[0].position.x;
        maxX = patrolPoints[0].position.x;
        minZ = patrolPoints[0].position.z;
        maxZ = patrolPoints[0].position.z;

        for (int i = 1; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null) continue;
            Vector3 pos = patrolPoints[i].position;

            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.z < minZ) minZ = pos.z;
            if (pos.z > maxZ) maxZ = pos.z;
        }

        minX -= zonePadding;
        maxX += zonePadding;
        minZ -= zonePadding;
        maxZ += zonePadding;
        hasBounds = true;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool playerInsideZone = !restrictToPatrolZone || !hasBounds || IsPointInsideZone(player.position);

        if (!isChasing && distanceToPlayer <= detectionRange && playerInsideZone)
        {
            isChasing = true;
        }
        else if (isChasing && (distanceToPlayer > loseSightRange || !playerInsideZone))
        {
            isChasing = false;
        }

        if (isChasing)
        {
            ChasePlayer(distanceToPlayer);
        }
        else
        {
            Patrol();
        }

        CheckIfStuck();
    }

    void CheckIfStuck()
    {
        float moved = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(lastCheckedPos.x, lastCheckedPos.z)
        );

        if (moved < 0.08f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= stuckCheckTime)
            {
                stuckTimer = 0f;
                lastCheckedPos = transform.position;

                // Agar patrol kar raha hai to agle point par jao, warna thoda side turn lo
                if (!isChasing && patrolPoints != null && patrolPoints.Length > 0)
                {
                    currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
                }
                else
                {
                    transform.Rotate(0f, 90f, 0f);
                }
            }
        }
        else
        {
            stuckTimer = 0f;
            lastCheckedPos = transform.position;
        }
    }

    bool IsBlockedAt(Vector3 dir, float distance)
    {
        Vector3 flatDir = new Vector3(dir.x, 0f, dir.z).normalized;
        if (flatDir.sqrMagnitude < 0.001f) return false;

        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        RaycastHit hit;

        if (Physics.Raycast(rayOrigin, flatDir, out hit, distance))
        {
            if (hit.collider.CompareTag("Player")) return false;
            return true;
        }
        return false;
    }

    void MoveWithWallSlide(Vector3 targetPos, float speed)
    {
        Vector3 desiredDir = targetPos - transform.position;
        desiredDir.y = 0f;
        if (desiredDir.sqrMagnitude < 0.001f) return;

        Vector3 moveDir = desiredDir.normalized;

        if (IsBlockedAt(moveDir, wallCheckDistance))
        {
            Vector3 rightDir = Quaternion.Euler(0f, 90f, 0f) * moveDir;
            Vector3 leftDir = Quaternion.Euler(0f, -90f, 0f) * moveDir;

            bool rightClear = !IsBlockedAt(rightDir, wallCheckDistance);
            bool leftClear = !IsBlockedAt(leftDir, wallCheckDistance);

            if (rightClear && leftClear)
            {
                float rightDist = Vector3.Distance(transform.position + rightDir, targetPos);
                float leftDist = Vector3.Distance(transform.position + leftDir, targetPos);
                moveDir = (rightDist < leftDist) ? rightDir : leftDir;
            }
            else if (rightClear)
            {
                moveDir = rightDir;
            }
            else if (leftClear)
            {
                moveDir = leftDir;
            }
            else
            {
                LookAtFlat(transform.position + desiredDir);
                return;
            }
        }

        float stepDistance = speed * Time.deltaTime;
        if (!IsBlockedAt(moveDir, stepDistance + 0.25f))
        {
            Vector3 nextPos = transform.position + moveDir * stepDistance;

            if (restrictToPatrolZone && hasBounds)
            {
                nextPos.x = Mathf.Clamp(nextPos.x, minX, maxX);
                nextPos.z = Mathf.Clamp(nextPos.z, minZ, maxZ);
            }

            Vector3 clampDelta = nextPos - transform.position;
            clampDelta.y = 0f;

            if (clampDelta.sqrMagnitude > 0.00001f && !IsBlockedAt(clampDelta.normalized, clampDelta.magnitude + 0.2f))
            {
                nextPos.y = transform.position.y;
                transform.position = nextPos;
            }
        }

        LookAtFlat(transform.position + moveDir);
    }

    bool IsPointInsideZone(Vector3 worldPos)
    {
        return worldPos.x >= minX && worldPos.x <= maxX &&
               worldPos.z >= minZ && worldPos.z <= maxZ;
    }

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPointIndex];
        if (targetPoint == null) return;

        Vector3 targetPos = new Vector3(targetPoint.position.x, transform.position.y, targetPoint.position.z);
        MoveWithWallSlide(targetPos, patrolSpeed);

        float flatDist = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(targetPos.x, targetPos.z)
        );

        if (flatDist < 0.45f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
    }

    void ChasePlayer(float distanceToPlayer)
    {
        Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        MoveWithWallSlide(targetPos, chaseSpeed);

        if (distanceToPlayer <= catchDistance)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void LookAtFlat(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        float xMin = patrolPoints[0].position.x, xMax = patrolPoints[0].position.x;
        float zMin = patrolPoints[0].position.z, zMax = patrolPoints[0].position.z;

        for (int i = 1; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null) continue;
            xMin = Mathf.Min(xMin, patrolPoints[i].position.x);
            xMax = Mathf.Max(xMax, patrolPoints[i].position.x);
            zMin = Mathf.Min(zMin, patrolPoints[i].position.z);
            zMax = Mathf.Max(zMax, patrolPoints[i].position.z);
        }

        xMin -= zonePadding; xMax += zonePadding;
        zMin -= zonePadding; zMax += zonePadding;

        Vector3 center = new Vector3((xMin + xMax) * 0.5f, transform.position.y, (zMin + zMax) * 0.5f);
        Vector3 size = new Vector3(xMax - xMin, 2f, zMax - zMin);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, size);
    }
}