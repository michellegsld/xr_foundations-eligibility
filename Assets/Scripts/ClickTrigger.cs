using System;
using System.Collections.Generic;
using UnityEngine;

// defines a playable spot on the tic-tac-toe grid
// defines if the player can input and saves it on mouse click
public class ClickTrigger : MonoBehaviour
{
	TicTacToeAI _ai;

	// Inspector defines these coords making up tic-tac-toe grid
	[SerializeField]
	private int _myCoordX = 0;
	[SerializeField]
	private int _myCoordY = 0;

	// defines if the ttt grid point can be selected by the player
	[SerializeField]
	private bool canClick;

	private void Awake()
	{
		_ai = FindObjectOfType<TicTacToeAI>();
	}

	private void Start(){

		_ai.onGameStarted.AddListener(AddReference);
		_ai.onGameStarted.AddListener(() => SetInputEndabled(true));
		_ai.onPlayerWin.AddListener((win) => SetInputEndabled(false));
	}

	private void SetInputEndabled(bool val){
		canClick = val;
	}

	private void AddReference()
	{
		_ai.RegisterTransform(_myCoordX, _myCoordY, this);
		canClick = true;
	}

	private void OnMouseDown()
	{
		if(canClick){
			_ai.PlayerSelects(_myCoordX, _myCoordY);
		}
	}
}
