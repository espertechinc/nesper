///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;

using XLR8.CGLib;

namespace com.espertech.esper.events.bean
{
    public class InstanceManufacturerFastCtor : InstanceManufacturer
    {
        private readonly FastConstructor _ctor;
        private readonly ExprEvaluator[] _expr;
        private readonly Type _targetClass;

        public InstanceManufacturerFastCtor(Type targetClass, FastConstructor ctor, ExprEvaluator[] expr)
        {
            _targetClass = targetClass;
            _ctor = ctor;
            _expr = expr;
        }

        public object Make(EvaluateParams evaluateParams)
        {
            var row = new object[_expr.Length];
            for (int i = 0; i < row.Length; i++)
            {
                row[i] = _expr[i].Evaluate(evaluateParams);
            }
            return MakeUnderlyingFromFastCtor(row, _ctor, _targetClass);
        }

        public static object MakeUnderlyingFromFastCtor(object[] properties, FastConstructor ctor, Type target)
        {
            try
            {
                return ctor.New(properties);
            }
            catch (TargetException e)
            {
                throw new EPException(
                    "TargetInvocationException received invoking constructor for type '" + target.Name + "': " +
                    e.InnerException.Message, e.InnerException);
            }
        }
    }
} // end of namespace