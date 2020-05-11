using RingerStaff.Views;
using Xamarin.Forms;

namespace RingerStaff
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(ChatPage), typeof(ChatPage));
        }
    }
}
