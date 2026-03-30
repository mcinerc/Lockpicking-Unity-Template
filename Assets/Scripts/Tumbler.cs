using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public enum TumblerState { Idle, Rising, Pickable, Falling, Set }

public class Tumbler : MonoBehaviour
{
    private const float SpringMinSize = 0.1f;
    
    [Header("References")]
    [SerializeField] private Transform spring;
    [SerializeField] private Transform tumbler;
    [SerializeField] private Transform tumblerTarget;
    
    [Header("Settings")]
    // how fast the tumblers move up when picked
    private float _minRaiseTime;
    private float _maxRaiseTime;
    private float _minPickableTime;
    private float _maxPickableTime;
    private float _minFallingTime;
    private float _maxFallingTime;

    private TumblerState _state = TumblerState.Idle;
    private float _timer = 0f; // timer used for all state transitions
    private float _target = 0f; // target time used for all state transitions
    private Vector3 _targetSize;

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
                _target = Random.Range(_minPickableTime, _maxPickableTime);
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
                _target = Random.Range(_minFallingTime, _maxFallingTime);
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
                _target = Random.Range(_minRaiseTime, _maxRaiseTime);
                spring.transform.localScale = Vector3.one;
                _state = TumblerState.Idle;
            }
        }
    }
    
    public void Initialize(float minRaise, float maxRaise, float minPickable, float maxPickable, float minFall, float maxFall)
    {
        _targetSize = new Vector3(1f, SpringMinSize, 1f);
        _minRaiseTime = minRaise;
        _maxRaiseTime = maxRaise;
        _minPickableTime = minPickable;
        _maxPickableTime = maxPickable;
        _minFallingTime = minFall;
        _maxFallingTime = maxFall;
        Reset();
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
        _target = Random.Range(_minRaiseTime, _maxRaiseTime);
    }

    public void Set()
    {
        _state = TumblerState.Set;
    }
}
