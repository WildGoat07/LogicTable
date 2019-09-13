using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LogicTable
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Public Fields

        public static Dictionary<Language, Dictionary<string, string>> Local;

        #endregion Public Fields

        #region Public Constructors

        public MainWindow()
        {
            InitLocal();
            InitializeComponent();
            inputBox.Text = "!A | B -> (C <-> !D & A)";
            intro.Text = Local[Language.FR]["intro"];
            helper.Text = Local[Language.FR]["help"];
            Title = Local[Language.FR]["winTitle"];
        }

        #endregion Public Constructors

        #region Public Methods

        public static void InitLocal()
        {
            Local = new Dictionary<Language, Dictionary<string, string>>();
            {
                var fr = new Dictionary<string, string>();
                fr.Add("help", "Les variables doivent être uniquement composées de lettres et/ou chiffres, et doivent commencer par une lettre. Opérateurs acceptés : & (et), | (ou), ->, <->, ! (non). Ordre de priorité : !, 🡘, →, &, |");
                fr.Add("intro", "Entrer l'équation logique à analyser :");
                fr.Add("winTitle", "Créateur de table logiques");
                Local.Add(Language.FR, fr);
            }
            {
                var en = new Dictionary<string, string>();
                en.Add("help", "The variables must contain only letters and/or digits, and need to start with a letter. Accepted operators : & (and), | (or), ->, <->, ! (not). Priority order : !, 🡘, →, &, |");
                en.Add("intro", "Enter the logic equation to parse :");
                en.Add("winTitle", "Logic tables creator");
                Local.Add(Language.EN, en);
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int curPos = inputBox.CaretIndex;
            var newT = inputBox.Text.Replace("->", "→");
            if (newT != inputBox.Text)
                curPos--;
            inputBox.Text = (string)newT.Clone();
            newT = inputBox.Text.Replace("<→", "🡘");
            if (newT != inputBox.Text)
                curPos--;
            inputBox.Text = (string)newT.Clone();
            inputBox.CaretIndex = curPos >= 0 ? curPos : 0;
        }

        #endregion Private Methods
    }
}