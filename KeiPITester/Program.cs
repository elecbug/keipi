using KeiPI;

public class Program
{
    public static void Main(string[] args)
    {
        for (int i = 0; i < Enum.GetValues(typeof(ApiType)).Length; i++)
        {
            Api api = new Api((ApiType)i);
        
            Console.WriteLine($"[{api.Name}] {api.ToRows(1)[0]}");
        }
    }
}