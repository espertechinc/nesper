///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.serde.serdeset.builtin;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // constant;

namespace com.espertech.esper.common.@internal.serde.compiletime.resolve
{
	public class SerdeCompileTimeResolverUtil
	{

		// Order of resolution:
		// (1) any serde providers are asked first, first one taking it counts
		// (2) class implements Externalizable (when allowed)
		// (3) class implements Serializable (when allowed)
		protected static SerdeProvision DetermineSerde(
			Type type,
			ICollection<SerdeProvider> serdeProviders,
			bool allowSerializable,
			bool allowExternalizable,
			bool allowSerializationFallback,
			SerdeProviderAdditionalInfo additionalInfo)
		{

			SerdeProvision serde;
			if (!serdeProviders.IsEmpty()) {
				serde = DetermineSerdeFromProviders(type, serdeProviders, additionalInfo);
				if (serde != null) {
					return serde;
				}

				if (type.IsArray) {
					SerdeProvision componentSerde = DetermineSerdeFromProviders(type.ComponentType, serdeProviders, additionalInfo);
					if (componentSerde != null) {
						return new SerdeProvisionParameterized(
							typeof(DIONullableObjectArraySerde),
							vars->Constant(type.ComponentType),
							vars->componentSerde.ToForge().Codegen(vars.Method, vars.Scope, vars.OptionalEventTypeResolver));
					}
				}
			}

			serde = DetermineSerializable(type, allowExternalizable, allowSerializable, allowSerializationFallback);
			if (serde != null) {
				return serde;
			}

			throw MakeFailedToFindException(type, allowExternalizable, allowSerializable, serdeProviders.Count, additionalInfo);
		}

		private static SerdeProvision DetermineSerdeFromProviders(
			Type type,
			ICollection<SerdeProvider> serdeProviders,
			SerdeProviderAdditionalInfo additionalInfo)
		{
			if (serdeProviders.IsEmpty()) {
				return null;
			}

			SerdeProviderContextClass context = new SerdeProviderContextClass(type, additionalInfo);
			foreach (SerdeProvider provider in serdeProviders) {
				try {
					SerdeProvision serde = provider.ResolveSerdeForClass(context);
					if (serde != null) {
						return serde;
					}
				}
				catch (DataInputOutputSerdeException ex) {
					throw ex;
				}
				catch (RuntimeException ex) {
					throw HandleProviderRuntimeException(provider, type, ex);
				}
			}

			return null;
		}

		private static SerdeProvision DetermineSerializable(
			Type type,
			bool allowExternalizable,
			bool allowSerializable,
			bool allowSerializationFallback)
		{
			if (allowSerializationFallback) {
				return new SerdeProvisionByClass(typeof(DIOSerializableObjectSerde));
			}

			if (TypeHelper.IsImplementsInterface(type, typeof(Externalizable)) && allowExternalizable) {
				return new SerdeProvisionByClass(typeof(DIOSerializableObjectSerde));
			}

			if (type.IsArray && TypeHelper.IsImplementsInterface(type.ComponentType, typeof(Externalizable)) && allowExternalizable) {
				return new SerdeProvisionByClass(typeof(DIOSerializableObjectSerde));
			}

			if (TypeHelper.IsImplementsInterface(type, typeof(Serializable)) && allowSerializable) {
				return new SerdeProvisionByClass(typeof(DIOSerializableObjectSerde));
			}

			if (type.IsArray && TypeHelper.IsImplementsInterface(type.ComponentType, typeof(Serializable)) && allowSerializable) {
				return new SerdeProvisionByClass(typeof(DIOSerializableObjectSerde));
			}

			return null;
		}

		private static DataInputOutputSerdeException HandleProviderRuntimeException(
			SerdeProvider provider,
			Type type,
			RuntimeException ex)
		{
			return new DataInputOutputSerdeException(
				"Unexpected exception invoking serde provider '" + provider.GetType().Name + "' passing '" + type.Name + "': " + ex.Message,
				ex);
		}

		private static DataInputOutputSerdeException MakeFailedToFindException(
			Type clazz,
			bool allowExternalizable,
			bool allowSerializable,
			int numSerdeProviders,
			SerdeProviderAdditionalInfo additionalInfo)
		{
			return new DataInputOutputSerdeException(
				"Failed to find serde for class '" +
				TypeHelper.GetClassNameFullyQualPretty(clazz) +
				"' for use with " +
				additionalInfo +
				" (" +
				"allowExternalizable=" +
				allowExternalizable +
				"," +
				"allowSerializable=" +
				allowSerializable +
				"," +
				"serdeProvider-count=" +
				numSerdeProviders +
				")");
		}
	}
} // end of namespace
