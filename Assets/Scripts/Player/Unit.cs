using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface UnitInterface
{   
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
    public int _maximumHealth = 10;
    public int MaximumHealth { get { return _maximumHealth; } set { _maximumHealth = value; } }
    public int CurrentHealth { get; set; }
    public int _damageAttack = 2;
    public int DamageAttack { get { return _maximumHealth; } set { _maximumHealth = value; } }
    public int _attackRange = 1;
    public int AttackRange { get { return _maximumHealth; } set { _maximumHealth = value; } }
    public int _moveRange = 3;
    public int MoveRange { get { return _maximumHealth; } set { _maximumHealth = value; } }

    public void Start()
    {
        CurrentHealth = MaximumHealth;
    }
    public void TakeDamage(int amount)
    {
        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
    }

    /*
    private void OnValidate()
    {
        LevelGrid grid = FindObjectOfType<LevelGrid>();
        if (grid == null) return;
        int x, y;
        grid.GetXY(transform.position, out x, out y);
        transform.position = grid.GetWorldPosition(x, y);
    }
    */

}
