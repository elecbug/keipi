namespace KeiPI
{
    public partial class Api
    {
        public static string Version => "0.1.0";

        public static string GetVersion()
        {
            return Version;
        }

        public static string GetVersionWithPrefix()
        {
            return $"KeiPI v{Version}";
        }
    }
}