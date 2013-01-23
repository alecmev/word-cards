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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Media;
using System.Timers;
using System.Xml;
using System.IO;

namespace MyFa
{
    public delegate void EmptyDelegate();

    public partial class MainWindow : Window
    {
        private string lastOpenedPath;
        private XmlDocument cardXml;
        private string cardFileName;
        private XmlNodeList[] cardCollection;
        private int cardMode;
        private int cardNumber;
        private List<string> cardTranslations;
        private int cardTranslation;
        private SolidColorBrush colorDefault;
        private SolidColorBrush colorCorrect;
        private SolidColorBrush colorWrong;
        private Random random;

        public MainWindow()
        {
            InitializeComponent();
            UpdateLastOpened();

            cardFileName = "";
            cardCollection = new XmlNodeList[5];
            cardMode = 0;
            cardNumber = 0;
            colorDefault = new SolidColorBrush(Color.FromArgb(0x1F, 0xCC, 0x99, 0x00));
            colorCorrect = new SolidColorBrush(Color.FromArgb(0x1F, 0x00, 0xCC, 0x00));
            colorWrong = new SolidColorBrush(Color.FromArgb(0x1F, 0xCC, 0x00, 0x00));
            random = new Random();

            Loaded += new RoutedEventHandler(OnLoad);
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            MinWidth = Width;
            MinHeight = Height;
            SizeToContent = System.Windows.SizeToContent.Manual;

            this.Left = (SystemParameters.PrimaryScreenWidth / 2) - (this.ActualWidth / 2);
            this.Top = (SystemParameters.PrimaryScreenHeight / 2) - (this.ActualHeight / 2);
        }

        private void UpdateLastOpened()
        {
            LastOpened.Visibility = System.Windows.Visibility.Collapsed;
            lastOpenedPath = "";
            if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFa Cards") && File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFa Cards\LastOpened.txt"))
            {
                StreamReader sr = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFa Cards\LastOpened.txt");
                lastOpenedPath = sr.ReadLine();
                sr.Close();

                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFa Cards\" + lastOpenedPath))
                {
                    XmlDocument tmpXml = new XmlDocument();
                    tmpXml.Load(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFa Cards\" + lastOpenedPath);

                    if (tmpXml.SelectNodes(@"/cards").Count == 1)
                    {
                        LastOpened.Visibility = System.Windows.Visibility.Visible;
                        LastOpened.Title = lastOpenedPath;
                        lastOpenedPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\MyFa Cards\" + lastOpenedPath;
                    }
                }
            }
        }

        private void LastOpenedClick(object sender, RoutedEventArgs e)
        {
            cardFileName = lastOpenedPath;
            Open();
        }

        private void CollectionsClick(object sender, RoutedEventArgs e)
        {
            CollectionsWindow ow = new CollectionsWindow();
            ow.Owner = this;

            if (ow.ShowDialog() == true)
            {
                cardFileName = ow.Path;
                Open();
            }
        }

        private void Open()
        {
            UpdateLastOpened();

            cardCollection = new XmlNodeList[5];
            cardMode = 0;
            cardNumber = 0;
            cardXml = new XmlDocument();
            cardXml.Load(cardFileName);

            cardCollection[0] = cardXml.SelectNodes(@"//card[@mode = '0']");
            cardCollection[1] = cardXml.SelectNodes(@"//card[@mode = '1']");
            cardCollection[2] = cardXml.SelectNodes(@"//card[@mode = '2']");
            cardCollection[3] = cardXml.SelectNodes(@"//card[@mode = '3']");

            TranslationBorderDefault.Visibility = System.Windows.Visibility.Collapsed;
            NextCard(false);
        }

        /// <summary>
        /// Picks next card for user, manages changes and saves them to the dictionary file.
        /// </summary>
        /// <param name="action">Why this function was called. True - switch learning mode; false - card is learned; null - pick next card.</param>
        private void NextCard(bool? action)
        {
            if (cardCollection[0].Count + cardCollection[1].Count + cardCollection[2].Count == 0)
            {
                cardMode = 0;
                cardNumber = 0;
            }

            // Increase card "shown" property
            if (cardMode > 0)
            {
                cardCollection[cardMode][cardNumber].Attributes["shown"].Value = (int.Parse(cardCollection[cardMode][cardNumber].Attributes["shown"].Value) + 1).ToString();
                cardXml.SelectSingleNode("cards").ReplaceChild(cardCollection[cardMode][cardNumber], cardXml.SelectSingleNode(@"//card[@word = '" + cardCollection[cardMode][cardNumber].Attributes["word"].Value + @"']"));

                if (cardMode == 2)
                {
                    cardCollection[cardMode][cardNumber].Attributes["tested"].Value = (int.Parse(cardCollection[cardMode][cardNumber].Attributes["tested"].Value) + 1).ToString();
                    cardXml.SelectSingleNode("cards").ReplaceChild(cardCollection[cardMode][cardNumber], cardXml.SelectSingleNode(@"//card[@word = '" + cardCollection[cardMode][cardNumber].Attributes["word"].Value + @"']"));

                    /*if (TranslationTextTesting.Text == cardTranslations[cardTranslation])
                    {
                        cardCollection[cardMode][cardNumber].Attributes["score"].Value = (int.Parse(cardCollection[cardMode][cardNumber].Attributes["score"].Value) + 1).ToString();
                        cardXml.SelectSingleNode("cards").ReplaceChild(cardCollection[cardMode][cardNumber], cardXml.SelectSingleNode(@"//card[@word = '" + cardCollection[cardMode][cardNumber].Attributes["word"].Value + @"']"));
                    }*/
                }
            }

            // Switch learning mode
            if (action == true)
            {
                cardCollection[cardMode][cardNumber].Attributes["mode"].Value = (cardCollection[cardMode][cardNumber].Attributes["mode"].Value == "1" ? "2" : "1");
                cardXml.SelectSingleNode("cards").ReplaceChild(cardCollection[cardMode][cardNumber], cardXml.SelectSingleNode(@"//card[@word = '" + cardCollection[cardMode][cardNumber].Attributes["word"].Value + @"']"));
            }
            // Card is learned
            else if (action == false)
            {
                if (cardMode > 0)
                {
                    cardCollection[cardMode][cardNumber].Attributes["mode"].Value = "3";
                    cardXml.SelectSingleNode("cards").ReplaceChild(cardCollection[cardMode][cardNumber], cardXml.SelectSingleNode(@"//card[@word = '" + cardCollection[cardMode][cardNumber].Attributes["word"].Value + @"']"));
                }

                int tmp = 8 - (cardCollection[1].Count + cardCollection[2].Count);
                XmlNode tmpNode = null;

                for (int i = 0; i < tmp && i < cardCollection[0].Count; ++i)
                {
                    if (tmpNode == null) tmpNode = cardCollection[0][random.Next(cardCollection[0].Count)];
                    else while (tmpNode.Attributes["mode"].Value == "1") tmpNode = cardCollection[0][random.Next(cardCollection[0].Count)];
                    tmpNode.Attributes["mode"].Value = "1";
                    cardXml.SelectSingleNode("cards").ReplaceChild(tmpNode, cardXml.SelectSingleNode(@"//card[@word = '" + tmpNode.Attributes["word"].Value + @"']"));
                }
            }

            // Updating card collection
            cardCollection[0] = cardXml.SelectNodes(@"//card[@mode = '0']");
            cardCollection[1] = cardXml.SelectNodes(@"//card[@mode = '1']");
            cardCollection[2] = cardXml.SelectNodes(@"//card[@mode = '2']");
            cardCollection[3] = cardXml.SelectNodes(@"//card[@mode = '3']");

            // There are cards to learn
            if (cardCollection[1].Count + cardCollection[2].Count > 0)
            {
                // Picking random card
                if (cardCollection[1].Count + cardCollection[2].Count > 1)
                {
                    int prevMode = cardMode, prevNumber = cardNumber;
                    while (prevMode == cardMode && prevNumber == cardNumber)
                    {
                        cardMode = random.Next(2) + 1;
                        if (cardCollection[cardMode].Count == 0) cardMode = 3 - cardMode;
                        cardNumber = random.Next(cardCollection[cardMode].Count);
                    }
                }
                else
                {
                    cardMode = 2 - cardCollection[1].Count;
                    cardNumber = 0;
                }

                // Setting original word and transcription
                OriginalWord.Text = cardCollection[cardMode][cardNumber].Attributes["word"].Value.ToUpper();
                //OriginalTranscription.Text = cardCollection[cardMode][cardNumber].Attributes["transcription"].Value;
                cardTranslations = new List<string>();
                XmlNodeList tmpTranslations = cardXml.SelectNodes(@"/cards/card[@word = '" + cardCollection[cardMode][cardNumber].Attributes["word"].Value + @"']/translations/translation");
                foreach (XmlNode tmpTranslation in tmpTranslations) cardTranslations.Add(tmpTranslation.Attributes["word"].Value.ToUpper());
                cardTranslation = 0;

                // Learning translation
                if (cardMode == 1)
                {
                    if (cardTranslations.Count == 1) DisableTranslationSwitch();
                    else EnableTranslationSwitch();

                    TranslationTextLearning.Text = cardTranslations[cardTranslation];
                    TranslationBorderTesting.Visibility = System.Windows.Visibility.Collapsed;
                    TranslationBorderLearning.Visibility = System.Windows.Visibility.Visible;
                    TranslationGridLearning.Opacity = 0.0;
                }
                // Testing translation
                else
                {
                    TranslationTextTesting.Text = "";
                    TranslationBorderLearning.Visibility = System.Windows.Visibility.Collapsed;
                    TranslationBorderTesting.Visibility = System.Windows.Visibility.Visible;
                    TranslationTextTesting.Focus();
                }

                //PlaySound(OriginalWord.Text.Split(' ')[0]);
                PlaySound(OriginalWord.Text);
            }
            else
            {
                OriginalWord.Text = "Wonderful! " + cardCollection[3].Count.ToString() + " words learned!";
                //OriginalTranscription.Text = "You have learned " + cardCollection[3].Count.ToString() + " words!";

                TranslationBorderTesting.Visibility = System.Windows.Visibility.Collapsed;
                TranslationBorderLearning.Visibility = System.Windows.Visibility.Collapsed;
                TranslationBorderDefault.Visibility = System.Windows.Visibility.Visible;
            }

            cardXml.Save(cardFileName);
        }

        private void EnableTranslationSwitch()
        {
            NextTranslationImage.Visibility = System.Windows.Visibility.Visible;
            NextTranslationImage.IsEnabled = true;
            PreviousTranslationImage.Visibility = System.Windows.Visibility.Visible;
            PreviousTranslationImage.IsEnabled = true;
        }

        private void DisableTranslationSwitch()
        {
            NextTranslationImage.IsEnabled = false;
            NextTranslationImage.Visibility = System.Windows.Visibility.Hidden;
            PreviousTranslationImage.IsEnabled = false;
            PreviousTranslationImage.Visibility = System.Windows.Visibility.Hidden;
        }

        private void ExposeMouseEnter(object sender, MouseEventArgs e)
        {
            TranslationMouseEnterLearning.Begin();
        }

        private void ExposeMouseLeave(object sender, MouseEventArgs e)
        {
            TranslationMouseLeaveLearning.Begin();
        }

        private void SoundClick(object sender, RoutedEventArgs e)
        {
            //if (TranslationBorderDefault.Visibility == System.Windows.Visibility.Collapsed) PlaySound(OriginalWord.Text.Split(' ')[0]);
            if (TranslationBorderDefault.Visibility == System.Windows.Visibility.Collapsed) PlaySound(OriginalWord.Text);
        }

        private void PlaySound(string word)
        {
            //if (word != "") (new System.Speech.Synthesis.SpeechSynthesizer()).SpeakAsync(word[0] + word.Substring(1).ToLower() + "!");
            if (word != "") (new System.Speech.Synthesis.SpeechSynthesizer()).SpeakAsync(word[0] + word.Substring(1).ToLower() + "!");
        }

        private void MainBorderMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (cardFileName != "") NextCard(null);
        }

        private void NextOneClick(object sender, RoutedEventArgs e)
        {
            if (TranslationBorderDefault.Visibility == System.Windows.Visibility.Collapsed) NextCard(null);
        }

        private void SwitchModeClick(object sender, RoutedEventArgs e)
        {
            if (TranslationBorderDefault.Visibility == System.Windows.Visibility.Collapsed) NextCard(true);
        }

        private void LearnedClick(object sender, RoutedEventArgs e)
        {
            if (cardFileName != "") NextCard(false);
        }

        private void NextTranslationClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (cardTranslations.Count > 1)
            {
                if (++cardTranslation == cardTranslations.Count) cardTranslation = 0;
                TranslationMouseLeaveLearning.Completed += new EventHandler(TranslationChangeFadeOutCompleted);
                TranslationMouseLeaveLearning.Begin();
            }
        }

        private void PreviousTranslationClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (--cardTranslation == -1) cardTranslation = cardTranslations.Count - 1;
            TranslationMouseLeaveLearning.Completed += new EventHandler(TranslationChangeFadeOutCompleted);
            TranslationMouseLeaveLearning.Begin();
        }

        private void TranslationChangeFadeOutCompleted(object sender, EventArgs e)
        {
            TranslationMouseLeaveLearning.Completed -= new EventHandler(TranslationChangeFadeOutCompleted);
            TranslationTextLearning.Text = cardTranslations[cardTranslation];
            TranslationMouseEnterLearning.Completed += new EventHandler(TranslationChangeFadeInCompleted);
            TranslationMouseEnterLearning.Begin();
        }

        private void TranslationChangeFadeInCompleted(object sender, EventArgs e)
        {
            TranslationMouseEnterLearning.Completed -= new EventHandler(TranslationChangeFadeInCompleted);
        }

        private void TranslationTextTestingChanged(object sender, TextChangedEventArgs e)
        {
            if (cardMode == 2)
            {
                TranslationTextTesting.Text = TranslationTextTesting.Text.ToUpper();
                TranslationTextTesting.SelectionStart = TranslationTextTesting.Text.Length;

                bool isCorrect = false, isTranslated = false;
                foreach (string tmp in cardTranslations)
                {
                    if (tmp.StartsWith(TranslationTextTesting.Text))
                    {
                        isCorrect = true;
                        cardTranslation = cardTranslations.IndexOf(tmp);
                        if (TranslationTextTesting.Text.Length == tmp.Length) isTranslated = true;
                        break;
                    }
                }

                if (TranslationTextTesting.Text.Length == 0) TranslationBorderTesting.Background = colorDefault;
                else if (isCorrect)
                {
                    TranslationBorderTesting.Background = colorCorrect;

                    if (isTranslated)
                    {
                        if (cardTranslations.Count == 1) DisableTranslationSwitch();
                        else EnableTranslationSwitch();

                        TranslationTextLearning.Text = cardTranslations[cardTranslation];
                        TranslationBorderTesting.Visibility = System.Windows.Visibility.Collapsed;
                        TranslationBorderLearning.Visibility = System.Windows.Visibility.Visible;
                        TranslationMouseEnterLearning.Begin();
                    }
                }
                else TranslationBorderTesting.Background = colorWrong;
            }
        }

        /*private void VolumeTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(changeVolume, null);
        }

        private void ChangeVolume()
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                if (mediaPlayer.Position.TotalMilliseconds < 300 && mediaPlayer.Volume < 0.5) mediaPlayer.Volume += 0.05;
                else if ((mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds - mediaPlayer.Position.TotalMilliseconds) < 300 && mediaPlayer.Volume > 0.0) mediaPlayer.Volume -= 0.05;
            }
        }

        private void SoundEnded(object sender, EventArgs e)
        {
            volumeTimer.Stop();
        }*/

        /*volumeTimer.Stop();
        mediaPlayer.Stop();
        mediaPlayer.Open(new Uri(@"Sounds\" + word.ToLower() + @".wav", UriKind.Relative));
        mediaPlayer.Volume = 0.0;
        mediaPlayer.Position = TimeSpan.Zero;
        mediaPlayer.Play();
        volumeTimer.Start();*/

        /*mediaPlayer = new MediaPlayer();
        mediaPlayer.MediaEnded += new EventHandler(SoundEnded);
        volumeTimer = new Timer();
        volumeTimer.Interval = 10;
        volumeTimer.Elapsed += new ElapsedEventHandler(VolumeTimerElapsed);
        changeVolume = new EmptyDelegate(ChangeVolume);*/

        /*private MediaPlayer mediaPlayer;
        private Timer volumeTimer;
        private EmptyDelegate changeVolume;*/
    }
}