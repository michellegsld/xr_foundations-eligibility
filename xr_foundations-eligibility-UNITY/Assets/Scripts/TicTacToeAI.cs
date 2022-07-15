using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// the "icon" choices between [null] / x / o
public enum TicTacToeState{none, cross, circle}

// called to enote the winner
[System.Serializable]
public class WinnerEvent : UnityEvent<int>
{
}

public class TicTacToeAI : MonoBehaviour
{
	// to set the ai difficulty, either 0 or 1
	int _aiLevel;

	/// Initalized under: StartGame() and Used under: ValidSpot(int x, int y)
	/// A matrix made up of TicTacToeState choices
	/// Defines current state of the tic-tac-toe grid
	TicTacToeState[,] boardState;

	/// Used/toggled under: PlayerSelects(int x, int y) and AiSelects(int x, int y)
	// (easy inspector visual) bool to determine if it's the player's current turn
	[SerializeField]
	private bool _isPlayerTurn;

	// Used under: StartGame()
	// (easy inspector visual) int of current grid size, usually 3 x 3
	[SerializeField]
	private int _gridSize = 3;

	// to assign player & ai visuals (states are the icons)
	[SerializeField]
	private TicTacToeState playerState = TicTacToeState.cross;
	TicTacToeState aiState = TicTacToeState.circle;

	// To assign the actual icon GameObjects in the inspector
	[SerializeField]
	private GameObject _xPrefab;
	[SerializeField]
	private GameObject _oPrefab;

	// Used under StartGame()
	public UnityEvent onGameStarted;

	//Call This event with the player number to denote the winner
	public WinnerEvent onPlayerWin;

	// makes up the spots you fill on the grid
	ClickTrigger[,] _triggers;

	private void Awake()
	{
		if(onPlayerWin == null){
			onPlayerWin = new WinnerEvent();
		}
	}

	// Called under: StartingPanel GO's Button(s)
	// Sets AI difficulty and initalizes the game
	public void StartAI(int AILevel){
		_aiLevel = AILevel;
		StartGame();
	}

	// Called under: ClickTrigger.cs -> ClickTrigger.AddReference()
	// Call to save input placed on the tic-tac-toe grid
	public void RegisterTransform(int myCoordX, int myCoordY, ClickTrigger clickTrigger)
	{
		_triggers[myCoordX, myCoordY] = clickTrigger;
	}

	// Called under: StartAI(int AILevel)
	// Initalizes a new tic-tac-toe grid
	private void StartGame()
	{
		_triggers = new ClickTrigger[3,3];

		// initalizes new boardState to none
		boardState = new TicTacToeState[_gridSize,_gridSize];
		for (int i = 0; i < _gridSize; i++) {
			for (int j = 0; j < _gridSize; j++) {
				boardState[i,j] = (int) TicTacToeState.none;
			}
		}

		onGameStarted.Invoke();
	}

	// Called under: ClickTrigger.cs -> ClickTrigger.OnMouseDown()
	// Displays Player icon at a specific point on the tic-tac-toe grid
	public void PlayerSelects(int coordX, int coordY){
		if (_isPlayerTurn || ValidSpot(coordX, coordY))
		{
			SetVisual(coordX, coordY, playerState);
			_isPlayerTurn = false;
		}
	}

	// Called under: ???
	// Displays AI icon at a specific point on the tic-tac-toe grid
	public void AiSelects(int coordX, int coordY){
		if (!_isPlayerTurn && ValidSpot(coordX, coordY))
		{
			SetVisual(coordX, coordY, aiState);
			_isPlayerTurn = true;
		}
	}

	// Called under: PlayerSelects(int x, int y) and AiSelects(int x, int y)
	// Displays the targetState at a specific point on the tic-tac-toe grid
	// targetState: either playerState or aiState
	private void SetVisual(int coordX, int coordY, TicTacToeState targetState)
	{
		Instantiate(
			targetState == TicTacToeState.circle ? _oPrefab : _xPrefab,
			_triggers[coordX, coordY].transform.position,
			Quaternion.identity
		);

		boardState[coordX, coordY] = targetState; // Saves this placement
	}

	private void AiPlacement(){
		// math to determine placement

	}

	// Call to check if the spot on the grid is free
	private bool ValidSpot(int x, int y) {
		if (boardState[x, y] == (int) TicTacToeState.none)
			return true;
		return false;
	}
}
