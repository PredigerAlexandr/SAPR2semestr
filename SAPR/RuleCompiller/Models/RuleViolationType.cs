using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Models
{
    /// <summary>
    /// Тип сообщения правила.
    /// </summary>
    public enum RuleViolationType
    {
        /// <summary>
        /// Информационное сообщение.
        /// </summary>
        Information = 0,
        /// <summary>
        /// Предупреждение.
        /// </summary>
        Warning = 1,
        /// <summary>
        /// Ошибка.
        /// </summary>
        Error = 2
    }
}
