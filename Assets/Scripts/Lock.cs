using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Lock : MonoBehaviour
{
    public UnityEvent onLockPicked;
    
    [Header("Input")] [SerializeField]
    private InputActionAsset inputActions;
    
    [Header("References")]
    [SerializeField] private GameObject tumblerPrefab;
    [SerializeField] private Transform tumblerOrigin;
    [SerializeField] private Transform lockpick;
    [SerializeField] private Transform lockpickTarget;
    
    [Header("Lock Settings")]
    [SerializeField] private float lockpickSpeed = 1f; // how fast the lockpick moves on the X axis
    
    [SerializeField] private float lockpickBaseHeight = -1f; // Y height that the lockpick sits at
    [SerializeField] private float lockpickPickingHeight = 0f; // Y height it goes to during picking

    [SerializeField] private float pickingHalfLength = 0.5f; // total time the lockpick spends moving up and down during a pick, split between an up and down motion, thus half-length
    [SerializeField] private int numberOfTumblersRetainedOnFailure = 0; // when failing to pick a tumbler, this amount will stay set, akin to higher thieving levels in TES4
    
    [Header("Tumbler Settings")]
    // basic tumbler settings
    [SerializeField] private int numberOfTumblers; // you can have any number of tumblers you want
    [SerializeField] private float tumblerSpacing; // physical X distance between tumblers
    
    // how fast the tumblers move up when picked
    [SerializeField] private float minRaiseTime = 0.1f;
    [SerializeField] private float maxRaiseTime = 0.5f;
    
    // how long the tumblers stay up and in a pickable state
    [SerializeField] private float minPickableTime = 0.1f;
    [SerializeField] private float maxPickableTime = 0.5f;
    
    // how long it takes the tumblers to return to neutral
    [SerializeField] private float minFallingTime = 0.1f;
    [SerializeField] private float maxFallingTime = 0.5f;

    private List<Tumbler> _tumblers;
    private int _currentTumbler = 0;

    private InputAction _movePick;
    private InputAction _hitTumbler;
    private InputAction _tryPick;

    private bool _picking = false;
    private float _pickingTimer = 0f;
    private bool _rising = true;
    
    private Tumbler CurrentTumbler => _tumblers[_currentTumbler];
    
    private void Awake()
    {
        // basic intialization
        _tumblers = new();
        
        for (int i = 0; i < numberOfTumblers; i++)
        {
            GameObject newTumbler = Instantiate(tumblerPrefab, tumblerOrigin);
            newTumbler.transform.position += new Vector3(tumblerSpacing * i, 0f, 0f);
            Tumbler t = newTumbler.GetComponent<Tumbler>();
            t.Initialize(minRaiseTime, maxRaiseTime, minPickableTime, maxPickableTime, minFallingTime, maxFallingTime);
            _tumblers.Add(t);
        }

        lockpickTarget.position = new Vector3(CurrentTumbler.transform.position.x, lockpickBaseHeight, 0f);
        lockpick.position = lockpickTarget.position;
        
        // input registration
        _movePick = inputActions.FindAction("MovePick");
        _hitTumbler = inputActions.FindAction("HitTumbler");
        _tryPick = inputActions.FindAction("TryPick");
        
        _movePick.performed += OnMovePick;
        _hitTumbler.performed += OnHitTumbler;
        _tryPick.performed += OnTryPick;
        
        _movePick.Enable();
        _hitTumbler.Enable();
        _tryPick.Enable();
    }
    
    private void Update()
    {
        // lockpick follows target with lerp for smooth movement
        lockpick.position = Vector3.Lerp(lockpick.position, lockpickTarget.position, lockpickSpeed * Time.deltaTime);

        // lockpick rising and descending controlled via code
        if (_picking)
        {
            _pickingTimer += Time.deltaTime;
            if (_pickingTimer > pickingHalfLength)
            {
                _pickingTimer = 0f;
                if (_rising)
                {
                    _rising = false;
                    lockpickTarget.position = new Vector3(CurrentTumbler.transform.position.x, lockpickBaseHeight, 0f);
                }
                else
                {
                    _rising = true;
                    _picking = false;
                }
            }
        }
    }
    
    private void OnMovePick(InputAction.CallbackContext obj)
    {
        if (_picking)
            return;
        
        float dir = obj.ReadValue<float>();

        // change index based on direction
        if (dir > 0)
        {
            _currentTumbler++;
        }
        else
        {
            _currentTumbler--;
        }

        // wraparound
        if (_currentTumbler > _tumblers.Count - 1)
        {
            _currentTumbler = 0;
        }

        if (_currentTumbler < 0)
        {
            _currentTumbler = _tumblers.Count - 1;
        }
        
        lockpickTarget.position = new Vector3(CurrentTumbler.transform.position.x, lockpickBaseHeight, 0f);
    }
    
    private void OnHitTumbler(InputAction.CallbackContext obj)
    {
        if (_picking || CurrentTumbler.GetTumblerState() == TumblerState.Set)
            return;

        _picking = true;
        
        CurrentTumbler.KnockTumbler();
        lockpickTarget.position = new Vector3(CurrentTumbler.transform.position.x, lockpickPickingHeight, 0f);
    }
    
    private void OnTryPick(InputAction.CallbackContext obj)
    {
        TumblerState state = _tumblers[_currentTumbler].GetTumblerState();

        if (state == TumblerState.Idle || state == TumblerState.Set)
            return;

        if (state == TumblerState.Pickable)
        {
            CurrentTumbler.Set();
            
            // Check if all tumblers are set
            foreach (var t in _tumblers)
            {
                if (t.GetTumblerState() != TumblerState.Set)
                    return;
            }
            
            onLockPicked?.Invoke();
            return;
        }
        
        // Failure, time to break the puzzle
        
        lockpickTarget.position = new Vector3(CurrentTumbler.transform.position.x, lockpickBaseHeight, 0f);
        lockpick.position = lockpickTarget.position;

        int tumblersSkipped = 0;
        _picking = false;
        _rising = true;
        
        foreach (var t in _tumblers)
        {
            if (tumblersSkipped < numberOfTumblersRetainedOnFailure && t.GetTumblerState() == TumblerState.Set)
            {
                tumblersSkipped++;
                continue;
            }
            t.Reset();
        }
    }
}
