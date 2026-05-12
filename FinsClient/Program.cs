class Program
{
        static async Task Main()
        {
             try
        {
            var client = new FinsClient("127.0.0.1", 9600);
            await client.ConnectAsync();
            await client.ReadDmWordAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        }
}