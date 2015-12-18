using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkBuilder : MonoBehaviour {

	public struct Bucket{
		public List<Node> nodes;
	}

	public float progress;

	bool working;


	/**
		Lines up nodes along y axis, centered. X position determined by the index i
		@param n List of nodes to position
		@param i Index of bucket, determines x location
	*/
	public void ArrangeNodes(List<Node> n, int id)
	{
		float x = (18f/10f)*id - 9f;
		float dy = .75f;
		float startY = 1 + ((n.Count-1)/2f)*dy*-1;

		for(int i=0; i<n.Count; i++)
		{
			//n[i].t.position = new Vector3(x, startY+i*dy, 0);
			n[i].t.position = new Vector3(n[i].averageValues[0], n[i].averageValues[1], n[i].averageValues[2])*10;
		}
	}


	/**
		Constructs a network given a list of buckets. Nodes in buckets with overlapping elements
		Are connected. Only adjecent buckets are checked.
		@param b Array of buckets from which to build the network
		@param n Network object to build upon
	*/
	public void BuildFromBuckets(Bucket[] b, Network n)
	{
		StartCoroutine(BuildFromBucketsWorker(b,n));
	}

	/**
		"Worker thread" for the build from buckets
	*/
	IEnumerator BuildFromBucketsWorker(Bucket[] b, Network n)
	{
		working = true;
		progress = 0;

		int counter=0;

		//Loop through buckets. Each bucket compares itself with the next bucket
		for(int i=0; i<b.Length-1; i++)
		{
			Node[] n1 = b[i].nodes.ToArray();
			Node[] n2 = b[i+1].nodes.ToArray();

			//Loop through all the nodes in bucket 1
			for(int n1Count=0; n1Count<n1.Length; n1Count++)
			{
				//Loop through the nodes in bucket 2
				for(int n2Count=0; n2Count<n2.Length; n2Count++)
				{
					int n2DataCount = n2[n2Count].GetCount();
					//If the two nodes share an element, add an edge between them
					for(int j=0; j<n2DataCount; j++)
					{
						int id = n2[n2Count].GetIndex(j);
						if(n1[n1Count].Contains(id))
						{
							n1[n1Count].AddEdge(n2[n2Count]);
							j=n2DataCount;
						}

						counter++;

						if(counter%10000==0)
						{
							yield return null;
							counter=0;
						}
					}

					n.AddNode(n2[n2Count]);
				}

				n.AddNode(n1[n1Count]);

				progress = (1f/b.Length)*(i+n1Count/n1.Length);
			}
		}

		n.ContainTransforms();
		n.Center();

		progress = 1;
		working = false;
	}

	/**
		Are we working on a task right now?
		@returns !working
	*/
	public bool Done()
	{
		return !working;
	}
}
