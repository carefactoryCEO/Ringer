using Ringer.Views;
using Xamarin.Forms;

namespace Ringer
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            //Routing.RegisterRoute("mappage", typeof(MapPage));
            Routing.RegisterRoute("chatpage", typeof(ChatPage));
            Routing.RegisterRoute("vidyopage", typeof(VidyoPage));
        }
    }
}
