using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public bool lookahead;	
	public bool minimax;
	public bool neural;
	public List<float[]> turn = new List<float[]>();
	public int lookaroundLimit = 5000;
	public int lookaroundCount = 0;
	public int lookaroundWins = 0;

	public int turnsAllowedPerGame = 100;
	public int numberOfGames = 16;
	public int numberOfEpochs = 100;
	public int currentGeneration = 0;

	//USED FOR GA
	public int totalBestScore = 0;
	public int bestScore = 0;
	public int totalRedScore = 0;
	public int totalBlackScore = 0;

	//USED TO CHECK IF GAME SHOULD BE TERMINATED
	private bool[] gameEndEarly;

	NeuralNetwork[] blackNets;
	NeuralNetwork blackNet;
	NeuralNetwork[] redNets;
	NeuralNetwork redNet;

	NeuralNetwork[] blackNetsChildren;
	NeuralNetwork[] redNetsChildren;

	public float[] input;
	public float[] output;

	public Piece[][] redPieces;
	public Piece[][] blackPieces;
	public GameObject[] board;
	public GameObject redPiece;
	public GameObject blackPiece;
	public int currentGame = 0;

	public GameObject boardGenerator;
	private bool trainingComplete = false;

	private float[][] miniMaxMove;
	public float posScore = 0;
	public float curScore = 0;
	public float negScore = 0;

	public Piece[][] miniMaxBlackPieces;
	public Piece[][] miniMaxRedPieces;
	public int NextBlackToMove;
	public int NextDirectionToMove = -1;
	private bool test = false;
	

	void GenerateNextGeneration() {

		NeuralNetwork[] blackNetsClone = (NeuralNetwork[])blackNets.Clone();
		NeuralNetwork[] redNetsClone = (NeuralNetwork[])redNets.Clone();

		Array.Sort(blackNetsClone,
    	delegate(NeuralNetwork x, NeuralNetwork y) { return x.score.CompareTo(y.score); });

		Array.Sort(redNetsClone,
    	delegate(NeuralNetwork x, NeuralNetwork y) { return x.score.CompareTo(y.score); });
		
		string huh = "SCORES: ";
		for (int i = 0; i < numberOfGames; i++) {
			huh += blackNetsClone[i].score + " , ";
		}
		Debug.Log(huh + " TOTAL SCORE: " + totalBlackScore);

		NeuralNetwork parent1 = blackNetsClone[numberOfGames-1];
		NeuralNetwork parent2 = blackNetsClone[numberOfGames-1];

		NeuralNetwork parent3 = redNetsClone[numberOfGames-1];
		NeuralNetwork parent4 = redNetsClone[numberOfGames-1];

		//FOR EACH CHILD
		for (int i = 0; i < numberOfGames; i++) {

			currentGame = i;

			//PICK A RANDOM NUMBER BETWEEN THE TOTAL FITNESS
			float p1 = UnityEngine.Random.Range(0, totalBlackScore+1);
			float p2 = UnityEngine.Random.Range(0, totalBlackScore+1);

			float p3 = UnityEngine.Random.Range(0, totalRedScore+1);
			float p4 = UnityEngine.Random.Range(0, totalRedScore+1);

			//SCALES VALUES TO EXCLUDE WEAKER CANDIDATES
			p1 = (float)p1;
			p2 = (float)p2;
			p3 = (float)p3;
			p4 = (float)p4;

			var pSum = 0;
			var pSum2 = 0;

			bool p1Chosen = false;
			bool p2Chosen = false;
			bool p3Chosen = false;
			bool p4Chosen = false;

			//FOR EACH POSSIBLE PARENT
			for (int j = numberOfGames-1; j >= 0; j--) {

				currentGame = j;

				pSum += blackNetsClone[j].score;
				//PICK BLACK PARENTS
				if (pSum >= p1) {
					if (!p1Chosen) {
						parent1 = blackNetsClone[j];
						p1Chosen = true;
					}
				}
				if (pSum >= p2) {
					if (!p2Chosen) {
						parent2 = blackNetsClone[j];
						p2Chosen = true;
					}
				}

				pSum2 += redNetsClone[j].score;
				//PICK RED PARENTS
				if (pSum2 >= p3) {
					if (!p3Chosen) {
						parent3 = redNetsClone[j];
						p3Chosen = true;
					}
				}
				if (pSum2 >= p4) {
					if (!p4Chosen) {
						parent4 = redNetsClone[j];
						p4Chosen = true;
					}
				}

			}

			//MIX WEIGHTS TOGETHER
			parent1.GetWeights();
			parent2.GetWeights();
			parent3.GetWeights();
			parent4.GetWeights();

			blackNetsChildren[i] = new NeuralNetwork(new int[] { 32, 30, 16 }); //initialize network
			blackNetsChildren[i].GetWeightsFromParents(parent1.weights, parent2.weights, (float)parent1.score/((float)parent1.score+(float)parent2.score));

			//redNetsChildren[i] = new NeuralNetwork(new int[] { 32,  30, 10, 16 }); //initialize network
			//redNetsChildren[i].GetWeightsFromParents(parent3.weights, parent4.weights, (float)parent3.score/((float)parent3.score+(float)parent4.score));

		}
	}

	void RestartSimulation() {
		input = new float[32];
		output = new float[16];
		gameEndEarly = new bool[numberOfGames];

		int x = currentGame;
		redPieces[x] = new Piece[12];
		blackPieces[x] = new Piece[12];

		int num = 0;
        for (int j = 0; j < 3; j++) {
			for (int i = 0; i < 4; i++) {
				GameObject piece = Instantiate(redPiece) as GameObject;
				Piece newPiece = new Piece( i, j, 1, piece);
				piece.transform.SetParent(board[x].transform);
				if(j%2 == 0){
					piece.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3 + board[x].GetComponent<RectTransform>().rect.height/8f + i*board[x].GetComponent<RectTransform>().rect.width/4f, 5 - j*board[x].GetComponent<RectTransform>().rect.height/8f);
				}
				else {
					piece.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3 + i*board[x].GetComponent<RectTransform>().rect.width/4f, 5 - j*board[x].GetComponent<RectTransform>().rect.height/8f);
				}
				redPieces[x][i+num] = newPiece;
			}
			num = num+4;
		}

		num = 0;
        for (int j = 7; j > 4; j--) {
			for (int i = 0; i < 4; i++) {
				GameObject piece = Instantiate(blackPiece) as GameObject;
				Piece newPiece = new Piece( i, j, -1, piece);
				piece.transform.SetParent(board[x].transform);
				if(j%2 == 0){
					piece.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3 + board[x].GetComponent<RectTransform>().rect.height/8f + i*board[x].GetComponent<RectTransform>().rect.width/4f, 5 - j*board[x].GetComponent<RectTransform>().rect.height/8f);
				}
				else {
					piece.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3 + i*board[x].GetComponent<RectTransform>().rect.width/4f, 5 - j*board[x].GetComponent<RectTransform>().rect.height/8f);
				}
				blackPieces[x][i+num] = newPiece;
			}
			num = num+4;
		}

	}

	void StartSimulation() {
		input = new float[32];
		output = new float[16];
		gameEndEarly = new bool[numberOfGames];

		for (int x = 0; x < numberOfGames; x++) {

		redPieces[x] = new Piece[12];
		blackPieces[x] = new Piece[12];

		int num = 0;
        for (int j = 0; j < 3; j++) {
			for (int i = 0; i < 4; i++) {
				GameObject piece = Instantiate(redPiece) as GameObject;
				Piece newPiece = new Piece( i, j, 1, piece);
				piece.transform.SetParent(board[x].transform);
				if(j%2 == 0){
					piece.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3 + board[x].GetComponent<RectTransform>().rect.height/8f + i*board[x].GetComponent<RectTransform>().rect.width/4f, 5 - j*board[x].GetComponent<RectTransform>().rect.height/8f);
				}
				else {
					piece.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3 + i*board[x].GetComponent<RectTransform>().rect.width/4f, 5 - j*board[x].GetComponent<RectTransform>().rect.height/8f);
				}
				redPieces[x][i+num] = newPiece;
			}
			num = num+4;
		}

		num = 0;
        for (int j = 7; j > 4; j--) {
			for (int i = 0; i < 4; i++) {
				GameObject piece = Instantiate(blackPiece) as GameObject;
				Piece newPiece = new Piece( i, j, -1, piece);
				piece.transform.SetParent(board[x].transform);
				if(j%2 == 0){
					piece.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3 + board[x].GetComponent<RectTransform>().rect.height/8f + i*board[x].GetComponent<RectTransform>().rect.width/4f, 5 - j*board[x].GetComponent<RectTransform>().rect.height/8f);
				}
				else {
					piece.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3 + i*board[x].GetComponent<RectTransform>().rect.width/4f, 5 - j*board[x].GetComponent<RectTransform>().rect.height/8f);
				}
				blackPieces[x][i+num] = newPiece;
			}
			num = num+4;
		}

		}
	}

	// Use this for initialization
	void Start () {
		board = new GameObject[numberOfGames];
		blackNets = new NeuralNetwork[numberOfGames];
		redNets = new NeuralNetwork[numberOfGames];
		blackNetsChildren = new NeuralNetwork[numberOfGames];
		redNetsChildren = new NeuralNetwork[numberOfGames];
					
		board = boardGenerator.GetComponent<BoardGenerator>().GenerateImageView();

		for (int num = 0; num < numberOfGames; num++) {
			blackNets[num] = new NeuralNetwork(new int[] { 32, 30, 16 }); //initialize network
			redNets[num] = new NeuralNetwork(new int[] { 32, 30, 16 }); //initialize network
		}

		blackNet = blackNets[0];

		redPieces = new Piece[numberOfGames][];
		blackPieces = new Piece[numberOfGames][];

		miniMaxRedPieces = new Piece[2][];
		miniMaxBlackPieces = new Piece[2][];

		if (!minimax) {
			if (lookahead) {
				StartCoroutine(trainByLookAhead());
			}
			else {
				StartSimulation();
				if (!neural) {
					trainingComplete = true;
					StartCoroutine(moveByAI());
				}
				else {
					StartCoroutine(trainByAI());
				}
			}
		}
		else {
				StartCoroutine(moveByMiniMax());
		}


	}

		IEnumerator trainByAI() {

		int w = 0;
		int l = 0;
		for (int epoch = 0; epoch < numberOfEpochs; epoch++) {
			turn.Clear();
			//blackNet.GetWeights();
			//Debug.Log("WEIGHT: " + blackNet.weights[0][0,0]);
			currentGeneration = epoch;
			redNets[0] = new NeuralNetwork(new int[] { 32, 30, 16 });
			float prevScore = 0;
			for (int i = 0; i < turnsAllowedPerGame; i++) {

				if (gameEndEarly[currentGame]) {
					continue;
				}	

				GetNetInput();
				turn.Add(input);
				output = blackNet.FeedForward (input);
				HandleAIDecision(blackPieces[0], -1);
				
				float[] outputTemp = new float[16];
				outputTemp[NextBlackToMove] = 1;
				outputTemp[NextDirectionToMove+12] = 1;
				float curScore = getScoreDif2();
				if (curScore > prevScore) {
					blackNet.BackProp (outputTemp);
				}
				prevScore = curScore;

				//yield return null;
				if (gameEndEarly[currentGame]) {
					continue;
				}
		
				GetNetInput();
				output = redNets[0].FeedForward (input);
				HandleAIDecision(redPieces[0], 1);
				
			}

			yield return null;
			
			currentGame = 0;

			bool win = false;
			bool loss = false;
        	for (int t = 0; t < 12; t++) {
				//NOT DEAD
				if (blackPieces[0][t].state != 0) {
					win = true;
				}
				//ENEMY NOT DEAD EITHER
				if (redPieces[0][t].state != 0) {
					loss = true;
				}
			}

			turn.Reverse();
			if (win) {
				if (!loss) {
					w ++;
					foreach (float[] t in turn) {
					input = (float[])t;
					output = blackNet.FeedForward (input);
					float[] output2 = new float[16];
					int index = getNextHighestNode(0, output);
					int index2 = getNextHighestDir(0);
					output2[index] = 1;
					output2[12+index2] = 1;
					blackNet.BackProp (output2);

					}
				}
			}
			else if (loss) {
				l ++;
				//TRAIN FROM LOSS?

			}
			else {		
				//TIE
					//foreach (float[] t in turn) {
					//input = (float[])t;
					//output = blackNet.FeedForward (input);
					//float[] output2 = new float[16];
					//int index = getNextHighestNode(0, output);
					//int index2 = getNextHighestDir(0);
					//output2[index] = 1;
					//output2[12+index2] = 1;
					//blackNet.BackProp (output2);
					//}
			}

			if (epoch % 100 == 0) {
				float ratio = 100;
				if (w != 0 && l != 0) {
					ratio = (float)w/((float)w+(float)l);
				}
				Debug.Log("WINS: " + w + " LOSS: " + l + " WIN RATIO: " + ratio);
				w = 0;
				l = 0;
			}


			for (int i = 0; i < 12; i++) {
				if (redPieces[0][i].obj != null) {
					Destroy(redPieces[0][i].obj);
				}
				if (blackPieces[0][i].obj != null) {
					Destroy(blackPieces[0][i].obj);
				}
			}

			RestartSimulation();

		}
			trainingComplete = true;

			//Max Number Of Turns Per Game
			int totalScore = 0;
        	for (int num = 0; num < numberOfGames; num++) {
				turn.Clear();
				currentGame = num;
				for (int i = 0; i < turnsAllowedPerGame; i++) {

					if (gameEndEarly[currentGame]) {
						continue;
					}	

					GetNetInput();
					output = blackNets[0].FeedForward (input);
					HandleAIDecision(blackPieces[num], -1);
			
					if (gameEndEarly[currentGame]) {
						continue;
					}
		
					GetNetInput();
					output = redNets[num].FeedForward (input);
					HandleAIDecision(redPieces[num], 1);

					yield return null;

				}



			//Destroy Pieces
			for (int i = 0; i < 12; i++) {
				if (redPieces[num][i].obj != null) {
					Destroy(redPieces[num][i].obj);
				}
				if (blackPieces[num][i].obj != null) {
					Destroy(blackPieces[num][i].obj);
				}
			}


			blackNets[0].score = getScore(true);
			totalScore += blackNets[0].score;
	
			
			}

			Debug.Log(totalScore);

			//RESET GAME
			currentGame = 0;
			StartSimulation();

		
		

	}

	public void resetPieces(int team, int depth) {
	
		if (depth == 0) {
		if (team == -1) {
			miniMaxBlackPieces[0] = new Piece[12];
			for (int x = 0; x < 12; x++) {
				if (blackPieces[0][x].obj != null) {
				Piece newPiece = new Piece( blackPieces[0][x].posX, blackPieces[0][x].posY, -1, blackPieces[0][x].obj);
				miniMaxBlackPieces[0][x] = newPiece;
				miniMaxBlackPieces[0][x].state = blackPieces[0][x].state;
				int newX = blackPieces[0][x].posX;
				int newY = blackPieces[0][x].posY;				

				if(newY%2 == 0){
					newPiece.obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3 + board[0].GetComponent<RectTransform>().rect.height/8f + newX*board[0].GetComponent<RectTransform>().rect.width/4f, 5 - newY*board[0].GetComponent<RectTransform>().rect.height/8f);
				}
				else {
					newPiece.obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3 + newX*board[0].GetComponent<RectTransform>().rect.width/4f, 5 - newY*board[0].GetComponent<RectTransform>().rect.height/8f);
				}
				}
				else {
					Piece newPiece = new Piece( blackPieces[0][x].posX, blackPieces[0][x].posY, 0, null);
					miniMaxBlackPieces[0][x] = newPiece;
				}
			}
		}
		else {
			miniMaxRedPieces[0] = new Piece[12];
			for (int x = 0; x < 12; x++) {
				if (redPieces[0][x].obj != null) {
				Piece newPiece = new Piece( redPieces[0][x].posX, redPieces[0][x].posY, 1, redPieces[0][x].obj);
				miniMaxRedPieces[0][x] = newPiece;
				miniMaxRedPieces[0][x].state = redPieces[0][x].state;
				int newX = redPieces[0][x].posX;
				int newY = redPieces[0][x].posY;				

				if(newY%2 == 0){
					newPiece.obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3 + board[0].GetComponent<RectTransform>().rect.height/8f + newX*board[0].GetComponent<RectTransform>().rect.width/4f, 5 - newY*board[0].GetComponent<RectTransform>().rect.height/8f);
				}
				else {
					newPiece.obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3 + newX*board[0].GetComponent<RectTransform>().rect.width/4f, 5 - newY*board[0].GetComponent<RectTransform>().rect.height/8f);
				}
				}
				else {
					Piece newPiece = new Piece( redPieces[0][x].posX, redPieces[0][x].posY, 0, null);
					miniMaxRedPieces[0][x] = newPiece;
				}
			}
		}
		}
		else {
			if (team == -1) {
			miniMaxBlackPieces[1] = new Piece[12];
			for (int x = 0; x < 12; x++) {
				if (blackPieces[0][x].obj != null) {
				Piece newPiece = new Piece( miniMaxBlackPieces[0][x].posX, miniMaxBlackPieces[0][x].posY, -1, miniMaxBlackPieces[0][x].obj);
				miniMaxBlackPieces[1][x] = newPiece;
				miniMaxBlackPieces[1][x].state = miniMaxBlackPieces[0][x].state;
				int newX = miniMaxBlackPieces[0][x].posX;
				int newY = miniMaxBlackPieces[0][x].posY;				

				if(newY%2 == 0){
					newPiece.obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3 + board[0].GetComponent<RectTransform>().rect.height/8f + newX*board[0].GetComponent<RectTransform>().rect.width/4f, 5 - newY*board[0].GetComponent<RectTransform>().rect.height/8f);
				}
				else {
					newPiece.obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3 + newX*board[0].GetComponent<RectTransform>().rect.width/4f, 5 - newY*board[0].GetComponent<RectTransform>().rect.height/8f);
				}
				}
				else {
					Piece newPiece = new Piece( miniMaxBlackPieces[0][x].posX, miniMaxBlackPieces[0][x].posY, 0, null);
					miniMaxBlackPieces[1][x] = newPiece;
				}
			}
		}
		else {
			miniMaxRedPieces[1] = new Piece[12];
			for (int x = 0; x < 12; x++) {
				if (redPieces[0][x].obj != null) {
				Piece newPiece = new Piece( miniMaxRedPieces[0][x].posX, miniMaxRedPieces[0][x].posY, 1, miniMaxRedPieces[0][x].obj);
				miniMaxRedPieces[1][x] = newPiece;
				miniMaxRedPieces[1][x].state = miniMaxRedPieces[0][x].state;
				int newX = miniMaxRedPieces[0][x].posX;
				int newY = miniMaxRedPieces[0][x].posY;				

				if(newY%2 == 0){
					newPiece.obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3 + board[0].GetComponent<RectTransform>().rect.height/8f + newX*board[0].GetComponent<RectTransform>().rect.width/4f, 5 - newY*board[0].GetComponent<RectTransform>().rect.height/8f);
				}
				else {
					newPiece.obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3 + newX*board[0].GetComponent<RectTransform>().rect.width/4f, 5 - newY*board[0].GetComponent<RectTransform>().rect.height/8f);
				}
				}
				else {
					Piece newPiece = new Piece( miniMaxRedPieces[0][x].posX, miniMaxRedPieces[0][x].posY, 0, null);
					miniMaxRedPieces[1][x] = newPiece;
				}
			}
		}
		}
	}

	IEnumerator moveByMiniMax() {
		
		StartSimulation();

		resetPieces(-1, 0);
		resetPieces(1, 0);

		resetPieces(-1, 1);
		resetPieces(1, 1);
		bool pieceMoved = false;

		//NUMBER OF TURNS
		for (int epoch = 0; epoch < numberOfEpochs; epoch++) {
			redNets[0] = new NeuralNetwork(new int[] { 32, 30, 16 });
		
			for (int t = 0; t < 100; t++) {

			posScore = 0;

			for (int i = 0; i < 12; i++) {
			for (int j = 0; j < 4; j++) {

				resetPieces(-1, 0);
				resetPieces(1, 0);
				//resetPieces(-1, 1);
				//resetPieces(1, 1);

				//SETUP FOR NEXT LAYER
				GetMiniMaxInput(0);
				miniMaxBlackPieces[0][i].GetLocationFromDirection(j);

				if (TryMovePieceMiniMax(miniMaxBlackPieces[0][i], j, 0)) {

					miniMaxBlackPieces[0][i].GetLocationFromDirection(j);
					if (getMiniMax(miniMaxBlackPieces[0][i], j, -1, 0)) {
						Debug.Log(t + " POSSCORE: " + posScore + " ,  i: " + i);
						NextBlackToMove = i;
						NextDirectionToMove = j;
					}


				}
			}
			}				
			
		

		resetPieces(-1, 0);
		resetPieces(1, 0);
		//resetPieces(-1, 1);
		//resetPieces(1, 1);

		//yield return new WaitForSeconds (0.1f);

		float[] outp = new float[16];
		//output = blackNets[0].FeedForward (input);
		pieceMoved = false;
		if (NextBlackToMove == -1 && NextDirectionToMove == -1) {
			for (int i = 0; i < 12; i++) {	
				int index = getNextHighestNode(i, outp);
				for (int j = 0; j < 4; j++) {
					int index2 = getNextHighestDir(j);
					if (!pieceMoved) {
					if (TryMovePiece(blackPieces[0][index], index2)) {
							MovePiece(blackPieces[0][index], index2);

							//MOVES NEXT BOARDS PIECES (FOR VISUALIZATION)
							currentGame = 1;
							GetNetInput();
							blackPieces[1][index].GetLocationFromDirection(index2);
							if (TryMovePiece(blackPieces[1][index], index2)) {
								MovePiece(blackPieces[1][index], index2);
							}

							currentGame = 0;
							pieceMoved = true;
						}
					}
				}
			}		
		}
		else {
			if (TryMovePiece(blackPieces[0][NextBlackToMove], NextDirectionToMove)) {
				MovePiece(blackPieces[0][NextBlackToMove], NextDirectionToMove);

				//MOVES NEXT BOARDS PIECES (FOR VISUALIZATION)
				currentGame = 1;
				GetNetInput();
				blackPieces[1][NextBlackToMove].GetLocationFromDirection(NextDirectionToMove);
				if (TryMovePiece(blackPieces[1][NextBlackToMove], NextDirectionToMove)) {
					MovePiece(blackPieces[1][NextBlackToMove], NextDirectionToMove);
				}
				
				currentGame = 2;
	
				float[] oT = new float[16];
				oT[NextBlackToMove] = 1;
				oT[12+NextDirectionToMove] = 1;

				//BACKPROPOGATION
				blackNets[0].BackProp(oT);

				float[] oT2 = blackNets[0].FeedForward (input);

				//USE NETWORK
				int index = getNextHighestNode(0, oT2);
				int index2 = getNextHighestDir(0);	
				if (index != NextBlackToMove) {
					if (t == 0) {
						Debug.Log(index + ", " + NextBlackToMove);
					}
				}		
				if (TryMovePiece(blackPieces[2][index], index2)) {
					MovePiece(blackPieces[2][index], index2);
				}

				currentGame = 0;
				NextBlackToMove = -1;
				NextDirectionToMove = -1;
			}
		}


		output = redNets[0].FeedForward (input);
		pieceMoved = false;
		for (int i2 = 0; i2 < 12; i2++) {	
			int index = getNextHighestNode(i2, output);
			for (int j2 = 0; j2 < 4; j2++) {
				int index2 = getNextHighestDir(j2);
				if (!pieceMoved) {
				if (TryMovePiece(redPieces[0][index], index2)) {
						MovePiece(redPieces[0][index], index2);

						//MOVES NEXT BOARDS PIECES (FOR VISUALIZATION)
						currentGame = 1;
						GetNetInput();
						redPieces[1][index].GetLocationFromDirection(index2);
						if (TryMovePiece(redPieces[1][index], index2)) {
							MovePiece(redPieces[1][index], index2);
						}

						currentGame = 2;
						GetNetInput();
						redPieces[2][index].GetLocationFromDirection(index2);
						if (TryMovePiece(redPieces[2][index], index2)) {
							MovePiece(redPieces[2][index], index2);
						}
						currentGame = 0;

						pieceMoved = true;
					}
				}
			}
		}			


		yield return null;
		posScore = 0;
		negScore = 0;

		}

			for (int x = 0; x < numberOfGames; x++) {
				for (int i = 0; i < 12; i++) {
					if (redPieces[x][i].obj != null) {
						Destroy(redPieces[x][i].obj);
					}
					if (blackPieces[x][i].obj != null) {
						Destroy(blackPieces[x][i].obj);
					}
				}
			}

			StartSimulation();
		}

	}

	IEnumerator moveByAI() {


		for (int epoch = 0; epoch < numberOfEpochs; epoch++) {

		for (int i = 0; i < turnsAllowedPerGame; i++) {

		//Max Number Of Turns Per Game
        for (int num = 0; num < numberOfGames; num++) {
			currentGame = num;

			if (gameEndEarly[currentGame]) {
				continue;
			}	

			GetNetInput();
			output = blackNets[num].FeedForward (input);
			HandleAIDecision(blackPieces[num], -1);
			
			if (gameEndEarly[currentGame]) {
				continue;
			}
		
			GetNetInput();
			output = redNets[num].FeedForward (input);
			HandleAIDecision(redPieces[num], 1);

		}

			yield return null;
	
		}

        for (int num = 0; num < numberOfGames; num++) {

			currentGame = num;

			//After Each Game
			blackNets[num].score = getScore(true);
			redNets[num].score = getScore(false);

		//Destroy Pieces
		for (int i = 0; i < 12; i++) {
			if (redPieces[num][i].obj != null) {
				Destroy(redPieces[num][i].obj);
			}
			if (blackPieces[num][i].obj != null) {
				Destroy(blackPieces[num][i].obj);
			}
		}

		//RESET GAME
		RestartSimulation();

	    }
		totalRedScore = 0;
		totalBlackScore = 0;
		for (int i = 0; i < redNets.Length; i++) {

			if (redNets[i].score > totalBestScore) {
				totalBestScore = redNets[i].score;
			} 
			if (blackNets[i].score > totalBestScore) {
				totalBestScore = blackNets[i].score;
			} 

			totalRedScore += redNets[i].score;
			totalBlackScore += blackNets[i].score;
			if (totalRedScore > redNets[i].score) {
				if (totalRedScore > bestScore) {
					bestScore = totalRedScore;
				}
			}
			if (totalBlackScore > blackNets[i].score) {
				if (totalBlackScore > bestScore) {
					bestScore = totalBlackScore;

					//blackNets[i].GetWeights();

				}

			}
		}

		//AFTER ALL GAMES, GENERATE NEW WEIGHTS
		GenerateNextGeneration();
		currentGeneration ++;

			for (int i = 0; i < redNets.Length; i++) {

				blackNetsChildren[i].GetWeights();
				blackNets[i] = blackNetsChildren[i];

			}
		}


	}

	public int getScore(bool side) {
		int s = 0;
		if (side) {
			for (int i = 0; i < 12; i++) {
				if (blackPieces[currentGame][i].state == 0) {
					s -= 1;
				}
				if (redPieces[currentGame][i].state == 0) {
					s += 2;
				}
			}
		}
		else {
			for (int i = 0; i < 12; i++) {
				if (redPieces[currentGame][i].state == 0) {
					s -= 1;
				}
				if (blackPieces[currentGame][i].state == 0) {
					s += 2;
				}
			}
		}
		if (s < 0) {
			s = 0;
		}
		return s;
	}

	public void GetNetInput() {

		//LOOP THROUGH EVERY SPOT ON THE BOARD
		int num = 0;
		for (int i = 0; i < 4; i++) {
			for (int j = 0; j < 8; j++) {

				input[num] = 0;

				//LOOP THROUGH EVERY PIECE
				for (int k = 0; k < 12; k++) {
					if (redPieces[currentGame][k].posX == i && redPieces[currentGame][k].posY == j) {
						if (redPieces[currentGame][k].state != 0) {
							if (redPieces[currentGame][k].state == 2) {
								input[num] = 1f;
							}
							else {
								input[num] = 0.5f;
							}
						}
					}

					if (blackPieces[currentGame][k].posX == i && blackPieces[currentGame][k].posY == j) {
						if (blackPieces[currentGame][k].state != 0) {
							if (blackPieces[currentGame][k].state == -2) {
								input[num] = -1f;
							}
							else {
								input[num] = -0.5f;
							}
						}
					}
				}

				num ++;
			}
		}


	}

	public void GetMiniMaxInput(int depth) {

		input = new float[32];

		//LOOP THROUGH EVERY SPOT ON THE BOARD
		int num = 0;
		for (int i = 0; i < 4; i++) {
			for (int j = 0; j < 8; j++) {

				//LOOP THROUGH EVERY PIECE
				for (int k = 0; k < 12; k++) {
					if (miniMaxRedPieces[depth][k].posX == i && miniMaxRedPieces[depth][k].posY == j) {
						if (miniMaxRedPieces[depth][k].state != 0) {
							if (miniMaxRedPieces[depth][k].state == 2) {
								input[num] = 1f;
							}
							else {
								input[num] = 0.5f;
							}
						}
					}

					if (miniMaxBlackPieces[depth][k].posX == i && miniMaxBlackPieces[depth][k].posY == j) {
						if (miniMaxBlackPieces[depth][k].state != 0) {
							if (miniMaxBlackPieces[depth][k].state == -2) {
								input[num] = -1f;
							}
							else {
								input[num] = -0.5f;
							}
						}
					}
				}

				num ++;
			}
		}
	}


	public bool LookAhead(float currentScore, int noImprovement, bool team, float numberOfTurnsElapsed) {
		
		float t = numberOfTurnsElapsed;
		
		if (currentScore == 0) {
			lookaroundCount ++;
		}

		if (lookaroundCount >= lookaroundLimit) {
			return false;
		}
		int turnsWithNoImprovement = noImprovement;
		Piece[] pieces = new Piece[12];
		if (!team) {
			pieces = blackPieces[0];
		}
		else {
			pieces = redPieces[0];
		}

				int index = 0;
				int index2 = 0;
				GetNetInput();

					if (TryMovePiece(pieces[index], index2)) {
						MovePiece(pieces[index], index2);

						t += 0.5f;
						
						if (currentScore >= getScoreDif2()) {
							turnsWithNoImprovement = 0;
							if (LookAhead(getScoreDif2(), turnsWithNoImprovement, !team, t)) {
								lookaroundWins ++;
							}
						}
						else {
							return false;
						}

						if (currentScore == getScoreDif2()) {
							turnsWithNoImprovement ++;
						}
						
						if (turnsWithNoImprovement >= 5) {
							//EXIT FROM THIS
							return false;
						}

						if (t > 100) {
							return false;
						}
					}
			
		
		return true;
	}

	IEnumerator trainByLookAhead() {
		int winninGames = 0;
		//while (winninGames < 1) {
		StartSimulation();
		GetNetInput();

		float currentScore = 1.2f;
		int turnsWithNoImprovement = 0;
		for (int i = 0; i < 12; i++) {	
			for (int j = 0; j < 4; j++) {
				if (LookAhead(0, 0, false, 0)) {
					Debug.Log("Win");
				}
				else {
					Debug.Log("Failure");
				}

				if (redPieces[0][i].obj != null) {
					Destroy(redPieces[0][i].obj);
				}
				if (blackPieces[0][i].obj != null) {
					Destroy(blackPieces[0][i].obj);
				}
				RestartSimulation();
			}
		}
		
		for (int i = 0; i < 12; i++) {
			if (redPieces[0][i].obj != null) {
				Destroy(redPieces[0][i].obj);
			}
			if (blackPieces[0][i].obj != null) {
				Destroy(blackPieces[0][i].obj);
			}
		}

		RestartSimulation();
		
		yield return null;

		//}

	}


	//LOOP THROUGH EVERY PIECE WHEN RUNNING THIS FUNCTION
	public bool getMiniMax(Piece pieceToCheck, int direction, int team, int depth) {
		
		if (pieceToCheck.state == 0) {
			return false;
		}
		
		//GET INPUT (BOARD STATE)
		GetMiniMaxInput(depth);
		pieceToCheck.GetLocationFromDirection(direction);
		if (TryMovePieceMiniMax(pieceToCheck, direction, depth)) {
			MovePiece(pieceToCheck, direction);
			
			if (team == -1) {
				curScore = getScoreDif(depth);
				if (posScore < getScoreDif(depth)) {
					posScore = getScoreDif(depth);
					return true;
				}
				else {
					return false;
				}
			}
			else {
				if (negScore < -getScoreDif(depth)) {
					negScore = -getScoreDif(depth);
					return true;
				}
				else {
					return false;
				}	
			}
		}
		return false;
	}

	public float getScoreDif(int depth) {
		float s = 0;
		for (int i = 0; i < 12; i++) {
			if (miniMaxRedPieces[depth][i].state == 0) {
				s += 1;
			}
			else if (miniMaxBlackPieces[depth][i].state == -2) {
				s += 2;
			}
			//if (miniMaxRedPieces[depth][i].state == 1) {
			//	s -= 0.9f;
			//}
			//else if (miniMaxRedPieces[depth][i].state == 2) {
			//	s -= 1.9f;
			//}
		}
		return s;
	}

	public float getScoreDif2() {
		float s = 0;
		for (int i = 0; i < 12; i++) {
			if (blackPieces[currentGame][i].state == -1) {
				s += 1;
			}
			else if (blackPieces[currentGame][i].state == -2) {
				s += 2;
			}
			if (redPieces[currentGame][i].state == 1) {
				s -= 0.9f;
			}
			else if (redPieces[currentGame][i].state == 2) {
				s -= 1.9f;
			}
		}
		return s;
	}


	public int getNextHighestDir(int index) {
		
		float[] direction = new float[4];
		for (int i = 0; i < direction.Length; i++) {
			direction[i] = output[12+i];
		}
		List<float> A = new List<float>(direction);
		

		var sorted = A
    		.Select((x, i) => new KeyValuePair<float, int>(x, i))
    		.OrderBy(x => x.Key)
    		.ToList();

		//List<float> B = sorted.Select(x => x.Key).ToList();
		List<int> idx = sorted.Select(x => x.Value).ToList();
		idx.Reverse(); 
		return idx[index];
	}

	public int getNextHighestNode(int index, float[]t) {
		
		float[] node = new float[12];
		string str = "";
		for (int i = 0; i < node.Length; i++) {
			node[i] = t[i];
			str += t[i];
			
		}
		if (test) {
			Debug.Log (str);
			test = false;
		}
		List<float> A = new List<float>(node);
		

		var sorted = A
    		.Select((x, i) => new KeyValuePair<float, int>(x, i))
    		.OrderBy(x => x.Key)
    		.ToList();

		List<int> idx = sorted.Select(x => x.Value).ToList();
		idx.Reverse(); 
		return idx[index];
	}

	public void HandleAIDecision(Piece[] pieces, int team) {

		bool turnOn = true;
		//CHECK FOR POSSIBLE JUMP
		if (turnOn) {
		float p1 = UnityEngine.Random.Range(numberOfEpochs/3, numberOfEpochs+1);
		//if (!neural) {
		if (!neural || (team == -1 && p1 < currentGeneration) || (team == 1 && p1 < currentGeneration/2)) {
		for (int num = 0; num < 12; num++) {
			if (team > 0) {
				for (int direction = 0; direction < 4; direction++) {
					if (CheckIfJumpAvailable(redPieces[currentGame][num], direction)) {
						MovePiece(redPieces[currentGame][num], direction);
						return;
					}
				}
			}
			else {
				for (int direction = 0; direction < 4; direction++) {
					if (CheckIfJumpAvailable(blackPieces[currentGame][num], direction)) {
						MovePiece(blackPieces[currentGame][num], direction);
						return;
					}
				}
			}
		}

		}
		}


		for (int i = 0; i < 12; i++) {	
			int index = getNextHighestNode(i, output);
			for (int j = 0; j < 4; j++) {
				int index2 = getNextHighestDir(j);
				if (TryMovePiece(pieces[index], index2)) {
					NextBlackToMove = index;
					NextDirectionToMove = index2;
					MovePiece(pieces[index], index2);
					return;
				}
			}
		}

		//NOT WORKING
		gameEndEarly[currentGame] = true;
		
	}

	public bool TryMovePieceMiniMax(Piece piece, int dir, int depth) {

		if (piece.state == 0) {
			return false;
		}

		piece.GetLocationFromDirection(dir);
		if (CheckAvailableMiniMaxMoves(piece, dir, depth)) {
			return true;
		}
		else {
			return false;
		}

	}

	public bool TryMovePiece(Piece piece, int dir) {

		if (piece.state == 0) {
			return false;
		}

		piece.GetLocationFromDirection(dir);
		if (CheckAvailableMoves(piece, dir)) {
			return true;
		}
		else {
			return false;
		}

	}

	public void MovePiece(Piece piece, int dir) {

		int newX = piece.desiredPosX.Value;
		int newY = piece.desiredPosY.Value;
		if (piece.jumpPosX != null && piece.jumpPosY != null) {
			newX = piece.jumpPosX.Value;
			newY = piece.jumpPosY.Value;
		}
		
		//Debug.Log("MOVEMENT");
		if(newY%2 == 0){
			piece.obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3 + board[currentGame].GetComponent<RectTransform>().rect.height/8f + newX*board[currentGame].GetComponent<RectTransform>().rect.width/4f, 5 - newY*board[currentGame].GetComponent<RectTransform>().rect.height/8f);
		}
		else {
			piece.obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-3 + newX*board[currentGame].GetComponent<RectTransform>().rect.width/4f, 5 - newY*board[currentGame].GetComponent<RectTransform>().rect.height/8f);
		}
		piece.posX = newX;
		piece.posY = newY;
		if (piece.state > 0) {
			if (newY == 7) {
				piece.state = 2;
			}
		}
		else if (piece.state < 0) {
			if (newY == 0) {
				piece.state = -2;
			}
		}
		piece.desiredPosX = null;
		piece.desiredPosY = null;
		piece.jumpPosX = null;
		piece.jumpPosY = null;
	}


	public bool CheckAvailableMoves(Piece piece, int dir) {

		int desiredPosX = piece.desiredPosX.Value;
		int desiredPosY = piece.desiredPosY.Value;
		if (piece.jumpPosX != null && piece.jumpPosY != null) {
			desiredPosX = piece.jumpPosX.Value;
			desiredPosY = piece.jumpPosY.Value;
		}

		//PREVENTS MOVING UP FOR RED AND DOWN FOR BLACK
        if (!(desiredPosY < piece.posY && piece.state == 1) && !(desiredPosY > piece.posY && piece.state == -1)) { 
            //CHECKS IF A PIECE WOULD MOVE OFF THE BOARD
            if (desiredPosX >= 0 && desiredPosX <= 3 && desiredPosY >= 0 && desiredPosY <= 7) {
                //CHECKS IF A PIECE OF THE SAME COLOR IS BLOCKING
                for (int num = 0; num < 12; num++) {
                    if (piece.state > 0) {
                        if (redPieces[currentGame][num].posX == desiredPosX && redPieces[currentGame][num].posY == desiredPosY && redPieces[currentGame][num].state != 0) {
							piece.jumpPosX = null;
							piece.jumpPosY = null;
                            return false;
                        }
                    }
                    else {
                        if (blackPieces[currentGame][num].posX == desiredPosX && blackPieces[currentGame][num].posY == desiredPosY && blackPieces[currentGame][num].state != 0) {
							piece.jumpPosX = null;
							piece.jumpPosY = null;
                            return false;
                        }   
                    }
                }

                //CHECKS IF A PIECE OF THE OPPOSITE COLOR IS IN DESIRED POSITION
            	if (piece.state > 0) {
                	for (int num = 0; num < 12; num++) {
                    	if (blackPieces[currentGame][num].posX == desiredPosX && blackPieces[currentGame][num].posY == desiredPosY && blackPieces[currentGame][num].state != 0) {
                        	//POSSIBLE JUMP MOVE (MIGHT BE BLOCKED)

							//This Piece is Blocking the Jump of another
							if (piece.jumpPosX != null || piece.jumpPosY != null) {
								piece.jumpPosX = null;
								piece.jumpPosY = null;
								return false;
							}

							piece.GetLocationFromJump(dir);
							if (CheckAvailableMoves(piece, dir)) {
								//CAN JUMP
								blackPieces[currentGame][num].state = 0;
								blackPieces[currentGame][num].desiredPosX = null;
								blackPieces[currentGame][num].desiredPosY = null;
								blackPieces[currentGame][num].jumpPosX = null;
								blackPieces[currentGame][num].jumpPosY = null;
								Destroy(blackPieces[currentGame][num].obj);
								return true;
							}
							else {
								piece.jumpPosX = null;
								piece.jumpPosY = null;
								return false;
							}
                        }
					}
				}
				else {
					for (int num = 0; num < 12; num++) {
                    	if (redPieces[currentGame][num].posX == desiredPosX && redPieces[currentGame][num].posY == desiredPosY && redPieces[currentGame][num].state != 0) {
                        	//POSSIBLE JUMP MOVE (MIGHT BE BLOCKED)
							
							//This Piece is Blocking the Jump of another
							if (piece.jumpPosX != null || piece.jumpPosY != null) {
								piece.jumpPosX = null;
								piece.jumpPosY = null;
								return false;
							}

							piece.GetLocationFromJump(dir);
							if (CheckAvailableMoves(piece, dir)) {
								//CAN JUMP
								redPieces[currentGame][num].state = 0;
								redPieces[currentGame][num].desiredPosX = null;
								redPieces[currentGame][num].desiredPosY = null;
								redPieces[currentGame][num].jumpPosX = null;
								redPieces[currentGame][num].jumpPosY = null;
								Destroy(redPieces[currentGame][num].obj);
								return true;
							}
							else {
								piece.jumpPosX = null;
								piece.jumpPosY = null;
								return false;
							}
                        }
					}
				}

				//CAN MOVE IN THAT DIRECTION
				return true;
			}
			else {
				piece.jumpPosX = null;
				piece.jumpPosY = null;
				return false;
			}
		}
		else {
			piece.jumpPosX = null;
			piece.jumpPosY = null;
			return false;
		}
	}


	public bool CheckIfJumpAvailable(Piece piece, int dir) {

		if (piece.state == 0) {
			piece.jumpPosX = null;
			piece.jumpPosY = null;
			return false;
		}

		piece.GetLocationFromDirection(dir);
		int desiredPosX = piece.desiredPosX.Value;
		int desiredPosY = piece.desiredPosY.Value;
		if (piece.jumpPosX != null && piece.jumpPosY != null) {
			desiredPosX = piece.jumpPosX.Value;
			desiredPosY = piece.jumpPosY.Value;
		}

		if (!(desiredPosY < piece.posY && piece.state == 1) && !(desiredPosY > piece.posY && piece.state == -1)) { 
            //CHECKS IF A PIECE WOULD MOVE OFF THE BOARD
            if (desiredPosX >= 0 && desiredPosX <= 3 && desiredPosY >= 0 && desiredPosY <= 7) {
                //CHECKS IF A PIECE OF THE SAME COLOR IS BLOCKING
                for (int num = 0; num < 12; num++) {
                    if (piece.state > 0) {
                        if (redPieces[currentGame][num].posX == desiredPosX && redPieces[currentGame][num].posY == desiredPosY && redPieces[currentGame][num].state != 0) {
							piece.jumpPosX = null;
							piece.jumpPosY = null;
                            return false;
                        }
                    }
                    else {
                        if (blackPieces[currentGame][num].posX == desiredPosX && blackPieces[currentGame][num].posY == desiredPosY && blackPieces[currentGame][num].state != 0) {
							piece.jumpPosX = null;
							piece.jumpPosY = null;
                            return false;
                        }   
                    }
                }


				if (piece.state > 0) {
                	for (int num = 0; num < 12; num++) {
						bool freeSpot = true;
                    	if (blackPieces[currentGame][num].posX == desiredPosX && blackPieces[currentGame][num].posY == desiredPosY && blackPieces[currentGame][num].state != 0) {
                        	//POSSIBLE JUMP MOVE (MIGHT BE BLOCKED)

							freeSpot = false;

							//This Piece is Blocking the Jump of another
							if (piece.jumpPosX != null || piece.jumpPosY != null) {
								piece.jumpPosX = null;
								piece.jumpPosY = null;
								return false;
							}

							piece.GetLocationFromJump(dir);
							if (CheckIfJumpAvailable(piece, dir)) {

								//CAN JUMP
								blackPieces[currentGame][num].state = 0;
								blackPieces[currentGame][num].desiredPosX = null;
								blackPieces[currentGame][num].desiredPosY = null;
								blackPieces[currentGame][num].jumpPosX = null;
								blackPieces[currentGame][num].jumpPosY = null;
								Destroy(blackPieces[currentGame][num].obj);
								return true;
							}
							else {
								piece.jumpPosX = null;
								piece.jumpPosY = null;
								return false;
							}
                        }
						if (freeSpot) {
							if (piece.jumpPosX != null && piece.jumpPosY != null) {
								return true;
							}
						}
					}
				}
				else {
					for (int num = 0; num < 12; num++) {
						bool freeSpot = true;
                    	if (redPieces[currentGame][num].posX == desiredPosX && redPieces[currentGame][num].posY == desiredPosY && redPieces[currentGame][num].state != 0) {
                        	//POSSIBLE JUMP MOVE (MIGHT BE BLOCKED)
							
							freeSpot = false;

							//This Piece is Blocking the Jump of another
							if (piece.jumpPosX != null || piece.jumpPosY != null) {
								piece.jumpPosX = null;
								piece.jumpPosY = null;
								return false;
							}

							piece.GetLocationFromJump(dir);
							if (CheckIfJumpAvailable(piece, dir)) {

								//CAN JUMP
								redPieces[currentGame][num].state = 0;
								redPieces[currentGame][num].desiredPosX = null;
								redPieces[currentGame][num].desiredPosY = null;
								redPieces[currentGame][num].jumpPosX = null;
								redPieces[currentGame][num].jumpPosY = null;
								Destroy(redPieces[currentGame][num].obj);
								return true;
							}
							else {
								piece.jumpPosX = null;
								piece.jumpPosY = null;
								return false;
							}
                        }
						if (freeSpot) {
							if (piece.jumpPosX != null && piece.jumpPosY != null) {
								return true;
							}
						}
					}
				}

				piece.jumpPosX = null;
				piece.jumpPosY = null;
				return false;

			}
			else {
				piece.jumpPosX = null;
				piece.jumpPosY = null;
				return false;
			}
		}
		else {
			piece.jumpPosX = null;
			piece.jumpPosY = null;
			return false;
		}
	}









	public bool CheckAvailableMiniMaxMoves(Piece piece, int dir, int depth) {

		int desiredPosX = piece.desiredPosX.Value;
		int desiredPosY = piece.desiredPosY.Value;
		if (piece.jumpPosX != null && piece.jumpPosY != null) {
			desiredPosX = piece.jumpPosX.Value;
			desiredPosY = piece.jumpPosY.Value;
		}

		//PREVENTS MOVING UP FOR RED AND DOWN FOR BLACK
        if (!(desiredPosY < piece.posY && piece.state == 1) && !(desiredPosY > piece.posY && piece.state == -1)) { 

            //CHECKS IF A PIECE WOULD MOVE OFF THE BOARD
            if (desiredPosX >= 0 && desiredPosX <= 3 && desiredPosY >= 0 && desiredPosY <= 7) {
                //CHECKS IF A PIECE OF THE SAME COLOR IS BLOCKING
                for (int num = 0; num < 12; num++) {
                    if (piece.state > 0) {
                        if (miniMaxRedPieces[depth][num].posX == desiredPosX && miniMaxRedPieces[depth][num].posY == desiredPosY && miniMaxRedPieces[depth][num].state != 0) {
							piece.jumpPosX = null;
							piece.jumpPosY = null;
                            return false;
                        }
                    }
                    else {
                        if (miniMaxBlackPieces[depth][num].posX == desiredPosX && miniMaxBlackPieces[depth][num].posY == desiredPosY && miniMaxBlackPieces[depth][num].state != 0) {
							piece.jumpPosX = null;
							piece.jumpPosY = null;
                            return false;
                        }   
                    }
                }

                //CHECKS IF A PIECE OF THE OPPOSITE COLOR IS IN DESIRED POSITION
            	if (piece.state > 0) {
                	for (int num = 0; num < 12; num++) {
                    	if (miniMaxBlackPieces[depth][num].posX == desiredPosX && miniMaxBlackPieces[depth][num].posY == desiredPosY && miniMaxBlackPieces[depth][num].state != 0) {
                        	//POSSIBLE JUMP MOVE (MIGHT BE BLOCKED)

							//This Piece is Blocking the Jump of another
							if (piece.jumpPosX != null || piece.jumpPosY != null) {
								piece.jumpPosX = null;
								piece.jumpPosY = null;
								return false;
							}

							piece.GetLocationFromJump(dir);
							if (CheckAvailableMiniMaxMoves(piece, dir, depth)) {
								//CAN JUMP
								miniMaxBlackPieces[depth][num].state = 0;
								miniMaxBlackPieces[depth][num].desiredPosX = null;
								miniMaxBlackPieces[depth][num].desiredPosY = null;
								miniMaxBlackPieces[depth][num].jumpPosX = null;
								miniMaxBlackPieces[depth][num].jumpPosY = null;
								return true;
							}
							else {
								piece.jumpPosX = null;
								piece.jumpPosY = null;
								return false;
							}
                        }
					}
				}
				else {
					for (int num = 0; num < 12; num++) {
                    	if (miniMaxRedPieces[depth][num].posX == desiredPosX && miniMaxRedPieces[depth][num].posY == desiredPosY && miniMaxRedPieces[depth][num].state != 0) {
                        	//POSSIBLE JUMP MOVE (MIGHT BE BLOCKED)
							
							//This Piece is Blocking the Jump of another
							if (piece.jumpPosX != null || piece.jumpPosY != null) {
								piece.jumpPosX = null;
								piece.jumpPosY = null;
								return false;
							}

							piece.GetLocationFromJump(dir);
							if (CheckAvailableMiniMaxMoves(piece, dir, depth)) {
								//CAN JUMP
								miniMaxRedPieces[depth][num].state = 0;
								miniMaxRedPieces[depth][num].desiredPosX = null;
								miniMaxRedPieces[depth][num].desiredPosY = null;
								miniMaxRedPieces[depth][num].jumpPosX = null;
								miniMaxRedPieces[depth][num].jumpPosY = null;
								return true;
							}
							else {
								piece.jumpPosX = null;
								piece.jumpPosY = null;
								return false;
							}
                        }
					}
				}

				//CAN MOVE IN THAT DIRECTION
				return true;
			}
			else {
				piece.jumpPosX = null;
				piece.jumpPosY = null;
				return false;
			}
		}
		else {
			piece.jumpPosX = null;
			piece.jumpPosY = null;
			return false;
		}
	}




}



public class Piece
{

	public int posX;
	public int posY;
	public int state;
	public GameObject obj;
	public int? desiredPosX;
	public int? desiredPosY;
	public int? jumpPosX;
	public int? jumpPosY;

	public Piece(int i, int j, int state, GameObject obj)
    {
		this.posX = i;
		this.posY = j;
		this.state = state;
		this.obj = obj;
	}

	public void GetLocationFromDirection(int num) {
		int newPosX;
		int newPosY;
		if (num == 0) {

			newPosY = this.posY - 1;
			if ((this.posY+1) % 2 == 0) {
				newPosX = this.posX - 1;
			}
			else {
				newPosX = this.posX;
			}

		}
		else if (num == 1) {

			newPosY = this.posY - 1;
			if ((this.posY+1) % 2 == 0) {
				newPosX = this.posX;
			}
			else {
				newPosX = this.posX + 1;
			}

		}

		else if (num == 2) {

			newPosY = this.posY + 1;
			if ((this.posY+1) % 2 == 0) {
				newPosX = this.posX - 1;
			}
			else {
				newPosX = this.posX;
			}

		}

		else {

			newPosY = this.posY + 1;
			if ((this.posY+1) % 2 == 0) {
				newPosX = this.posX;
			}
			else {
				newPosX = this.posX + 1;
			}

		}

		this.desiredPosX = newPosX;
		this.desiredPosY = newPosY;

	}

	public void GetLocationFromJump(int num) {
		int newPosX;
		int newPosY;
		if (num == 0) {

			newPosY = this.desiredPosY.Value - 1;
			if ((this.desiredPosY+1) % 2 == 0) {
				newPosX = this.desiredPosX.Value - 1;
			}
			else {
				newPosX = this.desiredPosX.Value;
			}

		}
		else if (num == 1) {

			newPosY = this.desiredPosY.Value - 1;
			if ((this.desiredPosY.Value+1) % 2 == 0) {
				newPosX = this.desiredPosX.Value;
			}
			else {
				newPosX = this.desiredPosX.Value + 1;
			}

		}

		else if (num == 2) {

			newPosY = this.desiredPosY.Value + 1;
			if ((this.desiredPosY.Value+1) % 2 == 0) {
				newPosX = this.desiredPosX.Value - 1;
			}
			else {
				newPosX = this.desiredPosX.Value;
			}

		}

		else {

			newPosY = this.desiredPosY.Value + 1;
			if ((this.desiredPosY.Value+1) % 2 == 0) {
				newPosX = this.desiredPosX.Value;
			}
			else {
				newPosX = this.desiredPosX.Value + 1;
			}

		}

		this.jumpPosX = newPosX;
		this.jumpPosY = newPosY;
		//Debug.Log("CURRENT 2: " + this.posX + ", " + this.posY + " ---- DESIRED: " + this.desiredPosX + ", " + this.desiredPosY);

	}

}