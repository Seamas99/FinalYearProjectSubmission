using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalYearProject.Controls.Behaviours
{
    public class EntryBehaviour : Behavior<Entry>
    {
        public int Maximum { get; set; }
        public int Minimum { get; set; }

        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnTextChanged;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnTextChanged;
            base.OnDetachingFrom(entry);
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(e.NewTextValue, out int value))
            {
                if (value > Maximum)
                    ((Entry)sender).Text = Maximum.ToString();
                if (value < Minimum)
                    ((Entry)sender).Text = Minimum.ToString();
            }
        }
    }
}
