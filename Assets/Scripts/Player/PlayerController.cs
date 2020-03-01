using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float _cameraMoveSpeed = 20;

    //Is active lets the player select tiles
    public bool IsActive { get; set; }
    Vector2 _previousTouchPosition;

    void Update()
    {
        //If not pointing on UI, Player can move camera or select a tile
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 currentTouchPosition = Vector2.zero;

            //Mobile Touch controls (Not tested)
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if(Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);
                    currentTouchPosition = touch.position;
                    if (touch.phase == TouchPhase.Moved)
                        SelectTile(currentTouchPosition);
                    else if (touch.phase == TouchPhase.Ended)
                        MoveCamera(currentTouchPosition);
                }
            }
            //Desktop controls
            else
            {
                currentTouchPosition = Input.mousePosition;
                if (Input.GetMouseButtonDown(0))
                    SelectTile(currentTouchPosition);
                else if (Input.GetMouseButton(1))
                    MoveCamera(currentTouchPosition);
            }

            _previousTouchPosition = currentTouchPosition;
        }
    }

    void SelectTile(Vector2 mousePosition)
    {
        if (!IsActive) return;
        Ray mouseRay = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(mouseRay, out hit, 100))
            GameManager.instance.SelectTile(hit.point, GameManager.ControllerType.Player);
    }

    void MoveCamera(Vector2 mousePosition)
    {
        Vector3 direction = mousePosition - _previousTouchPosition;
        Vector3 cameraPosition = Camera.main.transform.parent.position;
        direction = new Vector3(direction.x / Screen.width, 0, direction.y / Screen.height);
        cameraPosition -= direction * _cameraMoveSpeed;
        cameraPosition = GameManager.instance.ClampToGridBounds(cameraPosition);
        Camera.main.transform.parent.position = cameraPosition;
    }
}
