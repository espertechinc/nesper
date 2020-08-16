///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml.XPath;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.annotation;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.xml
{
	public class CreateSchemaXMLHelper
	{
		public static ConfigurationCommonEventTypeXMLDOM Configure(
			StatementBaseInfo @base,
			StatementCompileTimeServices services)
		{
			var config = new ConfigurationCommonEventTypeXMLDOM();
			var annotations = @base.StatementRawInfo.Annotations;

			var schemaAnnotations = AnnotationUtil.FindAnnotations(annotations, typeof(XMLSchemaAttribute));
			if (schemaAnnotations == null || schemaAnnotations.IsEmpty()) {
				throw new ExprValidationException("Required annotation @" + nameof(XMLSchemaAttribute) + " could not be found");
			}

			if (schemaAnnotations.Count > 1) {
				throw new ExprValidationException("Found multiple @" + nameof(XMLSchemaAttribute) + " annotations but expected a single annotation");
			}

			var schema = (XMLSchemaAttribute) schemaAnnotations[0];
			if (string.IsNullOrEmpty(schema.RootElementName)) {
				throw new ExprValidationException(
					"Required annotation field 'rootElementName' for annotation @" + nameof(XMLSchemaAttribute) + " could not be found");
			}

			config.RootElementName = schema.RootElementName.Trim();
			config.SchemaResource = NullIfEmpty(schema.SchemaResource);
			config.SchemaText = NullIfEmpty(schema.SchemaText);
			config.IsXPathPropertyExpr = schema.XPathPropertyExpr;
			config.DefaultNamespace = schema.DefaultNamespace;
			config.IsEventSenderValidatesRoot = schema.EventSenderValidatesRoot;
			config.IsAutoFragment = schema.AutoFragment;
			config.XPathFunctionResolver = NullIfEmpty(schema.XPathFunctionResolver);
			config.XPathVariableResolver = NullIfEmpty(schema.XPathVariableResolver);
			config.IsXPathResolvePropertiesAbsolute = schema.XPathResolvePropertiesAbsolute;
			config.RootElementNamespace = NullIfEmpty(schema.RootElementNamespace);

			var prefixes = AnnotationUtil.FindAnnotations(annotations, typeof(XMLSchemaNamespacePrefixAttribute));
			foreach (var prefixAnnotation in prefixes) {
				var prefix = (XMLSchemaNamespacePrefixAttribute) prefixAnnotation;
				config.AddNamespacePrefix(prefix.Prefix, prefix.Namespace);
			}

			var fields = AnnotationUtil.FindAnnotations(annotations, typeof(XMLSchemaFieldAttribute));
			foreach (var fieldAnnotation in fields) {
				var field = (XMLSchemaFieldAttribute) fieldAnnotation;
				var rtype = GetResultType(field.Type);
				if (string.IsNullOrWhiteSpace(field.EventTypeName)) {
					var castToType = NullIfEmpty(field.CastToType);
					config.AddXPathProperty(field.Name, field.XPath, rtype, castToType);
				}
				else {
					config.AddXPathPropertyFragment(field.Name, field.XPath, rtype, field.EventTypeName);
				}
			}

			return config;
		}

		private static string NullIfEmpty(string text)
		{
			return string.IsNullOrWhiteSpace(text)
				? null
				: text.Trim();
		}

		private static XPathResultType GetResultType(string type)
		{
			var localPart = type.ToLowerInvariant();
			return EnumHelper.Parse<XPathResultType>(localPart);
		}
	}
} // end of namespace
