using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    [SerializeField] LevelGrid _grid = null;
    public LevelGrid Grid { get { return _grid; } }


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

    private void Update()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(mouseRay, out hit, 100))
            {
                Grid.ClearPaintedTiles();
                Grid.PaintTilesInRange(hit.point, 2, Color.blue);
                Grid.PaintTile(hit.point, Color.green);
            }
        }
    }

    void Init()
    {
        Grid.Init();
        Unit[] units = FindObjectsOfType<Unit>();
        Vector3 tilePosition;
        int i = 0;
        for (; i < units.Length; i++)
        {
            if (Grid.GetNearestUnitFreePosition(units[i].transform.position, out tilePosition))
            {
                units[i].transform.position = tilePosition;
                Grid.SetGridTileUnit(tilePosition, units[i]);
            }
            else break;
        }

        for (; i < units.Length; i++)
        {
            Debug.LogError("NO TILES");
            Destroy(units[i].gameObject);
        }

        Vector3[] path = Grid.FindPathInRange(units[0].transform.position, units[1].transform.position, 20);
        for(i = 0; i < path.Length; i++)
        {
            Grid.PaintTile(path[i], Color.red);
        }
    }

    private IEnumerator GameLoop()
    {
        yield return null;
    }


}
