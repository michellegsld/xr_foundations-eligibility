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
	private int movesLeft = 9;

	// To tell the total number of spots used
	private bool winFlag = false;

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

	// Called only once
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

	// Called under: OnGUI()
	// Returns the tic-tac-toe grid as a string to be printed
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

	// Call to check if the spot on the grid is free
	private bool ValidSpot(int x, int y) {
		if (x > -1 && y > -1 && boardState[x, y] == (int) TicTacToeState.none)
			return true;

		return false;
	}

	// Called under: ClickTrigger.cs -> ClickTrigger.OnMouseDown()
	// Displays Player icon at a specific point on the tic-tac-toe grid
	public void PlayerSelects(int coordX, int coordY){
		if (_isPlayerTurn && ValidSpot(coordX, coordY)) {
			movesLeft--;
			SetVisual(coordX, coordY, playerState);
			findWinner();
			_isPlayerTurn = false;
		}

		if (!_isPlayerTurn)
			AiPlacement(coordX, coordY);
	}

	// Called under: AiPlacement(int x, int y)
	// Displays AI icon at a specific point on the tic-tac-toe grid
	public void AiSelects(int coordX, int coordY){
		if (!winFlag && ValidSpot(coordX, coordY)) {
			movesLeft--;
			SetVisual(coordX, coordY, aiState);
		}
		findWinner();
		_isPlayerTurn = true;
	}

	// This function makes the move for the ai
	// Change this to account for difficulty?
	private void AiPlacement(int x, int y){
		Tuple<int, int> rndOption = randomSelection();
		Tuple<int, int> aiOptionLinear = gridLinearSearch(x, y);
		Tuple<int, int> aiOptionDiag = gridDiagonalSearch(x, y);

		if (ValidSpot(aiOptionLinear.Item1, aiOptionLinear.Item2)) {
			AiSelects(aiOptionLinear.Item1, aiOptionLinear.Item2);
		} else if (ValidSpot(aiOptionDiag.Item1, aiOptionDiag.Item2)) {
			AiSelects(aiOptionDiag.Item1, aiOptionDiag.Item2);
		} else {
			AiSelects(rndOption.Item1, rndOption.Item2);
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

	// Returns where to find random empty spot on board
	private Tuple<int, int> randomSelection() {
		System.Random rnd = new System.Random();
		int row = -1;
		int col = -1;

		if (movesLeft > 1)
		{
			do {
				row = rnd.Next(0, 3);
				col = rnd.Next(0, 3);
			} while (boardState[row, col] != TicTacToeState.none);
		}

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
			} else if (ValidSpot(row, x)) {
				jEmpty = x;
			}
			if (boardState[x, col]== TicTacToeState.circle) {
				colCount++;
			} else if (ValidSpot(x, col)) {
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

		int n = 0;

		// R -> L Diagonal. They'd have the same index
		for (int i = 0; i < _gridSize; i++) {
			if (boardState[i, i] == TicTacToeState.circle) {
				rLCount++;
			} else if (ValidSpot(i, i)) {
				iEmpty = i;
			}
		}

		if (rLCount == 2) {
			return Tuple.Create(iEmpty, iEmpty);
		} else if (row + col != 2) {
			return Tuple.Create(-1, -1);
		}

		// L -> R Diagonal. Their index would add up to = 2
		for (int i = 0; i < _gridSize; i++, n = (_gridSize - 1) - i) {
			if (boardState[i, n] == TicTacToeState.circle) {
				lRCount++;
			} else if (ValidSpot(i, n)) {
				iEmpty = i;
				jEmpty = n;
			}
		}

		if (lRCount == 2) {
			return Tuple.Create(iEmpty, jEmpty);
		} else {
			return Tuple.Create(-1, -1);
		}
	}

	// Determines win condition if found
	private void findWinner() {
		if (_isPlayerTurn && isWinner(playerState)) {
			winFlag = true;
			onPlayerWin.Invoke(0);
		} else if (!_isPlayerTurn && isWinner(aiState)) {
			winFlag = true;
			onPlayerWin.Invoke(1);
		} else if (movesLeft < 1) {
			// check for win, going to create row search,
			onPlayerWin.Invoke(-1);
		}
	}

	// Hard code check for the winner
	private bool isWinner(TicTacToeState winIcon) {
		// rows:
		if (boardState[0, 0] == winIcon && boardState[0, 1] == winIcon && boardState[0, 2] == winIcon) { return true; }
		if (boardState[1, 0] == winIcon && boardState[1, 1] == winIcon && boardState[1, 2] == winIcon) { return true; }
		if (boardState[2, 0] == winIcon && boardState[2, 1] == winIcon && boardState[2, 2] == winIcon) { return true; }

		// columns:
		if (boardState[0, 0] == winIcon && boardState[1, 0] == winIcon && boardState[2, 0] == winIcon) { return true; }
		if (boardState[0, 1] == winIcon && boardState[1, 1] == winIcon && boardState[2, 1] == winIcon) { return true; }
		if (boardState[0, 2] == winIcon && boardState[1, 2] == winIcon && boardState[2, 2] == winIcon) { return true; }

		// Right -> Left and Left -> Right diagonals:
		if (boardState[0, 0] == winIcon && boardState[1, 1] == winIcon && boardState[2, 2] == winIcon) { return true; }
		if (boardState[0, 2] == winIcon && boardState[1, 1] == winIcon && boardState[2, 0] == winIcon) { return true; }

		return false;
	}
}

