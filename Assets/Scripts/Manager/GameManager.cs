using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum ControllerType
    {
        Player,
        AI
    }
    public enum SelectionState
    {
        None,
        Moving,
        Attacking
    }
    public class TileSelector
    {
        public LevelTile tile;
        public Vector3 selectedPosition;
        public Color previousColor;
        public bool wasPainted;

        public TileSelector(LevelTile Tile, Vector3 SelectedPosition, bool WasPainted, Color PreviousColor)
        {
            tile = Tile;
            selectedPosition = SelectedPosition;
            wasPainted = WasPainted;
            previousColor = PreviousColor;
        }
    }

    public static GameManager instance = null;

    [SerializeField] LevelGrid _grid = null;
    public LevelGrid Grid { get { return _grid; } }
    [SerializeField] UIController _uiController = null;
    public UIController UI { get { return _uiController; } }

    SelectionState _selectionState = SelectionState.None;

    TileSelector _selectedTile = null;
    TileSelector _secondarySelectedTile = null;
    LevelTile[] _rangeTiles;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        Init();
    }

    void Init()
    {
        _selectionState = SelectionState.None;

        Grid.Init();
        Unit[] units = FindObjectsOfType<Unit>();
        LevelTile tile;
        int i = 0;
        for (; i < units.Length; i++)
        {
            tile = Grid.GetNearestUnitFreeTile(units[i].transform.position);
            if (tile != null)
            {
                units[i].transform.position = tile.Position;
                tile.UnitInTile = units[i];
            }
            else break;
        }

        for (; i < units.Length; i++)
        {
            Debug.LogError("NO TILES");
            Destroy(units[i].gameObject);
        }

        /*
        Vector3[] path = Grid.FindPathInRange(units[0].transform.position, units[1].transform.position, 20);
        for(i = 0; i < path.Length; i++)
        {
            Grid.PaintTile(path[i], Color.red);
        }
        */
    }

    public void SelectTile(Vector3 worldPosition, ControllerType controlledBy)
    {
        if (_selectionState == SelectionState.None) {
            _selectedTile = SetTileSelector(_selectedTile, worldPosition, controlledBy);
            if(_selectedTile != null)
            {
                _selectedTile.tile.PaintTile(Color.green);
                //Show player menu
                UI.ShowPlayerUnitPanel(_selectedTile.tile.UnitInTile != null && _selectedTile.tile.UnitInTile.ControlledBy == controlledBy);
            }
        }
        else 
        {
            _secondarySelectedTile = SetTileSelector(_secondarySelectedTile, worldPosition, controlledBy);
            if(_secondarySelectedTile != null)
            {
                _secondarySelectedTile.tile.PaintTile(Color.yellow);
                if (controlledBy == ControllerType.Player)
                {
                    if (_selectionState == SelectionState.Moving)
                        UI.SetPlayerUnitConfirmButton(IsSecondaryTileInMoveRange());
                    else if (_selectionState == SelectionState.Attacking)
                        UI.SetPlayerUnitConfirmButton(IsSecondaryTileInAttackRange(controlledBy));
                }
            }
        }
    }

    bool IsSecondaryTileInMoveRange()
    {
        if (_secondarySelectedTile == null) return false;
        if (_rangeTiles == null) return false;
        for (int i = 0; i < _rangeTiles.Length; i++)
        {
            if (_rangeTiles[i] == _secondarySelectedTile.tile) return true;
        }
        return false;
    }

    bool IsSecondaryTileInAttackRange(ControllerType controlledBy)
    {
        if (_secondarySelectedTile == null) return false;
        if (_secondarySelectedTile.tile.UnitInTile == null) return false;
        if (_secondarySelectedTile.tile.UnitInTile.ControlledBy == controlledBy) return false;
        if (_rangeTiles == null) return false;
        for (int i = 0; i < _rangeTiles.Length; i++)
        {
            if (_rangeTiles[i] == _secondarySelectedTile.tile) return true;
        }
        return false;
    }

    TileSelector SetTileSelector(TileSelector previousTileSelector, Vector3 worldPosition, ControllerType controlledBy)
    {
        //Return previous selected tile to original color
        if (previousTileSelector != null)
        {
            if (previousTileSelector.wasPainted)
                previousTileSelector.tile.PaintTile(previousTileSelector.previousColor);
            else previousTileSelector.tile.IsSelected = false;
        }

        LevelTile tile = Grid.GetGridTile(worldPosition);

        if (tile == null) return previousTileSelector;

        return new TileSelector(tile, worldPosition, tile.IsSelected, tile.TileColor);
    }

    void PaintRangeTiles(Color color)
    {
        if (_rangeTiles == null) return;
        for(int i = 0; i < _rangeTiles.Length; i++)
            _rangeTiles[i].PaintTile(color);
    }

    void ClearRangeAndSecondaryTiles()
    {
        if (_secondarySelectedTile != null)
        {
            _secondarySelectedTile.tile.IsSelected = false;
            _secondarySelectedTile = null;
        }

        if (_rangeTiles == null) return;
        for (int i = 0; i < _rangeTiles.Length; i++)
            _rangeTiles[i].IsSelected = false;
    }

    void ChangeSelectionState(SelectionState newState)
    {
        _selectionState = newState;
        UI.SelectionStateChanged(_selectionState);
        
        if(_selectedTile?.tile.UnitInTile != null)
        {
            UI.SetPlayerUnitButtons(!_selectedTile.tile.UnitInTile.HasMoved, !_selectedTile.tile.UnitInTile.HasAttacked);
        }
    }

    public void ToggleUnitMoveState()
    {
        if (_selectedTile == null || _selectedTile.tile.UnitInTile == null)
            Debug.Break();
        ClearRangeAndSecondaryTiles();
        if(_selectionState == SelectionState.Moving)
        {
            ChangeSelectionState(SelectionState.None);
        }
        else
        {
            _rangeTiles = Grid.GetTilesInRange(_selectedTile.selectedPosition, _selectedTile.tile.UnitInTile.MoveRange);
            PaintRangeTiles(Color.blue);
            ChangeSelectionState(SelectionState.Moving);
        }
    }

    public void ToggleUnitAttackState()
    {
        if (_selectedTile == null || _selectedTile.tile.UnitInTile == null)
            Debug.Break();
        ClearRangeAndSecondaryTiles();
        if (_selectionState == SelectionState.Attacking)
        {
            ChangeSelectionState(SelectionState.None);
        }
        else
        {
            _rangeTiles = Grid.GetTilesInRange(_selectedTile.selectedPosition, _selectedTile.tile.UnitInTile.AttackRange, false);
            PaintRangeTiles(Color.red);
            ChangeSelectionState(SelectionState.Attacking);
        }
    }

    public void ConfirmAction()
    {
        if (_selectionState == SelectionState.Moving)
            MoveUnit();
        if (_selectionState == SelectionState.Attacking)
            AttackUnit();

    }

    public void MoveUnit()
    {
        Unit unit = _selectedTile.tile.UnitInTile;
        _secondarySelectedTile.tile.UnitInTile = unit;
        unit.transform.position = _secondarySelectedTile.tile.Position;
        unit.HasMoved = true;
        _selectedTile.tile.UnitInTile = null;
        _selectedTile.tile.IsSelected = false;
        _selectedTile = _secondarySelectedTile;
        _secondarySelectedTile = null;
        ToggleUnitMoveState();
        _selectedTile.wasPainted = false;
        _selectedTile.tile.PaintTile(Color.green);
    }

    public void AttackUnit()
    {
        Unit firstUnit = _selectedTile.tile.UnitInTile;
        Unit secondUnit = _secondarySelectedTile.tile.UnitInTile;
        secondUnit.TakeDamage(firstUnit.DamageAttack);
        firstUnit.HasAttacked = true;
        ToggleUnitAttackState();
        //CHECK IF UNIT IS DEAD
    }




    public void StartPlayersTurn()
    {
        //Show UI Start Animation
        //After UI Start done
            //Show Player General UI
    }

    public void StartAIsTrurn()
    {
        //Show UI Start Animation
        //After UI Start done
            //Let AI do things
    }

    public void SelectTile(Vector3 worldPosition, bool isPlayer)
    {
        //Get unit
        //if is unit && !selected unit
        //If unit is player and isPlayer
        //show 
    }
}
