using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WowLauncher
{
    public class Portal
    {
        private PortalConfig config;
        private TcpListener listener;
        private CancellationTokenSource cts;
        private void pushLog(string message)
        {
            config.Logger?.Invoke($"[Portal {config.RemotePort}] {message}");
        }

        public Portal(PortalConfig config)
        {
            this.config = config;
        }

        public void Start()
        {
            cts = new CancellationTokenSource();
            pushLog("Start listening...");
            try
            {
                listener = new TcpListener(IPAddress.Parse(config.LocalIPAddress), config.LocalListenPort);
                listener.Start();
                beginAcceptTcpClient(cts.Token);
            }
            catch (Exception ex)
            {
                pushLog("Listen failed." + ex.Message);
            }
        }

        private void beginAcceptTcpClient(CancellationToken token)
        {
            listener.AcceptTcpClientAsync(token).AsTask().ContinueWith(task =>
            {
                if (task.IsCanceled)
                    return;
                beginAcceptTcpClient(token);

                var localTcpClient = task.Result;
                var localName = localTcpClient.Client.RemoteEndPoint.ToString();
                var remoteTcpClient = new TcpClient();

                remoteTcpClient.ConnectAsync(config.RemoteHost, config.RemotePort, token)
                    .AsTask()
                    .ContinueWith(task =>
                {
                    if (task.IsCanceled)
                        return;
                    if (task.IsFaulted)
                    {
                        pushLog("Connect to remote endpoint failed." + task.Exception.InnerException.Message);
                        localTcpClient.Dispose();
                        remoteTcpClient.Dispose();
                        return;
                    }
                    newChannelConnected(localName, localTcpClient, remoteTcpClient, token);
                });
            });
        }

        private void newChannelConnected(string localName,TcpClient localTcpClient, TcpClient remoteTcpClient, CancellationToken token)
        {
            var localStream = localTcpClient.GetStream();
            var remoteStream = remoteTcpClient.GetStream();
            bool isFinalActionExecuted = false;
            Action<Task> finalAction = task =>
            {
                if (isFinalActionExecuted)
                    return;
                isFinalActionExecuted = true;

                pushLog($"[{localName}] disconnected.");
                localTcpClient?.Dispose();
                localTcpClient = null;
                remoteTcpClient?.Dispose();
                remoteTcpClient = null;                
            };
            localStream.CopyToAsync(remoteStream, token).ContinueWith(finalAction);
            remoteStream.CopyToAsync(localStream, token).ContinueWith(finalAction);
            pushLog($"[{localName}] connected.");
        }

        public void Stop()
        {
            listener.Stop();
            cts.Cancel();
        }
    }
}
