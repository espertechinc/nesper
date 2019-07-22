///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    /// <summary>
    ///     Writer for a property to an event.
    /// </summary>
    public class BeanEventPropertyWriter : EventPropertyWriterSPI
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Type clazz;
        private readonly MethodInfo writerMethod;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="clazz">to write to</param>
        /// <param name="writerMethod">write method</param>
        public BeanEventPropertyWriter(
            Type clazz,
            MethodInfo writerMethod)
        {
            this.clazz = clazz;
            this.writerMethod = writerMethod;
        }

        public virtual void Write(
            object value,
            EventBean target)
        {
            Invoke(new[] {value}, target.Underlying);
        }

        public CodegenExpression WriteCodegen(
            CodegenExpression assigned,
            CodegenExpression und,
            CodegenExpression target,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            return ExprDotMethod(und, writerMethod.Name, assigned);
        }

        public void WriteValue(
            object value,
            object target)
        {
            Invoke(new[] {value}, target);
        }

        protected void Invoke(
            object[] values,
            object target)
        {
            try {
                writerMethod.Invoke(target, values);
            }
            catch (MemberAccessException e) {
                Handle(e);
            }
            catch (TargetException e) {
                Handle(e);
            }
        }

        private void Handle(Exception e)
        {
            var message = "Unexpected exception encountered invoking setter-method '" +
                          writerMethod +
                          "' on class '" +
                          clazz.Name +
                          "' : " +
                          e.Message;
            Log.Error(message, e);
        }
    }
} // end of namespace