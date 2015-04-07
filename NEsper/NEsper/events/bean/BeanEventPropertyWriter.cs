///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Writer for a property to an event.
    /// </summary>
    public class BeanEventPropertyWriter : EventPropertyWriter
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Type _type;
        private readonly FastMethod _writerMethod;
        private readonly bool _valueMustNotBeNull;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="clazz">to write to</param>
        /// <param name="writerMethod">write method</param>
        public BeanEventPropertyWriter(Type clazz, FastMethod writerMethod)
        {
            var parameterType = writerMethod.Target.GetParameters()[0].ParameterType;

            _type = clazz;
            _writerMethod = writerMethod;
            _valueMustNotBeNull = parameterType.IsValueType && !parameterType.IsNullable();
        }

        public virtual void Write(Object value, EventBean target)
        {
            Invoke(new[] { value }, target.Underlying);
        }

        public virtual void WriteValue(Object value, Object target)
        {
            Invoke(new[] { value }, target);
        }

        protected void Invoke(Object[] values, Object target)
        {
            try
            {
                _writerMethod.Invoke(target, values);
            }
            catch (TargetInvocationException e)
            {
                String message = "Unexpected exception encountered invoking setter-method '" + _writerMethod.Target + "' on class '" +
                        _type.FullName + "' : " + e.InnerException.Message;
                Log.Error(message, e);
            }
        }
    }
}
