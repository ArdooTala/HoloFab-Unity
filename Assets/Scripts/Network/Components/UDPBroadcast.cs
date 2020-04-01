using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPBroadcast : MonoBehaviour
{
    UdpClient Client = new UdpClient();
    byte[] RequestData = Encoding.ASCII.GetBytes("HolloWorld");
    IPEndPoint ServerEp = new IPEndPoint(IPAddress.Any, 0);
    
    float lastTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        Client.EnableBroadcast = true;
        //Thread.Sleep(1000);
        //var ServerResponseData = Client.Receive(ref ServerEp);
        //var ServerResponse = Encoding.ASCII.GetString(ServerResponseData);
        //Debug.Log("Recived {0} " + ServerResponse + " from " + ServerEp.Address.ToString());

        //Client.Close();
    }

    // Update is called once per frame
    void Update()
    {
        if (true)//(Time.deltaTime - lastTime > 2)
        {
            lastTime = Time.deltaTime;
            Client.Send(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Broadcast, 8888));
        }
    }
}
