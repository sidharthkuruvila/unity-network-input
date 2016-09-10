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

	public SynchronizedCircularQueue<HandInfo> queue = new SynchronizedCircularQueue<HandInfo>(1024);

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
			var s = Encoding.ASCII.GetString (data, 0, data.Length);
			var d = JsonUtility.FromJson<TransferData>(s);
			var hi = new HandInfo ();
			if (d.left != null) {
				hi.left = readHand (d.left);
			}
			if (d.right != null) {
				hi.right = readHand (d.right);
			}
			queue.enqueue (hi);
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

	private Vector3 readHand(HandsInfo hi){
		return new Vector3(1 + hi.x/100, 9 - hi.z/100, hi.y/100);
	}
}


[AddComponentMenu("NetworkInput/NetworkInput")]
public class UdpServer : MonoBehaviour {
	public UdpServerWorker udpServerWorker;

	[SerializeField]
	public GameObject leftHand;

	[SerializeField]
	public GameObject rightHand;



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
		HandInfo res;
		while(udpServerWorker.queue.dequeue(out res)){
			leftHand.transform.position = res.left;
			rightHand.transform.position = res.right;
		}
	}

	void OnDisable() {
		Debug.Log  ("Shutting down worker udp networking thread! ");
		udpServerWorker.RequestStop();
		//We need to abort because newSock.Receive is blocking. Come up with a way to remove this
		udpServerThread.Abort ();
	}
}
[Serializable]
public class HandsInfo {
	public float x, y, z;
}
[Serializable]
public class TransferData {
	public long seq;
	public HandsInfo left;
	public HandsInfo right;
}


public class HandInfo {
	public Vector3 left;
	public Vector3 right;
}