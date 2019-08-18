using System;

using Ringer.Models;

namespace Ringer.ViewModels
{
    public class ItemDetailViewModel : INotifyPropertyChanged
    {
        public Item Item { get; set; }
        public ItemDetailViewModel(Item item = null)
        {
            Title = item?.Text;
            Item = item;
        }
    }
}
