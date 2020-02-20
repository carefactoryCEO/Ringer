using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using RingerStaff.Models;
using RingerStaff.Views;
using RingerStaff.ViewModels;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace RingerStaff.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class ItemsPage : ContentPage
    {
        ItemsViewModel viewModel;

        public ItemsPage()
        {
            InitializeComponent();

            BindingContext = viewModel = new ItemsViewModel();

            MessagingCenter.Subscribe<ItemsViewModel, Item>(this, "ScrollToItem", (obj, item) =>
            {
                ItemsListView.ScrollTo(item, ScrollToPosition.End, true);
            });
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var item = args.SelectedItem as Item;
            if (item == null)
                return;

            await Navigation.PushAsync(new ItemDetailPage(new ItemDetailViewModel(item)));

            // Manually deselect item.
            ItemsListView.SelectedItem = null;
        }

        async void AddItem_Clicked(object sender, EventArgs e)
        {
            var navPage = new Xamarin.Forms.NavigationPage(new NewItemPage());
            navPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);
            navPage.On<iOS>().SetPrefersLargeTitles(true);
            //navPage.BarBackgroundColor = Color.Pink;

            await Navigation.PushModalAsync(navPage);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (viewModel.Items.Count == 0)
                viewModel.LoadItemsCommand.Execute(null);
        }
    }
}