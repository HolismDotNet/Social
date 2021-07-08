using Holism.Api;

namespace Holism.Social.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Startup.AddControllerSearchAssembly(typeof(Controllers.CommentController).Assembly);
            Holism.Api.Config.ConfigureEverything();
            Application.Run();
        }
    }
}
