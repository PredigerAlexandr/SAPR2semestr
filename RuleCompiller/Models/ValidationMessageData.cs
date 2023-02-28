using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Models
{
    [Serializable]
    public class ValidationMessageData
    {
        public ValidationMessageData(RuleViolationType _violationType, string _header, string _text)
        {
            ViolationType = _violationType;
            Header = _header;
            Text = _text;
        }

        /// <summary>
        /// Уровень правила
        /// </summary>
        public RuleViolationType ViolationType { get; set; }
        /// <summary>
        /// Заголовок сообщения
        /// </summary>
        public string Header { get; set; }
        /// <summary>
        /// Текст сообщения
        /// </summary>
        public string Text { get; set; }
    }
}
