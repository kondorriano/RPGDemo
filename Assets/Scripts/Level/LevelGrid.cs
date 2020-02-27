using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    [Header("Level Parameters")]
    [Range(0, 40)]
    [SerializeField] int _width = 20;
    public int Width { get { return _width; } private set { _width = value; } }
    [Range(0, 40)]
    [SerializeField] int _height = 20;
    public int Height { get { return _height; } private set { _height = value; } }
    public Vector3 OriginPosition { get { return transform.position; } }

    [Header("Level Objects")]
    [SerializeField] GameObject _levelTile = null;
    [SerializeField] Transform _levelLocator = null;
    [SerializeField] Transform _levelCollider = null;

    [System.Serializable]
    public class Column
    {
        public LevelTile[] column;
    }


    [SerializeField] Column[] _grid;

    LevelTile this[int x, int y]
    {
        get
        {
            return _grid[x].column[y];
        }
    }

    private void OnValidate()
    {
        if (EditorApplication.isPlaying || !Application.isEditor) return;
        int i = 0;
        Column[] _newGrid = new Column[Width];
        for (; i < _grid.Length && i < Width; i++)
        {
            _newGrid[i] = _grid[i];
            if (_newGrid[i].column.Length != Height)
            {
                int j = 0;
                LevelTile[] newColumn = new LevelTile[Height];
                for (; j < _newGrid[i].column.Length && j < Height; j++)
                    newColumn[j] = _newGrid[i].column[j];

                //Delete extra  row tiles
                for (; j < _newGrid[i].column.Length; j++)
                    StartCoroutine(DestroyTileObject(_newGrid[i].column[j].TileObject));

                //Add extra rows
                for (; j < Height; j++)
                    newColumn[j] = GenerateNewTile(i,j);

                _newGrid[i].column = newColumn;
            }
        }

        //Delete extra column tiles
        for (; i < _grid.Length; i++)
        {
            for (int j = 0; j < _grid[i].column.Length; j++)
                StartCoroutine(DestroyTileObject(_grid[i].column[j].TileObject));
        }

        //Add extra columns
        for (; i < Width; i++)
        {
            _newGrid[i] = new Column();
            _newGrid[i].column = new LevelTile[Height];
            for (int j = 0; j < Height; j++)
                _newGrid[i].column[j] = GenerateNewTile(i, j);
        }

        _grid = _newGrid;
        if (_levelCollider == null)
            Debug.Break();
        _levelCollider.localScale = new Vector3(Width, 1, Height);
    }

    LevelTile GenerateNewTile(int x, int y)
    {
        if (_levelTile == null || _levelLocator == null)
            Debug.Break();
        GameObject instantiatedTile = Instantiate(_levelTile, GetWorldPosition(x, y), Quaternion.identity);
        instantiatedTile.transform.SetParent(_levelLocator);
        instantiatedTile.name = x + "x" + y;
        instantiatedTile.SetActive(true);

        Renderer pathRenderer = instantiatedTile.transform.Find("PathRenderer")?.GetComponent<Renderer>();
        LevelTile newTile = new LevelTile(instantiatedTile, pathRenderer);
        return newTile;
    }

    IEnumerator DestroyTileObject(GameObject go)
    {
        yield return new WaitForEndOfFrame();
        DestroyImmediate(go);
    }

    public LevelTile GetGridTile(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetGridTile(x,y);
    }
    public LevelTile GetGridTile(int x, int y)
    {
        if (x < 0 || x >= Width) return null;
        if (y < 0 || y >= Height) return null;
        return this[x, y];
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x + .5f, 0, y + .5f) + OriginPosition;
    }
    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - OriginPosition).x);
        y = Mathf.FloorToInt((worldPosition - OriginPosition).z);
    }
}
