using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelTile
{
    #region VARIABLES AND PROPERTIES
    [SerializeField] Renderer _pathRenderer = null;
    public Renderer PathRenderer { get { return _pathRenderer; } private set { _pathRenderer = value; } }
    [SerializeField] GameObject _tileObject = null;
    public GameObject TileObject { get { return _tileObject; } private set { _tileObject = value; } }
    public Vector3 Position { get { return _tileObject.transform.position; } }
    [SerializeField] Unit _unitInTile = null;
    public Unit UnitInTile { get { return _unitInTile; } set { _unitInTile = value; } }

    public bool IsSelected { get { return _pathRenderer.enabled; } set { _pathRenderer.enabled = value; } }
    public Color TileColor { get { return _pathRenderer.material.color; } set { _pathRenderer.material.color = value; } }
    #endregion

    public LevelTile(GameObject tileObject, Renderer pathRenderer)
    {
        TileObject = tileObject;
        PathRenderer = pathRenderer;
    }

    public void PaintTile(Color color)
    {
        IsSelected = true;
        TileColor = color;
    }
}
