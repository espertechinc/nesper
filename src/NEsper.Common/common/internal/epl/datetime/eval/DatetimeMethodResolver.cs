///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.hook.datetimemethod;
using com.espertech.esper.common.@internal.epl.datetime.plugin;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.datetime.eval
{
    public class DatetimeMethodResolver
    {
        public static bool IsDateTimeMethod(
            string name,
            ImportServiceCompileTime importService)
        {
            foreach (var e in EnumHelper.GetValues<DatetimeMethodBuiltin>()) {
                var eNameCamel = e.GetNameCamel();
                if (eNameCamel.Equals(name, StringComparison.InvariantCultureIgnoreCase)) {
                    return true;
                }
            }

            try {
                return importService.ResolveDateTimeMethod(name) != null;
            }
            catch (ImportException e) {
                throw new ExprValidationException("Failed to resolve date-time-method '" + name + "': " + e.Message, e);
            }
        }

        public static DatetimeMethodDesc FromName(
            string name,
            ImportServiceCompileTime importService)
        {
            foreach (var e in EnumHelper.GetValues<DatetimeMethodBuiltin>()) {
                var eNameCamel = e.GetNameCamel();
                if (eNameCamel.Equals(name, StringComparison.InvariantCultureIgnoreCase)) {
                    return e.GetDescriptor();
                }
            }

            try {
                var factory = importService.ResolveDateTimeMethod(name);
                if (factory != null) {
                    var forgeFactory = TypeHelper.Instantiate<DateTimeMethodForgeFactory>(factory);
                    var descriptor = forgeFactory.Initialize(new DateTimeMethodInitializeContext());
                    var plugin = new DTMPluginForgeFactory(forgeFactory);
                    return new DatetimeMethodDesc(DateTimeMethodEnum.PLUGIN, plugin, descriptor.Footprints);
                }
            }
            catch (Exception ex) {
                throw new ExprValidationException(
                    "Failed to resolve date-time-method '" + name + "' :" + ex.Message,
                    ex);
            }

            return null;
        }
    }
} // end of namespace