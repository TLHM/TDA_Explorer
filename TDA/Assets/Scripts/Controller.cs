using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {
	public static bool loaded;

	//Pointers to other scripts
	public R_Bridge r;
	public ClusterMaster cm;
	public NetworkBuilder nb;

	//Prefab objects
	public Transform nodeFab;
	public Transform edgeFab;

	public string dataFile;		/**< File path to the data file in question, relative to StreamingAssets, for now */

	public Network n;		/**< The network that we build using, should be an existing obj in scene */
	NetworkBuilder.Bucket[] buckets;

	//Small variables to tweak
	public int bucketNum;
	public float epsilon;
	public float idealLen;

	public bool relax;	/**< Do we use the positioning from the data, or relax it? */

	bool processing;

	//Loading GUI
	public GameObject load;
	public UnityEngine.UI.Text loadingMessage;
	public RectTransform loadingBar;


	void Start(){
		ClusterMaster.nodeFab = nodeFab;
		Node.edgeFab = edgeFab;
		Node.mass = 1;
		Node.forceScale = 1;

		Edge.idealLen = idealLen;
		Edge.idealLen2 = Edge.idealLen*Edge.idealLen;

		StartCoroutine(Test());
	}

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
		loadingMessage.text = "Filtering Data...";

		r.SetFilter(dataFile, filterType);

		//Wait for completion
		while(!r.Done())
		{
			yield return null;
		}

		processing = false;
		loadingMessage.text = "Done";
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
		loadingMessage.text = "Building Network...";

		buckets = new NetworkBuilder.Bucket[bucketNum];

		for(int i=0;i<bucketNum;i++)
		{
			buckets[i] = new NetworkBuilder.Bucket();
		}

		for(int i=0;i<bucketNum;i++)
		{
			loadingMessage.text = "Fetching Bucket No. "+i;
			loadingBar.localScale = new Vector3(0,1,1);

			NetworkBuilder.Bucket b = buckets[i];

			//Fetch out bucket as a string of data through R
			r.GetBucket(dataFile, i, bucketNum);

			while(!r.Done())
			{
				yield return null;
			}

			string[] data = r.GetData().Split("\n".ToCharArray());

			//Cluster the data points in the bucket
			loadingMessage.text = "Clustering Bucket No. "+i;
			yield return null;

			cm.ClusterVietorisRips(data, b, epsilon, ClusterMaster.EuclidMetric);

			while(!cm.Done())
			{
				loadingBar.localScale = new Vector3(cm.progress,1,1);
				yield return null;
			}

			buckets[i] = cm.GetBuilt();
			nb.ArrangeNodes(buckets[i].nodes, i);

			yield return null;
		}

		//Got our buckets, pass to the network builder
		loadingMessage.text = "Connecting Nodes...";
		loadingBar.localScale = new Vector3(0,1,1);
		Debug.Log("Building Edges");

		yield return null;

		nb.BuildFromBuckets(buckets, n);

		while(!nb.Done())
		{
			loadingBar.localScale = new Vector3(nb.progress,1,1);
			yield return null;
		}

		processing = false;
		loadingBar.localScale = Vector3.one;
		loadingMessage.text = "Done";

		load.SetActive(false);

		loaded = true;

		yield return null;

		if(relax)
		{
			Debug.Log("Relaxing!");
			yield return null;
			n.Relax();

			while(!n.Done()) yield return null;
		}
		Debug.Log("Finished!");
	}

	/**
		Runs a test, going through the full process of creating a network
	*/
	IEnumerator Test()
	{
		Filter(0);

		while(processing) yield return null;

		GetNetwork();
	}
}
