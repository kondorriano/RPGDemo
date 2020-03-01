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

    public void StartAITurn()
    {
        StartCoroutine(AITurn());
    }

    IEnumerator AITurn()
    {
        //Get All AI Units
        Unit[] aiUnits = GameManager.instance.GetAIUnits();
        foreach(Unit unit in aiUnits)
        {
            Unit playerUnit = GameManager.instance.GetClosestPlayerUnit(unit.Position);
            if (playerUnit == null) break;
            float playerUnitDistance = (playerUnit.Position - unit.Position).magnitude;

            //Select current Unit
            GameManager.instance.SelectTile(unit.Position, GameManager.ControllerType.AI);
            yield return _aiActionDelay;
            //Try attack Player Unit in attack range
            if (unit.AttackRange >= playerUnitDistance)
            {
                //Activate Attack state
                GameManager.instance.ToggleUnitAttackState();
                yield return _aiActionDelay;
                //Select chosen Player Unit
                GameManager.instance.SelectTile(playerUnit.Position, GameManager.ControllerType.AI);
                yield return _aiActionDelay;
                //Attack Unit
                GameManager.instance.ConfirmAction();
                yield return _aiActionDelay;
            }

            //if not attacked Unit Try Move
            if (!unit.HasAttacked)
            {
                //Find tile in range from Enemy Unit to Player Unit
                Vector3 closestFreePosition = GameManager.instance.FindClosestTileInRangeFromToUnit(unit, playerUnit, unit.MoveRange);
                if(closestFreePosition != unit.Position)
                {
                    //Activate Move state
                    GameManager.instance.ToggleUnitMoveState();
                    yield return _aiActionDelay;
                    //Select chosen Tile to Move to
                    GameManager.instance.SelectTile(closestFreePosition, GameManager.ControllerType.AI);
                    yield return _aiActionDelay;
                    //Move to Tile
                    GameManager.instance.ConfirmAction();

                    //If not attacked and Player Unit in range 
                    playerUnitDistance = (playerUnit.Position - unit.Position).magnitude;
                    if (unit.AttackRange >= playerUnitDistance)
                    {
                        //Activate Attack state
                        GameManager.instance.ToggleUnitAttackState();
                        yield return _aiActionDelay;
                        //Select chosen Player Unit
                        GameManager.instance.SelectTile(playerUnit.Position, GameManager.ControllerType.AI);
                        yield return _aiActionDelay;
                        //Attack Unit
                        GameManager.instance.ConfirmAction();
                        yield return _aiActionDelay;
                    }
                }
            }
        }

        //End AI Turn
        yield return _aiActionDelay;
        GameManager.instance.EndAIsTurn();
    }
}
