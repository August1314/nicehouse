using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 简单的随机漫步（NavMesh）行为，让角色在房间内自行走动。
/// 将脚本挂在带 NavMeshAgent 的角色上，并保证场景已烘焙 NavMesh。
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class AIWander : MonoBehaviour
{
    [Header("动画驱动（可选）")]
    [Tooltip("用于驱动行走动画的 Animator。若为空则不做动画更新。")]
    public Animator animator;
    [Tooltip("速度参数名（Animator 内的 float）")]
    public string speedParam = "Speed";
    [Tooltip("速度参数的平滑时间")]
    public float speedDampTime = 0.05f;

    [Header("漫步范围")]
    [Tooltip("以当前位置为圆心的随机半径")]
    public float wanderRadius = 6f;

    [Tooltip("在抵达后等待多久再找下一个点")]
    public float waitTime = 1.0f;

    [Tooltip("到达判定的容差，越大越容易触发下一次寻路")]
    public float arriveTolerance = 0.3f;

    [Tooltip("为找到可行走点允许的采样半径")]
    public float sampleRadius = 2.0f;

    [Tooltip("最多尝试几次随机点采样")]
    public int maxSamples = 6;

    private NavMeshAgent _agent;
    private bool _isWaiting;
    private int _speedHash;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (!string.IsNullOrEmpty(speedParam))
        {
            _speedHash = Animator.StringToHash(speedParam);
        }
    }

    private void OnEnable()
    {
        _isWaiting = false;
        TrySetNextDestination();
    }

    private void Update()
    {
        if (_agent == null || _agent.pathPending) return;
        if (_isWaiting) return;

        UpdateAnimator();

        if (_agent.remainingDistance <= _agent.stoppingDistance + arriveTolerance)
        {
            StartCoroutine(WaitAndPickNext());
        }
    }

    private IEnumerator WaitAndPickNext()
    {
        _isWaiting = true;
        if (waitTime > 0f)
        {
            yield return new WaitForSeconds(waitTime);
        }
        TrySetNextDestination();
        _isWaiting = false;
    }

    private void TrySetNextDestination()
    {
        Vector3 dest;
        if (FindRandomPoint(out dest))
        {
            _agent.SetDestination(dest);
        }
    }

    private bool FindRandomPoint(out Vector3 result)
    {
        for (int i = 0; i < maxSamples; i++)
        {
            Vector3 random = Random.insideUnitSphere * wanderRadius;
            random.y = 0f;
            Vector3 candidate = transform.position + random;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = transform.position;
        return false;
    }

    private void UpdateAnimator()
    {
        if (animator == null || _speedHash == 0) return;

        float planarSpeed = new Vector3(_agent.velocity.x, 0f, _agent.velocity.z).magnitude;
        animator.SetFloat(_speedHash, planarSpeed, speedDampTime, Time.deltaTime);
    }
}

