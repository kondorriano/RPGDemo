using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region CLASSES AND ENUMERATIONS
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

    //Information the tile selectors have about the tile they are on
    //Useful to return the tiles to their previous state if needed
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
    #endregion

    #region VARIABLES AND PROPERTIES
    public static GameManager instance = null;

    [Header("GameManager Connectors")]
    [SerializeField] LevelGrid _grid = null;
    public LevelGrid Grid { get { return _grid; } }
    [SerializeField] UIController _uiController = null;
    public UIController UI { get { return _uiController; } }
    [SerializeField] AIController _aiController = null;
    [SerializeField] PlayerController _playerController = null;

    /***UNITS***/
    List<Unit> _playerUnits;
    List<Unit> _aiUnits;
    /***SELECTION***/
    SelectionState _selectionState = SelectionState.None;
    TileSelector _selectedTile = null;
    TileSelector _secondarySelectedTile = null;
    LevelTile[] _rangeTiles;
    #endregion

    #region INITIALIZATION
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
        //Set Game time
        Time.timeScale = 1;

        //Variables Initialization
        _playerUnits = new List<Unit>();
        _aiUnits = new List<Unit>();
        _selectionState = SelectionState.None;

        //Player and AI Unit Setup
        //If they are out of bounds they will be placed inside the grids
        //If they are on an occupied tile they will be placed on the closest free tile
        //If every tile is occupied the rest of the units are destroyed
        Unit[] units = FindObjectsOfType<Unit>();
        LevelTile tile;
        int i = 0;
        for (; i < units.Length; i++)
        {
            tile = Grid.GetNearestUnitFreeTile(units[i].Position);
            if (tile != null)
            {
                units[i].Position = tile.Position;
                tile.UnitInTile = units[i];
                if (units[i].ControlledBy == ControllerType.Player)
                    _playerUnits.Add(units[i]);
                else if (units[i].ControlledBy == ControllerType.AI)
                    _aiUnits.Add(units[i]);
            }
            else break;
        }

        for (; i < units.Length; i++)
        {
            Debug.LogError("NO TILES");
            Destroy(units[i].gameObject);
        }

        //Set Players turn
        EndAIsTurn();
    }
    #endregion

    #region GRID UTILS
    public Vector3 ClampToGridBounds(Vector3 worldPosition)
    {
        Vector3 gridPosition = Grid.OriginPosition;
        worldPosition.x = Mathf.Clamp(worldPosition.x, gridPosition.x, gridPosition.x + Grid.Width);
        worldPosition.z = Mathf.Clamp(worldPosition.z, gridPosition.z, gridPosition.z + Grid.Height);
        return worldPosition;
    }
    #endregion

    #region GAME FUNCTIONS
    void EndGame(bool playerWins)
    {
        UI.ShowEndGame(playerWins);
        Time.timeScale = 0;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
    #endregion

    #region TURN FUNCTIONS
    public void EndPlayersTurn()
    {
        for (int i = 0; i < _playerUnits.Count; i++)
        {
            _playerUnits[i].HasAttacked = false;
            _playerUnits[i].HasMoved = false;
        }
        _playerController.IsActive = false;
        UI.SetTurnUI(false);
        ResetSelection();
        _aiController.StartAITurn();
    }

    public void EndAIsTurn()
    {
        for (int i = 0; i < _aiUnits.Count; i++)
        {
            _aiUnits[i].HasAttacked = false;
            _aiUnits[i].HasMoved = false;
        }
        _playerController.IsActive = true;
        UI.SetTurnUI(true);
        ResetSelection();
    }
    #endregion

    #region UNIT FUNCTIONS
    public Unit[] GetAIUnits()
    {
        return _aiUnits.ToArray();
    }

    public Vector3 FindClosestTileInRangeFromToUnit(Unit fromUnit, Unit toUnit, int range)
    {
        if (range < 1) return fromUnit.Position;
        Vector3[] path = Grid.FindPath(fromUnit.Position, toUnit.Position);
        if (path == null) return fromUnit.Position;

        int bestIndex = Mathf.Min(range - 1, path.Length - 1);
        return path[bestIndex];
    }

    public Unit GetClosestPlayerUnit(Vector3 position)
    {
        if (_playerUnits.Count < 1) return null;
        int closestUnitIndex = 0;
        float closestSqrDistance = (_playerUnits[closestUnitIndex].Position - position).sqrMagnitude;
        float currentSqrDistance;
        for (int i = 1; i < _playerUnits.Count; i++)
        {
            currentSqrDistance = (_playerUnits[i].Position - position).sqrMagnitude;
            if (closestSqrDistance > currentSqrDistance)
            {
                closestSqrDistance = currentSqrDistance;
                closestUnitIndex = i;
            }
        }
        return _playerUnits[closestUnitIndex];
    }

    public void UnitDefeated(Unit unit)
    {
        LevelTile tile = Grid.GetGridTile(unit.Position);
        tile.UnitInTile = null;
        _playerUnits.Remove(unit);
        _aiUnits.Remove(unit);
        Destroy(unit.gameObject);

        if (_playerUnits.Count < 1)
            EndGame(false);
        else if (_aiUnits.Count < 1)
            EndGame(true);
    }
    #endregion

    #region MOVE AND ATTACK ACTIONS
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
        unit.Position = _secondarySelectedTile.tile.Position;
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
    }
    #endregion

    #region SELECTION FUNCTIONS

    #region TILE SELECTION
    public void SelectTile(Vector3 worldPosition, ControllerType controlledBy)
    {
        if (_selectionState == SelectionState.None)
        {
            _selectedTile = SetTileSelector(_selectedTile, worldPosition, controlledBy);
            if (_selectedTile != null)
            {
                _selectedTile.tile.PaintTile(Color.green);
                //Show player menu
                UI.ShowPlayerUnitPanel(controlledBy == ControllerType.Player && _selectedTile.tile.UnitInTile != null && _selectedTile.tile.UnitInTile.ControlledBy == controlledBy);
            }
        }
        else
        {
            _secondarySelectedTile = SetTileSelector(_secondarySelectedTile, worldPosition, controlledBy);
            if (_secondarySelectedTile != null)
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

        if (_selectedTile?.tile.UnitInTile != null)
        {
            UI.SetPlayerUnitButtons(!_selectedTile.tile.UnitInTile.HasMoved, !_selectedTile.tile.UnitInTile.HasAttacked);
        }
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

    void ResetSelection()
    {
        if (_selectedTile != null)
        {
            _selectedTile.tile.IsSelected = false;
            _selectedTile = null;
        }

        if (_secondarySelectedTile != null)
        {
            _secondarySelectedTile.tile.IsSelected = false;
            _secondarySelectedTile = null;
        }
    }
    #endregion

    #region SECONDARY TILE CHECKS
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
    #endregion

    #region SECONDARY TILE PAINTING
    void PaintRangeTiles(Color color)
    {
        if (_rangeTiles == null) return;
        for (int i = 0; i < _rangeTiles.Length; i++)
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
    #endregion

    #region SELECTION STATE
    void ChangeSelectionState(SelectionState newState)
    {
        _selectionState = newState;
        UI.SelectionStateChanged(_selectionState);

        if (_selectedTile?.tile.UnitInTile != null)
        {
            UI.SetPlayerUnitButtons(!_selectedTile.tile.UnitInTile.HasMoved, !_selectedTile.tile.UnitInTile.HasAttacked);
        }
    }

    public void ToggleUnitMoveState()
    {
        if (_selectedTile == null)
            throw new ArgumentNullException("SelectedTile is null");
        else if (_selectedTile.tile.UnitInTile == null)
            throw new ArgumentNullException("SelectedTile has no Unit");

        ClearRangeAndSecondaryTiles();
        if (_selectionState == SelectionState.Moving)
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
        if (_selectedTile == null)
            throw new ArgumentNullException("SelectedTile is null");
        else if (_selectedTile.tile.UnitInTile == null)
            throw new ArgumentNullException("SelectedTile has no Unit");

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
    #endregion

    #endregion
}
