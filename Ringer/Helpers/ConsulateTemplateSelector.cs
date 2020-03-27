using System;
using Ringer.Models;
using Xamarin.Forms;

namespace Ringer.Helpers
{
    public class ConsulateTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FirstConsulateTemplate { get; set; }
        public DataTemplate LastConsulateTemplate { get; set; }
        public DataTemplate ConsulateTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var consulate = item as ConsulateModel;

            if (consulate.IsHeader)
                return FirstConsulateTemplate;

            if (consulate.IsFooter)
                return LastConsulateTemplate;

            return ConsulateTemplate;
        }
    }
}
