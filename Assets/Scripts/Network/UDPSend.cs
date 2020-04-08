// #define DEBUG
#define DEBUGWARNING
#undef DEBUG
// #undef DEBUGWARNING

using System;
using System.Collections.Generic;

#if WINDOWS_UWP
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#else
using System.Net;
using System.Net.Sockets;
#endif

namespace HoloFab {
	// UDP sender.
	public class UDPSend {
		// An IP and a port for UDP communication to send to.
		private string remoteIP;
		private int remotePort;
        
		public bool success = false;
        
		// Network Objects:
		#if WINDOWS_UWP
		private string sourceName = "UDP Send Interface UWP";
		// Connection Object Reference.
		private DatagramSocket client;
		private static string broadcastAddress = "255.255.255.255";
		#else
		private string sourceName = "UDP Send Interface";
		// Connection Object Reference.
		private UdpClient client;
		#endif
		// History:
		// - Debug History.
		public List<string> debugMessages = new List<string>();
        
		// Constructor.
		public UDPSend(string _remoteIP, int _remotePort=12121){
			this.remoteIP = _remoteIP;
			this.remotePort = _remotePort;
			this.debugMessages = new List<string>();
		}
        
		#if WINDOWS_UWP
		// Start a connection and send given byte array.
		public async void Send(byte[] sendBuffer) {
			this.success = false;
			// Stop client if set previously.
			if (this.client != null) {
				this.client.Dispose();
				this.client = null; // Good Practice?
			}
			try {
				// Open new one.
				this.client = new DatagramSocket();
				// Write.
				using (var stream = await this.client.GetOutputStreamAsync(new HostName(this.remoteIP),
				                                                           this.remotePort.ToString())) {
					using (DataWriter writer = new DataWriter(stream)) {
						writer.WriteBytes(sendBuffer);
						await writer.StoreAsync();
					}
				}
				// Close.
				this.client.Dispose();
				this.client = null; // Good Practice?
				// Acknowledge.
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Data Sent!", ref this.debugMessages);
				#endif
				this.success = true;
				return;
			} catch (Exception exception) {
				// Exception.
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Exception: " + exception.ToString(), ref this.debugMessages);
				#endif
			}
		}
		// Broadcast Message to everyone.
		public async void Broadcast(byte[] sendBuffer) {
			// Reset.
			if (this.client != null) {
				this.client.Dispose();
				this.client = null; // Good Practice?
			}
			try {
				// Open.
				this.client = new DatagramSocket();
				// Write.
				using (var stream = await this.client.GetOutputStreamAsync(new HostName(UDPSend.broadcastAddress),
				                                                           this.remotePort.ToString())) {
					using (DataWriter writer = new DataWriter(stream)) {
						writer.WriteBytes(sendBuffer);
						await writer.StoreAsync();
					}
				}
				// Close.
				this.client.Dispose();
				// Acknowledge.
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Broadcast Sent!", ref this.debugMessages);
				#endif
				this.success = true;
				return;
			} catch (Exception exception) {
				// Exception.
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Exception: " + exception.ToString(), ref this.debugMessages);
				#endif
			}
		}
		#else
		// Start a connection and send given byte array.
		public void Send(byte[] sendBuffer) {
			this.success = false;
			// Reset.
			if (this.client != null) {
				this.client.Close();
				this.client = null; // Good Practice?
			}
			try {
				// Open.
				this.client = new UdpClient(this.remoteIP, this.remotePort);
				// Write.
				this.client.Send(sendBuffer, sendBuffer.Length);
				// Close.
				this.client.Close();
				// Acknowledge.
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Data Sent!", ref this.debugMessages);
				#endif
				this.success = true;
				return;
			} catch (Exception exception) {
				// Exception.
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Exception: " + exception.ToString(), ref this.debugMessages);
				#endif
			}
		}
		// Broadcast Message to everyone.
		public void Broadcast(byte[] sendBuffer) {
			// Reset.
			if (this.client != null) {
				this.client.Close();
				this.client = null; // Good Practice?
			}
			try {
				// Open.
				this.client = new UdpClient();
				// Write.
				this.client.Send(sendBuffer, sendBuffer.Length, new IPEndPoint(IPAddress.Broadcast, this.remotePort));
				// Close.
				this.client.Close();
				// Acknowledge.
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Broadcast Sent!", ref this.debugMessages);
				#endif
				this.success = true;
				return;
			} catch (Exception exception) {
				// Exception.
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "Exception: " + exception.ToString(), ref this.debugMessages);
				#endif
			}
		}
		#endif
	}
}