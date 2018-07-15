using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BoardGenerator : MonoBehaviour {
	private int x = 9;
	private int y = 9;
	private Transform[,] board;
	private Transform[,] ballsMatrix;
	private List<Position> availablePositions;
	private List<Transform> destroyNodesList;
	private List<Transform> randomNodes;
	public Transform boardP;
	public Transform ball;
	public Camera cam;
	public Text gameOverText;
	public Text replaceText;
	private Transform startPoint = null;
	private Transform endPoint = null;
	private List<Node> globalPath;
	private List<Transform> randomDestroy;
	private bool colorIndicator = false;
	private bool ballIndicator = false;
	private bool moveIndicator = false;
	private bool destroyObjectsIndicator = false;
	private bool sizeIndicator = true;
	private bool callFirstTime = true;
	private bool canReplace = true;
	private bool gameOver = false;
	private Vector3 smallSphereSize = new Vector3(0.5f, 0.5f, 0.5f);
	private Vector3 sphereSize = new Vector3(0.8f, 0.8f, 0.8f);
	public float speed = 6f;
	public int score = 0;
	private int currIndx = 0;
	//public Material material;
	// Use this for initialization
	void Start () {
		gameOverText.gameObject.SetActive(false);
		replaceText.gameObject.SetActive(false);
		generateBoard();
		addBalls();
		callFirstTime = false;
		addBalls();
	}

	private int tmp = 1;
	private int m_x;
	private int m_y;
	private Transform upStart;
	void Update(){

		if (gameOver){
			gameOverText.gameObject.SetActive(true);
			return;
		}
		if (ballIndicator && startPoint != null){
			Vector3 t = startPoint.localScale;
			if (t.y > 2.5)
				tmp = -1;
			if (tmp == -1 && t.y < 0)
				tmp = 1;
			t.y += Time.deltaTime*tmp * speed ;
			startPoint.localScale = t;
		}

		moveBallsToDestination();

		if (destroyObjectsIndicator){
			destroyAllSelectedObjects();
		}

		operateRandomBalls();

		
	}


	public void Restart(){
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void operateRandomBalls(){
		if (!sizeIndicator){
			foreach(Transform t in randomNodes){
				t.localScale = smallSphereSize;
			}
		}
		if (sizeIndicator){
			foreach(Transform t in randomNodes){
				t.localScale = sphereSize;
				if (availablePositions.Count == 0)
					gameOver = true;
			}
		}
	}
	public void moveBallsToDestination(){
		if(moveIndicator){
			if(currIndx == 0){
				upStart = ballsMatrix[globalPath[0].getX(), globalPath[0].getY()];
				currIndx++;
				
				
			}
			else if (currIndx == globalPath.Count){
				moveIndicator = false;
				currIndx = 0;
				sizeIndicator = true;
			}
			else{
				Transform currPoint = board[globalPath[currIndx].getX(), globalPath[currIndx].getY()];
				Vector3 pos = Vector3.MoveTowards(upStart.position, currPoint.position, speed * Time.deltaTime);
				upStart.position = pos; 
				if (upStart.position == currPoint.position)
					currIndx++;
			}

		}
	}
	public void generateBoard(){
		board = new Transform[x, y];
		ballsMatrix = new Transform[x, y];
		availablePositions = new List<Position>();
		for (int i = 0; i < x; i++){
			for (int j = 0; j < y; j++){
				Vector3 position = getPosition(i, j);
				Transform newP = Instantiate(boardP, position, Quaternion.Euler(Vector3.right * 90)) as Transform;
				newP.localScale = Vector3.one * 0.88f;
				newP.GetComponent<Cell>().setIndexes(i, j);
				newP.GetComponent<Cell>().m_bg = this;
				board[i, j] = newP;
				board[i, j].parent = transform;
				ballsMatrix[i, j] = null;
				availablePositions.Add(new Position(i, j));
			}
			
		}

		makeObstacle();
	}

	public void makeObstacle(){
		int numOfObstacles = Random.Range(3, 9);
		for (int i = 0; i < numOfObstacles; i++){
			int ind = Random.Range(0, availablePositions.Count);
			Position p = availablePositions[ind];
			board[p.getX(), p.getY()].GetComponent<Cell>().isObstacle = true;
			board[p.getX(), p.getY()].gameObject.SetActive(false);
			deleteFromList(ind);

		}
	}


	public Vector3 getPosition(int i, int j){
		return new Vector3(-x*1.0f / 2 + 0.5f + i, 0,  -y*1.0f / 2 + 0.5f + j);
	}

	
	public IEnumerator addAll(){

		while(destroyObjectsIndicator == true)
			yield return null;

		for (int i = 0; i < 3; i++){
			if (availablePositions.Count == 0){
				sizeIndicator = true;
				yield break;
			}

			int color = Random.Range(0, 4);
			int indx = Random.Range(0, availablePositions.Count);
			Position p = availablePositions[indx];
			Vector3 position = getPosition(p.getX(), p.getY());
			Transform newBall = Instantiate(ball, position, Quaternion.identity) as Transform;
			newBall.GetComponent<Ball>().setColor(color);
			newBall.GetComponent<Ball>().m_bg = this;
			newBall.GetComponent<Ball>().setIndexes(p.getX(), p.getY());
			Color newBallColor = newBall.GetComponent<Ball>().getColor();
			newBall.GetComponent<Renderer>().material.color = newBallColor;
			ballsMatrix[p.getX(), p.getY()] = newBall;
			bool res = checkLines(p, true);
			if(res){
				ballsMatrix[p.getX(), p.getY()] = null;
				Destroy(newBall.gameObject);
				i--;
				continue;
			}
			randomNodes.Add(newBall);
			deleteFromList(indx);
		}
		if (availablePositions.Count == 0){
				sizeIndicator = true;
				yield break;
		}

	}
	
	public void addBalls(){
		randomNodes = new List<Transform>();
		
		if(callFirstTime){
			callFirstTime = false;
			sizeIndicator = true;
		}
		else
			sizeIndicator = false;

		StartCoroutine(addAll());
		

	}

	public void destroyAllSelectedObjects(){
		foreach(Transform t in destroyNodesList){
				t.Translate(0, -Time.deltaTime * (speed - 3f), 0);
				if (t.position.y < - 1){
					Position position = new Position(t.GetComponent<Ball>().getX(), t.GetComponent<Ball>().getY());
					Destroy(t.gameObject);
					ballsMatrix[position.getX(), position.getY()] = null;
					availablePositions.Add(position);
					destroyObjectsIndicator = false;
				}
			}

	}

	public class Position{
		private int m_x;
		private int m_y;
		public Position(int i ,int j){
			m_x = i;
			m_y = j;
		}
		public int getX(){
			return m_x;
		}
		public int getY(){
			return m_y;
		}
	}

	public void deleteFromList(int indx){
		availablePositions[indx] = availablePositions[availablePositions.Count - 1];
		availablePositions.RemoveAt(availablePositions.Count - 1);
	}
	
	public void deleteFromList(Position p){
		for (int i = 0; i < availablePositions.Count; i++){
			if (availablePositions[i].getX() == p.getX() && availablePositions[i].getY() == p.getY()){
				availablePositions[i] = availablePositions[availablePositions.Count - 1];
				availablePositions.RemoveAt(availablePositions.Count - 1);
				break;
			}
		}
	}

	public void setStartPoint(int i , int j){
		replaceText.gameObject.SetActive(false);
		if(startPoint != null){
			Vector3 vec = startPoint.localScale;
			vec.y = 0.8f;
			startPoint.localScale = vec;
		}
		foreach(Transform t in randomNodes){
			if (t.GetComponent<Ball>().getX() == i && t.GetComponent<Ball>().getY() == j){
				if(startPoint != null)
					startPoint = null;
				return;
				
			}
		}
		
		startPoint = ballsMatrix[i, j];
		ballIndicator = true;
		if (colorIndicator){
			endPoint.GetComponent<Renderer>().material.color = Color.white;
			colorIndicator = false;
			endPoint = null;
		}
	}


	public void setEndPoint(int i, int j){
		if (startPoint == null)
			return;
		endPoint = board[i, j];
		if (endPoint.GetComponent<Cell>().isObstacle || ballsMatrix[i, j] != null){
			startPoint = null;
			endPoint = null;
			return;
		}
		Vector3 vec = startPoint.localScale;
		vec.y = 0.8f;
		startPoint.localScale = vec;
		ballIndicator = false;
		moveBalls();
		startPoint = null;
		if(!colorIndicator)
			endPoint = null;
	}


	/* minimum path will be found(if he exists) with a* algorithm , then ball will be moved on that position if path exists*/

	public void moveBalls(){
		Position startPosition = new Position(startPoint.GetComponent<Ball>().getX(), startPoint.GetComponent<Ball>().getY());
		Position endPosition = new Position(endPoint.GetComponent<Cell>().getX(), endPoint.GetComponent<Cell>().getY());
		bool isPath = shortestPath(startPosition, endPosition);
		if (!isPath){
			Transform pos = board[endPosition.getX(), endPosition.getY()];
			pos.GetComponent<Renderer>().material.color = Color.red;
			colorIndicator = true;
			return;

		}
		moveIndicator = true;

		StartCoroutine(checkBool(startPosition, endPosition));

		
		
	}
	/*this coroutine allows to  wait until ball come to right place */
	public IEnumerator checkBool (Position startPosition, Position endPosition){
		while(moveIndicator)
			yield return null;
		ballsMatrix[startPosition.getX(), startPosition.getY()] = null;
		upStart.GetComponent<Ball>().setIndexes(endPosition.getX(), endPosition.getY());
		ballsMatrix[endPosition.getX(), endPosition.getY()] = upStart;
		deleteFromList(endPosition);
		availablePositions.Add(startPosition);
		checkLines(endPosition);
		addBalls();
	}

	public bool shortestPath(Position startPosition, Position endPosition){

		Node[,] nodeMatrix = new Node[x, y];
		for (int i = 0; i < x; i++){
			for (int j = 0; j < y; j++){
				nodeMatrix[i, j] = new Node(new Position(i, j));
			}
		}

		Node start = nodeMatrix[startPosition.getX(), startPosition.getY()];
		Node end = nodeMatrix[endPosition.getX(), endPosition.getY()];
		Heap openList = new Heap(81);
		HashSet<Node> closedList = new HashSet<Node>();
		openList.add(start);
		bool success = false;

		while(openList.count() > 0){
			
			Node current = openList.remove();
			
			closedList.Add(current);
			if (current == end){
				success = true;
				break;
			}
			List<Node> neighbours = getNeighbours(current, nodeMatrix);
			foreach(Node neighbour in neighbours){
				int posX = neighbour.getX();
				int posY = neighbour.getY();
				if (ballsMatrix[posX, posY] != null || closedList.Contains(neighbour) || board[posX, posY].GetComponent<Cell>().isObstacle)
					continue;
				int newDistance = current.m_gCost + distance(current, neighbour);
				if (newDistance < neighbour.m_gCost || !openList.contains(neighbour)){
					neighbour.m_gCost = newDistance;
					neighbour.m_hCost = distance(neighbour, end);
					neighbour.parent = current;
					if (!openList.contains(neighbour))
						openList.add(neighbour);
				}
				
			}
			
		}
		//there is no path
		if (!success){
			return false;
		}
		
		
		Node curr = end;
		globalPath = new List<Node>();
		while(curr != start){
			globalPath.Add(curr);
			curr = curr.parent;
		}
		globalPath.Add(start);
		globalPath.Reverse();
		return true;
	}


	//for heuristics i will use manhattan distance

	public int distance(Node start, Node end){
		return System.Math.Abs(start.getX() - end.getX()) + System.Math.Abs(start.getY() - end.getY());
	}

	public List<Node> getNeighbours(Node node, Node[,] matrix){
		List<Node> result = new List<Node>();
		
		int neighX = node.getX() + 1;
		int neighY = node.getY();

		if (neighX >= 0 && neighX < x && neighY>=0 && neighY < y)
				result.Add(matrix[neighX, neighY]);

		neighX -= 2;
		if (neighX >= 0 && neighX < x && neighY>=0 && neighY < y)
				result.Add(matrix[neighX, neighY]);
		neighY += 1;
		neighX++;
		if(neighX >= 0 && neighX < x && neighY>=0 && neighY < y)
				result.Add(matrix[neighX, neighY]);
		neighY -= 2;
		if(neighX >= 0 && neighX < x && neighY>=0 && neighY < y)
				result.Add(matrix[neighX, neighY]);

		return result;
	}

	/*function checks if there is 5 or more balls in line and update score */

	public bool checkLines(Position pos, bool random = false){
		destroyNodesList = new List<Transform>();
		int horizontalResult = checkHorizontal(pos);
		int verticalResult = checkVertical(pos);
		int diagonalResult1 = checkDiagonal1(pos);
		int diagonalResult2 = checkDiagonal2(pos);
	
		if (horizontalResult > 4){
			if(!random)
				score += 5 + (horizontalResult - 5) * 2;
		}

		if (verticalResult > 4){
			if(!random)
				score +=  5 + (verticalResult - 5) * 2;
		}

		if (diagonalResult1 > 4){
			if(!random)
			 	score += 5 + (diagonalResult1 - 5) * 2;
		}

		if (diagonalResult2 > 4){
			if(!random)
			 	score += 5 + (diagonalResult2 - 5) * 2;
		}
		if (destroyNodesList.Count != 0 && random)
			return true;
		if (destroyNodesList.Count != 0){
			destroyNodesList.Add(ballsMatrix[pos.getX(), pos.getY()]);
			destroyObjectsIndicator = true;
			return true;
		}
		return false;
		
	}


	public int checkHorizontal(Position pos){
		int posX = pos.getX();
		int posY = pos.getY();
		List<Transform> currList = new List<Transform>();
		//first going left 
		int i = posX - 1;
		while (i >= 0){
			if (ballsMatrix[i, posY] != null && ballsMatrix[i, posY].localScale == sphereSize){
				if (ballsMatrix[i, posY].GetComponent<Ball>().getColor() == ballsMatrix[posX, posY].GetComponent<Ball>().getColor()){
					currList.Add(ballsMatrix[i, posY]);
					
				}
				
				else{
					break;
				}
			}
			else
				break;
			i--;
		}

		// now going right

		i = posX + 1;
		while (i < x){
			if (ballsMatrix[i, posY] != null && ballsMatrix[i, posY].localScale == sphereSize){
				if (ballsMatrix[i, posY].GetComponent<Ball>().getColor() == ballsMatrix[posX, posY].GetComponent<Ball>().getColor()){
					currList.Add(ballsMatrix[i, posY]);
					
				}
				else
					break;
			}
			else
				break;
			i++;
		}			
		
		if (currList.Count >= 4){
			foreach(Transform node in currList){
				destroyNodesList.Add(node);
			}
		}
		return currList.Count + 1;

	}

	public int checkVertical(Position pos){
		int posX = pos.getX();
		int posY = pos.getY();
		List<Transform> currList = new List<Transform>();
		int i = posY - 1;
		while (i >= 0){
			if (ballsMatrix[posX, i] != null){
				if (ballsMatrix[posX, i].GetComponent<Ball>().getColor() == ballsMatrix[posX, posY].GetComponent<Ball>().getColor()){
					
					currList.Add(ballsMatrix[posX, i]);
				}
				else
					break;
			}
			else
				break;
			i--;
		}

		i = posY + 1;
		while (i < y){
			if (ballsMatrix[posX, i] != null){
				if (ballsMatrix[posX, i].GetComponent<Ball>().getColor() == ballsMatrix[posX, posY].GetComponent<Ball>().getColor()){
					currList.Add(ballsMatrix[posX, i]);
				}
				else
					break;
			}
			else
				break;
			i++;
		}

		if (currList.Count >= 4){
			foreach(Transform node in currList)
				destroyNodesList.Add(node);
		}

		return currList.Count + 1;

	}

	public int checkDiagonal1(Position pos){
		int posX = pos.getX();
		int posY = pos.getY();
		List<Transform> currList = new List<Transform>();

		int i = posX - 1;
		int j = posY + 1;

		while (i >= 0 && j < y){
			if (ballsMatrix[i, j] != null ){
				if (ballsMatrix[i, j].GetComponent<Ball>().getColor() == ballsMatrix[posX, posY].GetComponent<Ball>().getColor()){
					currList.Add(ballsMatrix[i,  j]);
				}
				else
					break;
			}
			else
				break;
			i--;
			j++;
		}

		i = posX + 1;
		j = posY - 1;

		while (i < x && j >= 0){
			if (ballsMatrix[i, j] != null ){
				if (ballsMatrix[i, j].GetComponent<Ball>().getColor() == ballsMatrix[posX, posY].GetComponent<Ball>().getColor()){
					currList.Add(ballsMatrix[i,  j]);
				}
				else
					break;
			}
			else
				break;
			i++;
			j--;
		}


		if (currList.Count >= 4){
			foreach(Transform node in currList)
				destroyNodesList.Add(node);
		}

		return currList.Count + 1;

	}

	public int checkDiagonal2(Position pos){
		int posX = pos.getX();
		int posY = pos.getY();
		List<Transform> currList = new List<Transform>();

		int i = posX - 1;
		int j = posY - 1;

		while (i >= 0 && j >= 0){
			if (ballsMatrix[i, j] != null ){
				if (ballsMatrix[i, j].GetComponent<Ball>().getColor() == ballsMatrix[posX, posY].GetComponent<Ball>().getColor()){
						currList.Add(ballsMatrix[i,  j]);
				}
				else
					break;
			}
			else
				break;
			i--;
			j--;
		}

		i = posX + 1;
		j = posY + 1;

		while (i < x && j < y){
			if (ballsMatrix[i, j] != null ){
				if (ballsMatrix[i, j].GetComponent<Ball>().getColor() == ballsMatrix[posX, posY].GetComponent<Ball>().getColor()){
					currList.Add(ballsMatrix[i,  j]);
				}
				else
					break;
			}
			else
				break;
			i++;
			j++;
		}

		if (currList.Count >= 4){
			foreach(Transform node in currList)
				destroyNodesList.Add(node);
		}

		return currList.Count + 1;
	}

	public void replaceBalls(){
		if(moveIndicator || destroyObjectsIndicator)
			return;
		if(!canReplace){
			replaceText.gameObject.SetActive(true);
			return;
		}
		canReplace = false;
		foreach(Transform t in randomNodes){
			int i = t.GetComponent<Ball>().getX();
			int j = t.GetComponent<Ball>().getY();
			ballsMatrix[i, j] = null;
			Destroy(t.gameObject);
			availablePositions.Add(new Position(i, j));
		}
		addBalls();
		
	}


	public class Node{
		private Position m_p;
		public int m_gCost;
		public int m_hCost;
		public Node parent;
		public Node(Position p){
			m_p = p;
		}

		public int fCost(){
			return m_gCost + m_hCost;
		}

		public int getX(){
			return m_p.getX();
		}
		public int getY(){
			return m_p.getY();
		}
	}



	//min heap structure
	public class Heap{

		private Node[] m_nodes;
		private int m_count;

		public Heap(int maxSize){
			m_nodes = new Node[maxSize];
			m_count = 0;
		}

		public void add(Node node){
			m_count++;
			m_nodes[m_count] = node;
			arrangeHeap();
		}

		public bool contains(Node n){
			for (int i = 1; i <= m_count; i++){
				if (m_nodes[i] == n)
					return true;
			}
			return false;
		}

		public Node remove(){
			Node res = m_nodes[1];
			m_nodes[1] = m_nodes[m_count];
			m_count--;
			sortDown();
			return res;
		}

		public int count(){
			return m_count;
		}

		public void arrangeHeap(){
			int parent = m_count / 2;
			int indx = m_count;
			while (parent > 0){
				if (m_nodes[parent].fCost() > m_nodes[indx].fCost()){
					Node tmp = m_nodes[parent];
					m_nodes[parent] = m_nodes[indx];
					m_nodes[indx] = tmp;
					indx = parent;
					parent = indx / 2;
				}
				else
					break;
			}
		}


		public void sortDown(){
			int leftChild = 2;
			int rightChild = 3;
			int swapIndx;
			int curr = 1;
			bool swaping = true;
			while (swaping){

				if (leftChild <= m_count){
					swapIndx = leftChild;
					if (rightChild <= m_count){
						if (m_nodes[rightChild].fCost() <= m_nodes[leftChild].fCost())
							swapIndx = rightChild;
					}
					if (m_nodes[swapIndx].fCost() <= m_nodes[curr].fCost()){
						Node tmp = m_nodes[curr];
						m_nodes[curr] = m_nodes[swapIndx];
						m_nodes[swapIndx] = tmp;
						curr = swapIndx;
						leftChild = curr * 2;
						rightChild = curr * 2 + 1;
					}
					else
						swaping = false;

						
				}
				else
					swaping = false;

			}
		}


	}
}
