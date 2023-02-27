using System;
using DynamicExpression = System.Linq.Dynamic.DynamicExpression;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RuleCompiller.Helpers
{
    /// <summary>
    /// Хэлпер для компиляции и запуска динамического выражения, заданного в виде строки, над указанным объектом.
    /// </summary>
    public static class DynamicExpressionHelper
    {
        /// <summary>
        /// Осуществляет компиляцию и запуск динамического выражения.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="expressionString"></param>
        /// <returns></returns>
        public static object Run(object target, string expressionString)
        {
            if (!expressionString.Contains("=>"))
            {
                var expressionKey = string.Format("{0}_{1}_{2}", "B3604E50-853C-40DF-B6CD-71E35F222955", target.GetType(), expressionString);

                //var expressionDelegate = DynamicExpressionCache.Get(expressionKey) as Delegate;

                Delegate expressionDelegate = null;

                if (expressionDelegate == null)
                {
                    // Объявляем параметр для динамического выражения.
                    ParameterExpression x = Expression.Parameter(target.GetType(), "x");

                    // Осуществляем разбор выражения.
                    LambdaExpression exp = DynamicExpression.ParseLambda(new[] { x }, typeof(object), expressionString);

                    // Компилируем выражение 
                    expressionDelegate = exp.Compile();


                    //// Получение параметров кэширования
                    //// Режим кэширования
                    //var cacheModeParam = ConfigurationManager.AppSettings["modelRuleCompiledCacheMode"];
                    //// Длительность кэширования
                    //var cacheDurationParam = ConfigurationManager.AppSettings["modelRuleCompiledCacheExpiration"];

                    //DynamicExpressionCache.CacheMode cacheMode;
                    //int cacheDuration = 0;

                    //// Выполняем парсинг и проверку на валидность параметров
                    //var cacheIsSet = Enum.TryParse(cacheModeParam, true, out cacheMode) &&
                    //                 int.TryParse(cacheDurationParam, out cacheDuration) &&
                    //                 cacheMode != DynamicExpressionCache.CacheMode.None;

                    //if (cacheIsSet)
                    //{
                    //    var policy = DynamicExpressionCache.CreatePolicy(cacheMode, cacheDuration);
                    //    DynamicExpressionCache.Add(expressionKey, expressionDelegate, policy);
                    //}
                }

                // Вызываем его на выполнение.
                return expressionDelegate.DynamicInvoke(target);
            }
            else
            {
                var parts = GetExpressionParts(expressionString).Select(p => System.Linq.Dynamic.DynamicStatement.FromString(p)).ToArray();

                int parameterIndex = 1;

                string translatedExpression = "x";

                Dictionary<ParameterExpression, object> parameters = new Dictionary<ParameterExpression, object>();

                parameters.Add(System.Linq.Expressions.Expression.Parameter(target.GetType(), "x"), target);

                foreach (var part in parts.Skip(1))
                {
                    Delegate func = ParseLambdaExpression(translatedExpression, parameters.Keys.ToArray());
                    Type argumentType = func.Method.ReturnType;
                    Type argumentIEnumerableType = argumentType.Name == "IEnumerable`1"
                        ? argumentType
                        : argumentType.GetInterface("IEnumerable`1");
                    Type collectionItemType = argumentIEnumerableType != null
                        ? argumentIEnumerableType.GetGenericArguments()[0]
                        : null;

                    if (!part.IsMethod)
                    {
                        translatedExpression = string.Format("{0}.{1}", translatedExpression, part.Expression);
                    }
                    else
                    {
                        if (part.Name == "Where")
                        {
                            string parameterName = string.Format("a{0}", parameterIndex++);

                            Delegate d = (Delegate)ParseLambdaExpression(part.Args[0].Expression, collectionItemType, typeof(bool));

                            parameters.Add(System.Linq.Expressions.Expression.Parameter(d.GetType(), parameterName), d);

                            translatedExpression = string.Format("{0}.Where({1})", translatedExpression, parameterName);
                        }
                        else if (part.Name == "Select")
                        {
                            string parameterName = string.Format("a{0}", parameterIndex++);

                            Delegate d = (Delegate)ParseLambdaExpression(part.Args[0].Expression, collectionItemType, null);

                            parameters.Add(System.Linq.Expressions.Expression.Parameter(d.GetType(), parameterName), d);

                            translatedExpression = string.Format("{0}.Select({1})", translatedExpression, parameterName);
                        }
                        else
                        {
                            translatedExpression = string.Format("{0}.{1}", translatedExpression, part.Expression);
                        }
                    }
                }

                return ExecuteSimple(translatedExpression, parameters);
            }
        }

        private static object ExecuteSimple(string expression, Dictionary<ParameterExpression, object> parameters)
        {
            LambdaExpression exp = System.Linq.Dynamic.DynamicExpression.ParseLambda(parameters.Keys.ToArray(), typeof(object), expression);

            return exp.Compile().DynamicInvoke(parameters.Values.ToArray());
        }

        private static object ExecuteSimple(string expression, object parameter)
        {
            ParameterExpression x = System.Linq.Expressions.Expression.Parameter(parameter.GetType(), "x");
            // Осуществляем разбор выражения.
            LambdaExpression exp = System.Linq.Dynamic.DynamicExpression.ParseLambda(new[] { x }, typeof(object), expression);

            return exp.Compile().DynamicInvoke(parameter);
        }

        private static object ExecuteSimple(string expression, object parameter1, object parameter2)
        {
            ParameterExpression x = System.Linq.Expressions.Expression.Parameter(parameter1.GetType(), "x");
            ParameterExpression y = System.Linq.Expressions.Expression.Parameter(parameter2.GetType(), "y");

            // Осуществляем разбор выражения.
            LambdaExpression exp = System.Linq.Dynamic.DynamicExpression.ParseLambda(new[] { x, y }, typeof(object), expression);

            return exp.Compile().DynamicInvoke(parameter1, parameter2);
        }

        private static Delegate ParseLambdaExpression(string expressionSource, Type sourceType, Type resultType)
        {
            string[] expressionParts = expressionSource.Split(new[] { "=>" }, StringSplitOptions.RemoveEmptyEntries).Select(part => part.TrimStart(' ').TrimEnd(' ')).ToArray();

            string parameterName = expressionParts.First();
            string expression = expressionParts.Last();

            ParameterExpression parameter = Expression.Parameter(sourceType, parameterName);

            LambdaExpression lambda = System.Linq.Dynamic.DynamicExpression.ParseLambda(new[] { parameter }, resultType, expression);

            return lambda.Compile();
        }

        private static Delegate ParseLambdaExpression(string expression, params ParameterExpression[] parameters)
        {
            LambdaExpression lambda = System.Linq.Dynamic.DynamicExpression.ParseLambda(parameters, null, expression);

            return lambda.Compile();
        }

        private static IEnumerable<string> GetExpressionParts(string expression)
        {
            int count = 0;
            string part = string.Empty;

            foreach (char c in expression)
            {
                switch (c)
                {
                    case '(':
                        count++;
                        break;
                    case ')':
                        count--;
                        break;
                }

                if (c != '.' || count != 0)
                {
                    part += c;
                }
                else
                {
                    yield return part;
                    part = string.Empty;
                }
            }

            yield return part;
        }
    }

    //internal static class DynamicExpressionCache
    //{
    //    internal static object Get(string expressionKey)
    //    {
    //        ObjectCache cache = MemoryCache.Default;
    //        return cache[expressionKey];
    //    }

    //    internal static void Add(string key, object objectToCache, CacheItemPolicy policy)
    //    {
    //        ObjectCache cache = MemoryCache.Default;
    //        cache.Add(key, objectToCache, policy);
    //    }

    //    /// <summary>
    //    /// Создает объект политики кэширования
    //    /// </summary>
    //    /// <param name="cacheMode">Режим кэширования</param>
    //    /// <param name="expirationTime">Длительность хранения объекта в кэш, секунд</param>
    //    /// <returns>Созданный объект политики</returns>
    //    internal static CacheItemPolicy CreatePolicy(CacheMode cacheMode, int expirationTime)
    //    {
    //        if (CacheMode.Absolute == cacheMode)
    //        {
    //            return new CacheItemPolicy
    //            {
    //                AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddSeconds(expirationTime))
    //            };
    //        }
    //        if (CacheMode.Sliding == cacheMode)
    //        {
    //            return new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, 0, expirationTime) };
    //        }

    //        return null;
    //    }

    /// <summary>
    /// Режимы кэширования
    /// </summary>
    internal enum CacheMode
    {
        /// <summary>
        /// Кэширование не выполняется
        /// </summary>
        None,
        /// <summary>
        /// Абсолютный режим
        /// </summary>
        Absolute,
        /// <summary>
        /// Скользящий режим
        /// </summary>
        Sliding
    }

}
