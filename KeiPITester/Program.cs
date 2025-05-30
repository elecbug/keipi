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

        Console.WriteLine("No cache test");
        for (int a = 0; a < Enum.GetValues(typeof(ApiType)).Length; a++)
        {
            Api api = new Api((ApiType)a);

            for (int i = 0; i < 5; i++)
            {
                //Console.WriteLine($"[{api.Name}]\n{api.GetRow(i * 10 + 1)}");
                api.GetRow(i * 10 + 1);
            }
        }

        Console.WriteLine("Cache test");
        for (int a = 0; a < Enum.GetValues(typeof(ApiType)).Length; a++)
        {
            Api api = new Api((ApiType)a);

            for (int i = 0; i < 5; i++)
            {
                //Console.WriteLine($"[{api.Name}]\n{api.GetRow(i * 10 + 1)}");
                api.GetRow(i * 10 + 1);
            }
        }
    }
}