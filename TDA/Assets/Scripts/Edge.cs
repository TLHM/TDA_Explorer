using UnityEngine;
using System.Collections;

public class Edge : MonoBehaviour {

	public Transform t;
	public LineRenderer lr;

	Node n1;
	Node n2;

	Color myColor;
	Vector3 dir;

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
