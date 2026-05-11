class Program
{
    static async Task Main()
    {
        var simulator = new FinsSimulator(9600);
        await simulator.StartAsync();
    }
}