namespace KeiPI
{
    /// <summary>
    /// Represents the main API class for KeiPI, providing version information and methods to retrieve it.
    /// </summary>
    public partial class Api
    {
        /// <summary>
        /// Gets the version of the KeiPI API.
        /// </summary>
        public static string Version => "0.1.0";

        /// <summary>
        /// Gets the version of the KeiPI API with a prefix for display purposes.
        /// </summary>
        /// <returns></returns>
        public static string GetVersionWithPrefix()
        {
            return $"KeiPI v{Version}";
        }
    }
}