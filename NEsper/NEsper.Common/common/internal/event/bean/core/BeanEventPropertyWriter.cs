///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
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
        private readonly MemberInfo _writerMember;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="clazz">to write to</param>
        /// <param name="writerMember">write method</param>
        public BeanEventPropertyWriter(
            Type clazz,
            MemberInfo writerMember)
        {
            this.clazz = clazz;
            this._writerMember = writerMember;
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
            return ExprDotMethod(und, _writerMember.Name, assigned);
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
                var writeMember = _writerMember;
                if (writeMember is MethodInfo writeMethod)
                {
                    writeMethod.Invoke(target, values);
                }
                else if (writeMember is PropertyInfo writeProperty)
                {
                    writeProperty.SetValue(target, values[0]);
                }
                else
                {
                    throw new IllegalStateException("writeMember of invalid type");
                }
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
            var message = new StringBuilder();

            message.Append("Unexpected exception encountered ");

            if (_writerMember is MethodInfo) {
                message.Append("invoking setter-method '");
                message.Append(_writerMember);
                message.Append("'");
            }
            else if (_writerMember is PropertyInfo) {
                message.Append("setting property '");
                message.Append(_writerMember);
                message.Append("'");
            }

            message.Append(" on class '");
            message.Append(clazz.Name);
            message.Append("' : ");
            message.Append(e.Message);

            Log.Error(message.ToString(), e);
        }
    }
} // end of namespace