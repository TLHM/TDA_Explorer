using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Network : MonoBehaviour {
	public Transform t;
	List<Node> nodes;

	bool working;

	//Simulation Variables
	public float dt;					/**< Delta Time variable. Should being at around 1, and will automatically be reduced as the simulation continues. */

	/**
		For slowing down the simulation, and ending it
		The frame count is for ticks of the updateNodes coroutine
	*/
	public int framesUntilCheck;
	int framesPerCheck;
	bool upNodes;

	/**
		Let's the network get a shape with physics
	*/
	public void Relax()
	{
		if(Done()) StartCoroutine(RelaxNodes());
	}

	/**
		As the name implies, updates the nodes with a simple physics simulation
		Takes several frames, if needed.
		Edges are updated each real frame in Update()
	*/
	IEnumerator RelaxNodes()
	{
		working = true;
		framesUntilCheck = 50;
		framesPerCheck = 50;
		upNodes = true;

		while(working){
			//If we shouldn't be updating nodes, keep spinning
			if(!upNodes){
				yield return null;
				continue;
			}

			//Prep some variables
			Node n;
			int count = 0;	//How many nodes we updated
			int frameLimit = 50000;	//How many nodes we update before we should wait for the next frame

			framesUntilCheck--;

			float totalDist=0;
			int relevantEdgeCount=0;

			//Calculate the force each node is feeling
			for(int i=0;i<nodes.Count;i++){
				n=nodes[i];

				Vector3 repulse;
				//Get forces from other nodes
				for(int j=i+1;j<nodes.Count;j++){
					repulse = n.CalcRepulse(nodes[j]);
					n.force+=repulse;
					nodes[j].force-=repulse;
					count++;

					if(count>frameLimit){
						count=0;
						yield return null;
					}
				}

				//Get force from edges
				Vector3 edgeF;
				for(int j=0;j<n.edges.Count;j++){
					//Could also apply edge force to both here - future improvement
					edgeF = n.edges[j].GetForce(n);
					n.force+=edgeF;

					count++;

					if(count>frameLimit){
						count=0;
						yield return null;
					}
				}
				n.ApplyForce(dt);
			}

			//If we're checking the positions to see wether to slow/end the simulation, do so
			Vector3 dif = Vector3.zero;
			int difCount=0;
			//Process regular nodes
			for(int i=0;i<nodes.Count;i++){
				nodes[i].UpdatePos();
				/*Vector2 edgeInfo = nodes[i].UpdatePos();
				relevantEdgeCount+=(int)edgeInfo.x;
				totalDist+=edgeInfo.y;*/

				if(framesUntilCheck<0)
				{
					dif+=nodes[i].difFromHistory();
					nodes[i].recordHistory();
					difCount++;
				}
			}

			//Edge.avLen = totalDist/relevantEdgeCount;

			//Results of history check
			if(framesUntilCheck<0)
			{
				//Check the average change in position from a while back
				//If its below a threshhold, slow down, and if simplified, un-simplify
				//Reset timeuntil and history check
				dif/=difCount+0f;
				//Debug.Log(dif.magnitude);
				if(dif.magnitude<.05f*dt)
				{
					if(dt<.01f)
					{
						upNodes = false;
						working = false;
						framesUntilCheck = 50;
						/*yield return StartCoroutine(SaveSolvedFile());
						yield return null;
						yield return StartCoroutine(SaveJSON());*/
					}
					else
					{
						dt*=.5f;
						framesUntilCheck = framesPerCheck;
						if(framesPerCheck>10) framesPerCheck -= 10;
					}

				}
				else
				{
					framesUntilCheck = framesPerCheck;
				}
			}

			yield return null;
		}
	}


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

	/**
		Are we working on a task right now?
		@returns !working
	*/
	public bool Done()
	{
		return !working;
	}
}
