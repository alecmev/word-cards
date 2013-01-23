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
    public partial class CardWindow : Window
    {
        public Card TheCard;
        private Dictionary<string, UIElement[]> translations;

        public CardWindow(Card newTheCard)
        {
            InitializeComponent();

            TheCard = newTheCard;
            translations = new Dictionary<string, UIElement[]>();

            ModeInput.SelectedIndex = (int)TheCard.Mode;
            WordInput.Text = TheCard.Word;
            foreach (string tmpTranslation in TheCard.Translations)
            {
                TranslationInput.Text = tmpTranslation;
                AddTranslationClick(null, null);
            }

            Loaded += new RoutedEventHandler(OnLoad);
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            MinWidth = Width;
            MinHeight = Height;
            SizeToContent = System.Windows.SizeToContent.Manual;

            this.Left = Owner.Left + (Owner.ActualWidth / 2) - (this.ActualWidth / 2);
            this.Top = Owner.Top + (Owner.ActualHeight / 2) - (this.ActualHeight / 2);
        }

        private void AddTranslationClick(object sender, RoutedEventArgs e)
        {
            TranslationInput.Text = TranslationInput.Text.Trim();
            if (TranslationInput.Text != "" && !translations.ContainsKey(TranslationInput.Text))
            {
                RowDefinition tmpRow = new RowDefinition();
                tmpRow.Height = new GridLength(2);
                TranslationsGrid.RowDefinitions.Insert(1, tmpRow);

                tmpRow = new RowDefinition();
                tmpRow.Height = GridLength.Auto;
                TranslationsGrid.RowDefinitions.Insert(1, tmpRow);

                TextBox tmpTextBox = new TextBox();
                tmpTextBox.Text = TranslationInput.Text;
                tmpTextBox.Height = 24;
                tmpTextBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                tmpTextBox.Tag = TranslationInput.Text;
                tmpTextBox.LostFocus += new RoutedEventHandler(TranslationTextChanged);
                Grid.SetColumn(tmpTextBox, 1);
                Grid.SetRow(tmpTextBox, TranslationsGrid.RowDefinitions.Count - 2);

                Button tmpButton = new Button();
                tmpButton.Content = "-";
                tmpButton.FontFamily = TranslationAddButton.FontFamily;
                tmpButton.FontSize = TranslationAddButton.FontSize;
                tmpButton.FontWeight = TranslationAddButton.FontWeight;
                tmpButton.Foreground = Brushes.Red;
                tmpButton.Tag = TranslationInput.Text;
                tmpButton.Click += new RoutedEventHandler(RemoveTranslationClick);
                Grid.SetColumn(tmpButton, 3);
                Grid.SetRow(tmpButton, TranslationsGrid.RowDefinitions.Count - 2);

                translations.Add(TranslationInput.Text, new UIElement[] { tmpTextBox, tmpButton });
                TranslationsGrid.Children.Add(tmpTextBox);
                TranslationsGrid.Children.Add(tmpButton);
                TranslationInput.Text = "";

                this.Height += 24;
            }
        }

        private void AddTranslationPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) AddTranslationClick(null, null);
        }

        private void TranslationTextChanged(object sender, RoutedEventArgs e)
        {
            TextBox tmpTextBox = (TextBox)sender;
            Button tmpButton = (Button)translations[(string)tmpTextBox.Tag][1];
            tmpTextBox.Text = tmpTextBox.Text.Trim();

            if (tmpTextBox.Text != "")
            {
                if (!translations.ContainsKey(tmpTextBox.Text))
                {
                    translations.Remove((string)tmpTextBox.Tag);
                    tmpTextBox.Tag = tmpTextBox.Text;
                    tmpButton.Tag = tmpTextBox.Text;
                    translations.Add(tmpTextBox.Text, new UIElement[] { tmpTextBox, tmpButton });
                }
                else if (tmpTextBox.Text != (string)tmpTextBox.Tag) RemoveTranslationClick(tmpButton, null);
            }
            else RemoveTranslationClick(tmpButton, null);
        }

        private void RemoveTranslationClick(object sender, RoutedEventArgs e)
        {
            Button tmpButton = (Button)sender;
            TextBox tmpTextBox = (TextBox)translations[(string)tmpButton.Tag][0];
            translations.Remove((string)tmpButton.Tag);

            int tmpRow = Grid.GetRow(tmpButton);
            foreach (UIElement tmpElement in TranslationsGrid.Children)
            {
                int tmpElementRow = Grid.GetRow(tmpElement);
                if (tmpElementRow > tmpRow) Grid.SetRow(tmpElement, tmpElementRow - 2);
            }

            TranslationsGrid.Children.Remove(tmpButton);
            TranslationsGrid.Children.Remove(tmpTextBox);
            TranslationsGrid.RowDefinitions.RemoveRange(TranslationsGrid.RowDefinitions.Count - 3, 2);

            this.Height -= 24;
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            foreach (KeyValuePair<string, UIElement[]> tmpPair in translations)
            {
                if (((TextBox)tmpPair.Value[0]).Text != (string)((TextBox)tmpPair.Value[0]).Tag)
                {
                    TranslationTextChanged(tmpPair.Value[0], e);
                    break;
                }
            }

            TheCard.Mode = (CardMode)ModeInput.SelectedIndex;
            TheCard.Word = WordInput.Text;
            AddTranslationClick(null, null);
            TheCard.Translations = translations.Keys.ToList();
            DialogResult = true;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}