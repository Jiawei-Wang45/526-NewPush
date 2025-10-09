using UnityEngine;
using Pathfinding;
using System.Collections; // ��Ҫ���룬��ʹ��Э��
using System.Collections.Generic; // ��Ҫ���룬��ʹ��List

public class EnemyController : MonoBehaviour
{
    public PlayerControllerTest pcTest;
    public EnemyStats enemyStats;
    public Rigidbody2D rb;
    public GameManager gameManager;
    public GameObject BoundHealthbar;
    public float RotationSpeed = 15.0f;
    public float enemySpeed;

    // --- Ѱ·��ر��� ---
    [Header("Pathfinding")]
    public float nextWaypointDistance = 1f; // ����·����ж�����
    public float pathUpdateInterval = 0.5f; // ·�����µ�Ƶ�ʣ��룩

    private Seeker mySeeker;
    private Path path;
    private int currentWaypoint = 0;
    private bool reachedEndOfPath = false;
    private bool isAttacking;

    private void Start()
    {
        mySeeker = GetComponent<Seeker>();
        pcTest = FindFirstObjectByType<PlayerControllerTest>();
        enemyStats = GetComponent<EnemyStats>();
        rb = GetComponent<Rigidbody2D>();
        gameManager = FindFirstObjectByType<GameManager>();
        RefreshStats();

        // ����һ��Э���������Եظ���Ѱ··��
        StartCoroutine(UpdatePath());
    }

    /// <summary>
    /// �����Ը���·����Э��
    /// </summary>
    IEnumerator UpdatePath()
    {
        // ȷ��Seeker�Ѿ�׼����
        if (mySeeker.IsDone())
        {
            mySeeker.StartPath(rb.position, pcTest.transform.position, OnPathComplete);
        }

        // ����ѭ���������趨�ļ��ʱ���ظ�ִ��
        while (true)
        {
            yield return new WaitForSeconds(pathUpdateInterval);
            if (mySeeker.IsDone() && gameManager.isPlayerAlive)
            {
                mySeeker.StartPath(rb.position, pcTest.transform.position, OnPathComplete);
            }
        }
    }

    /// <summary>
    /// ��·���������ʱ�����õĻص�����
    /// </summary>
    public void OnPathComplete(Path p)
    {
        // ���·������û�г���
        if (!p.error)
        {
            path = p;
            // ���õ�ǰ·������������·������㿪ʼ
            currentWaypoint = 0;
        }
    }

    public void RefreshStats()
    {
        enemySpeed = enemyStats.enemyMovementSpeed;
    }

    private void Update()
    {
        if (gameManager.isPlayerAlive)
        {
            // �ⲿ�ֱ��ֲ��䣬�õ���ʼ�ճ������
            Vector2 direction = (Vector2)pcTest.transform.position - rb.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);

            // �����߼����ֲ���
            if (isAttacking)
            {
                pcTest.TakeDamage(enemyStats.enemyDamage);
            }
        }
    }

    private void FixedUpdate()
    {
        if (!gameManager.isPlayerAlive)
        {
            rb.linearVelocity = Vector2.zero; // �������ʱֹͣ�ƶ�
            return;
        }

        // --- ����Ѱ·�ƶ��߼� ---
        if (path == null)
        {
            // �����û�м�����κ�·�����򲻽����κβ���
            return;
        }

        // ����Ƿ��Ѿ�����·�����յ�
        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            rb.linearVelocity = Vector2.zero; // ���յ㴦ֹͣ
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        // ���㳯��ǰ·��ķ�������
        // ע�⣺path.vectorPath�еĵ���Vector3����Ҫת��ΪVector2
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;

        // ���ݷ�����ٶ��ƶ�����
        rb.linearVelocity = direction * enemySpeed;

        // �����뵱ǰ·��ľ���
        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        // ����뵱ǰ·��ľ���С���ж����룬���ƶ�����һ��·��
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerControllerTest>(out PlayerControllerTest pcScript))
        {
            isAttacking = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerControllerTest>(out PlayerControllerTest pcScript))
        {
            isAttacking = false;
        }
    }
}