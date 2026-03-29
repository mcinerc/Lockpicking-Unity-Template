using System;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public enum TumblerState { Idle, Rising, Pickable, Falling, Set }

public class Tumbler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform spring;
    [SerializeField] private Transform tumbler;
    [SerializeField] private Transform tumblerTarget;

    [Header("Settings")]
    [SerializeField] private float springMinSize = 0.1f;
    
    // how fast the tumblers move up when picked
    [SerializeField] private float minRaiseTime = 0.1f;
    [SerializeField] private float maxRaiseTime = 0.5f;
    
    // how long the tumblers stay up and in a pickable state
    [SerializeField] private float minPickableTime = 0.1f;
    [SerializeField] private float maxPickableTime = 0.5f;
    
    // how long it takes the tumblers to return to neutral
    [SerializeField] private float minFallingTime = 0.1f;
    [SerializeField] private float maxFallingTime = 0.5f;

    private TumblerState _state = TumblerState.Idle;
    private float _timer = 0f; // timer used for all state transitions
    private float _target = 0f; // target time used for all state transitions
    private Vector3 _targetSize;

    private void Awake()
    {
        _targetSize = new Vector3(1f, springMinSize, 1f);
        Reset();
    }

    private void Update()
    {
        tumbler.position = tumblerTarget.position;
        
        if (_state == TumblerState.Rising)
        {
            float step = math.remap(0f, _target, 0f, 1f, _timer);
            spring.transform.localScale = Vector3.Lerp(Vector3.one, _targetSize, step);

            _timer += Time.deltaTime;

            if (_timer > _target)
            {
                _timer = 0f;
                _target = Random.Range(minPickableTime, maxPickableTime);
                spring.transform.localScale = _targetSize;
                _state = TumblerState.Pickable;
            }
        }
        else if (_state == TumblerState.Pickable)
        {
            _timer += Time.deltaTime;

            if (_timer > _target)
            {
                _timer = 0f;
                _target = Random.Range(minFallingTime, maxFallingTime);
                _state = TumblerState.Falling;
            }
        }
        else if (_state == TumblerState.Falling)
        {
            float step = math.remap(0f, _target, 0f, 1f, _timer);
            spring.transform.localScale = Vector3.Lerp(_targetSize, Vector3.one, step);

            _timer += Time.deltaTime;

            if (_timer > _target)
            {
                _timer = 0f;
                _target = Random.Range(minRaiseTime, maxRaiseTime);
                spring.transform.localScale = Vector3.one;
                _state = TumblerState.Idle;
            }
        }
    }
    
    public void KnockTumbler()
    {
        if (_state != TumblerState.Idle)
            return;

        _state = TumblerState.Rising;
    }

    public TumblerState GetTumblerState()
    {
        return _state;
    }

    public void Reset()
    {
        _state = TumblerState.Idle;
        spring.transform.localScale = Vector3.one;
        _target = Random.Range(minRaiseTime, maxRaiseTime);
    }

    public void Set()
    {
        _state = TumblerState.Set;
    }
}
