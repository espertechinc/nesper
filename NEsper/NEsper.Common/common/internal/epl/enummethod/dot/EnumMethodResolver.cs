///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.hook.enummethod;
using com.espertech.esper.common.@internal.epl.enummethod.eval.plugin;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
	public class EnumMethodResolver
	{
		public static bool IsEnumerationMethod(
			string name,
			ImportServiceCompileTime classpathImportService)
		{
			foreach (EnumMethodBuiltin e in EnumHelper.GetValues<EnumMethodBuiltin>()) {
				var eNameCamel = e.GetNameCamel();
				if (string.Equals(eNameCamel, name, StringComparison.InvariantCultureIgnoreCase)) {
					return true;
				}
			}

			try {
				return classpathImportService.ResolveEnumMethod(name) != null;
			}
			catch (ImportException e) {
				throw new ExprValidationException("Failed to resolve enum-method '" + name + "': " + e.Message, e);
			}
		}

		public static EnumMethodDesc FromName(
			string name,
			ImportServiceCompileTime classpathImportService)
		{
			foreach (EnumMethodBuiltin e in EnumHelper.GetValues<EnumMethodBuiltin>()) {
				var eNameCamel = e.GetNameCamel();
				if (string.Equals(eNameCamel, name, StringComparison.InvariantCultureIgnoreCase)) {
					return e.GetDescriptor();
				}
			}

			try {
				Type factory = classpathImportService.ResolveEnumMethod(name);
				if (factory != null) {
					EnumMethodForgeFactory forgeFactory = TypeHelper.Instantiate<EnumMethodForgeFactory>(factory);
					EnumMethodDescriptor descriptor = forgeFactory.Initialize(new EnumMethodInitializeContext());
					ExprDotForgeEnumMethodFactoryPlugin plugin = new ExprDotForgeEnumMethodFactoryPlugin(forgeFactory);
					return new EnumMethodDesc(name, EnumMethodEnum.PLUGIN, plugin, descriptor.Footprints);
				}
			}
			catch (Exception ex) {
				throw new ExprValidationException("Failed to resolve date-time-method '" + name + "' :" + ex.Message, ex);
			}

			return null;
		}
	}
} // end of namespace
