﻿using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MoveController : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private PositionAnchor positionAnchor;
    [SerializeField] private Animator animator;

    [Header("Target Movement")]
    [SerializeField] private float distanceThreshold = 2f;

    [Header("Random Movement")]
    [SerializeField] private bool startIdle;
    [SerializeField] private float minIdleDuration = 2f;
    [SerializeField] private float maxIdleDuration = 5f;

    [Header("Radial Movement")]
    [SerializeField] private float radius = 4f;
    [SerializeField] private float radialSpeed = 0.25f;

    private enum MovementType
    {
        TARGET,
        DIRECTION,
        POSITION,
        RADIAL
    }

    private MovementType type;
    private Coroutine positionReachedRoutine;
    private bool isIdle;
    private float idleTimer;

    private Transform target;
    private Vector3 direction;
    private Vector3 position;
    private Vector3 origin;
    private float angle;

    public event UnityAction OnStart;
    public event UnityAction OnEnd;
    public event UnityAction<Vector3> OnUpdate;

    public float Speed => speed;
    public float DistanceThreshold => distanceThreshold;

    public bool IsAtDestination => Vector3.Distance(transform.position, TargetPosition) < 0.1f;

    public Vector3 TargetPosition => type switch
    {
        MovementType.TARGET => GetTargetPosition(),
        MovementType.DIRECTION => transform.position + direction,
        MovementType.RADIAL => GetRadialPosition(),
        _ => position,
    };

    #region Public Methods
    public void SetTarget(Transform target)
    {
        this.target = target;
        SetType(MovementType.TARGET);
    }

    public void SetDirection(Vector3 direction)
    {
        this.direction = direction;
        SetType(MovementType.DIRECTION);
    }

    public void SetPosition(Vector3 position, UnityAction onComplete = null)
    {
        this.position = position;
        SetType(MovementType.POSITION);

        OnStart?.Invoke();
        positionReachedRoutine = StartCoroutine(WaitUntilDestinationReached(onComplete));
    }

    public void StartRadialMovement(Vector3 origin)
    {
        this.origin = origin;
        SetType(MovementType.RADIAL);
    }

    private void SetType(MovementType type)
    {
        this.type = type;
        isIdle = false;
        if (positionReachedRoutine != null) StopCoroutine(positionReachedRoutine);
    }

    public void StartRandomMovement()
    {
        if (startIdle) StartIdle();
        else SetPosition(positionAnchor.Position, StartIdle);
    }

    public void Stop()
    {
        SetPosition(transform.position);
    }

    public void SetAnimator(Animator animator)
    {
        this.animator = animator;
    }

    public void SetBounds(Collider collider)
    {
        positionAnchor.Bounds = collider;
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    public void SetLookTarget(Transform target)
    {
        var direction = target.position - transform.position;
        direction.y = 0;
        transform.forward = direction;
    }

    public void SetDistanceThreshold(float distanceThreshold)
    {
        this.distanceThreshold = distanceThreshold;
    }
    #endregion

    private void Awake()
    {
        Stop();
    }

    private void Update()
    {
        if (isIdle) UpdateIdle();
        else UpdatePosition();
    }

    private Vector3 GetTargetPosition()
    {
        var direction = (target.position - transform.position);
        return target.position - direction.normalized * distanceThreshold;
    }

    private Vector3 GetRadialPosition()
    {
        angle += Time.deltaTime * radialSpeed;
        var x = Mathf.Cos(angle);
        var z = Mathf.Sin(angle);
        var direction = new Vector3(x, 0, z) * radius;
        return origin + direction;
    }

    private void UpdatePosition()
    {
        if (IsAtDestination) return;

        if (animator) animator.SetBool("isMoving", !IsAtDestination);

        direction = (TargetPosition - transform.position).normalized;


        float angle = Vector3.Angle(transform.forward, direction);
        if (angle < Mathf.PI) transform.position += speed * Time.deltaTime * direction;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 500 * Time.deltaTime);
        }

        OnUpdate?.Invoke(direction);
    }

    private IEnumerator WaitUntilDestinationReached(UnityAction onComplete)
    {
        yield return new WaitUntil(() => IsAtDestination);
        onComplete?.Invoke();
        OnEnd?.Invoke();
    }

    private void StartIdle()
    {
        idleTimer = Random.Range(0f, maxIdleDuration - minIdleDuration);
        isIdle = true;
    }

    private void UpdateIdle()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer > maxIdleDuration) SetPosition(positionAnchor.Position, StartIdle);
    }
}
