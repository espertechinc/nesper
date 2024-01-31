///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.settings
{
    public class ImportUtil
    {
        /// <summary>
        ///     Returns an instance of a hook as specified by an annotation.
        /// </summary>
        /// <param name="annotations">to search</param>
        /// <param name="hookType">type to look for</param>
        /// <param name="interfaceExpected">interface required</param>
        /// <param name="importService">for resolving references</param>
        /// <returns>hook instance</returns>
        /// <throws>ExprValidationException if instantiation failed</throws>
        public static object GetAnnotationHook(
            Attribute[] annotations,
            HookType hookType,
            Type interfaceExpected,
            ImportService importService)
        {
            if (annotations == null) {
                return null;
            }

            string hookClass = null;
            for (var i = 0; i < annotations.Length; i++) {
                if (annotations[i] is HookAttribute hookAttribute) {
                    if (hookAttribute.HookType == hookType) {
                        hookClass = hookAttribute.Hook;
                    }
                }
            }

            if (hookClass == null) {
                return null;
            }

            Type clazz;
            try {
                clazz = importService.ResolveType(hookClass, false, ExtensionClassEmpty.INSTANCE);
            }
            catch (Exception e) {
                throw new ExprValidationException(
                    $"Failed to resolve hook provider of hook type '{hookType}' import '{hookClass}' :{e.Message}",
                    e);
            }

            var clazzName = clazz.Name;
            if (!clazz.IsImplementsInterface(interfaceExpected)) {
                var interfaceExpectedName = interfaceExpected.Name;
                throw new ExprValidationException(
                    $"Hook provider for hook type '{hookType}' class '{clazzName}' does not implement the required '{interfaceExpectedName}' interface");
            }

            try {
                return TypeHelper.Instantiate(clazz);
            }
            catch (Exception e) {
                throw new ExprValidationException(
                    "Failed to instantiate hook provider of hook type '" +
                    hookType +
                    "' " +
                    "class '" +
                    clazzName +
                    "' :" +
                    e.Message);
            }
        }
    }
} // end of namespace