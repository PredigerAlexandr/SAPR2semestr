using RuleCompiller.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RuleCompiller.Validators
{
    /// <summary>
    /// Интерфейс для расширения функционала правил.
    /// </summary>
    public interface ICustomRuleFactory
    {
        /// <summary>
        /// Создает правило на основе XML-узла его представления.
        /// </summary>
        /// <param name="xRule">XML-узел правила.</param>
        /// <returns></returns>
        Rule Create(XElement xRule);
    }
}
