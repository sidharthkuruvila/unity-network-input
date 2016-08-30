using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Threading;
using System;
using System.Net;

//Don't know if this actually works need to read up on threading
//in c#
using System.Net.Sockets;
using System.Text;



public class SynchronizedCircularQueue<T> {

	System.Object synObject = new System.Object();
	T[] queue;
	int head = 0;
	int tail = 0;
	int size;
	public SynchronizedCircularQueue (int size){
		queue = new T[size];
		this.size = size;
	}

	public void enqueue(T v){
		lock (synObject) {
			queue [head] = v;
			head =  (head + 1) % size;

			//If start catches up with end then push end up one position.
			if (head == tail) {
				tail = (tail + 1) % size;
			}
		}
	}
	public bool dequeue(out T value){
		value = queue [tail];
		if (tail == head) {
			return false;
		}
		lock (synObject) {
			T v = queue [tail];
			tail = (tail + 1) % size;
			value = v;
			return true;
		}
	}
}


public class UdpServerWorker {

	public SynchronizedCircularQueue<Vector3> queue = new SynchronizedCircularQueue<Vector3>(1024);

	// This method will be called when the thread is started. 
	public void DoWork()
	{
		//Copied from https://stackoverflow.com/questions/4844581/how-do-i-make-a-udp-server-in-c

		IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
		UdpClient newsock = new UdpClient(ipep);
		IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
		while(!_shouldStop)
		{
			byte[] data = newsock.Receive(ref sender);
			var l = Encoding.ASCII.GetString (data, 0, data.Length).Split (new char[] {','});
			var v = new Vector3(float.Parse (l[0]), float.Parse (l[1]), float.Parse (l[2]));
			queue.enqueue (v);
		}

		Debug.Log ("worker thread: terminating gracefully.");
	}
	public void RequestStop()
	{
		_shouldStop = true;
	}
	// Volatile is used as hint to the compiler that this data 
	// member will be accessed by multiple threads. 
	private volatile bool _shouldStop;
}

[AddComponentMenu("NetworkInput/NetworkInput")]
public class UdpServer : MonoBehaviour {
	public UdpServerWorker udpServerWorker;

	[SerializeField]
	public GameObject gameObject;



	Thread udpServerThread;
	void Start () {
		if (udpServerThread != null) {
			udpServerWorker.RequestStop ();
		}

		udpServerWorker = new UdpServerWorker ();
		udpServerThread = new Thread (udpServerWorker.DoWork);
		udpServerThread.Start ();
	}

	// Update is called once per frame
	void Update () {
		Vector3 res;
		while(udpServerWorker.queue.dequeue(out res)){
			gameObject.transform.position = res;
		}
	}

	void OnDisable() {
		Debug.Log  ("Shutting down worker udp networking thread! ");
		udpServerWorker.RequestStop();
		//We need to abort because newSock.Receive is blocking. Come up with a way to remove this
		udpServerThread.Abort ();
	}
}
