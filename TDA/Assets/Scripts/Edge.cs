using UnityEngine;
using System.Collections;

/**
	Represents an edge between two nodes in our graph
	Can calculate the force it applies on its nodes and update its visual representation
*/
public class Edge : MonoBehaviour {
	public static float idealLen;		/**< Ideal length for the edges*/
	public static float idealLen2; 	/**< Ideal length for the edges squared*/

	public Transform t;					/**< The transform of this edge */
	public LineRenderer lr;				/**< The visual line renderer for this edge */

	Node n1;									/**< First node that comprises this edge */
	Node n2;									/**< Second node that comprises this edge */

	Vector3 dir;							/**< The vector from node2 to node1 */


	/**
		Returns a Vector3 that represents the force applied on a node
		@param n Which node we want the force for. If it doesn't belong to this edge, a zero force is returned
	*/
	public Vector3 GetForce(Node n){

		bool node1;
		if(n==n1) node1=true;
		else if(n==n2) node1=false;
		else return Vector3.zero;
		dir = n1.t.localPosition - n2.t.localPosition;

		float mag = Mathf.Min((dir.sqrMagnitude/idealLen),200);
		return (dir.normalized*mag)*(node1?-1:1);
	}

	/**
		Sets the two nodes of this edge to those passed in
		@param m1 The first node
		@param m2 The second node
	*/
	public void SetNodes(Node m1, Node m2)
	{
		n1=m1;
		n2=m2;
	}

	/**
		Updates the visual component of this edge to correspond with the nodes
	*/
	public void UpdateVisual()
	{
		//Update the line renderer visual
		dir = n1.t.localPosition - n2.t.localPosition;
		float mag = dir.magnitude;
		t.localPosition = n2.t.localPosition;
		lr.SetPosition(0,Vector3.zero);
		lr.SetPosition(1,dir);

		//curColor = g.Evaluate(mag/(avLen*colorFactor));
		//lr.SetColors(curColor,curColor);
	}
}
