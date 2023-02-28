using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller
{
    public class SimpleCodeFormatter
    {
        public static string Format(string code, string tabulationString = "\t")
        {
            StringBuilder result = new StringBuilder(code.Length * 3);
            int spacesCounter = 0;
            char lastSymbol = '\0';
            for (int i = 0; i < code.Length; i++)
            {
                switch (code[i])
                {
                    case '\r':
                    case '\n':
                    case '\t':
                        continue;
                    case '/': //skip comment line
                        if (code[i + 1] == '/')
                        {
                            for (; i < code.Length; i++)
                            {
                                if (code[i] == '\n') break;
                                result.Append(code[i]);
                            }
                            NewLine(result, spacesCounter, tabulationString);
                        }
                        break;
                    case ':'://case and goto fixers
                        result.Append(':');
                        NewLine(result, spacesCounter, tabulationString);
                        break;
                    case '{':
                        NewLine(result, spacesCounter, tabulationString);
                        spacesCounter++;
                        result.Append('{');
                        i++;
                        if (i >= code.Length) break;
                        SkipSpaces(code, ref i);
                        if (code[i] != '}') NewLine(result, spacesCounter, tabulationString);
                        i--;
                        break;
                    case '}':
                        spacesCounter--;
                        NewLine(result, spacesCounter, tabulationString);
                        result.Append('}');
                        i++;
                        if (i >= code.Length) break;
                        SkipSpaces(code, ref i);
                        if(code[i] != ';' && code[i] != '}') NewLine(result, spacesCounter, tabulationString);
                        i--;
                        break;
                    case ';':
                        result.Append(';');
                        i++;
                        SkipSpaces(code, ref i);
                        if (code[i] != '}') NewLine(result, spacesCounter, tabulationString);
                        i--;
                        break;
                    case '"':
                        SkipString(result, code, ref i);
                        i--;
                        break;
                    case '(':
                        SkipCircleBrackets(result, code, ref i);
                        break;
                    case '\'':
                        SkipSymbolConstant(result, code, ref i);
                        break;
                    default:
                        result.Append(code[i]);
                        break;
                }
            }
            return result.ToString();
        }


        protected static void NewLine(StringBuilder stringBuilder, int spacesCounter, string tabulationString)
        {
            stringBuilder.Append("\r\n");
            for (; spacesCounter > 0; spacesCounter--) stringBuilder.Append(tabulationString);
        }

        protected static void SkipSpaces(string code, ref int index)
        {
            for (; index < code.Length; index++)
            {
                if (code[index] != ' ' && code[index] != '\t' && code[index] != '\r' && code[index] != '\n') break;
            }
        }

        protected static void SkipString(StringBuilder stringBuilder, string code, ref int index)
        {
            if (code[index] == '\"')
            {
                stringBuilder.Append('"');
                for (++index; index < code.Length; index++)
                {
                    if (code[index] == '\\')
                    {
                        stringBuilder.Append('\\');
                        index++;
                        if (index == code.Length) throw new Exception("Bad code file");
                        stringBuilder.Append(code[index]);
                    }
                    else if (code[index] == '"')
                    {
                        stringBuilder.Append('"');
                        index++;
                        break;
                    }
                    else
                    {
                        stringBuilder.Append(code[index]);
                    }
                }
            }
        }

        protected static void SkipSymbolConstant(StringBuilder stringBuilder, string code, ref int index)
        {
            stringBuilder.Append('\'');
            for (++index; index < code.Length && code[index] != '\''; index++) stringBuilder.Append(code[index]);
            stringBuilder.Append('\'');
            index++;
        }

        protected static void SkipCircleBrackets(StringBuilder stringBuilder, string code, ref int index)
        {
            int bracketsCount = 0;

            for (; index < code.Length; index++)
            {
                stringBuilder.Append(code[index]);
                if (code[index] == '(') bracketsCount++;
                if (code[index] == ')')
                {
                    bracketsCount--;
                    if (bracketsCount <= 0) break;
                }
            }
        }
    }
}
