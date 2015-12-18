using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Node : MonoBehaviour {
	public static Transform edgeFab;

	public Transform t;

	public List<Edge> edges;

	List<int> containedIndexes;
	List<int> bucketIndexes;
	List<int> toMerge;

	public float[] averageValues;


	/**
		Add a node index to be merged with later.
	*/
	public void AddMerge(int i)
	{
		if(toMerge==null) toMerge = new List<int>();

		toMerge.Add(i);
	}

	/**
		Merges another node into this one. Destroys the other node.
		Currently only saves containedIndexes. Nodes should have no edges when merging occurs
		@param n The node to merge into this one and destroy
	*/
	public void Merge(Node n)
	{
		float fracMe = GetCount()/(n.GetCount()+GetCount());
		float fracTh = 1 - fracMe;

		for(int i=0; i<averageValues.Length; i++)
		{
			averageValues[i] = averageValues[i]*fracMe + n.averageValues[i]*fracTh;
		}

		for(int i=0; i<n.GetCount(); i++)
		{
			int id = n.GetIndex(i);
			if(!containedIndexes.Contains(id)) containedIndexes.Add(id);

			id = n.GetIndexB(i);
			if(!bucketIndexes.Contains(id)) bucketIndexes.Add(id);
		}

		Destroy(n.gameObject);
	}

	/**
		Merges all nodes in toMerge with this one, recursively.
		@param ns Array of all the nodes, with the appropriate indexes
	*/
	public void ApplyMerge(Node[] ns)
	{
		if(toMerge==null) return;

		for(int i=0;i<toMerge.Count;i++)
		{
			ns[toMerge[i]].ApplyMerge(ns);

			Merge(ns[toMerge[i]]);
		}
	}

	/**
		Adds the data point in string d to the node
		Currently just adds the index to the containedIndexes
		@param d the data point as a string with fields separated by commas
	*/
	public void AddDataPoint(string d, int bucketId)
	{
		string[] dats = d.Split(",".ToCharArray());

		if(containedIndexes==null) containedIndexes = new List<int>();
		if(bucketIndexes==null) bucketIndexes = new List<int>();
		if(averageValues.Length==0) averageValues = new float[dats.Length-2];

		string idstr = dats[1];//.Replace("\"","");
		int id = -1;
		if(!int.TryParse(idstr, out id))
		{
			Debug.LogError(idstr);
		}
		else{
			//Update average values
			for(int i=0;i<averageValues.Length;i++)
			{
				float old = averageValues[i]*containedIndexes.Count;
				averageValues[i] = (float.Parse(dats[i+2]) + old)/(containedIndexes.Count+1);
			}

			containedIndexes.Add(id);
			bucketIndexes.Add(bucketId);
		}
	}

	/**
		Returns the number of contained indices
		@returns the count of the list containedIndexes
	*/
	public int GetCount()
	{
		return containedIndexes.Count;
	}

	/**
		Wrapper for containedIndexes[i]
		@param i index of relevant entry in containedIndexes
	*/
	public int GetIndex(int i)
	{
		return containedIndexes[i];
	}

	/**
		Wrapper for bucketIndexes[i]
		@param i index of relevant entry in bucketIndexes
	*/
	public int GetIndexB(int i)
	{
		return bucketIndexes[i];
	}

	/**
		Checks if this node contains the data point with the id
		@param id The data id to check.
		@returns true if the id is in containedIndexes
	*/
	public bool Contains(int id)
	{
		return containedIndexes.Contains(id);
	}

	/**
		Adds an edge between this node and the node n.
		@param n the node to add an edge to.
	*/
	public void AddEdge(Node n)
	{
		Transform t = Instantiate(edgeFab) as Transform;
		Edge e = t.GetComponent<Edge>();
		e.SetNodes(this, n);

		if(edges==null) edges = new List<Edge>();
		edges.Add(e);

		e.UpdateVisual();
	}
}
