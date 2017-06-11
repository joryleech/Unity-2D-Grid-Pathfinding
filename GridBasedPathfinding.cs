using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/*
 * GridBasePathfinding
 * Author: Jory Leech
 * Description: Creates a list of points using the a* algorithm, to create navigation between two points
 * 
 * Implementation Guide:
 * 	Create a seperate instance of this class for every individual path at any given time
 * 	Run makePath to generate a path
 *  When Path is not null the path is generated.
 *  Use reset path or destroy and recreate instance to find a new path.
 */
public class GridBasedPathfinding : MonoBehaviour {
	//The Granularity Of The Search
	public float gridSize=1;
	public BoxCollider2D collideChecker;
	public Vector2 startPosition;
	public Vector2 destinationPosition;
	LinkedList<Vector2> Path;
	public string navMeshTag = "Navigation";
	//How many loop cycles per frame
	public int cyclesPerFrame=25;
	// Use this for initialization

	public LinkedList<Vector2> getPath(){
		return this.Path;
	}
	public void resetPath(){
		Path = null;
	}
	/*
	 * Function makePath
	 * Param1, sp, the starting point of the path
	 * Param2, dp, the destination point of the path
	 * Param3, e, a function to perform on completion
	 * Description: Generates the path with a coroutine.
	 * 
	 */
	public void makePath(Vector2 sp,Vector2 dp,Action e){
		startPosition = sp;destinationPosition = dp;
		StartCoroutine(genTrail (new Vector2 (Mathf.Round(startPosition.x), Mathf.Round(startPosition.y)), new Vector2 (Mathf.Round(destinationPosition.x), Mathf.Round(destinationPosition.y)),e));
	}
	/*
	 * Function genTrail
	 * Param1, start, the starting point of the path
	 * Param2, dest, the destination point of the path
	 * Param3, onFinish, a function to perform on completion
	 * Description: the coroutine to generate the path.
	 * 
	 */
	public IEnumerator genTrail(Vector2 start, Vector2 dest,Action onFinish){
		//Debug.DrawLine (start, dest, Color.red, 10, false);
		Path = null;
		Node found = null;
		LinkedList<Node> openList = new LinkedList<Node> ();
		Node startNode = new Node (start,getG(start,dest));
		HashSet<Vector2> closedList = new HashSet<Vector2> ();
		openList.AddLast (startNode);
		int run = 0;
		while (found == null && openList.Count >0) {
			run++;
			if (run % this.cyclesPerFrame == 0) {
				yield return null;
			}
			Node currentNode = openList.First.Value;
			foreach(Node n in openList){
				if (n.AStar < currentNode.AStar) {
					currentNode = n;
				}
			}	openList.Remove (currentNode);
			if (currentNode.currentPos.Equals (dest)) {
				found = currentNode;
				break;
			}
			//Add all the new nodes.
			
			for (int x = -1; x <= 1; x++) {
				for (int y = -1; y <= 1; y++) {
					if (x*x != y*y) {
						Vector2 point = new Vector2 (currentNode.currentPos.x + x, currentNode.currentPos.y + y);

						Vector2 pointWithOffset = new Vector2 (currentNode.currentPos.x + x + 0.5f, currentNode.currentPos.y + y - 0.5f);
						if (!pointInCollider (pointWithOffset) && !closedList.Contains(point)) {
							Node newNode = new Node (point, currentNode,getG(point,dest)+currentNode.HCount+1);
							openList.AddFirst(newNode);
							closedList.Add (point);
						}
					}
				}
			}

		}
		if (found != null) {
			LinkedList<Vector2> returnList = new LinkedList<Vector2> ();
			returnList.AddFirst (found.currentPos);
			while (found.parent != null) {
				returnList.AddFirst (found.parent.currentPos);
				found = found.parent;
			}
			Path = returnList;
			if (onFinish != null) {
				onFinish ();
			}
			yield break;
		}
		Debug.Log ("path failed");
		yield break;
	}
	/*
	 * Function PointInCollider
	 * Param1, point, the point to check for navmesh collison
	 * Description: Returns whether or not the point is in the navmesh.
	 * 
	 */
	bool pointInCollider(Vector2 point){
		if (this.navMeshTag != null) {
			Collider2D[] points = Physics2D.OverlapPointAll (point);
			for (int i = 0; i < points.Length; i++) {
				if (points [i].CompareTag ("Navigation")) {
					return false;
				}
			}
			return true;
		}
		return false;
	}
	/*
	 * The heuristic used to discover a path. 
	 * Since the movement is using cardinal directions, we can make a simple heuristic.
	 * That only checks the distance. 
	 * 
	 **/
	int getAStar(Node node){
		return node.AStar;
	}
	int getG(Vector2 pos,Vector2 dest){
		return Mathf.RoundToInt(Mathf.Abs(pos.y-dest.y) + Mathf.Abs (pos.x - dest.x));
	}

	class Node{
		public Vector2 currentPos;
		public Node parent;
		public int HCount;
		public int AStar;
		public Node(Vector2 pos,Node old,int astar){
			currentPos=pos;
			parent=old;
			HCount=parent.HCount+1;
			AStar=astar;
		}
		public Node(Vector2 pos,int astar){
			currentPos = pos;
			parent=null;
			HCount=0;
			AStar=astar;
		}
	}
}
