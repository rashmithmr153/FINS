
using System.Net.Sockets;
// using System.Runtime.CompilerServices;

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

        //read the 20 bytes the client sends
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

        byte [] readCommand= new byte[36];
        await stream.ReadAsync(readCommand,0,readCommand.Length);
        Console.WriteLine("Command from client :");
        foreach (byte b in readCommand)
        {
            Console.Write($"{b:X2}");
        }
        Console.WriteLine();

        byte [] replyCommand= BuildReply();
        await stream.WriteAsync(replyCommand,0,replyCommand.Length);
        Console.WriteLine("Reply sent");
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
    }
    private byte[] BuildReply()
    {
        return new byte[]
        {
            // FINS TCP Header (16 bytes)
            0x46, 0x49, 0x4E, 0x53,  // "FINS" magic
            0x00, 0x00, 0x00, 0x18,  // length = 24 (everything after this point)
            0x00, 0x00, 0x00, 0x02,  // command = 2 (FINS command reply)
            0x00, 0x00, 0x00, 0x00,  // error = 0 (no error)

            // FINS Frame Header (10 bytes)
            0xC0,  // ICF = C0 (this is a RESPONSE, not a command)
            0x00,  // RSV = always 0
            0x02,  // GCT = always 2
            0x00,  // DNA = local network
            0x01,  // DA1 = destination is your client node (1)
            0x00,  // DA2 = CPU unit
            0x00,  // SNA = local network
            0x01,  // SA1 = source is PLC node (1)
            0x00,  // SA2 = always 0
            0x00,  // SID = always 0

            // Command echo (2 bytes)
            // PLC echoes back what command was requested
            0x01, 0x01,  // memory read echo

            // Response code (2 bytes)
            // 00 00 = success
            0x00, 0x00,

            // Actual data (2 bytes)
            // This is the value stored in DM word 0
            // 0x00, 0x2A = 42 in decimal
            0x00, 0x2A
        };
    }
}