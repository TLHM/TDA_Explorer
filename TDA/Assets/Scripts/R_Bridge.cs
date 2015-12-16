using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System.Diagnostics;

public class R_Bridge : MonoBehaviour {
	int currentFilter;

	Process rProcess;
	StringBuilder rOut;
	StreamWriter rStreamWriter;
	StreamReader err;
	int numOutputLines;

	bool processFinished;	/**< is the thread we spawn finished? */
	bool working;		/**< are we currently working on a task? */

	string data;

	void Start () {
		/*
		Initial test just running R directly, but the input didn't seem to
		work very well. Now we use shell as an intermediary

		rProcess = new Process();
		rProcess.StartInfo.FileName = "r";
		rProcess.StartInfo.Arguments = "--vanilla < test.R";

		// Set UseShell Execute tp false for redirection
		rProcess.StartInfo.UseShellExecute = false;

		//Redirect the standard output of the command
		//Stream is read asyncronously using an event handler
		rProcess.StartInfo.RedirectStandardOutput = true;
		rProcess.StartInfo.RedirectStandardError = true;
		rOut = new StringBuilder("");

		//Set our event handler to async read the output
		rProcess.OutputDataReceived += new DataReceivedEventHandler(ROutputHandler);

		//Redirect standard input as well.
		//This stream is used asynchronously
		rProcess.StartInfo.RedirectStandardInput = true;

		//Begin Process!
		rProcess.Start();

		//Use Stream writer to write the input
		rStreamWriter = rProcess.StandardInput;

		//Start the asynchronous read of the output stream
		rProcess.BeginOutputReadLine();
		numOutputLines=0;
		err = rProcess.StandardError;*/

		//Make sure we have a nice Temp directory
		if(!Directory.Exists(Application.dataPath+"/Temp"))
		{
			Directory.CreateDirectory(Application.dataPath+"/Temp");
		}
	}

	/**
		Launches process to calculate the filter value for each row of data, and sort by the values
		@param filePath path to the .csv file we want to analyze
		@param filterType which filter should we use? Should make this an enum, really
	*/
	public void SetFilter(string filePath, int filterType)
	{
		if(Done())
		{
			StartCoroutine(FilterProcess(filePath, filterType));
		}
		currentFilter = filterType;
	}

	/**
		Creates process to calculate the filter value for each row of data, and sort by the values
		@param filePath path to the .csv file we want to analyze
		@param filterType which filter should we use? Should make this an enum, really
	*/
	IEnumerator FilterProcess(string filePath, int filterType)
	{
		working = true;

		//Start a new process, run our shell script
		rProcess = new Process();
		rProcess.StartInfo.FileName = "sh";

		string p = Application.streamingAssetsPath;
		string d = Application.dataPath;

		//Args: shellScriptPath dataPath filterType outPath unused rFile
		rProcess.StartInfo.Arguments =
			p+"/tester.sh"+
			" "+p+"/"+filePath+
			" "+"filerlol"+
			" "+d+"/Temp/out_"+filePath+
			" "+"irrelevant"+
			" "+p+"/test.r";

		// Set UseShell Execute to false for redirection
		rProcess.StartInfo.UseShellExecute = false;

		//Redirect the standard output of the command
		//Stream is read asyncronously using an event handler
		rProcess.StartInfo.RedirectStandardOutput = true;
		rProcess.StartInfo.RedirectStandardError = true;
		rOut = new StringBuilder("");

		//Set our event handler to async read the output
		rProcess.OutputDataReceived += new DataReceivedEventHandler(ROutputHandler);

		//Get notified when the script finishes
		rProcess.EnableRaisingEvents = true;
		rProcess.Exited += new System.EventHandler(EndProcess);

		//Begin Process!
		processFinished = false;
		rProcess.Start();

		//Start the asynchronous read of the output stream
		rProcess.BeginOutputReadLine();
		numOutputLines=0;
		err = rProcess.StandardError;

		//Wait for the process to complete
		while(!processFinished)
		{
			yield return null;
		}

		//Clean up a bit
		rProcess.Close();
		if(rStreamWriter!=null) rStreamWriter.Close();
		if(err!=null) err.Close();

		UnityEngine.Debug.Log("csv filtered!");
		working = false;
	}

	public void GetBucket(string filePath, int whichBucket, int bucketCount)
	{
		if(!working)
		{
			StartCoroutine(GetBucketWorker(filePath, whichBucket, bucketCount));
		}
	}

	/**
		Grabs a selection of rows from the filtered data, returns it as a string through out file
		@param filePath path to the .csv file we want to analyze, relative to StreamingAssets
		@param whichBucket should be between 0 and bucketCount, which bucket are we fetching?
		@param bucketCount How many buckets the data will be cut up into
		@param data Out string of the data retrieved
	*/
	IEnumerator GetBucketWorker(string filePath, int whichBucket, int bucketCount)
	{
		working = true;

		//Start a new process, run our shell script
		rProcess = new Process();
		rProcess.StartInfo.FileName = "sh";

		string p = Application.streamingAssetsPath;
		string d = Application.dataPath;

		//Args: shellScriptPath dataPath filterType outPath unused rFile
		rProcess.StartInfo.Arguments =
			p+"/tester.sh"+
			" "+d+"/Temp/out_"+filePath+
			" "+d+"/Temp/outBucket_"+filePath+
			" "+whichBucket+
			" "+bucketCount+
			" "+p+"/bucket.r";

		// Set UseShell Execute to false for redirection
		rProcess.StartInfo.UseShellExecute = false;

		//Redirect the standard output of the command
		//Stream is read asyncronously using an event handler
		rProcess.StartInfo.RedirectStandardOutput = true;
		rProcess.StartInfo.RedirectStandardError = true;
		rOut = new StringBuilder("");

		//Set our event handler to async read the output
		rProcess.OutputDataReceived += new DataReceivedEventHandler(ROutputHandler);

		//Get notified when the script finishes
		rProcess.EnableRaisingEvents = true;
		rProcess.Exited += new System.EventHandler(EndProcess);

		//Begin Process!
		processFinished = false;
		rProcess.Start();

		//Start the asynchronous read of the output stream
		rProcess.BeginOutputReadLine();
		numOutputLines=0;
		err = rProcess.StandardError;

		//Wait for the process to complete
		while(!processFinished)
		{
			yield return null;
		}

		//Clean up a bit
		rProcess.Close();
		if(rStreamWriter!=null) rStreamWriter.Close();
		if(err!=null) err.Close();

		//Read in data from file
		data = File.ReadAllText(d+"/Temp/outBucket_"+filePath);

		//Now finished!
		working = false;
	}

	void ROutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
	{
		if(!string.IsNullOrEmpty(outLine.Data))
		{
			numOutputLines++;

			rOut.Append("\n" +
                    "[" + numOutputLines.ToString() + "] - " + outLine.Data);
			UnityEngine.Debug.Log("[" + numOutputLines.ToString() + "] - " +outLine.Data);
		}
		else
		{
			string er = err.ReadLine();
			if(!string.IsNullOrEmpty(er)) UnityEngine.Debug.Log("ERROR: "+err.ReadLine());
		}
	}

	/*
	Initial test for using a shell script to interface with R, probs doesn't work on windows,
	but that's easy enough to change down the line

	Kept so can be copied

	IEnumerator LaunchTest()
	{
		rProcess = new Process();
		rProcess.StartInfo.FileName = "sh";

		string p = Application.streamingAssetsPath;

		//Args: dataPath filterType outPath rFile
		rProcess.StartInfo.Arguments = p+"/tester.sh"+
			" "+p+"/horse-reference.csv"+
			" "+"filerlol"+
			" "+p+"/horseOut.csv"+
			" "+p+"/test.r";

		// Set UseShell Execute tp false for redirection
		rProcess.StartInfo.UseShellExecute = false;

		//Redirect the standard output of the command
		//Stream is read asyncronously using an event handler
		rProcess.StartInfo.RedirectStandardOutput = true;
		rProcess.StartInfo.RedirectStandardError = true;
		rOut = new StringBuilder("");

		//Set our event handler to async read the output
		rProcess.OutputDataReceived += new DataReceivedEventHandler(ROutputHandler);

		//Get notified when the script finishes
		rProcess.EnableRaisingEvents = true;
		rProcess.Exited += new System.EventHandler(EndProcess);

		//Begin Process!
		processFinished = false;
		rProcess.Start();

		//Start the asynchronous read of the output stream
		rProcess.BeginOutputReadLine();
		numOutputLines=0;
		err = rProcess.StandardError;

		while(!processFinished)
		{
			yield return null;
		}

		rProcess.Close();
		if(rStreamWriter!=null) rStreamWriter.Close();
		if(err!=null) err.Close();

		UnityEngine.Debug.Log("csv filtered!");
	}*/

	void EndProcess(object sender, System.EventArgs e)
	{
		UnityEngine.Debug.Log("HAI");
		processFinished = true;
	}

	void OnDestroy()
	{
		rProcess.Close();
		if(rStreamWriter!=null) rStreamWriter.Close();
		if(err!=null) err.Close();
	}

	/**
		Have we finished the current task?
	*/
	public bool Done()
	{
		return !working;
	}

	/**
		Returns our current data
	*/
	public string GetData()
	{
		return data;
	}
}
