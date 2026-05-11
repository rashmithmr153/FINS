
using System.Net.Sockets;

class FinsSimulator
{
    private TcpListener _listener;
    private int _port;

    public FinsSimulator(int port)
    {
        _port=port;
        _listener=new TcpListener(_port);
    }

    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine($"Fake PLC listening on port {_port}...");

        // wait for FinsClient to connect
        TcpClient client = await _listener.AcceptTcpClientAsync();
        Console.WriteLine("Client connected!");

        NetworkStream stream = client.GetStream();

        // TODO 1: read the 20 bytes the client sends
        byte[] data= new byte[20];
        await stream.ReadAsync(data,0,data.Length);

        Console.WriteLine("handshake data: ");
        foreach (byte b in data)
        {
            Console.Write($"{b:X2}");
        }
        Console.WriteLine();

        byte [] reply= BuildHandshakeReply();

        await stream.WriteAsync(reply,0,reply.Length);
    }

    private byte[] BuildHandshakeReply()
    {
        byte [] reply= new byte[]
        {
            // 46 49 4E 53  → "FINS"
            0x46, 0x49, 0x4E, 0x53,

            // 00 00 00 10  → length = 16
            0x00,0x00,0x00,0x10,

            // 00 00 00 01  → command = 1 (reply)
            0x00,0x00,0x00,0x01,
            
            // 00 00 00 00  → error = 0
            0x00,0x00,0x00,0x00,

            // 00 00 00 01  → PLC node
            0x00,0x00,0x00,0x01,

            // 00 00 00 01  → client node
            0x00,0x00,0x00,0x01,
            
        };
        return reply;
        // TODO: return these exact 24 bytes:
        
        
    }
}