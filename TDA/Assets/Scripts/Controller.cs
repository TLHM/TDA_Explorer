using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {
	//Pointers to other scripts
	public R_Bridge r;
	public ClusterMaster cm;
	public NetworkBuilder nb;

	public string dataFile;		/**< File path to the data file in question, relative to StreamingAssets, for now */

	public Network n;		/**< The network that we build using, should be an existing obj in scene */
	NetworkBuilder.Bucket[] buckets;
	public int bucketNum;

	bool processing;

	/**
		Creates a new csv in Data/Temp that is ordered by the filter value, for simpler processing later
		Work is done by Filter Data, which is started here
		@param filterType Selects one of the available filters. Currently does nothing.
	*/
	public void Filter(int filterType)
	{
		if(!processing)
			StartCoroutine(FilterData(filterType));
	}

	/**
		The actual meat of Filter. Calls R_Bridge to do the heavy lifting with R
		@param filterType Selects one of the available filters. Currently does nothing.
		@sa Filter
	*/
	IEnumerator FilterData(int filterType)
	{
		processing = true;
		r.SetFilter(dataFile, filterType);

		//Wait for completion
		while(!r.Done())
		{
			yield return null;
		}

		processing = false;
	}

	/**
		Builds a network with filtered data!
	*/
	public void GetNetwork()
	{
		if(!processing)
			StartCoroutine(BuildNetwork());
	}

	/**
		Actual builder
	*/
	IEnumerator BuildNetwork()
	{
		processing = true;

		buckets = new NetworkBuilder.Bucket[bucketNum];

		for(int i=0;i<bucketNum;i++)
		{
			NetworkBuilder.Bucket b = new NetworkBuilder.Bucket();

			//Fetch out bucket as a string of data through R
			r.GetBucket(dataFile, i, bucketNum);

			while(!r.Done())
			{
				yield return null;
			}

			string[] data = r.GetData().Split("\n".ToCharArray());

			//Work clustering magic
			//push clusters as nodes into bucket

			buckets[i] = b;
		}

		//Got our buckets, pass to the network builder
		nb.BuildFromBuckets(buckets, out n);

		while(!nb.Done())
		{
			yield return null;
		}

		processing = false;
	}
}
