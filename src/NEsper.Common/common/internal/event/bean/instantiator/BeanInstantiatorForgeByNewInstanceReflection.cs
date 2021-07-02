///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.instantiator
{
    public class BeanInstantiatorForgeByNewInstanceReflection : BeanInstantiatorForge,
        BeanInstantiator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Type clazz;

        public BeanInstantiatorForgeByNewInstanceReflection(Type clazz)
        {
            this.clazz = clazz;
        }

        public object Instantiate()
        {
            try {
                return TypeHelper.Instantiate(clazz);
            }
            catch (MemberAccessException e) {
                return Handle(e);
            }
            catch (TargetException e) {
                return Handle(e);
            }
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope)
        {
            return NewInstance(clazz);
        }

        public BeanInstantiator BeanInstantiator => this;

        private object Handle(Exception e)
        {
            var message = "Unexpected exception encountered invoking newInstance on class '" +
                          clazz.Name +
                          "': " +
                          e.Message;
            Log.Error(message, e);
            return null;
        }
    }
} // end of namespace