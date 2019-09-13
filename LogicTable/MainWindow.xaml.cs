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
            generateButton.Content = Local[Language.FR]["button"];
            Title = Local[Language.FR]["winTitle"];
        }

        #endregion Public Constructors

        #region Public Methods

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

        public UnparsedEq DivideStr(string eq, ref int index)
        {
            UnparsedEq result = new UnparsedEq();
            result.str = "";
            result.subEqs = new List<UnparsedEq>();
            int nbSubEq = 0;
            while (eq[index] != ')')
            {
                char current = eq[index];
                if (current == '(')
                {
                    index++;
                    result.str += "%" + nbSubEq.ToString("000");
                    nbSubEq++;
                    result.subEqs.Add(DivideStr(eq, ref index));
                }
                else
                    result.str += current;
                index++;
            }
            return result;
        }

        public string getStr(Equation eq)
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
                        return '(' + getStr(oeq.Left) + ") && (" + getStr(oeq.Right) + ')';

                    case OperatorEquation.OperatorType.OR:
                        return '(' + getStr(oeq.Left) + ") || (" + getStr(oeq.Right) + ')';

                    case OperatorEquation.OperatorType.EQUIVALENT:
                        return '(' + getStr(oeq.Left) + ") <-> (" + getStr(oeq.Right) + ')';

                    case OperatorEquation.OperatorType.INVOLVING:
                        return '(' + getStr(oeq.Left) + ") -> (" + getStr(oeq.Right) + ')';
                }
            }
            return "";
        }

        public Equation Parse(string str, UnparsedEq currentUnparsed, List<string> registeredVars)
        {
            int index = str.IndexOf("🡘");
            if (index == -1)
            {
                index = str.IndexOf("→");
                if (index == -1)
                {
                    index = str.IndexOf("|");
                    if (index == -1)
                    {
                        index = str.IndexOf("&");
                        if (index == -1)
                        {
                            index = 0;
                            SkipBlank(str, ref index);
                            if (str[index] == '!')
                            {
                                index++;
                                var eq = new Not();
                                eq.InternalEquation = Parse(str.Substring(index), currentUnparsed, registeredVars);
                                return eq;
                            }
                            else if (char.IsLetter(str[index]))
                            {
                                var variableName = "";
                                char currentChar = str[index];
                                while (char.IsLetterOrDigit(currentChar) && index < str.Length)
                                {
                                    variableName += currentChar;
                                    index++;
                                    if (index < str.Length)
                                        currentChar = str[index];
                                }
                                SkipBlank(str, ref index);
                                if (index < str.Length)
                                    throw new Exception("Unexpected char :" + str[index]);
                                if (!registeredVars.Contains(variableName))
                                    registeredVars.Add(variableName);
                                var eq = new Constant();
                                eq.Name = variableName;
                                return eq;
                            }
                            else if (str[index] == '%')
                            {
                                var nbStr = str.Substring(index + 1, 3);
                                index += 4;
                                SkipBlank(str, ref index);
                                if (index < str.Length)
                                    throw new Exception("Unexpected char :" + str[index]);
                                var nextUnparsed = currentUnparsed.subEqs[int.Parse(nbStr)];
                                var eq = Parse(nextUnparsed.str, nextUnparsed, registeredVars);
                                return eq;
                            }
                            else
                                throw new Exception("wrong var starting char");
                        }
                        else
                        {
                            var strs = str.Split(new string[] { "&" }, 2, StringSplitOptions.None);
                            var eq = new OperatorEquation();
                            eq.Operator = OperatorEquation.OperatorType.AND;
                            eq.Left = Parse(strs[0], currentUnparsed, registeredVars);
                            eq.Right = Parse(strs[1], currentUnparsed, registeredVars);
                            return eq;
                        }
                    }
                    else
                    {
                        var strs = str.Split(new string[] { "|" }, 2, StringSplitOptions.None);
                        var eq = new OperatorEquation();
                        eq.Operator = OperatorEquation.OperatorType.OR;
                        eq.Left = Parse(strs[0], currentUnparsed, registeredVars);
                        eq.Right = Parse(strs[1], currentUnparsed, registeredVars);
                        return eq;
                    }
                }
                else
                {
                    var strs = str.Split(new string[] { "→" }, 2, StringSplitOptions.None);
                    var eq = new OperatorEquation();
                    eq.Operator = OperatorEquation.OperatorType.INVOLVING;
                    eq.Left = Parse(strs[0], currentUnparsed, registeredVars);
                    eq.Right = Parse(strs[1], currentUnparsed, registeredVars);
                    return eq;
                }
            }
            else
            {
                var strs = str.Split(new string[] { "🡘" }, 2, StringSplitOptions.None);
                var eq = new OperatorEquation();
                eq.Operator = OperatorEquation.OperatorType.EQUIVALENT;
                eq.Left = Parse(strs[0], currentUnparsed, registeredVars);
                eq.Right = Parse(strs[1], currentUnparsed, registeredVars);
                return eq;
            }
        }

        public string ParseToHTML(string eq)
        {
            string result =
@"
<!DOCTYPE HTML>
<html>
<head>
    <meta charset=""UTF-8"" />
    <title>" + Local[Language.FR]["tabTitle"] + "</title>" +
@"
</head>
<body>
    <table>

";
            string footer =
@"
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

            return getStr(globalEq);
        }

        public void SkipBlank(string str, ref int index)
        {
            var blankChar = new char[] { ' ', '\n', '\t' };
            while (index < str.Length && blankChar.Contains(str[index]))
                index++;
        }

        #endregion Public Methods

        #region Private Methods

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            error.Text = "";
            Console.WriteLine(ParseToHTML(inputBox.Text + ")"));
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