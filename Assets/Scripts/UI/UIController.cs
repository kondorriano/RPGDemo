using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    #region VARIABLES
    [SerializeField] GameObject _playerUnitPanel = null;
    [SerializeField] GameObject _endGamePanel = null;
    [SerializeField] Button _moveButton = null;
    [SerializeField] Button _attackButton = null;
    [SerializeField] Button _confirmButton = null;
    [SerializeField] Button _endTurnButton = null;
    [SerializeField] TextMeshProUGUI _moveText = null;
    [SerializeField] TextMeshProUGUI _attackText = null;
    [SerializeField] TextMeshProUGUI _turnText = null;
    [SerializeField] TextMeshProUGUI _endGameText = null;
    #endregion

    #region UI HANDLING
    public void ShowPlayerUnitPanel(bool show)
    {
        _playerUnitPanel.SetActive(show);
    }

    public void ShowPlayerUnitConfirmButton(bool show)
    {
        _confirmButton.gameObject.SetActive(show);
        if (!show) SetPlayerUnitConfirmButton(false);
    }

    public void ShowEndGame(bool playerWins)
    {
        _endGamePanel.SetActive(true);
        _endGameText.text = (playerWins) ? "Player Wins!" : "Enemies Win";
        ShowPlayerUnitPanel(false);
        SetEndTurnButton(false);
    }

    public void SelectionStateChanged(GameManager.SelectionState state)
    {
        _moveText.text = (state == GameManager.SelectionState.Moving) ? "Cancel" : "Move";
        _attackText.text = (state == GameManager.SelectionState.Attacking) ? "Cancel" : "Attack";
        ShowPlayerUnitConfirmButton(state != GameManager.SelectionState.None);
    }
    #endregion

    #region BUTTON HANDLING
    public void SetPlayerUnitConfirmButton(bool canConfirm)
    {
        _confirmButton.interactable = canConfirm;
    }

    public void SetPlayerUnitButtons(bool canMove, bool canAttack)
    {
        _moveButton.interactable = canMove;
        _attackButton.interactable = canAttack;
    }

    public void SetTurnUI(bool usePlayersText)
    {
        SetEndTurnButton(usePlayersText);
        _turnText.text = (usePlayersText) ? "Player's Turn" : "Enemy's Turn";
    }

    public void SetEndTurnButton(bool show)
    {
        _endTurnButton.gameObject.SetActive(show);
    }
    #endregion

    #region BUTTON EVENTS
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
    public void EndTurnButtonPressed()
    {
        GameManager.instance.EndPlayersTurn();
    }
    public void PlayAgainButtonPressed()
    {
        GameManager.instance.RestartGame();
    }
    public void ExitButtonPressed()
    {
        GameManager.instance.QuitGame();
    }
    #endregion
}
