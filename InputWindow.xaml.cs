using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Xml;

namespace MyFa
{
    public partial class InputWindow : Window
    {
        public new string Title
        {
            get { return InputTitle.Text; }
            set { InputTitle.Text = value; }
        }
        public string Text
        {
            get { return Input.Text.Trim(); }
        }

        public InputWindow()
        {
            InitializeComponent();

            OkayButton.IsEnabled = false;
            OkayButton.Filter = Brushes.White;

            Input.Focus();
        }

        private void OkayClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void InputTextChanged(object sender, TextChangedEventArgs e)
        {
            if (Text != "")
            {
                OkayButton.Filter = Brushes.Green;
                OkayButton.IsEnabled = true;
            }
            else
            {
                OkayButton.IsEnabled = false;
                OkayButton.Filter = Brushes.White;
            }
        }

        private void WindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && OkayButton.IsEnabled) DialogResult = true;
        }
    }
}