using UnityEngine;
using System.Collections;

public class ClusterMaster : MonoBehaviour {
	bool building;

	public delegate float Metric(string a, string b);


	/**
		Are we working on building a network?
	*/
	public bool Done()
	{
		return building;
	}
}
