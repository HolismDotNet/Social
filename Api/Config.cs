using Holism.Api;

namespace Holism.Social.UserApi
{
    public class Config
    {
        public static void ConfigureEverything()
        {
            Startup.AddControllerSearchAssembly(typeof(Config).Assembly);
        }
    }
}
