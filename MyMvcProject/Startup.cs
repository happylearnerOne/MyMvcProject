using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MyMvcProject.Startup))]
namespace MyMvcProject
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
