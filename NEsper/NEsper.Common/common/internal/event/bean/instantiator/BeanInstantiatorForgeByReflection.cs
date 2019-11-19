///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.instantiator
{
    public class BeanInstantiatorForgeByReflection : BeanInstantiatorForge,
        BeanInstantiator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly MethodInfo method;

        public BeanInstantiatorForgeByReflection(MethodInfo method)
        {
            this.method = method;
        }

        public object Instantiate()
        {
            try {
                return method.Invoke(null, null);
            }
            catch (TargetException e) {
                var message = "Unexpected exception encountered invoking factory method '" +
                              method.Name +
                              "' on class '" +
                              method.DeclaringType.Name +
                              "': " +
                              e.InnerException.Message;
                Log.Error(message, e);
                return null;
            }
            catch (MemberAccessException ex) {
                var message = "Unexpected exception encountered invoking factory method '" +
                              method.Name +
                              "' on class '" +
                              method.DeclaringType.Name +
                              "': " +
                              ex.Message;
                Log.Error(message, ex);
                return null;
            }
        }

        public BeanInstantiator BeanInstantiator => this;

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(method.DeclaringType, method.Name);
        }
    }
} // end of namespace