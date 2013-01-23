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
using System.Windows.Controls.Primitives;

namespace MyFa
{
    public partial class ManagementWindow : Window
    {
        private string path;
        private XmlDocument cardXml;
        private Dictionary<string, Card> cards;
        private List<CardForDataGrid> cardsForDataGrid;

        private System.Windows.Forms.OpenFileDialog importFileDialog;

        public ManagementWindow(string newPath)
        {
            InitializeComponent();

            path = newPath;
            cardXml = new XmlDocument();
            cardXml.Load(path);

            XmlNodeList tmpCards = cardXml.SelectNodes(@"//card");
            cardsForDataGrid = new List<CardForDataGrid>();
            cards = new Dictionary<string, Card>();
            XmlNodeList tmpTranslations;
            List<string> tmpTranslationsList;

            foreach (XmlNode tmpCard in tmpCards)
            {
                tmpTranslations = cardXml.SelectNodes(@"/cards/card[@word = '" + tmpCard.Attributes["word"].Value + @"']/translations/translation");
                tmpTranslationsList = new List<string>();
                foreach (XmlNode tmpTranslation in tmpTranslations) tmpTranslationsList.Add(tmpTranslation.Attributes["word"].Value);
                cards.Add(tmpCard.Attributes["word"].Value, new Card() {
                    Mode = (CardMode)Int32.Parse(tmpCard.Attributes["mode"].Value),
                    Word = tmpCard.Attributes["word"].Value,
                    Translations = tmpTranslationsList,
                    Shown = Int32.Parse(tmpCard.Attributes["shown"].Value),
                    Tested = Int32.Parse(tmpCard.Attributes["tested"].Value)
                });
                cardsForDataGrid.Add(cards[tmpCard.Attributes["word"].Value].Data);
            }

            List<string> tmpModes = new List<string>() { "Untouched", "Learning", "Testing", "Done" };
            ModeComboBox.ItemsSource = tmpModes;

            CardGrid.ItemsSource = cardsForDataGrid;

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

        private void ImportClick(object sender, RoutedEventArgs e)
        {
            importFileDialog = new System.Windows.Forms.OpenFileDialog();
            importFileDialog.Multiselect = false;
            importFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            importFileDialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            importFileDialog.FilterIndex = 0;
            importFileDialog.RestoreDirectory = false;

            if (importFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                XmlDocument tmpXml = new XmlDocument();
                tmpXml.Load(importFileDialog.OpenFile());

                XmlNodeList tmpCards = tmpXml.SelectNodes(@"//card");
                XmlNodeList tmpTranslations;
                List<string> tmpTranslationsList;
                string tmpWord;
                string[] separatedTranslations;

                foreach (XmlNode tmpCard in tmpCards)
                {
                    tmpWord = tmpCard.SelectSingleNode(@"word").InnerText.ToLower().Trim();
                    tmpTranslations = tmpCard.SelectNodes(@"meanings/meaning/translations/word");
                    tmpTranslationsList = new List<string>();
                    foreach (XmlNode tmpTranslation in tmpTranslations)
                    {
                        separatedTranslations = tmpTranslation.InnerText.ToLower().Split(',');
                        foreach (string tmpTranslationWord in separatedTranslations) tmpTranslationsList.Add(tmpTranslationWord.Trim());
                    }

                    if (!cards.ContainsKey(tmpWord))
                    {
                        cards.Add(tmpWord, new Card()
                        {
                            Mode = CardMode.Untouched,
                            Word = tmpWord,
                            Translations = tmpTranslationsList,
                            Shown = 0,
                            Tested = 0
                        });
                        cardsForDataGrid.Add(cards[tmpWord].Data);
                    }
                    else
                    {
                        cardsForDataGrid.Remove(cards[tmpWord].Data);
                        cards[tmpWord].Translations = cards[tmpWord].Translations.Concat(tmpTranslationsList).Distinct().ToList();
                        cards[tmpWord].Mode = CardMode.Untouched;
                        cardsForDataGrid.Add(cards[tmpWord].Data);
                    }
                }

                CardGrid.ItemsSource = null;
                CardGrid.ItemsSource = cardsForDataGrid;
            }
        }

        private void SelectAllClick(object sender, RoutedEventArgs e)
        {
            if (CardGrid.SelectedItems.Count == CardGrid.Items.Count) CardGrid.SelectedItems.Clear();
            else CardGrid.SelectAll();
        }

        private void AddNewClick(object sender, RoutedEventArgs e)
        {
            CardWindow cw = new CardWindow(new Card() {
                Mode = 0,
                Word = "",
                Translations = new List<string>(),
                Shown = 0,
                Tested = 0
            });
            cw.Owner = this;

            if (cw.ShowDialog() == true)
            {
                cards.Add(cw.TheCard.Word, cw.TheCard);
                cardsForDataGrid.Add(cw.TheCard.Data);

                CardGrid.ItemsSource = null;
                CardGrid.ItemsSource = cardsForDataGrid;
                CardGrid.SelectedItem = cw.TheCard.Data;
            }
        }

        private void EditClick(object sender, RoutedEventArgs e)
        {
            if (CardGrid.SelectedItems.Count == 1)
            {
                string word = ((CardForDataGrid)CardGrid.SelectedItem).Word;
                Card oldCard = cards[word];
                cardsForDataGrid.Remove(cards[word].Data);
                cards.Remove(word);
                CardWindow cw = new CardWindow(oldCard);
                cw.Owner = this;

                if (cw.ShowDialog() == true) oldCard = cw.TheCard;

                cards.Add(oldCard.Word, oldCard);
                cardsForDataGrid.Add(oldCard.Data);

                CardGrid.ItemsSource = null;
                CardGrid.ItemsSource = cardsForDataGrid;
                CardGrid.SelectedItem = oldCard.Data;
            }
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            if (CardGrid.SelectedItems.Count > 0 && MessageBox.Show("Delete card" + (CardGrid.SelectedItems.Count > 1 ? "s" : "") + "?", "MyFa Cards - Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                foreach (CardForDataGrid selectedCard in CardGrid.SelectedItems)
                {
                    cards.Remove(selectedCard.Word.ToLower());
                    cardsForDataGrid.Remove(selectedCard);
                }
                CardGrid.ItemsSource = null;
                CardGrid.ItemsSource = cardsForDataGrid;
            }
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            XmlTextWriter writer = new XmlTextWriter(File.Create(path), Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            writer.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
            writer.WriteStartElement("cards");

            foreach (Card tmpCard in cards.Values)
            {
                writer.WriteStartElement("card");
                writer.WriteAttributeString("word", tmpCard.Word);
                writer.WriteAttributeString("mode", ((int)tmpCard.Mode).ToString());
                writer.WriteAttributeString("shown", tmpCard.Shown.ToString());
                writer.WriteAttributeString("tested", tmpCard.Tested.ToString());
                writer.WriteStartElement("translations");

                foreach (string tmpTranslation in tmpCard.Translations)
                {
                    writer.WriteStartElement("translation");
                    writer.WriteAttributeString("word", tmpTranslation);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.Close();

            DialogResult = true;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /*private void CheckFiles()
        {
            Path = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFa Cards";
            if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);
            FileInfo[] tmpFI = (new DirectoryInfo(Path)).GetFiles("*.xml");
            SelectFile.Items.Clear();
            string lastOpenedPath = "";

            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFa Cards\LastOpened.txt"))
            {
                StreamReader sr = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFa Cards\LastOpened.txt");
                lastOpenedPath = sr.ReadLine();
                sr.Close();
            }

            SelectFile.SelectedIndex = 0;
            foreach (FileInfo tmpFile in tmpFI)
            {
                XmlDocument tmp = new XmlDocument();
                tmp.Load(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFa Cards\" + tmpFile.Name);
                if (tmp.SelectNodes(@"//card[@word != '' and @mode and @shown and @tested]").Count == 0) continue;
                ComboBoxItem tmpItem = new ComboBoxItem();
                tmpItem.Content = tmpFile.Name;
                SelectFile.Items.Add(tmpItem);
                if (tmpFile.Name == lastOpenedPath) SelectFile.SelectedIndex = tmpFI.ToList().IndexOf(tmpFile);
            }

            if (SelectFile.Items.Count == 0)
            {
                OpenButton.IsEnabled = false;
                OpenButton.Filter = Brushes.White;
                MessageBox.Show("No any card collection in correct XML format found! Import them, please!");
            }
            else
            {
                OpenButton.Filter = Brushes.Green;
                OpenButton.IsEnabled = true;
            }
        }*/
    }

    public class CardForDataGrid
    {
        public string Mode { get; set; }
        public string Word { get; set; }
        public string Translations { get; set; }
        public string Shown { get; set; }
        public string Tested { get; set; }
    }

    public class Card
    {
        private CardMode mode;
        public CardMode Mode { get { return mode; } set { mode = value; Convert(); } }

        private string word;
        public string Word { get { return word; } set { word = value; Convert(); } }

        private List<string> translations;
        public List<string> Translations { get { return translations; } set { translations = value; Convert(); } }

        private int shown;
        public int Shown { get { return shown; } set { shown = value; Convert(); } }

        private int tested;
        public int Tested { get { return tested; } set { tested = value; Convert(); } }

        private CardForDataGrid data;
        public CardForDataGrid Data { get { return data; } private set { data = value; } }

        /*public Card(Card copy)
        {
            Mode = copy.Mode;
            Word = copy.Word;
            Translations = copy.Translations;
            Shown = copy.Shown;
            Tested = copy.Tested;
            Data = copy.Data;
        }*/

        public void Convert()
        {
            string tmpString = "";
            if (Translations != null && Translations.Count > 0)
            {
                foreach (string translation in Translations) tmpString += translation + ", ";
                tmpString = tmpString.Remove(tmpString.Length - 2);
            }
            Data = new CardForDataGrid() {
                Mode = Mode.ToString(),
                Word = Word,
                Translations = tmpString,
                Shown = Shown.ToString(),
                Tested = Tested.ToString()
            };
        }
    }

    public enum CardMode
    {
        Untouched,
        Learning,
        Testing,
        Done
    }
}