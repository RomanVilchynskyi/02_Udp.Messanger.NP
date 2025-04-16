using System.Net;
using System.Net.Sockets;
using System.Text;

public class ChatServer
{
    const int port = 4040;
    const string JOIN_CMD = "$<join>";
    UdpClient server;
    IPEndPoint client = null;
    List<IPEndPoint> members;
    public ChatServer()
    {
        server = new UdpClient(port);
        members = new List<IPEndPoint>();
    }

    private void AddMember(IPEndPoint member)
    {
        if (!members.Contains(member))
            members.Add(member);
        Console.WriteLine($"Member was added ---- number {members.Count}");
    }
    private async void SendAllMembers(string message)
    {
        byte[] data = Encoding.Unicode.GetBytes(message);
        foreach(var item in members) 
        {
            await server.SendAsync(data, data.Length, item);
        }
    }
    public void Start()
    {
        while (true)
        {
            byte[] data = server.Receive(ref client);
            string message = Encoding.Unicode.GetString(data);
            switch (message)
            {
                case JOIN_CMD:
                    AddMember(client);
                    break;
                default:
                    Console.WriteLine($"{DateTime.Now.ToShortTimeString()} -- {message} from {client}");
                    SendAllMembers(message);
                    break;
            }
        }
    }

}


internal class Program
{

    private static void Main(string[] args)
    {
        ChatServer chat = new ChatServer();
        chat.Start();
       
    }
}