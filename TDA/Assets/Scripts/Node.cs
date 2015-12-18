using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Node : MonoBehaviour {
	public static Transform edgeFab;	/**< The edge object that is the base for all edges */
	public static float forceScale;	/**< Gets set by Controller. A multiplier on force */
	public static float mass;			/**< In case we want to tweak it, it is just 1 now */
	public int id;							/**< Identifier number for "location" in its network */

	public Transform t;					/**< Transform for this object */

	public List<Edge> edges;			/**< Keeps track of all the edges that contain this node */

	List<int> containedIndexes;	/**< Row ids of all data points contained in this node */
	List<int> bucketIndexes;		/**< Used in network creation, tracks temporary id for each data point contained */

	public float[] averageValues;	/**< Average values for each field in the data contained */

	//Used for the physics simulation
	public Vector3 force;	/**< Contains the force that a node is affectd by each update tick */
	Vector3 velocity;			/**< Current velocity of the node */
	Vector3 last;				/**< Previous velocity. Used to detect and reduce degenerate behavior with larger time steps */

	//Used to estimate slowing down time, ending simulation
	Vector3 history;		/**< A previous position. Used to detect when the simulation is approachign a stable solution */

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
		Calculates a repulsion force from another node
		@param n The node we're recieving a repulsive force from.
	*/
	public Vector3 CalcRepulse(Node n){
		Vector3 dir = t.localPosition-n.t.localPosition;
		float mag = dir.magnitude;

		//In case we're right on top of one another
		if(mag==0) return .5f*Random.onUnitSphere;

		mag = Mathf.Max(mag,.05f);
		return dir.normalized * (Edge.idealLen2/mag);
	}

	/**
		Applies the previously calculated force to self, resets it for the next frame
		Contains some checks to stop degenerate behavior
		@param dt The time step we are simulating (delta time).
	*/
	public void ApplyForce(float dt){
		float sqrMag = force.sqrMagnitude;
		if(sqrMag<.05f){
			force.Set(0,0,0);
		}
		//Makes sure the force is not too large
		if(sqrMag>200)
		{
			force*=.1f;
		}

		//Check if we're flip flopping
		if((force.normalized+last.normalized).sqrMagnitude<.01f)
		{
			velocity.Set(0,0,0);
			force*=.5f;
		}

		velocity+=(force/mass)*forceScale*dt;
		last = force;
		force.Set(0,0,0);
	}

	/**
		Actually moves the node using the velocity calculated with ApplyForce
		Lowers velocity as well. This helps reduce degenerate behavior,
		though it means the simulation takes longer.
		@return Returns a Vector2. X value is the number of edges updated, Y value is their summed lengths.
	*/
	public void UpdatePos(){
		t.localPosition+=velocity*Time.deltaTime;
		velocity*=.5f;

		//int edgeCount=0;
		//float totalDist=0;
		for(int i=0;i<edges.Count;i++)
		{
			edges[i].UpdateVisual();
			/*float d=edges[i].UpdateVisual();
			if(d!=0)
			{
				edgeCount++;
				totalDist+=d;
			}*/
		}

		//return new Vector2(edgeCount,totalDist);
	}

	/**
		Sets the speed to zero
	*/
	public void clearVel(){
		velocity.Set(0,0,0);
	}

	/**Saves current local position as history */
	public void recordHistory(){
		history = t.localPosition;
	}

	/**Returns the difference between history position and current posisiton (local) */
	public Vector3 difFromHistory(){
		return t.localPosition - history;
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
