using System;

namespace Extrasoft.Controls
{
    public class DateTimePickerEventArgs : EventArgs
    {
        internal DateTimePickerEventArgs(DateTime? date)
        {
            Date = date;
        }
        public DateTime? Date { get; }
    }
}