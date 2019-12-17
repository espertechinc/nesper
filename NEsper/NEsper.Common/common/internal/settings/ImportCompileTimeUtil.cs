///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.settings
{
    public class ImportCompileTimeUtil
    {
        public static object ResolveIdentAsEnumConst(
            string constant,
            ImportServiceCompileTime importService,
            bool isAnnotation)
        {
            var enumValue = ResolveIdentAsEnum(constant, importService, isAnnotation);
            if (enumValue == null) {
                return null;
            }

            try {
                return enumValue.EnumField.GetValue(null);
            }
            catch (MemberAccessException e) {
                throw new ExprValidationException(
                    "Exception accessing field '" + enumValue.EnumField.Name + "': " + e.Message,
                    e);
            }
        }

        public static EnumValue ResolveIdentAsEnum(
            string constant,
            ImportServiceCompileTime importService,
            bool isAnnotation)
        {
            var lastDotIndex = constant.LastIndexOf('.');
            if (lastDotIndex == -1) {
                return null;
            }

            var className = constant.Substring(0, lastDotIndex);
            var constName = constant.Substring(lastDotIndex + 1);

            // un-escape
            className = Unescape(className);
            constName = Unescape(constName);

            Type clazz;
            try {
                clazz = importService.ResolveClass(className, isAnnotation);
            }
            catch (ImportException) {
                return null;
            }

            var field = clazz.GetField(constName);
            if (field == null) {
                return null;
            }

            if (field.IsPublic && field.IsStatic) {
                return new EnumValue(clazz, field);
            }

            return null;
        }

        private static string Unescape(string name)
        {
            if (name.StartsWith("`") && name.EndsWith("`")) {
                return name.Substring(1, name.Length - 2);
            }

            return name;
        }
    }
} // end of namespace