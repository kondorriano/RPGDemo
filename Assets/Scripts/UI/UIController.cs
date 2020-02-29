using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] GameObject _playerUnitPanel = null;
    [SerializeField] Button _moveButton = null;
    [SerializeField] Button _attackButton = null;
    [SerializeField] Button _confirmButton = null;
    [SerializeField] TextMeshProUGUI _moveText = null;
    [SerializeField] TextMeshProUGUI _attackText = null;

    public void ShowPlayerUnitPanel(bool show)
    {
        _playerUnitPanel.SetActive(show);
    }

    public void ShowPlayerUnitConfirmButton(bool show)
    {
        _confirmButton.gameObject.SetActive(show);
        if (!show) SetPlayerUnitConfirmButton(false);
    }
    public void SetPlayerUnitConfirmButton(bool canConfirm)
    {
        _confirmButton.interactable = canConfirm;
    }

    public void SetPlayerUnitButtons(bool canMove, bool canAttack)
    {
        _moveButton.interactable = canMove;
        _attackButton.interactable = canAttack;
    }

    public void SelectionStateChanged(GameManager.SelectionState state)
    {
        _moveText.text = (state == GameManager.SelectionState.Moving) ? "Cancel" : "Move";
        _attackText.text = (state == GameManager.SelectionState.Attacking) ? "Cancel" : "Attack";
        ShowPlayerUnitConfirmButton(state != GameManager.SelectionState.None);
    }

    public void MoveButtonPressed()
    {
        GameManager.instance.ToggleUnitMoveState();
    }
    public void AttackButtonPressed()
    {
        GameManager.instance.ToggleUnitAttackState();
    }

    public void ConfirmButtonPressed()
    {
        GameManager.instance.ConfirmAction();
    }
}
