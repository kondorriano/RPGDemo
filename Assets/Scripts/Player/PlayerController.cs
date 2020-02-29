using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    // Update is called once per frame
    Vector2 _previousTouchPosition;

    void Update()
    {

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 currentTouchPosition = Vector2.zero;

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                //Touch Controls
                //Works with GetMouseButton too
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
            else
            {
                currentTouchPosition = Input.mousePosition;
                //Desktop controls
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
        Ray mouseRay = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(mouseRay, out hit, 100))
            GameManager.instance.SelectTile(hit.point, GameManager.ControllerType.Player);
    }

    void MoveCamera(Vector2 mousePosition)
    {
        Vector3 direction = mousePosition - _previousTouchPosition;
        direction = new Vector3(direction.x / Screen.width, 0, direction.y / Screen.height);
        Camera.main.transform.root.position -= direction * 20;
    }
}
