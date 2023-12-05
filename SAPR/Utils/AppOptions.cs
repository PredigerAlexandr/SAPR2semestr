namespace SAPR.Utils
{
    public class AppOptions
    {
        public ConnectionString ConnectionStrings { get; set; }
        public Directory Directories { get; set; }
        public class ConnectionString
        {
            public string DefaultConnection { get; set; }
        }
        public class Directory
        {
            public string GeneratedCode { get; set; }
            public string Certificate { get; set; }
        }
    }
}
