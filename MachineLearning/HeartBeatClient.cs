
namespace MachineLearning
{

    // using System.Net.WebSockets;


    public class HeartBeatClient
    {
        
        private const int HeartbeatIntervalMs = 5000; // Heartbeat every 5 seconds



        public async System.Threading.Tasks.Task StartAsync()
        {
            const string serverUrl = "wss://localhost:7118/ws"; // Replace with your server URL
            await StartAsync(serverUrl);
        } // End Task StartAsync 


        public async System.Threading.Tasks.Task StartAsync(string serverUrl)
        {
            await StartAsync(new System.Uri(serverUrl));
        } // End Task StartAsync 


        public async System.Threading.Tasks.Task StartAsync(System.Uri serverUrl)
        {
            System.Threading.CancellationTokenSource _cancellationSource = new System.Threading.CancellationTokenSource();

            using (System.Net.WebSockets.ClientWebSocket socket = new System.Net.WebSockets.ClientWebSocket())
            {
                try
                {
                    await socket.ConnectAsync(serverUrl, _cancellationSource.Token);

                    System.Console.WriteLine("WebSocket connection established.");

                    System.Threading.Tasks.Task receiveTask = ReceiveMessagesAsync(socket, _cancellationSource.Token);
                    System.Threading.Tasks.Task sendTask = SendHeartbeatsAsync(socket, _cancellationSource.Token);

                    await System.Threading.Tasks.Task.WhenAll(receiveTask, sendTask);
                } // End Catch 
                catch (System.Exception ex)
                {
                    System.Console.WriteLine($"Error: {ex.Message}");
                } // End Catch 

            } // End Using socket 

        } // End Task StartAsync 


        private async System.Threading.Tasks.Task ReceiveMessagesAsync(System.Net.WebSockets.ClientWebSocket socket, System.Threading.CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    System.ArraySegment<byte> buffer = new System.ArraySegment<byte>(new byte[1024]);
                    System.Net.WebSockets.WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, cancellationToken);

                    if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                    {
                        System.Console.WriteLine("Server closed connection.");
                        break;
                    } // End if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close) 

                    string message = System.Text.Encoding.UTF8.GetString(buffer.ToArray(), 0, result.Count);
                    System.Console.WriteLine($"Server message: {message}");

                    // Process server messages here
                } // End Catch 
                catch (System.OperationCanceledException)
                {
                    break;
                } // End Catch 
                catch (System.Exception ex)
                {
                    if (socket.State == System.Net.WebSockets.WebSocketState.Aborted)
                    {
                        System.Console.WriteLine($"Socket closed unexpectedly: {ex.Message}");
                        break;
                    } // End if (_socket.State == System.Net.WebSockets.WebSocketState.Aborted) 

                    System.Console.WriteLine($"Error receiving message: {ex.Message}");
                } // End Catch 

            } // Whend 

        } // End Task ReceiveMessagesAsync 


        private async System.Threading.Tasks.Task SendHeartbeatsAsync(System.Net.WebSockets.ClientWebSocket socket, System.Threading.CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    string heartbeatMessage = "PING";
                    byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(heartbeatMessage);
                    await socket.SendAsync(messageBytes, System.Net.WebSockets.WebSocketMessageType.Text, true, cancellationToken);
                    System.Console.WriteLine("Sent heartbeat message.");

                    await System.Threading.Tasks.Task.Delay(HeartbeatIntervalMs, cancellationToken);
                }
                catch (System.OperationCanceledException)
                {
                    break;
                } // End catch 
                catch (System.Exception ex)
                {
                    if (socket.State == System.Net.WebSockets.WebSocketState.Aborted)
                    {
                        System.Console.WriteLine($"Socket closed unexpectedly: {ex.Message}");
                        break;
                    } // End if (_socket.State == System.Net.WebSockets.WebSocketState.Aborted) 

                    System.Console.WriteLine($"Error sending heartbeat: {ex.Message}");
                } // End Catch 

            } // Whend 

        } // End Task SendHeartbeatsAsync 


        public static async System.Threading.Tasks.Task TestAsync()
        {
            HeartBeatClient client = new HeartBeatClient();
            await client.StartAsync();
        } // End Task TestAsync 


    } // End Class HeartBeatClient 


} // End Namespace 
