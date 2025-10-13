using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MatchGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        TextBlock lastTextBlockClicked;
        bool findingMatch = false;
        DispatcherTimer timer = new DispatcherTimer();
        int tenthsOfSecondsElapsd;
        int matchesfound;

        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromSeconds(.1);
            timer.Tick += Timer_Tick;
            SetUpGame();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            tenthsOfSecondsElapsd++;
            TimetextBlock.Text = (tenthsOfSecondsElapsd / 10F).ToString("0.0s");
            if (matchesfound == 8)
            {
                timer.Stop();
                TimetextBlock.Text += " - Play Again ?";
            }
        }

        private void SetUpGame()
        {
            List<String> animalEmoji = new List<string>()
            {
                "🦊","🦊",
                "🦁","🦁",
                "🐘","🐘",
                "🦌","🦌",
                "🐵","🐵",
                "🐫","🐫",
                "🐳","🐳",
                "🐗","🐗"
            };


            Random random = new Random();
            foreach (var textblock in mainGrid.Children.OfType<TextBlock>())
            {
                if (textblock.Name != "TimetextBlock")
                {
                    int index = random.Next(animalEmoji.Count);
                    string nextemoji = animalEmoji[index];
                    textblock.Text = nextemoji;
                    animalEmoji.RemoveAt(index);
                }
            }

            timer.Start();
            tenthsOfSecondsElapsd = 0;
            matchesfound = 0;



        }

        private void TextBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            TextBlock textBlock = sender as TextBlock;

            if (findingMatch == false)
            {
                textBlock.Visibility = Visibility.Hidden;
                lastTextBlockClicked = textBlock;
                findingMatch = true;
            }

            else if (textBlock.Text == lastTextBlockClicked.Text)
            {
                matchesfound++;
                textBlock.Visibility = Visibility.Hidden;
                findingMatch = false;
            }

            else
            {
                lastTextBlockClicked.Visibility = Visibility.Visible;
                findingMatch = false;
            }

        }

        private void TimetextBlock_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (matchesfound == 8)
                SetUpGame();
        }
    }
}
