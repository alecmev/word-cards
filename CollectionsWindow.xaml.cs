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
    public partial class CollectionsWindow : Window
    {
        public string Path;
        public string LastOpened
        {
            get
            {
                string tmp = "";
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFa Cards\LastOpened.txt"))
                {
                    StreamReader sr = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFa Cards\LastOpened.txt");
                    tmp = sr.ReadLine();
                    sr.Close();
                    if (File.Exists(Path + @"\" + tmp))
                    {
                        XmlDocument tmpXml = new XmlDocument();
                        tmpXml.Load(Path + @"\" + tmp);
                        if (tmpXml.SelectNodes(@"/cards").Count != 1) tmp = "";
                    }
                    else tmp = "";
                }
                return tmp;
            }
            set
            {
                StreamWriter sw = new StreamWriter(File.Create(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFa Cards\LastOpened.txt"));
                sw.Write(value);
                sw.Close();
            }
        }

        public CollectionsWindow()
        {
            InitializeComponent();
            CheckFiles();
        }

        private void ManageClick(object sender, RoutedEventArgs e)
        {
            ManagementWindow mw = new ManagementWindow(Path + @"\" + (string)((ComboBoxItem)SelectFile.SelectedItem).Content);
            mw.Owner = this;
            mw.ShowDialog();
            CheckFiles();
        }

        private void CreateNewClick(object sender, RoutedEventArgs e)
        {
            InputWindow iw = new InputWindow();
            iw.Owner = this;
            iw.Title = "new collection name";

            bool? result = false;
            while ((result = iw.ShowDialog()) == true && File.Exists(Path + @"\" + iw.Text + @".xml") && MessageBox.Show("File with this name exists already!", "MyFa Cards - Wrong name", MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.OK) { }

            if (result == true)
            {
                try
                {
                    XmlTextWriter writer = new XmlTextWriter(File.Create(Path + @"\" + iw.Text + @".xml"), Encoding.UTF8);
                    writer.Formatting = Formatting.Indented;
                    writer.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
                    writer.WriteStartElement("cards");
                    writer.WriteEndElement();
                    writer.Close();

                    LastOpened = iw.Text + @".xml";

                    CheckFiles();
                    ManageClick(sender, e);
                }
                catch
                {
                    MessageBox.Show("Error occured, new collection was not created.", "MyFa Cards - Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            if (SelectFile.Items.Count > 0 && MessageBox.Show("Are you sure?", "MyFa Cards - Delete Collection", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (LastOpened == (string)((ComboBoxItem)SelectFile.SelectedItem).Content)
                {
                    LastOpened = "";
                    ((MainWindow)Owner).LastOpened.Visibility = System.Windows.Visibility.Collapsed;
                    ((MainWindow)Owner).TranslationBorderLearning.Visibility = System.Windows.Visibility.Collapsed;
                    ((MainWindow)Owner).TranslationBorderTesting.Visibility = System.Windows.Visibility.Collapsed;
                    ((MainWindow)Owner).TranslationBorderDefault.Visibility = System.Windows.Visibility.Visible;
                    ((MainWindow)Owner).OriginalWord.Text = "MyFa Cards";
                    //((MainWindow)Owner).OriginalTranscription.Text = "GET STARTED BY CLICKING HELP";
                }
                File.Delete(Path + @"\" + (string)((ComboBoxItem)SelectFile.SelectedItem).Content);
                CheckFiles();
            }
        }

        private void RecheckFilesClick(object sender, RoutedEventArgs e)
        {
            CheckFiles();
        }

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            Path += @"\" + (string)((ComboBoxItem)SelectFile.SelectedItem).Content;
            LastOpened = (string)((ComboBoxItem)SelectFile.SelectedItem).Content;
            DialogResult = true;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CheckFiles()
        {
            Path = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFa Cards";
            if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);
            FileInfo[] tmpFI = (new DirectoryInfo(Path)).GetFiles("*.xml");
            SelectFile.Items.Clear();
            string lastOpenedPath = LastOpened;

            SelectFile.SelectedIndex = 0;
            foreach (FileInfo tmpFile in tmpFI)
            {
                XmlDocument tmpXml = new XmlDocument();
                tmpXml.Load(Path + @"\" + tmpFile.Name);
                if (tmpXml.SelectNodes(@"/cards").Count != 1) continue;

                ComboBoxItem tmpItem = new ComboBoxItem();
                tmpItem.Content = tmpFile.Name;
                SelectFile.Items.Add(tmpItem);
                if (tmpFile.Name == lastOpenedPath) SelectFile.SelectedIndex = tmpFI.ToList().IndexOf(tmpFile);
            }

            if (SelectFile.Items.Count == 0)
            {
                ManageButton.IsEnabled = false;
                ManageButton.Filter = Brushes.White;
                OpenButton.IsEnabled = false;
                OpenButton.Filter = Brushes.White;
                MessageBox.Show("No any card collection in correct XML format found! Put them in " + Path + " or create new, please!");
            }
            else
            {
                ManageButton.Filter = Brushes.Gold;
                ManageButton.IsEnabled = true;
                OpenButton.Filter = Brushes.Green;
                OpenButton.IsEnabled = true;
            }
        }
    }
}