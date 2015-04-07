///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.type
{
    /// <summary>
    /// Enumeration for the type of arithmatic to use.
    /// </summary>

    public enum MinMaxTypeEnum
    {
        MAX,
        MIN
    }

    public static class MinMaxTypeEnumExtensions
    {
        public static string GetExpressionText(this MinMaxTypeEnum @enum)
        {
            switch(@enum)
            {
                case MinMaxTypeEnum.MAX:
                    return "max";
                case MinMaxTypeEnum.MIN:
                    return "min";
            }

            throw new ArgumentException("invalid value for enum");
        }

        /// <summary>Executes child expression nodes and compares results, returning the min/max. </summary>
        /// <param name="eventsPerStream">events per stream</param>
        /// <param name="isNewData">true if new data</param>
        /// <param name="exprEvaluatorContext">the expression evaluator context</param>
        /// <returns>result</returns>
        public delegate Object Computer(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>
        /// Determines minimum using a conversion to normalize type.
        /// </summary>
        public static Computer CreateMinDoubleComputer(ExprEvaluator[] childNodes)
        {
            var typeCaster = CastHelper.GetCastConverter<Double>();

            return delegate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
                   {
                       Object valueResult = null;
                       Double typedResult = Double.MaxValue;

                       for (int ii = 0; ii < childNodes.Length; ii++)
                       {
                           var valueChild = childNodes[ii].Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                           if (valueChild == null)
                           {
                               return null;
                           }

                           var typedChild = typeCaster.Invoke(valueChild);
                           if (typedChild < typedResult)
                           {
                               valueResult = valueChild;
                               typedResult = typedChild;
                           }
                       }

                       return valueResult;
                   };
        }

        /// <summary>
        /// Determines maximum using a conversion to normalize type.
        /// </summary>
        public static Computer CreateMaxDoubleComputer(ExprEvaluator[] childNodes)
        {
            var typeCaster = CastHelper.GetCastConverter<Double>();

            return delegate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
                   {
                       Object valueResult = null;
                       Double typedResult = Double.MinValue;

                       for (int ii = 0; ii < childNodes.Length; ii++)
                       {
                           var valueChild = childNodes[ii].Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                           if (valueChild == null)
                           {
                               return null;
                           }

                           var typedChild = typeCaster(valueChild);
                           if (typedChild > typedResult)
                           {
                               valueResult = valueChild;
                               typedResult = typedChild;
                           }
                       }

                       return valueResult;
                   };
        }

        /// <summary>
        /// Determines minimum using a conversion to normalize type.
        /// </summary>
        public static Computer CreateMinDecimalComputer(ExprEvaluator[] childNodes)
        {
            var typeCaster = CastHelper.GetCastConverter<Decimal>();

            return delegate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
                   {
                       Object valueResult = null;
                       Decimal typedResult = Decimal.MaxValue;

                       for (int ii = 0; ii < childNodes.Length; ii++)
                       {
                           var valueChild = childNodes[ii].Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                           if (valueChild == null)
                           {
                               return null;
                           }

                           var typedChild = typeCaster.Invoke(valueChild);
                           if (typedChild < typedResult)
                           {
                               valueResult = valueChild;
                               typedResult = typedChild;
                           }
                       }

                       return valueResult;
                   };
        }

        /// <summary>
        /// Determines maximum using a conversion to normalize type.
        /// </summary>
        public static Computer CreateMaxDecimalComputer(ExprEvaluator[] childNodes)
        {
            var typeCaster = CastHelper.GetCastConverter<Decimal>();

            return delegate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
                   {
                       Object valueResult = null;
                       Decimal typedResult = Decimal.MinValue;

                       for (int ii = 0; ii < childNodes.Length; ii++)
                       {
                           var valueChild = childNodes[ii].Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                           if (valueChild == null)
                           {
                               return null;
                           }

                           var typedChild = typeCaster(valueChild);
                           if (typedChild > typedResult)
                           {
                               valueResult = valueChild;
                               typedResult = typedChild;
                           }
                       }

                       return valueResult;
                   };
        }
    }
}
