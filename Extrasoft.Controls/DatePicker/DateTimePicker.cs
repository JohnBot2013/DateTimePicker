using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Extrasoft.Controls.DatePicker
{
    /// <summary>
    /// An iOS DateTime picker in a more traditional style (designed for use on tablets)
    /// The DateTime picker supports nullable datetime values and is ideal for use in filters.
    /// </summary>
    public class DateTimePicker : UIView
    {
        public event EventHandler<DateTimePickerEventArgs> NowPressedEvent;
        public event EventHandler<DateTimePickerEventArgs> ClearPressedEvent;
        public event EventHandler<DateTimePickerEventArgs> DonePressedEvent;

        private const string DayShortNames = "SuMoTuWeThFrSa";

        private UIButton[] dayButtons;
        private UILabel[] dayLabels;
        private UIButton nowButton;
        private UIButton clearButton;
        private UIButton doneButton;
        private UILabel hourLabel;
        private UIButton hourDownButton;
        private UIButton hourUpButton;
        private UILabel hourTextLabel;
        private UILabel minuteLabel;
        private UIButton minuteDownButton;
        private UIButton minuteUpButton;
        private UILabel minuteTextLabel;
        private UIButton previousMonthButton;
        private UIButton nextMonthButton;
        private UILabel currentDateLabel;
        private UIButton currentMonthYearButton;
        private UILabel currentDate;
        private UITableView yearList;
        private UIView yearListOverlay;

        private UIView divider1;
        private UIView divider2;
        private UIView divider3;
        private UIView divider4;
        private UIView divider5;

        private readonly nfloat dividerHeight = 2f;

        private readonly nfloat paddingLeft = 10f;
        private readonly nfloat paddingRight = 10f;
        private readonly nfloat paddingTop = 10f;
        private readonly nfloat paddingBottom = 10f;

        private nfloat totalWidth;
        private nfloat totalHeight;
        private nfloat usedWidth;
        private nfloat usedHeight;

        private nfloat gridWidth;
        private nfloat gridHeight;

        private nfloat imageButtonWidth;
        private nfloat imageButtonHeight;

        private const int DefaultMinMaxYearOffset = 50;

        private readonly nfloat dayHorizontalSpacing = 2;
        private readonly nfloat dayVerticalSpacing = 2;

        // Base width of control to use for scaling fonts
        private readonly nfloat baseControlWidth = 300;
        // Font size when control width = 300
        private readonly nfloat baseButtonFontSize = 18;
        // Font size when control width = 300
        private readonly nfloat baseTextFontSize = 17;

        // Adjust font size by this percent
        private readonly nfloat fontSizeChange = .125f;
        // Adjust font size when width changes by this much from the base size
        private readonly nfloat fontSizeWidthChange = 50;

        private nfloat buttonFontSize;
        private nfloat textFontSize;

        public DateTimePicker()
            :this(new CGRect(0, 0, 150, 250))
        {
        }

        public DateTimePicker(CGRect frame)
        {
            AutoAdjustFontSize = true;
            TextColor = UIColor.Black;
            SelectedDayColor = UIColor.LightGray;
            BorderColor = UIColor.LightGray;
            MaxYear = DateTime.Now.Year + DefaultMinMaxYearOffset;
            MinYear = DateTime.Now.Year - DefaultMinMaxYearOffset;
            InitialiseControl();
            CalculateComponentSizesBasedOnTheControlSize(frame);
            LayoutAllControls();
        }

        /// <summary>
        /// The current DateTime?
        /// </summary>
        public DateTime? Value { get; set; }
        /// <summary>
        /// When true then adjust the font size based on the overall control size
        /// </summary>
        public bool AutoAdjustFontSize { get; set; }
        /// <summary>
        /// The DateTime format for the currently selected date shown at the bottom of the control
        /// </summary>
        public string CurrentDateTimeFormat { get; set; }
        /// <summary>
        /// The color for all text in the control
        /// </summary>
        public UIColor TextColor { get; set; }
        /// <summary>
        /// Whether to show a border round the control
        /// </summary>
        public bool Border { get; set; }
        /// <summary>
        /// The border color. This is also used for the color of the splitters in the control
        /// </summary>
        public UIColor BorderColor { get; set; }
        /// <summary>
        /// The maximum value of the available years in the Year List
        /// </summary>
        public int MaxYear { get; set; }
        /// <summary>
        /// The minimum value of the available years in the Year List
        /// </summary>
        public int MinYear { get; set; }
        /// <summary>
        /// The background color for the selected day
        /// </summary>
        public UIColor SelectedDayColor { get; set; }

        private void UpdateUiWhenDateHasChanged(DateTime? oldDate, DateTime? newDate)
        {            
            var date = newDate ?? DateTime.Now;

            if (DoTheDaysNeedRedrawing(oldDate, newDate))
                LayoutTheDaysGrid(date);

            SetTheSelectedDay(date);

            hourTextLabel.Text = $"{date:HH}";
            minuteTextLabel.Text = $"{date:mm}";
            if (newDate.HasValue)
                currentDate.Text = date.ToString(string.IsNullOrEmpty(CurrentDateTimeFormat) ? "g" : CurrentDateTimeFormat);
            else
                currentDate.Text = string.Empty;
        }

        private bool DoTheDaysNeedRedrawing(DateTime? oldDate, DateTime? newDate)
        {
            if (oldDate == null && newDate == null)
                return false;
            if (oldDate.HasValue && newDate.HasValue && oldDate.Value == newDate.Value)
                return false;
            return true;
        }

        private void SetTheSelectedDay(DateTime date)
        {
            currentMonthYearButton.SetTitle($"{date:MMMM} {date:yyyy}", UIControlState.Normal);
            for (int i = 0; i < dayButtons.Length; i++)
            {
                var dayOfMonth = i + 1;
                var button = dayButtons[i];
                button.BackgroundColor = dayOfMonth == date.Day ? SelectedDayColor : UIColor.Clear;
            }
        }

        private void LayoutTheDaysGrid(DateTime date)
        {
            var y = gridHeight * 4;
            var root = new DateTime(date.Year, date.Month, 1);
            var dayOfWeek = (int)root.DayOfWeek; // 0=Su, 6=Sa
            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            var x = paddingLeft + (dayOfWeek * (gridWidth + dayHorizontalSpacing));
            for (var i = 0; i < dayButtons.Length; i++)
            {
                var dayOfMonth = i + 1;
                var button = dayButtons[i];
                if (i < daysInMonth)
                {
                    button.Hidden = false;
                    button.Frame = new CGRect(x, y, gridWidth, gridHeight);
                }
                else
                {
                    button.Hidden = true;
                }
                x += (gridWidth + dayHorizontalSpacing);
                dayOfWeek++;
                if (dayOfWeek > (int)DayOfWeek.Saturday)
                {
                    x = paddingLeft;
                    dayOfWeek = (int)DayOfWeek.Sunday;
                    y += gridHeight + dayVerticalSpacing;
                }

                if (AutoAdjustFontSize)
                {
                    button.Font = GetTheFontForAButton();
                }
            }
        }

        private void CalculateComponentSizesBasedOnTheControlSize(CGRect bounds)
        {
            totalWidth = bounds.Width;
            totalHeight = bounds.Height;
            usedWidth = totalWidth - paddingLeft - paddingRight;
            usedHeight = totalHeight - paddingTop - paddingBottom;
            gridWidth = (usedWidth - (dayHorizontalSpacing * 6f)) / 7f;
            // The proportions vertically of the days area is 6/14 and we will always size this area for 6 rows
            var daysAreaHeight = (usedHeight / 17f) * 6f;
            gridHeight = (daysAreaHeight - (dayVerticalSpacing * 5f)) / 6f;
            imageButtonHeight = gridHeight;
            imageButtonWidth = gridWidth;
            // font sizes
            var diff = baseControlWidth - totalWidth;
            var absDiff = Math.Abs(diff);
            if (absDiff >= fontSizeWidthChange)
            {
                var multiplier = (int)absDiff / fontSizeWidthChange;
                var scale = ((nfloat)multiplier) * fontSizeChange;
                if (diff > 0)
                {
                    buttonFontSize = baseButtonFontSize - (baseButtonFontSize * scale);
                    textFontSize = baseTextFontSize - (baseTextFontSize * scale);
                }
                else
                {
                    buttonFontSize = baseButtonFontSize + (baseButtonFontSize * scale);
                    textFontSize = baseTextFontSize + (baseTextFontSize * scale);
                }
            }
            else
            {
                buttonFontSize = baseButtonFontSize;
                textFontSize = baseTextFontSize;
            }

        }

        private void SetTheTextColorForAllControls()
        {
            currentMonthYearButton.SetTitleColor(TextColor, UIControlState.Normal);
            for (int i = 0; i < 7; i++)
            {
                dayLabels[i].TextColor = TextColor;
            }
            SetTheSelectedDay(Value ?? DateTime.Now);
            hourLabel.TextColor = TextColor;
            minuteLabel.TextColor = TextColor;
            hourDownButton.SetTitleColor(TextColor, UIControlState.Normal);
            hourTextLabel.TextColor = TextColor;
            hourUpButton.SetTitleColor(TextColor, UIControlState.Normal);
            minuteDownButton.SetTitleColor(TextColor, UIControlState.Normal);
            minuteTextLabel.TextColor = TextColor;
            minuteUpButton.SetTitleColor(TextColor, UIControlState.Normal);
            nowButton.SetTitleColor(TextColor, UIControlState.Normal);
            clearButton.SetTitleColor(TextColor, UIControlState.Normal);
            doneButton.SetTitleColor(TextColor, UIControlState.Normal);
            currentDate.TextColor = TextColor;
        }

        public UIFont GetTheFontForAButton()
        {
            return UIFont.SystemFontOfSize(buttonFontSize);
        }

        public UIFont GetTheFontForALabelOrTextField()
        {
            return UIFont.SystemFontOfSize(textFontSize);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            CalculateComponentSizesBasedOnTheControlSize(this.Bounds);

            if (Border)
            {
                Layer.BorderColor = BorderColor.CGColor;
                Layer.BorderWidth = 1.0f;
                Layer.MasksToBounds = true;
            }

            LayoutAllControls();
        }

        private void LayoutAllControls()
        {
            // Year List Mask
            yearListOverlay.Frame = new CGRect(0, 0, this.Bounds.Width, this.Bounds.Height);

            // Navigation buttons and Month/Year label
            var y = paddingTop;
            previousMonthButton.Frame = new CGRect(paddingLeft, y, imageButtonWidth, imageButtonHeight);
            currentMonthYearButton.Frame = new CGRect(paddingLeft + gridWidth + 1f, y, (gridWidth * 5), gridHeight);
            nextMonthButton.Frame = new CGRect(paddingLeft + usedWidth - imageButtonWidth, y, imageButtonWidth, imageButtonHeight);
            if (AutoAdjustFontSize)
            {
                previousMonthButton.Font = GetTheFontForAButton();
                currentMonthYearButton.Font = GetTheFontForALabelOrTextField();
                nextMonthButton.Font = GetTheFontForAButton();
            }

            y += (gridHeight * 1.5f);
            divider1.Frame = new CGRect(0, y, totalWidth, dividerHeight);
            y += (gridHeight / 2f);

            // Days in week labels
            var x = paddingLeft;
            for (int i = 0; i < 7; i++)
            {
                var label = dayLabels[i];
                label.Frame = new CGRect(x, y, gridWidth, gridHeight);
                label.Font = GetTheFontForALabelOrTextField();
                x += (gridWidth + dayHorizontalSpacing);
            }

            // Day buttons depend on DateTime
            LayoutTheDaysGrid(Value ?? DateTime.Now);

            // Move past the days area
            y += (gridHeight * 8.5f);

            // Divider Line
            divider2.Frame = new CGRect(0, y, totalWidth, dividerHeight);
            y += (gridHeight / 2f);

            hourLabel.Frame = new CGRect(paddingLeft, y, (gridWidth * 3.5f), gridHeight);
            minuteLabel.Frame = new CGRect(paddingLeft + (gridWidth * 3.5f) + 1, y, (gridWidth * 3.5), gridHeight);
            if (AutoAdjustFontSize)
            {
                hourLabel.Font = GetTheFontForALabelOrTextField();
                minuteLabel.Font = GetTheFontForALabelOrTextField();
            }

            y += gridHeight;

            hourDownButton.Frame = new CGRect(paddingLeft, y, imageButtonWidth, imageButtonHeight);
            hourTextLabel.Frame = new CGRect(paddingLeft + gridWidth + 1, y, gridWidth * 1.5f, gridHeight);
            hourUpButton.Frame = new CGRect(paddingLeft + (gridWidth * 2.5f) + 1, y, imageButtonWidth, imageButtonHeight);
            minuteDownButton.Frame = new CGRect(paddingLeft + (gridWidth * 3.5) + 1, y, imageButtonWidth, imageButtonHeight);
            minuteTextLabel.Frame = new CGRect(paddingLeft + (gridWidth * 4.5f) + 1, y, (gridWidth * 1.5f), gridHeight);
            minuteUpButton.Frame = new CGRect(paddingLeft + (gridWidth * 6) + 1, y, imageButtonWidth, imageButtonHeight);
            if (AutoAdjustFontSize)
            {
                hourDownButton.Font = GetTheFontForAButton();
                hourTextLabel.Font = GetTheFontForALabelOrTextField();
                hourUpButton.Font = GetTheFontForAButton();
                minuteDownButton.Font = GetTheFontForAButton();
                minuteTextLabel.Font = GetTheFontForALabelOrTextField();
                minuteUpButton.Font = GetTheFontForAButton();
            }

            y += gridHeight;

            y += (gridHeight / 2f);
            divider3.Frame = new CGRect(0, y, totalWidth, dividerHeight);
            y += (gridHeight / 2f);

            nowButton.Frame = new CGRect(paddingRight, y, (gridWidth * 2f) + dayHorizontalSpacing, gridHeight);
            clearButton.Frame = new CGRect(paddingRight + (gridWidth * 2.5f) + (dayHorizontalSpacing * 2), y, (gridWidth * 2), gridHeight);
            doneButton.Frame = new CGRect(paddingLeft + (gridWidth * 5f) + (dayHorizontalSpacing * 5), y, (gridWidth * 2f), gridHeight);
            if (AutoAdjustFontSize)
            {
                nowButton.Font = GetTheFontForAButton();
                clearButton.Font = GetTheFontForAButton();
                doneButton.Font = GetTheFontForAButton();
            }

            y += gridHeight;

            y += (gridHeight / 2f);
            divider4.Frame = new CGRect(0, y, totalWidth, dividerHeight);
            y += gridHeight;

            currentDate.Frame = new CGRect(paddingLeft, y, usedWidth, gridHeight);
            if (AutoAdjustFontSize)
            {
                currentDate.Font = GetTheFontForALabelOrTextField();
            }
            SetTheTextColorForAllControls();
        }

        /// <summary>
        /// Create all required controls and assign to the relevant views
        /// </summary>
        private void InitialiseControl()
        {
            // The buttons used to display days in month
            var buttonList = new List<UIButton>();
            for (int i = 1; i <= 31; i++)
            {
                var button = CreateDayButton(i);
                buttonList.Add(button);
                this.AddSubview(button);
            }
            dayButtons = buttonList.ToArray();

            // The labels displaying Sunday thru Saturday
            var labelList = new List<UILabel>();
            for (int i = 0; i < 7; i++)
            {
                var label = CreateLabel(DayShortNames.Substring(i * 2, 2));
                labelList.Add(label);
                this.AddSubview(label);
            }
            dayLabels = labelList.ToArray();

            nowButton = CreateButtonButton("Now", OnNowButtonTouchUpInside);
            clearButton = CreateButtonButton("Clear", OnClearButtonTouchUpInside);
            doneButton = CreateButtonButton("Done", OnDoneButtonTouchUpInside);
            previousMonthButton = CreateNavButton(MonthNavigation.Previous);
            nextMonthButton = CreateNavButton(MonthNavigation.Next);
            hourDownButton = CreatePlusMinusButton(PlusMinus.Minus, OnHourDownButtonTouchUpInside);
            hourUpButton = CreatePlusMinusButton(PlusMinus.Plus, OnHourUpButtonTouchUpInside);
            minuteDownButton = CreatePlusMinusButton(PlusMinus.Minus, OnMinuteDownButtonTouchUpInside);
            minuteUpButton = CreatePlusMinusButton(PlusMinus.Plus, OnMinuteUpButtonTouchUpInside);
            currentDateLabel = CreateLabel(string.Empty);
            hourTextLabel = CreateLabel(string.Empty);
            minuteTextLabel = CreateLabel(string.Empty);
            currentMonthYearButton = CreateCurrentMonthYearButton();
            hourLabel = CreateLabel("Hour");
            minuteLabel = CreateLabel("Minute");
            currentDate = CreateLabel("");
            divider1 = CreateSeparator();
            divider2 = CreateSeparator();
            divider3 = CreateSeparator();
            divider4 = CreateSeparator();
            divider5 = CreateSeparator();

            yearList = CreateYearSelectorTableView();
            yearListOverlay = CreateYearListOverlay();           

            this.AddSubviews(new[]
            {
                nowButton,
                clearButton,
                doneButton,
                previousMonthButton,
                nextMonthButton,
                hourDownButton,
                hourUpButton,
                minuteDownButton,
                minuteUpButton,
                currentDateLabel,
                hourTextLabel,
                minuteTextLabel,
                currentMonthYearButton,
                hourLabel,
                minuteLabel,
                currentDate,
                divider1,
                divider2,
                divider3,
                divider4,
                divider5,
                yearList,
                yearListOverlay
            });
        }

        /// <summary>
        /// Create the overlay view used when the Year List is displayed.
        /// This is used to allow a user to tap outside of the Year List to close it instead of selecting a year.
        /// </summary>
        /// <returns></returns>
        private UIView CreateYearListOverlay()
        {
            var tap = new UITapGestureRecognizer(CloseYearList);
            var view = new UIView
            {
                BackgroundColor = UIColor.Black,
                Alpha = 0.05f,
                UserInteractionEnabled = true,
                Hidden = true
            };
            view.AddGestureRecognizer(tap);
            return view;
        }

        /// <summary>
        /// Create the button used to display the current Month and Year at the top of the picker
        /// </summary>
        /// <returns></returns>
        private UIButton CreateCurrentMonthYearButton()
        {
            var btn = CreateButton();
            btn.TouchUpInside += OnShowYearList;
            return btn;
        }

        /// <summary>
        /// Handle showing the Year List.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShowYearList(object sender, EventArgs e)
        {
            var years = GetYearList();
            int year = GetCurrentMonthYearYear();
            int index = year > 0 ? Array.IndexOf(years, year.ToString()) : -1;
            // Create the TableView's data source
            var source = new YearSource(this, years);
            // Year List - year tapped handler
            source.YearSelectedEvent += OnYearSelected;            
            yearList.Source = source;
            // Position the Year List
            yearList.Frame = GetYearListFrame();
            // Show the overlay
            yearListOverlay.Hidden = false;
            yearList.Hidden = false;
            // Ensure the YearList is in front of all other controls
            this.BringSubviewToFront(yearList);
            // Scroll the Year List to the current year
            if (index > -1)
            {
                var indexPath = NSIndexPath.FromRowSection(index, 0);
                yearList.SelectRow(indexPath, true, UITableViewScrollPosition.Middle);
            }
        }

        /// <summary>
        /// Create the table view used to list available years when the CurrentMonthYear button is clicked
        /// </summary>
        /// <returns></returns>
        private UITableView CreateYearSelectorTableView()
        {
            var table = new UITableView { Hidden = true };
            // Add a border
            table.Layer.BorderColor = BorderColor.CGColor;
            table.Layer.BorderWidth = 1.0f;
            table.Layer.MasksToBounds = true;
            return table;
        }

        /// <summary>
        /// Calculate the position for the Year List when it is to be displayed
        /// </summary>
        /// <returns></returns>
        private CGRect GetYearListFrame()
        {
            // Position the list under the current year/month label
            var x = currentMonthYearButton.Frame.X;
            var y = currentMonthYearButton.Frame.Y + currentMonthYearButton.Frame.Height;
            var width = currentMonthYearButton.Bounds.Width;
            // Ensure we have some margin at the bottom of the list
            var height = this.Bounds.Height - y - paddingBottom;
            return new CGRect(x, y, width, height);
        }

        /// <summary>
        /// Handle a year selection
        /// </summary>
        private void OnYearSelected(object sender, YearSelectedEventArgs e)
        {
            CloseYearList();
            yearList.Hidden = true;
            var oldDate = Value ?? DateTime.Now;
            var month = oldDate.Month;
            var day = oldDate.Day;
            if (month == 2 && day > 28)
            {
                day = DateTime.IsLeapYear(e.Year) ? 29 : 28;
            }
            var newDate = new DateTime(e.Year, month, day, oldDate.Hour, oldDate.Minute, 0);
            Value = newDate;
            UpdateUiWhenDateHasChanged(oldDate, Value);
        }

        /// <summary>
        /// When the Year List should close then clean up behind
        /// </summary>
        private void CloseYearList()
        {
            if (yearList.Source is YearSource source)
            {
                source.YearSelectedEvent -= OnYearSelected;
                source.Dispose();
                yearList.Source = null;
            }

            yearList.Hidden = true;
            yearListOverlay.Hidden = true;
        }

        /// <summary>
        /// Get the Year from the current Month Year button
        /// </summary>
        /// <returns></returns>
        private int GetCurrentMonthYearYear()
        {
            var txt = currentMonthYearButton.Title(UIControlState.Normal);
            if (string.IsNullOrEmpty(txt))
                return -1;
            var yr = txt.Substring(txt.Length - 4, 4);
            return Convert.ToInt32(yr);
        }

        /// <summary>
        /// Generate the list of years to be displayed in the Year List
        /// </summary>
        /// <returns></returns>
        private string[] GetYearList()
        {
            var list = new List<string>();
            for (int i = MinYear; i <= MaxYear; i++)
            {
                list.Add(i.ToString());
            }

            return list.ToArray();
        }

        /// <summary>
        /// Create a (month) navigation button
        /// </summary>
        private UIButton CreateNavButton(MonthNavigation nav)
        {
            var button = CreateButton();
            switch (nav)
            {
                case MonthNavigation.Previous:
                    button.SetBackgroundImage(UIImage.FromBundle("left"), UIControlState.Normal);
                    button.TouchUpInside += OnPrevMonthButtonTouchUpInside;
                    break;
                default:
                    button.SetBackgroundImage(UIImage.FromBundle("right"), UIControlState.Normal);
                    button.TouchUpInside += OnNextMonthButtonTouchUpInside;
                    break;
            }
            return button;
        }

        /// <summary>
        /// Create a button used to display days in a month (in the days grid)
        /// </summary>
        private UIButton CreateDayButton(int day)
        {
            var button = CreateButton();
            button.SetTitle(day.ToString(), UIControlState.Normal);
            button.TouchUpInside += OnDayButtonTouchUpInside;
            return button;
        }

        /// <summary>
        /// Create a button used to increment/decrement hours or minutes
        /// </summary>
        private UIButton CreatePlusMinusButton(PlusMinus plusMinus, EventHandler handler)
        {
            var button = CreateButton();
            button.TouchUpInside += handler;
            switch (plusMinus)
            {
                case PlusMinus.Minus:
                    button.SetTitle("-", UIControlState.Normal);
                    button.TitleLabel.TextAlignment = UITextAlignment.Right;
                    break;
                default:
                    button.SetTitle("+", UIControlState.Normal);
                    button.TitleLabel.TextAlignment = UITextAlignment.Left;
                    break;
            }
            return button;
        }

        /// <summary>
        /// Create a button used as a general button (e.g. Done, Clear, Now) complete with handler
        /// </summary>
        private UIButton CreateButtonButton(string title, EventHandler handler)
        {
            var button = CreateButton();
            button.SetTitle(title, UIControlState.Normal);
            button.TouchUpInside += handler;
            return button;
        }

        /// <summary>
        /// Create a label
        /// </summary>
        private UILabel CreateLabel(string text)
        {
            var label = new UILabel {Text = text, TextAlignment = UITextAlignment.Center};
            return label;
        }

        /// <summary>
        /// Create a button
        /// </summary>
        private UIButton CreateButton()
        {
            var button = new UIButton(UIButtonType.Plain);
            button.SetTitleColor(TextColor, UIControlState.Normal);
            button.TitleLabel.TextAlignment = UITextAlignment.Center;
            return button;
        }

        /// <summary>
        /// Create a separator view
        /// </summary>
        private UIView CreateSeparator()
        {
            var view = new UIView
            {
                BackgroundColor = BorderColor
            };
            return view;
        }

        private void OnPrevMonthButtonTouchUpInside(object sender, EventArgs e)
        {
            var oldDate = Value;
            var date = Value ?? DateTime.Now;
            Value = date.AddMonths(-1);
            UpdateUiWhenDateHasChanged(oldDate, Value);
        }

        private void OnNextMonthButtonTouchUpInside(object sender, EventArgs e)
        {
            var oldDate = Value;
            var date = Value ?? DateTime.Now;
            Value = date.AddMonths(1);
            UpdateUiWhenDateHasChanged(oldDate, Value);
        }

        private void OnDayButtonTouchUpInside(object sender, EventArgs e)
        {
            var oldDate = Value;
            var day = Convert.ToInt32(((UIButton) sender).Title(UIControlState.Normal));
            var date = Value ?? DateTime.Now;
            var newDate = new DateTime(date.Year, date.Month, day, date.Hour, date.Minute, date.Second);
            Value = newDate;
            UpdateUiWhenDateHasChanged(oldDate, newDate);
        }

        private void OnHourDownButtonTouchUpInside(object sender, EventArgs e)
        {
            var oldDate = Value;
            var date = Value ?? DateTime.Now;
            date = date.AddHours(-1);
            Value = date;
            UpdateUiWhenDateHasChanged(oldDate, date);
        }

        private void OnHourUpButtonTouchUpInside(object sender, EventArgs e)
        {
            var oldDate = Value;
            var date = Value ?? DateTime.Now;
            date = date.AddHours(1);
            Value = date;
            UpdateUiWhenDateHasChanged(oldDate, date);
        }

        private void OnMinuteDownButtonTouchUpInside(object sender, EventArgs e)
        {
            var oldDate = Value;
            var date = Value ?? DateTime.Now;
            date = date.AddMinutes(-1);
            Value = date;
            UpdateUiWhenDateHasChanged(oldDate, date);
        }

        private void OnMinuteUpButtonTouchUpInside(object sender, EventArgs e)
        {
            var oldDate = Value;
            var date = Value ?? DateTime.Now;
            date = date.AddMinutes(1);
            Value = date;
            UpdateUiWhenDateHasChanged(oldDate, date);
        }

        private void OnNowButtonTouchUpInside(object sender, EventArgs e)
        {
            var oldDate = Value;
            Value = DateTime.Now;
            UpdateUiWhenDateHasChanged(oldDate, Value);
            OnNowPressed();
        }

        private void OnClearButtonTouchUpInside(object sender, EventArgs e)
        {
            var oldDate = Value;
            Value = null;
            UpdateUiWhenDateHasChanged(oldDate, Value);
            OnClearPressed();
        }

        private void OnDoneButtonTouchUpInside(object sender, EventArgs e)
        {
            OnDonePressed();
        }

        private void OnNowPressed()
        {
            NowPressedEvent?.Invoke(this, new DateTimePickerEventArgs(Value));
        }

        private void OnClearPressed()
        {
            ClearPressedEvent?.Invoke(this, new DateTimePickerEventArgs(Value));
        }

        private void OnDonePressed()
        {
            DonePressedEvent?.Invoke(this, new DateTimePickerEventArgs(Value));
        }

        public enum MonthNavigation
        {
            Previous,
            Next
        }

        public enum PlusMinus
        {
            Plus,
            Minus
        }

        /// <summary>
        /// Provides the data source for the Year List table view
        /// </summary>
        private class YearSource : UITableViewSource
        {
            public event EventHandler<YearSelectedEventArgs> YearSelectedEvent;

            private const string CellIdentifier = "YearCell";

            private string[] years;
            private DateTimePicker picker;

            public YearSource(DateTimePicker picker, string[] years)
            {
                this.picker = picker;
                this.years = years;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return years.Length;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell(CellIdentifier) ?? new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier);
                cell.TextLabel.Font = picker.GetTheFontForALabelOrTextField();
                cell.TextLabel.Text = years[indexPath.Row];
                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                OnYearSelected(years[indexPath.Row]);
            }

            private void OnYearSelected(string yearValue)
            {
                YearSelectedEvent?.Invoke(this, new YearSelectedEventArgs(Convert.ToInt32(yearValue)));
            }
        }

        public class YearSelectedEventArgs : EventArgs
        {
            public YearSelectedEventArgs(int year)
            {
                Year = year;
            }
            public int Year { get; }
        }
    }

}