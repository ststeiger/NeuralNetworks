
namespace MachineLearning
{



    // using System.Net.WebSockets;



    class TestWebSocket
    {


        static async System.Threading.Tasks.Task Test(string[] args)
        {
            using (System.Net.WebSockets.ClientWebSocket  client = new System.Net.WebSockets.ClientWebSocket())
            {
                try
                {
                    System.Uri serverUri = new System.Uri("ws://localhost:5000"); // Replace with your server URL
                    await client.ConnectAsync(serverUri, System.Threading.CancellationToken.None);
                    System.Console.WriteLine("Connected to WebSocket server");

                    await System.Threading.Tasks.Task.WhenAll(ReceiveMessages(client), SendMessages(client));
                }
                catch (System.Exception e)
                {
                    System.Console.WriteLine($"Exception: {e.Message}");
                }
            }
        }


        static async System.Threading.Tasks.Task ReceiveMessages(System.Net.WebSockets.ClientWebSocket client)
        {
            byte[] buffer = new byte[1024];
            while (client.State == System.Net.WebSockets.WebSocketState.Open)
            {
                System.Net.WebSockets.WebSocketReceiveResult result = await client.ReceiveAsync(new System.ArraySegment<byte>(buffer), System.Threading.CancellationToken.None);
                string message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                System.Console.WriteLine($"Received from server: {message}");
            }
        }


        static async System.Threading.Tasks.Task SendMessages(System.Net.WebSockets.ClientWebSocket client)
        {
            while (client.State == System.Net.WebSockets.WebSocketState.Open)
            {
                string? message = System.Console.ReadLine();
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
                await client.SendAsync(new System.ArraySegment<byte>(buffer), System.Net.WebSockets.WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
            }
        }


    }

}
