using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface UnitInterface
{   
    GameManager.ControllerType ControlledBy { get; set; }
    int MaximumHealth { get; set; }
    int CurrentHealth { get; set; }
    int DamageAttack { get; set; }
    int AttackRange { get; set; }
    int MoveRange { get; set; }

    void Start();
    void TakeDamage(int amount);
}
public class Unit : MonoBehaviour, UnitInterface
{
    [Header("Unit Parameters")]
    [SerializeField] GameManager.ControllerType _controlledBy = GameManager.ControllerType.Player;
    public GameManager.ControllerType ControlledBy { get { return _controlledBy; } set { _controlledBy = value; } }
    [SerializeField] int _maximumHealth = 10;
    public int MaximumHealth { get { return _maximumHealth; } set { _maximumHealth = value; } }
    public int CurrentHealth { get; set; }
    [SerializeField] int _damageAttack = 2;
    public int DamageAttack { get { return _damageAttack; } set { _damageAttack = value; } }
    [SerializeField] int _attackRange = 1;
    public int AttackRange { get { return _attackRange; } set { _attackRange = value; } }
    [SerializeField] int _moveRange = 3;
    public int MoveRange { get { return _moveRange; } set { _moveRange = value; } }

    public bool HasMoved { get; set; }
    public bool HasAttacked { get; set; }


    public void Start()
    {
        CurrentHealth = MaximumHealth;
    }
    public void TakeDamage(int amount)
    {
        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
    }
}
