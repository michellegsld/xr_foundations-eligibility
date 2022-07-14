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

	///! Used in ???
	/// A matrix made up of TicTacToeState choices
	/// Defines current state of the tic-tac-toe grid
	TicTacToeState[,] boardState;

	/// ! Used in ???
	// (easy inspector visual) bool if it's the player's current turn
	// Use to determine if it's the player's turn
	[SerializeField]
	private bool _isPlayerTurn;

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

	// ! Used in ?? Defined where?
	// call this event when the game starts under StartGame()
	public UnityEvent onGameStarted;

	//Call This event with the player number to denote the winner
	public WinnerEvent onPlayerWin;

	// makes up the squares you fill on the grid
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
	// Initalizes a new tic-tac-toe grid and invokes the onGameStarted event
	private void StartGame()
	{
		_triggers = new ClickTrigger[3,3];
		onGameStarted.Invoke();
	}

	// Called under: ClickTrigger.cs -> ClickTrigger.OnMouseDown()
	// Displays Player icon at a specific point on the tic-tac-toe grid
	public void PlayerSelects(int coordX, int coordY){

		SetVisual(coordX, coordY, playerState);
	}

	// Called under: ???
	// Displays AI icon at a specific point on the tic-tac-toe grid
	public void AiSelects(int coordX, int coordY){

		SetVisual(coordX, coordY, aiState);
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
	}
}
