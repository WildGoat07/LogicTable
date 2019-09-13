using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicTable
{
    public partial class MainWindow
    {
        #region Public Methods

        public static UnparsedEq DivideStr(string eq, ref int index)
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

        public static Equation Parse(string str, UnparsedEq currentUnparsed, List<string> registeredVars)
        {
            int index = str.IndexOf("*");
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
                            var strs = new List<string>();
                            foreach (var item in new string(str.Reverse().ToArray()).Split(new string[] { "&" }, 2, StringSplitOptions.None))
                            {
                                strs.Add(new string(item.Reverse().ToArray()));
                            }
                            var eq = new OperatorEquation();
                            eq.Operator = OperatorEquation.OperatorType.AND;
                            eq.Right = Parse(strs[0], currentUnparsed, registeredVars);
                            eq.Left = Parse(strs[1], currentUnparsed, registeredVars);
                            return eq;
                        }
                    }
                    else
                    {
                        var strs = new List<string>();
                        foreach (var item in new string(str.Reverse().ToArray()).Split(new string[] { "|" }, 2, StringSplitOptions.None))
                        {
                            strs.Add(new string(item.Reverse().ToArray()));
                        }
                        var eq = new OperatorEquation();
                        eq.Operator = OperatorEquation.OperatorType.OR;
                        eq.Right = Parse(strs[0], currentUnparsed, registeredVars);
                        eq.Left = Parse(strs[1], currentUnparsed, registeredVars);
                        return eq;
                    }
                }
                else
                {
                    var strs = new List<string>();
                    foreach (var item in new string(str.Reverse().ToArray()).Split(new string[] { "→" }, 2, StringSplitOptions.None))
                    {
                        strs.Add(new string(item.Reverse().ToArray()));
                    }
                    var eq = new OperatorEquation();
                    eq.Operator = OperatorEquation.OperatorType.INVOLVING;
                    eq.Right = Parse(strs[0], currentUnparsed, registeredVars);
                    eq.Left = Parse(strs[1], currentUnparsed, registeredVars);
                    return eq;
                }
            }
            else
            {
                var strs = new List<string>();
                foreach (var item in new string(str.Reverse().ToArray()).Split(new string[] { "*" }, 2, StringSplitOptions.None))
                {
                    strs.Add(new string(item.Reverse().ToArray()));
                }
                var eq = new OperatorEquation();
                eq.Operator = OperatorEquation.OperatorType.EQUIVALENT;
                eq.Right = Parse(strs[0], currentUnparsed, registeredVars);
                eq.Left = Parse(strs[1], currentUnparsed, registeredVars);
                return eq;
            }
        }

        static public void SkipBlank(string str, ref int index)
        {
            var blankChar = new char[] { ' ', '\n', '\t' };
            while (index < str.Length && blankChar.Contains(str[index]))
                index++;
        }

        #endregion Public Methods
    }
}