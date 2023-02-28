using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller
{
    public class CodeTreeHandler
    {
        public string xmlDataParameterName = "xmlData";
        public string validationDataVariableName = "validationData";
        public string violationsVariableName = "violations";

        protected List<VariableInfo> variablesInfo = new List<VariableInfo>();

        public Stack<string> validatorsStack = new Stack<string>();

        public List<string> usingPropertys = new List<string>();

        public bool CheckBlockErrors()
        {
            if (validatorsStack.Contains("if")) return true;
            return false;
        }

        public CodeTreeHandler()
        {
            variablesInfo.Add(new VariableInfo() { name = xmlDataParameterName });
            variablesInfo.Add(new VariableInfo() { name = validationDataVariableName });
            variablesInfo.Add(new VariableInfo() { name = violationsVariableName });
        }

        public void AddProperty(string propertyName)
        {
            if (!usingPropertys.Contains(propertyName)) usingPropertys.Add(propertyName);
        }

        public string GetUniqueVariableName(string name)
        {
            name = CompileTool.FormatVariableName(name);
            for (int i = 0, size = variablesInfo.Count; i < size; i++)
            {
                if (variablesInfo[i].name == name) return name + (++variablesInfo[i].id);
            }
            variablesInfo.Add(new VariableInfo() { name = name });
            return name;
        }
    }
}
