public class AppData : IVersionedData
{
    public int Version { get; set; } = 1;
    public int Size { get; set; } = 1;
    public int Color { get; set; } = 1;
    public int Moves { get; set; } = 1;
}