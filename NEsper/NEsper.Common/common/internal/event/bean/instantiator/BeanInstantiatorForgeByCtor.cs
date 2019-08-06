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
    public class BeanInstantiatorForgeByCtor : BeanInstantiatorForge
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Type underlyingType;

        public BeanInstantiatorForgeByCtor(Type underlyingType)
        {
            this.underlyingType = underlyingType;
        }

        public BeanInstantiator BeanInstantiator => new BeanInstantiatorByCtor(GetCtor(underlyingType));

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope)
        {
            var ctor = codegenClassScope.AddFieldUnshared(
                true,
                typeof(ConstructorInfo),
                StaticMethod(typeof(BeanInstantiatorForgeByCtor), "GetSunJVMCtor", Constant(underlyingType)));
            return StaticMethod(typeof(BeanInstantiatorForgeByCtor), "InstantiateSunJVMCtor", ctor);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="underlyingType">underlying</param>
        /// <returns>ctor</returns>
        public static ConstructorInfo GetCtor(Type underlyingType)
        {
            return underlyingType.GetConstructor(new Type[] { });
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="ctor">ctor</param>
        /// <returns>object</returns>
        public static object InstantiateCtor(ConstructorInfo ctor)
        {
            try {
                return ctor.Invoke(new object[0] { });
            }
            catch (TargetException e) {
                var message = "Unexpected exception encountered invoking constructor '" +
                              ctor.Name +
                              "' on class '" +
                              ctor.DeclaringType.Name +
                              "': " +
                              e.InnerException.Message;
                Log.Error(message, e);
                return null;
            }
            catch (Exception ex) when (ex is MemberAccessException) {
                return Handle(ex, ctor);
            }
        }

        private static object Handle(
            Exception e,
            ConstructorInfo ctor)
        {
            var message = "Unexpected exception encountered invoking newInstance on class '" +
                          ctor.DeclaringType.Name +
                          "': " +
                          e.Message;
            Log.Error(message, e);
            return null;
        }
    }
} // end of namespace