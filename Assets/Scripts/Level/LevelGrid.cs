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
        if (EditorApplication.isPlaying) return;
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

        Camera camera = Camera.main;
        if (camera == null)
            Debug.Break();
        camera.transform.root.position = new Vector3(Width * .5f -.5f,0, 0);
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

    public void Init()
    {
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

    public LevelTile GetNearestUnitFreeTile(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetNearestUnitFreeTile(x, y);
    }

    public LevelTile GetNearestUnitFreeTile(int x, int y)
    {
        if (x < 0 || x >= Width) x = Mathf.Clamp(x, 0, Width-1);
        if (y < 0 || y >= Height) y = Mathf.Clamp(y, 0, Height-1);

        LevelTile currentTile = GetGridTile(x, y);
        if (currentTile.UnitInTile == null)
            return currentTile;

        List<int[]> searchTiles = new List<int[]>();
        bool[,] checkedTiles = new bool[Width, Height];

        searchTiles.Add(new int[] { x, y });

        int[] currentTileInfo;

        while (searchTiles.Count > 0)
        {
            currentTileInfo = searchTiles[0];
            searchTiles.RemoveAt(0);
            x = currentTileInfo[0];
            y = currentTileInfo[1];
            checkedTiles[x, y] = true;

            int newX, newY;
            for (int i = 0; i < directions.Length; i++)
            {
                newX = x + directions[i][0];
                newY = y + directions[i][1];

                currentTile = GetGridTile(newX, newY);
                if (currentTile == null) 
                    continue;
                if (currentTile.UnitInTile == null)
                    return currentTile;

                if(!checkedTiles[newX, newY]) 
                    searchTiles.Add(new int[] { newX, newY});
            }
                
        }

        return null;
    }

    public Vector3[] FindPathInRange(Vector3 startPosition, Vector3 endPosition, int range)
    {
        if (range < 1) return null;

        int startX, startY, endX, endY;
        GetXY(startPosition, out startX, out startY);
        GetXY(endPosition, out endX, out endY);

        if (startX < 0 || startX >= Width) return null;
        if (startY < 0 || startY >= Height) return null;
        if (endX < 0 || endX >= Width) return null;
        if (endY < 0 || endY >= Height) return null;

        //CHECK IF OUT OF RANGE OR IN SAME POSITION
        if (Mathf.Pow(endX - startX, 2) + Mathf.Pow(endY - startY, 2) > range * range) return null;
        LevelTile endTile = GetGridTile(endX, endY);
        LevelTile startTile = GetGridTile(startX, startY);
        if (startTile == endTile) return null;
        List<int[]> searchTiles = new List<int[]>();
        TileInPath[,] tilesInRange = new TileInPath[Width, Height];       
        tilesInRange[startX, startY] = new TileInPath(startX, startY, null);

        int x, y;
        for (int i = 0; i < directions.Length; i++)
        {
            x = startX + directions[i][0];
            y = startY + directions[i][1];
            if (x < 0 || x >= Width) continue;
            if (y < 0 || y >= Height) continue;
            tilesInRange[x,y] = new TileInPath(x, y, tilesInRange[startX, startY]);
            searchTiles.Add(new int[] { x, y, range - 1 });
        }

        LevelTile currentTile;
        int[] currentTileInfo;
        while (searchTiles.Count > 0)
        {
            currentTileInfo = searchTiles[0];
            searchTiles.RemoveAt(0);
            x = currentTileInfo[0];
            y = currentTileInfo[1];
            range = currentTileInfo[2];
            currentTile = GetGridTile(x, y);
            if(currentTile == endTile)
            {

                TileInPath currentTileInPath = tilesInRange[x,y];
                List<Vector3> path = new List<Vector3>();
                //END
                while (currentTileInPath != tilesInRange[startX, startY])
                {
                    path.Add(GetWorldPosition(currentTileInPath.x, currentTileInPath.y));
                    currentTileInPath = currentTileInPath.parent;
                }

                path.Reverse();
                return path.ToArray();
            }
            if (range < 1 || currentTile == null || currentTile.UnitInTile != null)
                continue;

            int newX, newY;
            for (int i = 0; i < directions.Length; i++)
            {
                newX = x + directions[i][0];
                newY = y + directions[i][1];
                if (newX < 0 || newX >= Width) continue;
                if (newY < 0 || newY >= Height) continue;
                if (tilesInRange[newX, newY] != null) continue;
                tilesInRange[newX, newY] = new TileInPath(newX, newY, tilesInRange[x, y]);
                searchTiles.Add(new int[] { newX, newY, range - 1 });
            }
        }

        return null;
    }

    public class TileInPath
    {
        public TileInPath parent;
        public int x;
        public int y;
        public TileInPath(int _x, int _y, TileInPath _parent)
        {
            x = _x;
            y = _y;
            parent = _parent;
        }
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, 0, y) + OriginPosition;
    }
    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - OriginPosition).x + .5f);
        y = Mathf.FloorToInt((worldPosition - OriginPosition).z + .5f);
    }

    public LevelTile[] GetTilesInRange(Vector3 worldPosition, int range, bool onlyFreeTiles = true)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetTilesInRange(x, y, range, onlyFreeTiles);
    }
    public LevelTile[] GetTilesInRange(int x, int y, int range, bool onlyFreeTiles = true)
    {
        LevelTile currentTile = GetGridTile(x, y);
        List<LevelTile> tilesInRange = new List<LevelTile>();
        if (range < 0 || currentTile == null) return tilesInRange.ToArray();
        List<int[]> searchTiles = new List<int[]>();
        bool[,] checkedTiles = new bool[Width, Height];
        checkedTiles[x, y] = true;

        //First tile neighbors always checked even if tile is not free
        if (!onlyFreeTiles || currentTile.UnitInTile == null)
            tilesInRange.Add(currentTile);
        if (range > 0)
        {
            for (int i = 0; i < directions.Length; i++)
                searchTiles.Add(new int[] { x + directions[i][0], y + directions[i][1], range - 1 });
        }
        int[] currentTileInfo;
        while (searchTiles.Count > 0)
        {
            currentTileInfo = searchTiles[0];
            searchTiles.RemoveAt(0);
            x = currentTileInfo[0];
            y = currentTileInfo[1];
            range = currentTileInfo[2];
            currentTile = GetGridTile(x, y);
            if (range < 0 || currentTile == null || checkedTiles[x, y] ||(onlyFreeTiles && currentTile.UnitInTile != null)) 
                continue;

            checkedTiles[x, y] = true;
            tilesInRange.Add(currentTile);
            if(range > 0)
            {
                for (int i = 0; i < directions.Length; i++)
                    searchTiles.Add(new int[] { x + directions[i][0], y + directions[i][1], range-1 });
            }            
        }

        return tilesInRange.ToArray();
    }

    readonly int[][] directions = new int[][]
    {
        new int[] {1, 0},
        new int[] {0, 1},
        new int[] {-1, 0},
        new int[] {0, -1}
    };

}
