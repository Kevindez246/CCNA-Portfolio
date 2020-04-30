using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MatchGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetUpGame();
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
                int index = random.Next(animalEmoji.Count);
                string nextemoji = animalEmoji[index];
                textblock.Text = nextemoji;
                animalEmoji.RemoveAt(index);

            }
        }
    }
}
