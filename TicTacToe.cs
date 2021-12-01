using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToe : MonoBehaviour {

	public bool neural;

	NeuralNetwork[] first;
	NeuralNetwork[] second;


	NeuralNetwork[] firstChildren;
	NeuralNetwork[] secondChildren;
	
	public GameObject[] board;
	public int numberOfEpochs = 100;
	public int numberOfGames = 4;
	public int currentGeneration = 0;

	public float[] input;
	public float[] output;
	public int[][,] boardLayout;
	public int currentGame = 0;
	private bool[] gameEndEarly;

	public GameObject[][] pieces;
	public GameObject redPiece;
	public GameObject blackPiece;
	public GameObject boardGenerator;

	public int totalRedScore = 0;
	public int totalBlackScore = 0;

	// Use this for initialization
	void Start () {
		first = new NeuralNetwork[numberOfGames];
		second = new NeuralNetwork[numberOfGames];

		firstChildren = new NeuralNetwork[numberOfGames];
		secondChildren = new NeuralNetwork[numberOfGames];

		boardLayout = new int[numberOfGames][,];
		gameEndEarly = new bool[numberOfGames];
		pieces = new GameObject[numberOfGames][];
		for (int num = 0; num < numberOfGames; num++) {
			NeuralNetwork temp = new NeuralNetwork(new int[] { 9, 9, 9 }); //initialize network
			first[num] = temp;
			NeuralNetwork temp2 = new NeuralNetwork(new int[] { 9, 9, 9 }); //initialize network
			second[num] = temp2;
			boardLayout[num] = new int[3, 3];
			pieces[num] = new GameObject[9];
		}
		board = boardGenerator.GetComponent<BoardGenerator>().GenerateImageView();
		input = new float[9];
		output = new float[9];


		if (!neural) {
			StartCoroutine(moveByAI());
		}
		else {
			StartCoroutine(trainByAI());
		}

	}
	
	public void GetNetInput() {

		int num = 0;
		for (int j = 2; j >= 0; j--) {
            for (int i = 0; i < 3; i++) {
               input[num] = (float)boardLayout[currentGame][i,j];
			   num ++;
            }
        }
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {

			for (int w = 0; w < 3; w++) {
				first[w].GetWeights();
				for (int k = 0; k < first[w].weights.Length - 1; k++) {
					var msg = "Node " + w + " LAYER " + k + " WEIGHTS: ";
					for (int j2 = 0; j2 < first[w].weights[k].GetLength(0); j2++) {
  						for (int i2 = 0; i2 < first[w].weights[k].GetLength(1); i2++) {
    						msg += first[w].weights[k][j2, i2].ToString() + ", ";
  						}
					}
					Debug.Log(msg);
				}
			}
        }
	}

	public bool blockCheck() {

		if (checkForWinningMove(1) != -1) {
							//Debug.Log("FOUND BLOCK MOVE");
							GetNetInput();
							
							output = first[0].FeedForward (input);
							float[] tmp = new float[9];
							int num = 0;
							for (int j = 2; j >= 0; j--) {
            					for (int k = 0; k < 3; k++) {
									if (num == checkForWinningMove(1)) {
										tmp[num] = 1;
									}
									num ++;
								}
							}

							first[0].BackProp (tmp);
							output = tmp;
							HandleAIDecision(false);
							return true;
						}
						else {
							return false;
						}
		
	}

	public bool moveCheck() {
						//FINISH MOVE FOUND
						if (checkForWinningMove(-1) != -1) {
							//Debug.Log("FOUND MOVE");
							GetNetInput();

							output = first[0].FeedForward (input);
							float[] tmp = new float[9];
							int num = 0;
							string str = "";
							for (int j = 2; j >= 0; j--) {
            					for (int k = 0; k < 3; k++) {
									str += boardLayout[0][k,j] + ", ";
									if (num == checkForWinningMove(-1)) {
										tmp[num] = 1;
									}
									num ++;
								}
							}

							first[0].BackProp (tmp);
							output = tmp;
							HandleAIDecision(false);
							return true;
						}
						else {
							return false;
						}
	}

	public int checkForWinningMove(int team) {

		int[,] boardLayout2 = new int[3, 3];

		for (int num2 = 0; num2 < 9; num2++) {
		
		for (int j = 2; j >= 0; j--) {
            for (int i = 0; i < 3; i++) {
				boardLayout2[i,j] = boardLayout[0][i,j];
			}
		}
		int x = 0;
		for (int j = 2; j >= 0; j--) {
            for (int i = 0; i < 3; i++) {
				if (x == num2) {
					if (boardLayout2[i,j] == 0) {
						boardLayout2[i,j] = team;
					}
				}
				x ++;
			}
		}

		if ((boardLayout2[0,0] == team) && (boardLayout2[1,0] == team) && (boardLayout2[2,0] == team)) {
			gameEndEarly[currentGame] = true;
			return num2;
		}
		else if ((boardLayout2[0,1] == team) && (boardLayout2[1,1] == team) && (boardLayout2[2,1] == team)) {
			gameEndEarly[currentGame] = true;
			return num2;
		}
		else if ((boardLayout2[0,2] == team) && (boardLayout2[1,2] == team) && (boardLayout2[2,2] == team)) {
			gameEndEarly[currentGame] = true;
			return num2;
		}
		else if ((boardLayout2[0,0] == team) && (boardLayout2[0,1] == team) && (boardLayout2[0,2] == team)) {
			gameEndEarly[currentGame] = true;
			return num2;
		}
		else if ((boardLayout2[1,0] == team) && (boardLayout2[1,1] == team) && (boardLayout2[1,2] == team)) {
			gameEndEarly[currentGame] = true;
			return num2;
		}
		else if ((boardLayout2[2,0] == team) && (boardLayout2[2,1] == team) && (boardLayout2[2,2] == team)) {
			gameEndEarly[currentGame] = true;
			return num2;
		}
		else if ((boardLayout2[0,0] == team) && (boardLayout2[1,1] == team) && (boardLayout2[2,2] == team)) {
			gameEndEarly[currentGame] = true;
			return num2;
		}
		else if ((boardLayout2[0,2] == team) && (boardLayout2[1,1] == team) && (boardLayout2[2,0] == team)) {
			gameEndEarly[currentGame] = true;
			return num2;
		}


		}
		return -1;
	}

	IEnumerator trainByAI() {
			for (int i = 0; i < numberOfEpochs; i++) {
				
				currentGeneration = i;

				second[0] = new NeuralNetwork(new int[] { 9, 9, 9 });
				yield return null;

				//RESET FOR NEXT TRAINING SESSION
				boardLayout = new int[numberOfGames][,];
				boardLayout[0] = new int[3, 3];
				gameEndEarly[0] = false;
				for (int x = 0; x < 9; x++) {
					Destroy(pieces[0][x]);
				}

				//PLAYER
				GetNetInput();
				output = first[0].FeedForward (input);
				first[0].BackProp (new float[] { 1, 0, 0, 0, 0, 0, 0, 0 ,0 });
				output = new float[] { 1, 0, 0, 0, 0, 0, 0, 0 ,0 };
				HandleAIDecision(false);
				
				//AI
				GetNetInput();
				output = second[0].FeedForward (input);
				HandleAIDecision(true);
				
				if (boardLayout[0][1,1] != 1) { 

					//PLAYER
					GetNetInput();
					output = first[0].FeedForward (input);
					first[0].BackProp (new float[] { 0, 0, 0, 0, 1, 0, 0, 0 ,0 });
					output = new float[] { 0, 0, 0, 0, 1, 0, 0, 0 ,0 };
					HandleAIDecision(false);

					//AI
					GetNetInput();
					output = second[0].FeedForward (input);
					HandleAIDecision(true);


					if (boardLayout[0][2, 0] != 1) { 

						//PLAYER
						GetNetInput();
						output = first[0].FeedForward (input);
						first[0].BackProp (new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 1 });
						output = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 1 };
						HandleAIDecision(false);

						//GAME WON
						continue;

					}
					else {

						if (boardLayout[0][0,0] != 1 && boardLayout[0][1,0] != 1) {

							GetNetInput();
							output = first[0].FeedForward (input);
							first[0].BackProp (new float[] { 0, 0, 1, 0, 0, 1, 0, 0, 0 });
							output = new float[] { 0, 0, 1, 0, 0, 1, 0, 0, 0 };
							HandleAIDecision(false);

						}

						else {

							GetNetInput();
							output = first[0].FeedForward (input);
							first[0].BackProp (new float[] { 0, 0, 0, 0, 0, 0, 1, 1, 0 });
							output = new float[] { 0, 0, 0, 0, 0, 0, 1, 1, 0 };
							HandleAIDecision(false);

						}

						GetNetInput();
						output = second[0].FeedForward (input);
						HandleAIDecision(true);

						if (moveCheck()) {
							//GAME WON
							continue;
						}
													
						if (!blockCheck()) {

							//NO SPECIAL MOVE
							GetNetInput();
							output = first[0].FeedForward (input);
							output = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
							HandleAIDecision(false);

						}

						//AI
						GetNetInput();
						output = second[0].FeedForward (input);
						HandleAIDecision(true);

						//FINAL MOVE
						GetNetInput();
						output = first[0].FeedForward (input);
						output = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
						HandleAIDecision(false);
						


					}



				}

				//THEY TOOK THE MIDDLE
				else {
					
					//PLAYER
					GetNetInput();
					output = first[0].FeedForward (input);
					first[0].BackProp (new float[] { 0, 0, 0, 0, 0, 0, 1, 0 ,0 });
					output = new float[] { 0, 0, 0, 0, 0, 0, 1, 0 ,0 };
					HandleAIDecision(false);

					//AI
					GetNetInput();
					output = second[0].FeedForward (input);
					HandleAIDecision(true);

					//CHECK FOR WIN
					if (moveCheck()) {
						continue;
					}

					if (!blockCheck()) {

						//NO SPECIAL MOVE
						GetNetInput();
						output = first[0].FeedForward (input);
						output = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
						HandleAIDecision(false);

					}

						
					//AI
					GetNetInput();
					output = second[0].FeedForward (input);
					HandleAIDecision(true);

					//CHECK FOR WIN
					if (moveCheck()) {
						continue;
					}

					if (!blockCheck()) {

							//NO SPECIAL MOVE
							GetNetInput();
							output = first[0].FeedForward (input);
							output = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
							HandleAIDecision(false);

						}
						
					//AI
					GetNetInput();
					output = second[0].FeedForward (input);
					HandleAIDecision(true);



				}
				
			}

			//CHECK WEIGHTS
			first[0].GetWeights();
			for (int k = 0; k < first[0].weights.Length - 1; k++) {
			var msg = "LAYER " + k + " WEIGHTS: ";
			for (int j2 = 0; j2 < first[0].weights[k].GetLength(0); j2++) {
  				for (int i2 = 0; i2 < first[0].weights[k].GetLength(1); i2++) {
    				msg += first[0].weights[k][j2, i2].ToString() + ", ";
  				}
			}
			Debug.Log(msg);

			boardLayout = new int[numberOfGames][,];
			for (int num = 0; num < numberOfGames; num++) {
				boardLayout[num] = new int[3, 3];
				gameEndEarly[num] = false;
				for (int i = 0; i < 9; i++) {
					Destroy(pieces[num][i]);
				}
			}
		}

		string huh = "CHECK THIS: ";
		GetNetInput();
		output = first[0].FeedForward (input);
		for (int i = 0; i < output.Length; i++) {
			huh += output[i] + ", ";
		}
		Debug.Log(huh);


			//CHECK RESULTS AFTER
			bool wasWinner = false;
				for (int i = 0; i < 9; i++) {
					for (int num = 0; num < numberOfGames; num++) {
						yield return new WaitForSeconds(0.5f);
						currentGame = num;
			
						if (gameEndEarly[currentGame]) {
							continue;
						}	

						GetNetInput();
						output = first[0].FeedForward (input);
						HandleAIDecision(false);
						if (CheckForWinner(-1)) {
							wasWinner = true;
							continue;
						}

						if (gameEndEarly[currentGame]) {
							continue;
						}	

						GetNetInput();
						output = second[num].FeedForward (input);
						HandleAIDecision(true);
						if (CheckForWinner(1)) {
							wasWinner = true;
							continue;
						}
					}
				}

				yield return null;



	}

	IEnumerator moveByAI() {

		for (int epoch = 0; epoch < numberOfEpochs; epoch++) {
			for (int num2 = 0; num2 < numberOfGames; num2++) {
				bool wasWinner = false;
				for (int i = 0; i < 9; i++) {
					for (int num = 0; num < numberOfGames; num++) {
						currentGame = num;
			
						if (gameEndEarly[currentGame]) {
							continue;
						}	

						GetNetInput();
						output = first[num].FeedForward (input);
						HandleAIDecision(false);
						if (CheckForWinner(-1)) {
							first[num].score += 5;
							wasWinner = true;
							continue;
						}

						if (gameEndEarly[currentGame]) {
							continue;
						}	

						//yield return new WaitForSeconds(1);

						GetNetInput();
						output = second[num2].FeedForward (input);
						HandleAIDecision(true);
						if (CheckForWinner(1)) {
							second[num2].score += 5;
							wasWinner = true;
							continue;
						}
				}

				yield return null;

			}

			boardLayout = new int[numberOfGames][,];
			for (int num = 0; num < numberOfGames; num++) {
				if (!wasWinner) {
					first[num].score += 2;
					second[num].score += 2;
				}
				boardLayout[num] = new int[3, 3];
				gameEndEarly[num] = false;
				for (int i = 0; i < 9; i++) {
					Destroy(pieces[num][i]);
				}
			}

			}
			



			GenerateNextGeneration();
			currentGeneration ++;

			for (int i = 0; i < first.Length; i++) {

				firstChildren[i].GetWeights();
				first[i] = firstChildren[i];
				//secondChildren[i].GetWeights();
				//second[i] = secondChildren[i];
				first[i].score = 0;
				second[i].score = 0;
			}


		}
	}


	public void HandleAIDecision(bool team) {

		for (int t = 0; t < 9; t++) {	
			int index = getNextHighestNode(t);
			int num = 0;
			for (int j = 2; j >= 0; j--) {
            	for (int i = 0; i < 3; i++) {
					if (num == index) {
						if (boardLayout[currentGame][i,j] == 0) {
							GameObject piece;
							if (team) {
								boardLayout[currentGame][i,j] = 1;
								piece = Instantiate(redPiece) as GameObject;
							}
							else {
								boardLayout[currentGame][i,j] = -1;
								piece = Instantiate(blackPiece) as GameObject;
							}
							pieces[currentGame][num] = piece;
							piece.transform.SetParent(board[currentGame].transform);
							piece.GetComponent<RectTransform>().anchoredPosition = new Vector2(3 * i * board[currentGame].GetComponent<RectTransform>().rect.width/8f, 3 * j * board[currentGame].GetComponent<RectTransform>().rect.height/8f - 45);
				
							return;
						}
					}
					num ++;
            	}
        	}
		}
	}

	public bool CheckForWinner(int team) {


		if ((boardLayout[currentGame][0,0] == team) && (boardLayout[currentGame][1,0] == team) && (boardLayout[currentGame][2,0] == team)) {
			gameEndEarly[currentGame] = true;
			return true;
		}
		else if ((boardLayout[currentGame][0,1] == team) && (boardLayout[currentGame][1,1] == team) && (boardLayout[currentGame][2,1] == team)) {
			gameEndEarly[currentGame] = true;
			return true;
		}
		else if ((boardLayout[currentGame][0,2] == team) && (boardLayout[currentGame][1,2] == team) && (boardLayout[currentGame][2,2] == team)) {
			gameEndEarly[currentGame] = true;
			return true;
		}
		else if ((boardLayout[currentGame][0,0] == team) && (boardLayout[currentGame][0,1] == team) && (boardLayout[currentGame][0,2] == team)) {
			gameEndEarly[currentGame] = true;
			return true;
		}
		else if ((boardLayout[currentGame][1,0] == team) && (boardLayout[currentGame][1,1] == team) && (boardLayout[currentGame][1,2] == team)) {
			gameEndEarly[currentGame] = true;
			return true;
		}
		else if ((boardLayout[currentGame][2,0] == team) && (boardLayout[currentGame][2,1] == team) && (boardLayout[currentGame][2,2] == team)) {
			gameEndEarly[currentGame] = true;
			return true;
		}
		else if ((boardLayout[currentGame][0,0] == team) && (boardLayout[currentGame][1,1] == team) && (boardLayout[currentGame][2,2] == team)) {
			gameEndEarly[currentGame] = true;
			return true;
		}
		else if ((boardLayout[currentGame][0,2] == team) && (boardLayout[currentGame][1,1] == team) && (boardLayout[currentGame][2,0] == team)) {
			gameEndEarly[currentGame] = true;
			return true;
		}
		else {
			return false;
		}

	}

	public int getNextHighestNode(int index) {
		
		float[] node = new float[9];
		float cumulative = 0;
		for (int i = 0; i < node.Length; i++) {
			node[i] = output[i];
			cumulative += output[i];
		}
		if (cumulative == 0) {
			return 0;
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


	void GenerateNextGeneration() {

		NeuralNetwork[] firstClone = (NeuralNetwork[])first.Clone();
		NeuralNetwork[] secondClone = (NeuralNetwork[])second.Clone();

		Array.Sort(firstClone,
    	delegate(NeuralNetwork x, NeuralNetwork y) { return x.score.CompareTo(y.score); });

		Array.Sort(secondClone,
    	delegate(NeuralNetwork x, NeuralNetwork y) { return x.score.CompareTo(y.score); });
		
		int s = 0;
		int s2 = 0;
		string huh = "SCORES: ";
		for (int i = 0; i < numberOfGames; i++) {
			s += firstClone[i].score;
			s2 += secondClone[i].score;
			huh += firstClone[i].score + " , ";
		}
		totalBlackScore = s;
		totalRedScore = s2;
		Debug.Log(huh);

		NeuralNetwork parent1 = firstClone[numberOfGames-1];
		NeuralNetwork parent2 = firstClone[numberOfGames-2];

		NeuralNetwork parent3 = secondClone[numberOfGames-1];
		NeuralNetwork parent4 = secondClone[numberOfGames-1];

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

				pSum += firstClone[j].score;
				//PICK BLACK PARENTS
				if (pSum >= p1) {
					if (!p1Chosen) {
						parent1 = firstClone[j];
						p1Chosen = true;
					}
				}
				if (pSum >= p2) {
					if (!p2Chosen) {
						if (parent1 != firstClone[j]) {
							parent2 = firstClone[j];
							p2Chosen = true;
						}
					}
				}

				pSum2 += secondClone[j].score;
				//PICK RED PARENTS
				if (pSum2 >= p3) {
					if (!p3Chosen) {
						parent3 = secondClone[j];
						p3Chosen = true;
					}
				}
				if (pSum2 >= p4) {
					if (!p4Chosen) {
						parent4 = secondClone[j];
						p4Chosen = true;
					}
				}

			}

			//MIX WEIGHTS TOGETHER
			parent1.GetWeights();
			parent2.GetWeights();
			parent3.GetWeights();
			parent4.GetWeights();

			firstChildren[i] = new NeuralNetwork(new int[] { 9, 9, 9 }); //initialize network
			firstChildren[i].GetWeightsFromParents(parent1.weights, parent2.weights, (float)parent1.score/((float)parent2.score+(float)parent1.score));

			secondChildren[i] = new NeuralNetwork(new int[] { 9, 9, 9 }); //initialize network
			second[i] = secondChildren[i];
			//redNetsChildren[i].GetWeightsFromParents(parent3.weights, parent4.weights, (float)parent3.score/((float)parent3.score+(float)parent4.score));

		}
	}




}
