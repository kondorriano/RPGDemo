using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [SerializeField] float _aiActionTime = .4f;
    WaitForSeconds _aiActionDelay;
    private void Start()
    {
        _aiActionDelay = new WaitForSeconds(_aiActionTime);
    }

    public void StartAIsTurn()
    {
        StartCoroutine(AIsTurn());
    }

    IEnumerator AIsTurn()
    {
        Unit[] aiUnits = GameManager.instance.GetAIUnits();
        foreach(Unit unit in aiUnits)
        {
            Unit playerUnit = GameManager.instance.GetClosestPlayerUnit(unit.Position);
            if (playerUnit == null) break;
            float playerUnitDistance = (playerUnit.Position - unit.Position).magnitude;

            //Select unit
            GameManager.instance.SelectTile(unit.Position, GameManager.ControllerType.AI);
            yield return _aiActionDelay;
            //Try attack unit in attack range
            if (playerUnit.AttackRange >= playerUnitDistance)
            {
                //Activate attack 
                GameManager.instance.ToggleUnitAttackState();
                yield return _aiActionDelay;
                //Select chosen unit
                GameManager.instance.SelectTile(playerUnit.Position, GameManager.ControllerType.AI);
                yield return _aiActionDelay;
                //attack
                GameManager.instance.ConfirmAction();
                yield return _aiActionDelay;
            }

            //if not attacked move
            if (!unit.HasAttacked)
            {
                Vector3 closestFreePosition = GameManager.instance.GetClosestFreeTilePositionInRangeFromUnitToUnit(unit, playerUnit, unit.MoveRange);
                if(closestFreePosition != unit.Position)
                {
                    //Activate move
                    GameManager.instance.ToggleUnitMoveState();
                    yield return _aiActionDelay;
                    //Select chosen tile
                    GameManager.instance.SelectTile(closestFreePosition, GameManager.ControllerType.AI);
                    yield return _aiActionDelay;
                    //move to close unit
                    GameManager.instance.ConfirmAction();

                    //if not attacked and unit in range 
                    playerUnitDistance = (playerUnit.Position - unit.Position).magnitude;
                    if (playerUnit.AttackRange >= playerUnitDistance)
                    {
                        //Activate attack 
                        GameManager.instance.ToggleUnitAttackState();
                        yield return _aiActionDelay;
                        //Select chosen unit
                        GameManager.instance.SelectTile(playerUnit.Position, GameManager.ControllerType.AI);
                        yield return _aiActionDelay;
                        //attack
                        GameManager.instance.ConfirmAction();
                        yield return _aiActionDelay;
                    }
                }
            }
        }

        //End turn
        yield return _aiActionDelay;
        GameManager.instance.EndAIsTurn();
    }
}
