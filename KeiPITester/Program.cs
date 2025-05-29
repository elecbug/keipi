using KeiPI;

public class Program
{
    public static void Main(string[] args)
    {
        KeiPIWrapper w2 = new KeiPIWrapper(KeiPIType.GraduateSchool);
        for (int i = 1; i <= 5; i++)
        {
            Console.WriteLine($"Page {i}:\n\n\n {string.Join("", w2.ToRows(i).Select(x=>x.ToString()))}");
        }
    }
}