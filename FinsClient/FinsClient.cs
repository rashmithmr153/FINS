using System.Data;
using  System.Net.Sockets;
using System.Runtime.CompilerServices;

class FinsClient
{
    private TcpClient _tcpClient;
    private NetworkStream _networkStream;

    private string _host;
    private int _port;
    private byte _plcNode;
    private byte _clientNode;

    public FinsClient(string Host,int Port)
    {
        _host=Host;
        _port=Port;
    }

    public async Task ConnectAsync()
    {
        _tcpClient=new TcpClient();
        await _tcpClient.ConnectAsync(_host,_port);
        _networkStream=_tcpClient.GetStream();

        byte[] packet=BuildhandShake();
        _networkStream.Write(packet,0,packet.Length);

        byte[] reply= new byte[24];
        await _networkStream.ReadAsync(reply);
        _plcNode=reply[19];
        _clientNode=reply[23];

        Console.WriteLine("PLC reply: ");
        foreach (byte b in reply)
        {
            Console.Write($"{b:X2}");
        }
        Console.WriteLine();
    }

    public async Task ReadDmWordAsync()
    {
        byte []readCommand=BuildReadCommand();
        await _networkStream.WriteAsync(readCommand,0,readCommand.Length);
        byte [] reply=new byte[32];
        await _networkStream.ReadAsync(reply,0,reply.Length);

        Console.WriteLine("Read command reply:");
        foreach (byte b in reply)
        {
            Console.Write($"{b:X2}");
        }
        Console.WriteLine();
    }

    private byte[] BuildhandShake()
    {
//         [FINS][length=12][command=handshake][error=0][my node=auto]
//            4B      4B            4B              4B         4B
        byte[] packet = new byte[]
        {            // Bytes 0-3: "FINS" in ASCII
            0x46, 0x49, 0x4E, 0x53,

            // Bytes 4-7: Length = 12 (0x0C)
            0x00,0x00,0x00,0x0C,

            // Bytes 8-11: Command = 0 (handshake)
             0x00,0x00,0x00,0x00,

            // Bytes 12-15: Error = 0
             0x00,0x00,0x00,0x00,

            // Bytes 16-19: Client node = 0 (auto)
             0x00,0x00,0x00,0x00
        };
        return packet;
    }

    private byte[] BuildReadCommand()
    {
        byte []command=new byte[]
        {
            // FINS TCP Header (16 bytes)
            0x46, 0x49, 0x4E, 0x53,  // "FINS"
            0x00, 0x00, 0x00, 0x1C,  // length = 28
            0x00, 0x00, 0x00, 0x02,  // command = send FINS command
            0x00, 0x00, 0x00, 0x00,  // error = 0

            // FINS Frame header (10 bytes)
            0x80,        // ICF
            0x00,        // RSV
            0x02,        // GCT
            0x00,        // DNA
            _plcNode,    // DA1 ← uses the value we saved!
            0x00,        // DA2
            0x00,        // SNA
            _clientNode, // SA1 ← uses the value we saved!
            0x00,        // SA2
            0x00,        // SID

            // Command code (2 bytes)
            0x01,0x01,

            // Response code (2 bytes)
            0x00,0x00,

            // Read parameters (6 bytes)
            
            //memory area = 82 (DM area)
            0x82,
            
            //address = 00 00 00 (DM word 0)
            0x00,0x00,0x00,

            // number of items = 00 01 (read 1 word)
            0x00,0x01
        };
        return command;
    }
}