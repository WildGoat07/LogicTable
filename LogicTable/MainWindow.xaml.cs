using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            generateButton.Content = Local[Language.FR]["button"];
            Title = Local[Language.FR]["winTitle"];
        }

        #endregion Public Constructors

        #region Public Methods

        static public string getStr(Equation eq)
        {
            if (eq is Not neq)
                return "!" + getStr(neq.InternalEquation);
            if (eq is Constant c)
                return c.Name;
            if (eq is OperatorEquation oeq)
            {
                switch (oeq.Operator)
                {
                    case OperatorEquation.OperatorType.AND:
                        return '(' + getStr(oeq.Left) + " & " + getStr(oeq.Right) + ')';

                    case OperatorEquation.OperatorType.OR:
                        return '(' + getStr(oeq.Left) + " | " + getStr(oeq.Right) + ')';

                    case OperatorEquation.OperatorType.EQUIVALENT:
                        return '(' + getStr(oeq.Left) + " 🡘 " + getStr(oeq.Right) + ')';

                    case OperatorEquation.OperatorType.INVOLVING:
                        return '(' + getStr(oeq.Left) + " → " + getStr(oeq.Right) + ')';
                }
            }
            return "";
        }

        public static void InitLocal()
        {
            Local = new Dictionary<Language, Dictionary<string, string>>();
            {
                var fr = new Dictionary<string, string>();
                fr.Add("help", "Les variables doivent être uniquement composées de lettres et/ou chiffres, et doivent commencer par une lettre. Opérateurs acceptés : & (et), | (ou), ->, <->, ! (non). Ordre de priorité : (), !, &, |, →, 🡘");
                fr.Add("intro", "Entrer l'équation logique à analyser :");
                fr.Add("winTitle", "Créateur de table logiques");
                fr.Add("tabTitle", "Table logique");
                fr.Add("true", "VRAI");
                fr.Add("false", "FAUX");
                fr.Add("button", "Générer la page HTML");
                Local.Add(Language.FR, fr);
            }
            {
                var en = new Dictionary<string, string>();
                en.Add("help", "The variables must contain only letters and/or digits, and need to start with a letter. Accepted operators : & (and), | (or), ->, <->, ! (not). Priority order : (), !, &, |, →, 🡘");
                en.Add("intro", "Enter the logic equation to parse :");
                en.Add("winTitle", "Logic tables creator");
                en.Add("tabTitle", "Logic table");
                en.Add("true", "TRUE");
                en.Add("false", "FALSE");
                en.Add("button", "Generate HTML page");
                Local.Add(Language.EN, en);
            }
        }

        public string ParseToHTML(string eq)
        {
            eq = eq.Replace("🡘", "*");
            var result = new StringBuilder();
            result.Append(
@"
<!DOCTYPE HTML>
<html>
<head>
    <meta charset=""UTF-8"" />
    <style>
        table,
        th,
        td {
            border: 1px solid black;
            padding: 5px;
        }

        table {
            border-collapse: collapse;
        }
    </style>
    <title>" + Local[Language.FR]["tabTitle"] + "</title>" +
@"
</head>
<body>
    <table>
        <tbody>");
            string footer =
@"
        </tbody>
    </table>
</body>
</html>
";
            int index = 0;
            var registeredVars = new List<string>();
            Equation globalEq;
            try
            {
                var unparsed = DivideStr(eq, ref index);
                globalEq = Parse(unparsed.str, unparsed, registeredVars);
            }
            catch (Exception e)
            {
                error.Text = e.Message;
                return null;
            }
            registeredVars.Sort();
            var cases = new List<Dictionary<string, bool>>();
            for (long i = 0; i < Math.Pow(2, registeredVars.Count); i++)
            {
                var currCase = new Dictionary<string, bool>();
                for (int j = registeredVars.Count - 1; j >= 0; j--)
                    currCase.Add(registeredVars[registeredVars.Count - j - 1], (1 << j & i) == 0);
                cases.Add(currCase);
            }
            result.Append(
@"
            <tr>");
            foreach (var variable in registeredVars)
                result.Append(
@"
                <th>" + variable + "</th>");
            var signature = getStr(globalEq);
            signature = signature.Substring(1, signature.Length - 2);
            result.Append(
@"
                <th><em>" + signature +
@"</em></th>
            </tr>");
            foreach (var key in cases)
            {
                result.Append(
@"
            <tr>");
                foreach (var variable in key)
                {
                    result.Append(
@"
                <td>" + (variable.Value ? Local[Language.FR]["true"] : Local[Language.FR]["false"]) + "</td>");
                }
                result.Append(
@"
                <td>" + (globalEq.Test(key) ? Local[Language.FR]["true"] : Local[Language.FR]["false"]) + "</td>");
                result.Append(
@"
            </tr>");
            }
            result.Append(footer);
            return result.ToString();
        }

        #endregion Public Methods

        #region Private Methods

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            error.Text = "";
            var html = ParseToHTML(inputBox.Text + ")");
            if (html == null)
                return;
            var tmpDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "logicTableTemp");
            var tmpFile = System.IO.Path.Combine(tmpDir, "tmpGeneratedLogicTable.html");
            Directory.CreateDirectory(tmpDir);
            var sr = new StreamWriter(new FileStream(tmpFile, FileMode.Create));
            sr.Write(html);
            sr.Close();
            Process.Start(tmpFile);
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int curPos = inputBox.CaretIndex;
            var newT = inputBox.Text.Replace("->", "→");
            if (newT != inputBox.Text)
            {
                curPos--;
                inputBox.Text = (string)newT.Clone();
                inputBox.CaretIndex = curPos >= 0 ? curPos : 0;
                return;
            }
            newT = inputBox.Text.Replace("<→", "🡘");
            if (newT != inputBox.Text)
            {
                curPos--;
                inputBox.Text = (string)newT.Clone();
                inputBox.CaretIndex = curPos >= 0 ? curPos : 0;
            }
        }

        #endregion Private Methods
    }
}