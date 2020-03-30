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
        public DataTemplate KoreaConsulateTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var consulate = item as ConsulateModel;

            if (consulate.IsHeader)
                return FirstConsulateTemplate;

            if (consulate.IsFooter)
                return LastConsulateTemplate;

            if (consulate.IsInKorea)
                return KoreaConsulateTemplate;

            return ConsulateTemplate;
        }
    }

    public class ConsulateEmptyViewSelector : DataTemplateSelector
    {
        public DataTemplate LoadingTemplate { get; set; }
        public DataTemplate PermissionTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var state = item as string;

            if (state == "Permission")
                return PermissionTemplate;

            return LoadingTemplate;
        }
    }
}
