using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Network : MonoBehaviour {
	public Transform t;
	List<Node> nodes;


	/**
		Puts all transforms that are part of the network as children of this transform
	*/
	public void ContainTransforms()
	{
		for(int i=0; i<nodes.Count; i++)
		{
			for(int j=0; j<nodes[i].edges.Count; j++)
			{
				nodes[i].edges[j].t.SetParent(t);
			}

			nodes[i].t.SetParent(t);
		}
	}

	/**
		Centers the network on (0,0,0)
	*/
	public void Center()
	{
		Vector3 total = Vector3.zero;
		for(int i=0; i<nodes.Count; i++)
		{
			total+=nodes[i].t.position;
		}
		total*=(1f/nodes.Count);

		for(int i=0; i<nodes.Count; i++)
		{
			nodes[i].t.position = nodes[i].t.position-total;
		}

		UpdateEdges();
	}

	/**
		Updates the Edges
	*/
	public void UpdateEdges()
	{
		for(int i=0; i<nodes.Count; i++)
		{
			for(int j=0; j<nodes[i].edges.Count; j++)
			{
				nodes[i].edges[j].UpdateVisual();
			}
		}
	}

	/**
		Adds a node if it isn't already a part of the network
		@param n The node to be added to the network
	*/
	public void AddNode(Node n)
	{
		if(nodes==null) nodes = new List<Node>();

		if(!nodes.Contains(n)) nodes.Add(n);
	}
}
