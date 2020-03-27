using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using RingerStaff.Models;
using RingerStaff.Views;
using System.Linq;

namespace RingerStaff.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        private bool isRefreshing;

        public ObservableCollection<Item> Items { get; set; }
        public Command LoadItemsCommand { get; set; }
        public Command RefreshCommand { get; set; }

        public bool IsRefreshing { get => isRefreshing; set => SetProperty(ref isRefreshing, value); }

        public ItemsViewModel()
        {
            Title = "Browse";
            Items = new ObservableCollection<Item>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            RefreshCommand = new Command(async () => await ExcuteRefreshCommand());


            MessagingCenter.Subscribe<NewItemPage, Item>(this, "AddItem", async (obj, item) =>
            {
                var newItem = item as Item;
                Items.Add(newItem);

                MessagingCenter.Send<ItemsViewModel, Item>(this, "ScrollToItem", newItem);

                await DataStore.AddItemAsync(newItem);
            });
        }

        private async Task ExcuteRefreshCommand()
        {
            var firstItem = Items.First();

            for (int i = 0; i < 20; i++)
            {
                Items.Insert(0, new Item { Description = "Added Item", Text = $"{i}" });
            }

            IsRefreshing = false;

            MessagingCenter.Send(this, "ScrollToItem", firstItem);
        }

        async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await DataStore.GetItemsAsync(true);
                foreach (var item in items)
                {
                    Items.Add(item);
                }

                MessagingCenter.Send<ItemsViewModel, Item>(this, "ScrollToItem", items.Last());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}