using System;
using CoreGraphics;
using UIKit;

namespace DateTimePicker
{
    public partial class ViewController : UIViewController
    {
        private readonly Extrasoft.Controls.DatePicker.DateTimePicker picker;
        private readonly UIButton bigger;
        private readonly UIButton smaller;

        private readonly nfloat sizeIncrement = 20;

        public ViewController(IntPtr handle) : base(handle)
        {
            picker = new Extrasoft.Controls.DatePicker.DateTimePicker
            {
                CurrentDateTimeFormat = "dd/MM/yyyy HH:mm",
                Value = DateTime.Now
            };
            bigger = new UIButton();
            bigger.SetTitle("Bigger", UIControlState.Normal);
            bigger.TouchUpInside += Bigger_TouchUpInside;
            bigger.SetTitleColor(UIColor.Black, UIControlState.Normal);
            smaller = new UIButton();
            smaller.SetTitle("Smaller", UIControlState.Normal);
            smaller.TouchUpInside += Smaller_TouchUpInside;
            smaller.SetTitleColor(UIColor.Black, UIControlState.Normal);
        }

        private void Smaller_TouchUpInside(object sender, EventArgs e)
        {
            ResizePicker(sizeIncrement * -1);
        }

        private void Bigger_TouchUpInside(object sender, EventArgs e)
        {
            ResizePicker(sizeIncrement);
        }

        private void ResizePicker(nfloat increment)
        {
            var frame = picker.Frame;
            
            picker.Frame = new CGRect(frame.X, frame.Y, frame.Width + increment, frame.Height + increment);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.AddSubviews(smaller, bigger, picker);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();
            var bounds = this.View.Bounds;
            var width = bounds.Width / 2;
            var height = bounds.Height / 2;
            smaller.Frame = new CGRect(100, 60, 100, 20);
            bigger.Frame = new CGRect(width - 200, 60, 100, 20);
            var x = width / 2;
            var y = height / 2;
            picker.Frame = new CGRect(x, y, 380, 450);
        }

    }
}