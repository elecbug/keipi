using KeiPI;

public class Program
{
    public static void Main(string[] args)
    {
        //for (int i = 0; i < Enum.GetValues(typeof(ApiType)).Length; i++)
        //{
        //    Api api = new Api((ApiType)i);
        
        //    Console.WriteLine($"[{api.Name}]\n {api.ToRowsWithCount(25)[24]}");
        //}

        Api api = new Api(ApiType.Teach);

        for (int i = 25; i < 35; i++)
        {
            Console.WriteLine($"[{api.Name}]\n{api.ToRowsWithCount(i+1)[i]}");
        }
    }
}