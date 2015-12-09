using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System.Diagnostics;

public class R_Bridge : MonoBehaviour {
	Process rProcess;
	StringBuilder rOut;
	StreamWriter rStreamWriter;
	StreamReader err;
	int numOutputLines;

	void Start () {
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
		err = rProcess.StandardError;
		StartCoroutine(TestMe());
	}

	void ROutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
	{
		if(!string.IsNullOrEmpty(outLine.Data))
		{
			numOutputLines++;

			rOut.Append("\n" +
                    "[" + numOutputLines.ToString() + "] - " + outLine.Data);
			UnityEngine.Debug.Log("[" + numOutputLines.ToString() + "] - " +outLine.Data);

			if(outLine.Data=="> ???")
			{
				UnityEngine.Debug.Log("hai");
				//rStreamWriter.WriteLine("license()");
				//rStreamWriter.Close();
			}
		}
		else
		{
			string er = err.ReadLine();
			if(!string.IsNullOrEmpty(er)) UnityEngine.Debug.Log("ERROR: "+err.ReadLine());
		}
	}

	IEnumerator TestMe()
	{
		yield return new WaitForSeconds(2);
		//rStreamWriter = rProcess.StandardInput;
		rStreamWriter.Write("\n");
	}

	void OnDestroy()
	{
		rProcess.Close();
		if(rStreamWriter!=null) rStreamWriter.Close();
		if(err!=null) err.Close();
	}
}
