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
    public LevelTile(GameObject tileObject, Renderer pathRenderer)
    {
        TileObject = tileObject;
        PathRenderer = pathRenderer;
    }
}
