using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ecm.CustomControl
{
    public enum MaskType
    {
        Any,

        Decimal,

        Integer
    }

    public static class NumericTextBoxBehavior
    {
        public static int GetDecimalPoint(DependencyObject source)
        {
            return (int)source.GetValue(DecimalPointValueProperty);
        }

        public static MaskType GetMask(DependencyObject source)
        {
            return (MaskType)source.GetValue(MaskTypeProperty);
        }

        public static double GetMaximumValue(DependencyObject obj)
        {
            return (double)obj.GetValue(MaximumValueProperty);
        }

        public static double GetMinimumValue(DependencyObject obj)
        {
            return (double)obj.GetValue(MinimumValueProperty);
        }

        public static void SetDecimalPoint(DependencyObject source, MaskType value)
        {
            source.SetValue(DecimalPointValueProperty, value);
        }

        public static void SetMask(DependencyObject source, MaskType value)
        {
            source.SetValue(MaskTypeProperty, value);
        }

        public static void SetMaximumValue(DependencyObject obj, double value)
        {
            obj.SetValue(MaximumValueProperty, value);
        }

        public static void SetMinimumValue(DependencyObject obj, double value)
        {
            obj.SetValue(MinimumValueProperty, value);
        }

        private static void DecimalPointValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (d as TextBox);
            Validate(textBox, string.Empty + e.NewValue, GetMask(textBox));
        }

        private static string GetValidateValue(TextBox textBox, string inputText)
        {
            string validateValue;

            if (!string.IsNullOrEmpty(textBox.SelectedText))
            {
                string pre = textBox.Text.Substring(0, textBox.SelectionStart);
                string after = textBox.Text.Substring(
                    textBox.SelectionStart + textBox.SelectionLength,
                    textBox.Text.Length - (textBox.SelectionStart + textBox.SelectionLength));
                validateValue = pre + inputText + after;
            }
            else
            {
                string pre = textBox.Text.Substring(0, textBox.CaretIndex);
                string after = textBox.Text.Substring(textBox.CaretIndex, textBox.Text.Length - textBox.CaretIndex);
                validateValue = pre + inputText + after;
            }

            return validateValue;
        }

        private static void MaximumValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (d as TextBox);
            Validate(textBox, string.Empty + e.NewValue, GetMask(textBox));
        }

        private static void MinimumValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (d as TextBox);
            Validate(textBox, string.Empty + e.NewValue, GetMask(textBox));
        }

        private static void OnClipboardPaste(object sender, DataObjectPastingEventArgs e)
        {
            var textBox = sender as TextBox;
            var text = e.SourceDataObject.GetData(e.FormatToApply) as string;

            if (textBox != null && !string.IsNullOrEmpty(text) && !Validate(textBox, text, GetMask(textBox)))
            {
                e.CancelCommand();
            }

            if (textBox != null && !string.IsNullOrEmpty(text))
            {
                if (!ValidateValue(GetMask(textBox), text, GetMinimumValue(textBox), GetMaximumValue(textBox)))
                {
                    e.CancelCommand();
                }
            }
        }

        private static void OnMaskTypeStateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.PreviewTextInput -= TextBoxPreviewTextInput;
                textBox.PreviewKeyDown -= TextBoxPreviewKeyDown;

                DataObject.RemovePastingHandler(textBox, OnClipboardPaste);

                MaskType isCheck = ((e.NewValue != null && e.NewValue.GetType() == typeof(MaskType)))
                                       ? (MaskType)e.NewValue
                                       : MaskType.Any;

                if (isCheck != MaskType.Any)
                {
                    textBox.PreviewTextInput += TextBoxPreviewTextInput;
                    textBox.PreviewKeyDown += TextBoxPreviewKeyDown;
                    DataObject.AddPastingHandler(textBox, OnClipboardPaste);
                }
            }
        }

        private static void TextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;

            if (textBox != null && !Validate(textBox, e.Text, GetMask(textBox)))
            {
                e.Handled = true;
            }

            if (textBox != null)
            {
                string valueValidate = GetValidateValue(textBox, e.Text);
                if (!ValidateValue(GetMask(textBox), valueValidate, GetMinimumValue(textBox), GetMaximumValue(textBox)))
                {
                    e.Handled = true;
                }
            }
        }

        private static bool Validate(TextBox textBox, string newContent, MaskType maskType)
        {
            string testString;
            if (!string.IsNullOrEmpty(textBox.SelectedText))
            {
                string pre = textBox.Text.Substring(0, textBox.SelectionStart);
                string after = textBox.Text.Substring(
                    textBox.SelectionStart + textBox.SelectionLength,
                    textBox.Text.Length - (textBox.SelectionStart + textBox.SelectionLength));
                testString = pre + newContent + after;
            }
            else
            {
                string pre = textBox.Text.Substring(0, textBox.CaretIndex);
                string after = textBox.Text.Substring(textBox.CaretIndex, textBox.Text.Length - textBox.CaretIndex);
                testString = pre + newContent + after;
            }

            string pattern = @"^([-+]?)(\d*)$";
            if (maskType == MaskType.Decimal)
            {
                pattern = @"^([-+]?)(\d*)([,.]?)(\d*)$";
                int decimalPoint = GetDecimalPoint(textBox);
                if (decimalPoint > 0)
                {
                    pattern = @"^([-+]?)(\d*)([,.]?)(\d{0," + decimalPoint + "})$";
                }
            }
            else if (maskType == MaskType.Integer)
            {
                pattern = @"^([-+]?)(\d*)$";
            }

            var regExpr = new Regex(pattern);
            if (regExpr.IsMatch(testString))
            {
                return true;
            }

            return false;
        }

        private static bool ValidateValue(MaskType mask, string value, double min, double max)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            value = value.Trim();

            switch (mask)
            {
                case MaskType.Integer:
                    try
                    {
                        int intValue = Convert.ToInt32(value);
                        return (min <= intValue && intValue <= max);
                    }
                    catch
                    {
                    }
                    return false;
                case MaskType.Decimal:
                    try
                    {
                        double doubleValue = Convert.ToDouble(value);
                        return (Double.MinValue <= doubleValue && doubleValue <= Double.MaxValue);
                    }
                    catch
                    {
                    }
                    return false;
            }

            return value != string.Empty;
        }

        private static void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;

            if (e.Key == Key.Space)
            {
                if (textBox != null && !Validate(textBox, e.Key.ToString(), GetMask(textBox)))
                {
                    e.Handled = true;
                }
            }
        }

        public static readonly DependencyProperty DecimalPointValueProperty =
            DependencyProperty.RegisterAttached(
                "DecimalPoint",
                typeof(int),
                typeof(NumericTextBoxBehavior),
                new FrameworkPropertyMetadata(0, DecimalPointValueChangedCallback));

        public static readonly DependencyProperty MaskTypeProperty = DependencyProperty.RegisterAttached(
            "MaskType",
            typeof(MaskType),
            typeof(NumericTextBoxBehavior),
            new UIPropertyMetadata(MaskType.Any, OnMaskTypeStateChanged));

        public static readonly DependencyProperty MaximumValueProperty =
            DependencyProperty.RegisterAttached(
                "MaximumValue",
                typeof(double),
                typeof(NumericTextBoxBehavior),
                new FrameworkPropertyMetadata(double.NaN, MaximumValueChangedCallback));

        public static readonly DependencyProperty MinimumValueProperty =
            DependencyProperty.RegisterAttached(
                "MinimumValue",
                typeof(double),
                typeof(NumericTextBoxBehavior),
                new FrameworkPropertyMetadata(double.NaN, MinimumValueChangedCallback));
    }

    public static class NumericAutoComplateBoxBehavior
    {
        public static MaskType GetMask(DependencyObject source)
        {
            return (MaskType)source.GetValue(MaskTypeProperty);
        }

        public static double GetMaximumValue(DependencyObject obj)
        {
            return (double)obj.GetValue(MaximumValueProperty);
        }

        public static double GetMinimumValue(DependencyObject obj)
        {
            return (double)obj.GetValue(MinimumValueProperty);
        }

        public static void SetMask(DependencyObject source, MaskType value)
        {
            source.SetValue(MaskTypeProperty, value);
        }

        public static void SetMaximumValue(DependencyObject obj, double value)
        {
            obj.SetValue(MaximumValueProperty, value);
        }

        public static void SetMinimumValue(DependencyObject obj, double value)
        {
            obj.SetValue(MinimumValueProperty, value);
        }

        private static string GetValidateValue(TextBox textBox, string inputText)
        {
            string validateValue;

            if (!string.IsNullOrEmpty(textBox.SelectedText))
            {
                string pre = textBox.Text.Substring(0, textBox.SelectionStart);
                string after = textBox.Text.Substring(
                    textBox.SelectionStart + textBox.SelectionLength,
                    textBox.Text.Length - (textBox.SelectionStart + textBox.SelectionLength));
                validateValue = pre + inputText + after;
            }
            else
            {
                string pre = textBox.Text.Substring(0, textBox.CaretIndex);
                string after = textBox.Text.Substring(textBox.CaretIndex, textBox.Text.Length - textBox.CaretIndex);
                validateValue = pre + inputText + after;
            }

            return validateValue;
        }

        private static void MaximumValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (d as AutoCompleteBox);
            Validate(textBox, string.Empty + e.NewValue, GetMask(textBox));
        }

        private static void MinimumValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (d as AutoCompleteBox);
            Validate(textBox, string.Empty + e.NewValue, GetMask(textBox));
        }

        private static void OnClipboardPaste(object sender, DataObjectPastingEventArgs e)
        {
            var textBox = sender as AutoCompleteBox;
            var text = e.SourceDataObject.GetData(e.FormatToApply) as string;

            if (textBox != null && !string.IsNullOrEmpty(text) && !Validate(textBox, text, GetMask(textBox)))
            {
                e.CancelCommand();
            }

            if (textBox != null && !string.IsNullOrEmpty(text))
            {
                if (!ValidateValue(GetMask(textBox), text, GetMinimumValue(textBox), GetMaximumValue(textBox)))
                {
                    e.CancelCommand();
                }
            }
        }

        private static void OnMaskTypeStateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var textBox = sender as AutoCompleteBox;

            if (textBox != null)
            {
                textBox.PreviewTextInput -= TextBoxPreviewTextInput;
                textBox.PreviewKeyDown -= TextBoxPreviewKeyDown;

                DataObject.RemovePastingHandler(textBox, OnClipboardPaste);

                MaskType isCheck = ((e.NewValue != null && e.NewValue.GetType() == typeof(MaskType)))
                                       ? (MaskType)e.NewValue
                                       : MaskType.Any;

                if (isCheck != MaskType.Any)
                {
                    textBox.PreviewTextInput += TextBoxPreviewTextInput;
                    textBox.PreviewKeyDown += TextBoxPreviewKeyDown;
                    DataObject.AddPastingHandler(textBox, OnClipboardPaste);
                }
            }
        }

        private static void TextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as AutoCompleteBox;

            if (textBox != null && !Validate(textBox, e.Text, GetMask(textBox)))
            {
                e.Handled = true;
            }

            if (textBox != null)
            {
                string valueValidate = GetValidateValue(textBox.TextBox, e.Text);
                if (!ValidateValue(GetMask(textBox), valueValidate, GetMinimumValue(textBox), GetMaximumValue(textBox)))
                {
                    e.Handled = true;
                }
            }
        }

        private static bool Validate(AutoCompleteBox textBox, string newContent, MaskType maskType)
        {
            if (textBox.TextBox == null)
            {
                return false;
            }

            string testString;
            if (!string.IsNullOrEmpty(textBox.TextBox.SelectedText))
            {
                string pre = textBox.Text.Substring(0, textBox.TextBox.SelectionStart);
                string after = textBox.Text.Substring(
                    textBox.TextBox.SelectionStart + textBox.TextBox.SelectionLength,
                    textBox.Text.Length - (textBox.TextBox.SelectionStart + textBox.TextBox.SelectionLength));
                testString = pre + newContent + after;
            }
            else
            {
                string pre = textBox.TextBox.Text.Substring(0, textBox.TextBox.CaretIndex);
                string after = textBox.TextBox.Text.Substring(
                    textBox.TextBox.CaretIndex, textBox.TextBox.Text.Length - textBox.TextBox.CaretIndex);
                testString = pre + newContent + after;
            }

            string pattern = @"^([-+]?)(\d*)$";
            if (maskType == MaskType.Decimal)
            {
                pattern = @"^([-+]?)(\d*)([,.]?)(\d*)$";
            }
            else if (maskType == MaskType.Integer)
            {
                pattern = @"^([-+]?)(\d*)$";
            }

            var regExpr = new Regex(pattern);
            if (regExpr.IsMatch(testString))
            {
                return true;
            }

            return false;
        }

        private static bool ValidateValue(MaskType mask, string value, double min, double max)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            value = value.Trim();
            switch (mask)
            {
                case MaskType.Integer:
                    try
                    {
                        int intValue = Convert.ToInt32(value);
                        return (min <= intValue && intValue <= max);
                    }
                    catch
                    {
                    }
                    return false;

                case MaskType.Decimal:
                    try
                    {
                        double doubleValue = Convert.ToDouble(value);
                        return (Double.MinValue <= doubleValue && doubleValue <= Double.MaxValue);
                    }
                    catch
                    {
                    }
                    return false;
            }

            return value != string.Empty;
        }

        private static void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as AutoCompleteBox;

            if (e.Key == Key.Space)
            {
                if (textBox != null && !Validate(textBox, e.Key.ToString(), GetMask(textBox)))
                {
                    e.Handled = true;
                }
            }
        }

        public static readonly DependencyProperty MaskTypeProperty = DependencyProperty.RegisterAttached(
            "MaskType",
            typeof(MaskType),
            typeof(NumericAutoComplateBoxBehavior),
            new UIPropertyMetadata(MaskType.Any, OnMaskTypeStateChanged));

        public static readonly DependencyProperty MaximumValueProperty =
            DependencyProperty.RegisterAttached(
                "MaximumValue",
                typeof(double),
                typeof(NumericAutoComplateBoxBehavior),
                new FrameworkPropertyMetadata(double.NaN, MaximumValueChangedCallback));

        public static readonly DependencyProperty MinimumValueProperty =
            DependencyProperty.RegisterAttached(
                "MinimumValue",
                typeof(double),
                typeof(NumericAutoComplateBoxBehavior),
                new FrameworkPropertyMetadata(double.NaN, MinimumValueChangedCallback));
    }
}