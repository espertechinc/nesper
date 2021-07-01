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
using com.espertech.esper.compat.collections;
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

        private readonly Type _clazz;
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
            this._clazz = clazz;
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
            CodegenExpression underlying,
            CodegenExpression target,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var writeMember = _writerMember;
            if (writeMember is MethodInfo writeMethod) {
                return ExprDotMethod(underlying, writeMethod.Name, assigned);
            }
            else if (writeMember is PropertyInfo writeProperty) {
                return SetProperty(underlying, writeProperty.Name, assigned);
            }
            else {
                throw new IllegalStateException("writeMember of invalid type");
            }
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
                if (writeMember is MethodInfo writeMethod) {
                    var parameter = writeMethod.GetParameters()[0].ParameterType;
                    var isClass = parameter.IsClass;
                    var isNullable = parameter.IsNullable();
                    if (values[0] != null) {
                        writeMethod.Invoke(target, values);
                    }
                    else if (isClass || isNullable) {
                        writeMethod.Invoke(target, values);
                    }
                }
                else if (writeMember is PropertyInfo writeProperty) {
                    var isClass = writeProperty.PropertyType.IsClass;
                    var isNullable = writeProperty.PropertyType.IsNullable();
                    if (values[0] != null) {
                        writeProperty.SetValue(target, values[0]);
                    }
                    else if (isClass || isNullable) {
                        writeProperty.SetValue(target, values[0]);
                    }
                }
                else {
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
            message.Append(_clazz.Name);
            message.Append("' : ");
            message.Append(e.Message);

            Log.Error(message.ToString(), e);
        }
    }
} // end of namespace