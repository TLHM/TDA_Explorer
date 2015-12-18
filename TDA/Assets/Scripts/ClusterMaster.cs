using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
	Clusters data points into nodes using a metric to judge "distance" between points.
*/
public class ClusterMaster : MonoBehaviour {
	public static Transform nodeFab;
	public float progress;

	bool building;
	NetworkBuilder.Bucket built;

	public delegate float Metric(string a, string b);


	/**
		Creates clusters using teh Vietoris-Rips complex
		Computationally expensive, but will do as a starting point
		@param data A string arary containing the data points
		@param b The bucket to be filled up with nodes
		@param e Float value, threshold distance for the clustering
		@param m Metric function to determine "distance" between data points
	*/
	public void ClusterVietorisRips(string[] data, NetworkBuilder.Bucket b, float e, Metric m)
	{
		if(Done())
		{
			StartCoroutine(ClusterVietorisRips_Worker(data, b, e, m));
		}
	}

	/**
		Worker coroutine for the Vietoris-Rips complex clustering
		@sa ClusterVietorisRips
	*/
	IEnumerator ClusterVietorisRips_Worker(string[] data, NetworkBuilder.Bucket b, float e, Metric m)
	{
		building = true;
		progress=0;

		b.nodes = new List<Node>();

		int l = data.Length;
		Debug.Log("Data Length in bucket: "+l);
		int counter = 0;

		//Holds the index of the node that data point belongs to, offset by +1
		int[] nodeIndex = new int[l];

		//Start at 1 to avoid the header
		for(int i=1; i<l-1; i++)
		{
			if(data[i].Length<5) continue;

			bool gotLinked = false;

			for(int j=i+1; j<l; j++)
			{
				if(data[j].Length<5) continue;

				float met = m(data[i],data[j]);

				if(met<e)
				{
					//These two points belong together in a node
					gotLinked = true;
					//If they both belong to a node already, note to merge them
					if(nodeIndex[i]>0 && nodeIndex[j]>0)
					{
						if(nodeIndex[i] == nodeIndex[j]) continue;

						Node nj = b.nodes[nodeIndex[j]-1];

						int nl = nj.GetCount();
						for(int n=0; n<nl; n++)
						{
							int id = nj.GetIndexB(n);
							if(id!=j) nodeIndex[id] = nodeIndex[i];
						}
						nodeIndex[j] = nodeIndex[i];

						b.nodes[nodeIndex[i]-1].Merge(nj);
					}
					//If i has already been added to a node, add j to it
					else if(nodeIndex[i]>0){
						b.nodes[nodeIndex[i]-1].AddDataPoint(data[j], j);
						nodeIndex[j]=nodeIndex[i];
					}
					//If j belongs to a node,
					else if(nodeIndex[j]>0)
					{
						b.nodes[nodeIndex[j]-1].AddDataPoint(data[i], i);
						nodeIndex[i]=nodeIndex[j];
					}
					//Otherwise create a new node, add both to it
					else
					{
						int id = b.nodes.Count+1;
						b.nodes.Add(NewNode(data[i], i));
						b.nodes[id-1].AddDataPoint(data[j], j);

						nodeIndex[i]=id;
						nodeIndex[j]=id;
					}
				}

				counter++;
				if(counter>=1000)
				{
					yield return null;
					counter=0;
				}
			}

			progress += 1f/l;

			//If it has no close neighbors, give it its own node
			if(!gotLinked)
			{
				b.nodes.Add(NewNode(data[i], i));
				nodeIndex[i]=b.nodes.Count;
			}

			counter++;
			if(counter>=1000)
			{
				yield return null;
				counter=0;
			}
		}

		Debug.Log("Merging!");
		yield return null;

		//Cull the deleted
		List<Node> newNodes = new List<Node>();

		for(int i=0; i<b.nodes.Count; i++)
		{
			if(b.nodes[i]!=null)
			{
				newNodes.Add(b.nodes[i]);
				//b.nodes[i].ApplyMerge(nodeList);
			}
		}
		b.nodes = newNodes;
		built = b;

		Debug.Log("Clustered bucket with "+b.nodes.Count+" nodes");
		yield return null;

		building = false;
		progress = 1;
	}

	/**
		Creates a new Node
		@param d data point to start it off
		@param i the bucket index of d
		@returns Node that was created
	*/
	Node NewNode(string d, int i)
	{
		Transform t = Instantiate(nodeFab) as Transform;
		Node n = t.GetComponent<Node>();
		n.AddDataPoint(d, i);

		return n;
	}

	/**
		Are we working on building a network?
	*/
	public bool Done()
	{
		return !building;
	}

	/**
		Fetches completed bucket
	*/
	public NetworkBuilder.Bucket GetBuilt()
	{
		return built;
	}


	/**
		A basic default metric - euclidian distance in 3 dimensions
		Should be improved to handle unexpected values, and function in more than 3 dims
	*/
	public static float EuclidMetric(string a, string b)
	{
		string[] ad = a.Split(",".ToCharArray());
		string[] bd = b.Split(",".ToCharArray());
		float val = 0;

		try{
			for(int i=2;i<5;i++)
			{
				float dist = float.Parse(ad[i])-float.Parse(bd[i]);
				val+=dist*dist;
			}
		}
		catch(System.Exception e){
			Debug.LogError(a+"\n"+b);
		}

		return Mathf.Sqrt(val);
	}
}
