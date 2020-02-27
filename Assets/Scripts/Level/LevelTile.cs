using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelTile
{
    [SerializeField] Renderer _pathRenderer = null;
    public Renderer PathRenderer { get { return _pathRenderer; } private set { _pathRenderer = value; } }
    [SerializeField] GameObject _tileObject = null;
    public GameObject TileObject { get { return _tileObject; } private set { _tileObject = value; } }
    [SerializeField] Unit _unitInTile = null;
    public Unit UnitInTile { get { return _unitInTile; } set { _unitInTile = value; } }
    public LevelTile(GameObject tileObject, Renderer pathRenderer)
    {
        TileObject = tileObject;
        PathRenderer = pathRenderer;
    }


}
