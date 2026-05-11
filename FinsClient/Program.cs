class Program
{
        static async Task Main()
        {
            var client = new FinsClient("127.0.0.1", 9600);
            await client.ConnectAsync();
        }
}