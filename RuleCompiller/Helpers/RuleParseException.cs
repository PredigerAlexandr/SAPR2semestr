//using AST.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Helpers
{
    //public class RuleParseException : AstException
    //{
    //    public int LineNumber { get; private set; }

    //    public RuleParseException(int lineNumber, string message) : base(message)
    //    {
    //        LineNumber = lineNumber;
    //    }

    //    public RuleParseException(int lineNumber, string message, Exception innerException) : base(message, innerException)
    //    {
    //        LineNumber = lineNumber;
    //    }

    //    public Dictionary<int, string[]> GetLineInformation()
    //    {
    //        Dictionary<int, string[]> lines = new Dictionary<int, string[]>();

    //        Exception exception = this;

    //        int lineNumber = LineNumber;
    //        List<string> messages = new List<string>();

    //        while (exception != null)
    //        {
    //            RuleParseException ruleParseException = exception as RuleParseException;

    //            if (ruleParseException != null)
    //            {
    //                if (ruleParseException.LineNumber == lineNumber)
    //                {
    //                    messages.Add(ruleParseException.Message);
    //                }
    //                else
    //                {
    //                    lines[lineNumber] = messages.ToArray();

    //                    messages = new string[] { ruleParseException.Message }.ToList();
    //                    lineNumber = ruleParseException.LineNumber;
    //                }
    //            }
    //            else
    //            {
    //                messages.Add(exception.Message);
    //            }

    //            exception = exception.InnerException;
    //        }

    //        lines[lineNumber] = messages.ToArray();

    //        return lines;
    //    }
    //}
}
