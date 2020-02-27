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
            Debug.DrawRay(mouseRay.origin, mouseRay.direction*100, Color.black, 100);

            RaycastHit hit;
            if(Physics.Raycast(mouseRay, out hit, 100))
            {
                LevelTile tile = Grid.GetGridTile(hit.point);
                if (tile != null)
                {
                    tile.PathRenderer.enabled = true;
                    tile.PathRenderer.material.color = Color.blue;
                }
            }
        }
    }

    void Init()
    {
    }

    private IEnumerator GameLoop()
    {
        yield return null;
    }


}
