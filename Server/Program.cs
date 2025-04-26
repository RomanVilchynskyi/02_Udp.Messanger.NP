using System.Net;
using System.Net.Sockets;
using System.Text;

public class ChatServer
{
    const int port = 4040;
    const string JOIN_CMD = "$<join>";
    const string LEAVE_CMD = "$<leave>";
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
    private void RemoveMember(IPEndPoint member)
    {
        if (members.Contains(member))
        {
            members.Remove(member);
            Console.WriteLine($"Member left ---- number {members.Count} {members.Count}");
        }
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
            if (message == JOIN_CMD)
            {
                AddMember(client);
                SendAllMembers($"** New member added: {client} **");
            }
            else if (message.StartsWith(LEAVE_CMD))
            {
                RemoveMember(client);
                string nickname = message.Substring(LEAVE_CMD.Length).Trim();
                SendAllMembers($"** {nickname} left **");
            }
            else
            {
                Console.WriteLine($"{DateTime.Now.ToShortTimeString()} -- {message} from {client}");
                SendAllMembers(message);
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