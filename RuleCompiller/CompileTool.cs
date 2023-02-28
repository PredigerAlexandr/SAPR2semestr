using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller
{
    public class CompileTool
    {
        private static readonly Dictionary<char, string> convertedLetters = new Dictionary<char, string>
        {
            {'а', "a"},
            {'б', "b"},
            {'в', "v"},
            {'г', "g"},
            {'д', "d"},
            {'е', "e"},
            {'ё', "yo"},
            {'ж', "zh"},
            {'з', "z"},
            {'и', "i"},
            {'й', "j"},
            {'к', "k"},
            {'л', "l"},
            {'м', "m"},
            {'н', "n"},
            {'о', "o"},
            {'п', "p"},
            {'р', "r"},
            {'с', "s"},
            {'т', "t"},
            {'у', "u"},
            {'ф', "f"},
            {'х', "h"},
            {'ц', "c"},
            {'ч', "ch"},
            {'ш', "sh"},
            {'щ', "sch"},
            {'ъ', "j"},
            {'ы', "i"},
            {'ь', "j"},
            {'э', "e"},
            {'ю', "yu"},
            {'я', "ya"},
            {'А', "A"},
            {'Б', "B"},
            {'В', "V"},
            {'Г', "G"},
            {'Д', "D"},
            {'Е', "E"},
            {'Ё', "Yo"},
            {'Ж', "Zh"},
            {'З', "Z"},
            {'И', "I"},
            {'Й', "J"},
            {'К', "K"},
            {'Л', "L"},
            {'М', "M"},
            {'Н', "N"},
            {'О', "O"},
            {'П', "P"},
            {'Р', "R"},
            {'С', "S"},
            {'Т', "T"},
            {'У', "U"},
            {'Ф', "F"},
            {'Х', "H"},
            {'Ц', "C"},
            {'Ч', "Ch"},
            {'Ш', "Sh"},
            {'Щ', "Sch"},
            {'Ъ', "J"},
            {'Ы', "I"},
            {'Ь', "J"},
            {'Э', "E"},
            {'Ю', "Yu"},
            {'Я', "Ya"}
        };

        public static string FormatVariableName(string name)
        {
            name = char.ToLower(name[0]) + name.Substring(1);//first char to lowercase
            if (int.TryParse(""+name[0], out int tmp)) name = '_' + name;
            StringBuilder result = new StringBuilder(name.Length);
            char tmpSymbol = '\0';
            string tmpString = "";
            bool doubleSpaceFlag = false;
            for (int i = 0; i < name.Length; i++)
            {
                
                tmpSymbol = name[i];
                if (Char.IsDigit(tmpSymbol))
                {
                    doubleSpaceFlag = false;
                    result.Append(tmpSymbol);
                }
                else
                {
                    if (Char.IsLetter(tmpSymbol))
                    {
                        if (convertedLetters.TryGetValue(tmpSymbol, out tmpString))
                        {
                            doubleSpaceFlag = false;
                            result.Append(tmpString);
                        }
                        else result.Append(tmpSymbol);
                    }
                    else if (doubleSpaceFlag == false)
                    {
                        result.Append('_');
                        doubleSpaceFlag = true;
                    }
                }
            }

            return result.ToString();
        }

        public static string BuildString(string stringParameter)
        {
            return '"' + stringParameter.Replace("\\", "\\\\").Replace("\"", "\\\"") + '"';
        }

        public static bool IsExpressionInsideBlock(string expression)
        {
            expression = expression.Trim();
            if (expression.Length > 0 && expression[0] == '{')
            {
                int figureBracketsCounter = 0;
                for (int i = 0; i < expression.Length; i++)
                {
                    if (expression[i] == '{') figureBracketsCounter++;
                    else if (expression[i] == '}')
                    {
                        if (--figureBracketsCounter == 0) break;
                    }
                    else if (expression[i] == '"')
                    {
                        SkipString(expression, ref i);
                        i--;
                    }
                    else if (expression[i] == '\'')
                    {
                        SkipSymbolConstant(expression, ref i);
                        i--;
                    }
                }
                return figureBracketsCounter == 0;
            }
            return false;
        }

        public static void SkipString(string code, ref int index)
        {
            if (code[index] == '\"')
            {
                for (++index; index < code.Length; index++)
                {
                    if (code[index] == '\\')
                    {
                        index++;
                        if (index == code.Length) break;
                    }
                    else if (code[index] == '"')
                    {
                        index++;
                        break;
                    }
                }
            }
        }

        public static void SkipSymbolConstant(string code, ref int index)
        {
            for (++index; index < code.Length && code[index] != '\''; index++) ;
            index++;
        }

        public static bool IsUnuselessExpression(string expression)
        {
            if (string.IsNullOrEmpty(expression)) return true;

            for (int i = 0; i < expression.Length; i++)
            {
                if (
                    Char.IsLetterOrDigit(expression[i]) || 
                    (
                        expression[i] != '{' &&
                        expression[i] != '}' &&
                        expression[i] != ';' &&
                        expression[i] != ' ' &&
                        expression[i] != '\r' &&
                        expression[i] != '\n' &&
                        expression[i] != '\t'
                    )
                ) return false;
            }

            return true;
        }
    }

    public class VariableInfo
    {
        public string name = "";
        public int id = 0;
    }
}
