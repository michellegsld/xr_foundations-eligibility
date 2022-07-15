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

/**
	0 | 1 | 2
	3 | 4 | 5
	6 | 7 | 8
 */

public class TicTacToeAI : MonoBehaviour
{
	// To tell the total number of spots used
	private int move = 0;

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

	// To print the grid more easily
	private String grid = "";

	// to assign player & ai visuals (states are the icons)
	[SerializeField]
	private TicTacToeState playerState = TicTacToeState.circle;
	TicTacToeState aiState = TicTacToeState.cross;

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

		initalizeBoard();
		onGameStarted.Invoke();
	}

	// initalizes new boardState to none
	private void initalizeBoard() {
		if(boardState == null)
			boardState = new TicTacToeState[_gridSize,_gridSize];

		for (int i = 0; i < _gridSize; i++) {
			for (int j = 0; j < _gridSize; j++) {
				boardState[i, j] = (int) TicTacToeState.none;
			}
		}
	}

	// Call to check if the spot on the grid is free
	private bool ValidSpot(int x, int y) {
		if (boardState[x, y] == (int) TicTacToeState.none)
			return true;
		return false;
	}

	// Called under: ClickTrigger.cs -> ClickTrigger.OnMouseDown()
	// Displays Player icon at a specific point on the tic-tac-toe grid
	public void PlayerSelects(int coordX, int coordY){
		if (_isPlayerTurn && ValidSpot(coordX, coordY))
			SetVisual(coordX, coordY, playerState);
	}

	// Called under: ???
	// Displays AI icon at a specific point on the tic-tac-toe grid
	public void AiSelects(int coordX, int coordY){
		SetVisual(coordX, coordY, aiState);
		_isPlayerTurn = true;
	}

	// Called under: OnGUI()
	// Returns the tic-tac-toe grid as a string to print
	private String getGrid() {
		grid = "";

		if(boardState == null)
			initalizeBoard();

		for (int row = 0; row < 3; row++) {
			for (int col = 0; col < 3; col++) {
				if (boardState[row, col] == TicTacToeState.circle) {
					grid += "[o]";
				} else if (boardState[row, col] == TicTacToeState.cross) {
					grid += "[x]";
				} else {
					grid += "[_]";
				}
			}
			grid += "\n";
		}

		return grid;
	}

	// To actually display the text version of the grid
	void OnGUI() {
		GUIStyle guiSty = new GUIStyle();
		guiSty.fontSize = 50;

		String printGrid = getGrid();

		GUI.Label(new Rect(25, 20, 200, 200), printGrid, guiSty);
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
		move++;

		// check if that was winning move to end game
		// else if no empty spots then ended in tie

		// Next player's turn
		if (_isPlayerTurn) {
			_isPlayerTurn = false;
			AiPlacement(coordX, coordY);
		}

		// call func to check for win?

		if (move == 9) {
			// check for win, going to create row search, 
		}
	}

	// This function makes the move for the ai
	// Change this to account for difficulty?
	private void AiPlacement(int x, int y){
		Tuple<int, int> rndOption = randomSelection();
		Tuple<int, int> aiOptionLinear = gridLinearSearch(x, y);
		Tuple<int, int> aiOptionDiag = gridDiagonalSearch(x, y);

		if (aiOptionLinear.Item1 != -1) {
			AiSelects(aiOptionLinear.Item1, aiOptionLinear.Item2);
		} else if (aiOptionDiag.Item1 != -1) {
			AiSelects(aiOptionDiag.Item1, aiOptionDiag.Item2);
		} else {
			AiSelects(rndOption.Item1, rndOption.Item2);
		}
	}

	// Returns where to find random empty spot on board
	private Tuple<int, int> randomSelection() {
		System.Random rnd = new System.Random();
		int row = rnd.Next(0, 3);
		int col = rnd.Next(0, 3);

		do {
			row = rnd.Next(0, 3);
			col = rnd.Next(0, 3);
		} while (boardState[row, col] != TicTacToeState.none);

		return Tuple.Create(row, col);
	}

	// Searches by rows and columns
	private Tuple<int,int> gridLinearSearch(int row, int col) {
		int rowCount = 0;
		int colCount = 0;

		int iEmpty = -1;
		int jEmpty = -1;

		for (int x = 0; x < _gridSize; x++) {
			if (boardState[row, x] == TicTacToeState.circle) {
				rowCount++;
			} else if (boardState[row, x] == TicTacToeState.none) {
				jEmpty = x;
			}
			if (boardState[x, col]== TicTacToeState.circle) {
				colCount++;
			} else if (boardState[x, col] == TicTacToeState.none) {
				iEmpty = x;
			}
		}

		if (rowCount == 2) {
			return Tuple.Create(row, jEmpty);
		} else if (colCount == 2) {
			return Tuple.Create(iEmpty, col);
		} else {
			return Tuple.Create(-1, -1);
		}
	}

	// Searches by checking diagonals
	private Tuple<int,int> gridDiagonalSearch(int row, int col) {
		int rLCount = 0;
		int lRCount = 0;

		int iEmpty = -1;
		int jEmpty = -1;

		for (int i = 0; i < _gridSize; i++) {
			if (boardState[i, i] == TicTacToeState.circle) {
				rLCount++;
			} else if (boardState[row, i] == TicTacToeState.none) {
				iEmpty = i;
			}
		}

		if (rLCount == 2) {
			return Tuple.Create(iEmpty, iEmpty);
		} else if (row + col != 2) {
			return Tuple.Create(-1, -1);
		}

		for (int i = 0; i < 3; i++){
			for (int j = 2; j >= 0 && i + j == 2; j--) {
				if (boardState[i, j] == TicTacToeState.circle) {
					lRCount++;
				} else if (boardState[row, i] == TicTacToeState.none) {
					iEmpty = i;
					jEmpty = j;
				}
			}
		}

		if (lRCount == 2) {
			return Tuple.Create(iEmpty, jEmpty);
		} else {
			return Tuple.Create(-1, -1);
		}
	}
}
