///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Schema;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Helper class for mapping a XSD schema model to an internal representation.
    /// </summary>
    public class XSDSchemaMapper
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public const int DEFAULT_MAX_RECURSIVE_DEPTH = 10;

        /// <summary>
        /// Loads a schema from the provided Uri.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="schemaText">The schema text.</param>
        /// <returns></returns>
        public static XmlSchema LoadSchema(Uri uri, String schemaText)
        {
            if (uri == null)
            {
                var stringReader = new StringReader(schemaText);
                var schema = XmlSchema.Read(stringReader, null);
                if (schema == null)
                {
                    throw new ConfigurationException("Failed to read schema from schemaText");
                }

                return schema;
            }

            using (var client = new WebClient())
            {
                using (var resourceStream = client.OpenRead(uri))
                {
                    var schema = XmlSchema.Read(resourceStream, null);
                    if (schema == null)
                    {
                        throw new ConfigurationException("Failed to read schema via URL '" + uri + '\'');
                    }

                    return schema;
                }
            }
        }

        /// <summary>
        /// Loading and mapping of the schema to the internal representation.
        /// </summary>
        /// <param name="schemaResource">schema to load and map.</param>
        /// <param name="schemaText">The schema text.</param>
        /// <param name="engineImportService">The engine import service.</param>
        /// <param name="resourceManager">The resource manager.</param>
        /// <param name="maxRecusiveDepth">depth of maximal recursive element</param>
        /// <returns>
        /// model
        /// </returns>
        /// <exception cref="ConfigurationException">Failed to read schema ' + schemaResource + ' :  + ex.Message</exception>
        public static SchemaModel LoadAndMap(
            String schemaResource,
            String schemaText,
            EngineImportService engineImportService,
            IResourceManager resourceManager,
            int maxRecusiveDepth = DEFAULT_MAX_RECURSIVE_DEPTH)
        {
            // Load schema
            try
            {
                var schemaLocation = string.IsNullOrEmpty(schemaResource) ? null : resourceManager.ResolveResourceURL(schemaResource);
                var schema = LoadSchema(schemaLocation, schemaText);
                return Map(schema, schemaLocation, maxRecusiveDepth);
            }
            catch (ConfigurationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ConfigurationException("Failed to read schema '" + schemaResource + "' : " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Dictionary that maps qualified names to schema types.
        /// </summary>
        private readonly IDictionary<XmlQualifiedName, XmlSchemaType> _schemaTypeDictionary =
            new Dictionary<XmlQualifiedName, XmlSchemaType>();
        /// <summary>
        /// Dictionary that maps qualified names to schema elements.
        /// </summary>
        private readonly IDictionary<XmlQualifiedName, XmlSchemaElement> _schemaElementDictionary =
            new Dictionary<XmlQualifiedName, XmlSchemaElement>();
        /// <summary>
        /// Namespace list
        /// </summary>
        private readonly IList<string> _namespaceList =
            new List<string>();
        /// <summary>
        /// Component list
        /// </summary>
        private readonly IList<SchemaElementComplex> _components =
            new List<SchemaElementComplex>();

        /// <summary>
        /// Resolves a schema type.
        /// </summary>
        /// <param name="xsModel">The xs model.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private XmlSchemaType ResolveSchemaType(XmlSchema xsModel, XmlQualifiedName name)
        {
            var schemaType = _schemaTypeDictionary.Get(name);
            if (schemaType == null)
            {
                schemaType = ResolveSimpleType(xsModel, name);
            }

            return schemaType;
        }

        /// <summary>
        /// Resolves a simple schema type.
        /// </summary>
        /// <param name="xsModel">The xs model.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private static XmlSchemaType ResolveSimpleType(XmlSchema xsModel, XmlQualifiedName name)
        {
            XmlSchemaType schemaType = XmlSchemaSimpleType.GetBuiltInSimpleType(name);
            if (schemaType == null)
            {
                schemaType = XmlSchemaSimpleType.GetBuiltInComplexType(name);
                if (schemaType == null)
                {
                    return xsModel.SchemaTypes[name] as XmlSchemaSimpleType;
                }
            }

            return schemaType;
        }

        /// <summary>
        /// Resolves an element.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private XmlSchemaElement ResolveElement(XmlQualifiedName name)
        {
            return _schemaElementDictionary.Get(name);
        }

        /// <summary>
        /// Maps the specified XSD schema into the internal model for the schema.
        /// </summary>
        /// <param name="xsModel">The xs model.</param>
        /// <param name="schemaLocation">The schema location.</param>
        /// <param name="maxRecursiveDepth">The max recursive depth.</param>
        /// <returns></returns>
        private static SchemaModel Map(XmlSchema xsModel, Uri schemaLocation, int maxRecursiveDepth)
        {
            XSDSchemaMapper mapper = new XSDSchemaMapper();
            mapper.Import(xsModel, schemaLocation);
            return mapper.CreateModel();
        }

        /// <summary>
        /// Creates the model.
        /// </summary>
        /// <returns></returns>
        private SchemaModel CreateModel()
        {
            return new SchemaModel(_components, _namespaceList);
        }

        private void Import(XmlSchema xsModel, Uri schemaLocation)
        {
            ImportNamespaces(xsModel);
            ImportIncludes(xsModel, schemaLocation, Import);

            BuildElementDictionary(xsModel);
            BuildTypeDictionary(xsModel);

            // get top-level complex elements
            foreach (var schemaElement in xsModel.Items.OfType<XmlSchemaElement>())
            {
                var schemaType = schemaElement.SchemaType;
                if (schemaType == null)
                {
                    var schemaTypeName = schemaElement.SchemaTypeName;
                    if (!Equals(schemaTypeName, XmlQualifiedName.Empty))
                    {
                        schemaType = ResolveSchemaType(xsModel, schemaTypeName);
                    }
                }

                var complexElementType = schemaType as XmlSchemaComplexType;
                if (complexElementType != null)
                {
                    var complexActualName = schemaElement.QualifiedName;
                    if (Equals(complexActualName, XmlQualifiedName.Empty))
                        complexActualName = new XmlQualifiedName(
                            schemaElement.Name, xsModel.TargetNamespace);

                    var rootNode = new ElementPathNode(null, complexActualName);

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug(string.Format("Processing component {0}", complexActualName));
                    }

                    SchemaElementComplex complexElement = Process(
                        xsModel, complexActualName, complexElementType, false, rootNode);

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("Adding component {0}", complexActualName);
                    }

                    _components.Add(complexElement);
                }
            }
        }

        private void ImportIncludes(XmlSchema xsModel, Uri schemaLocation, Action<XmlSchema, Uri> importAction)
        {
            foreach (var include in xsModel.Includes)
            {
                var asImport = include as XmlSchemaImport;
                if (asImport != null)
                {
                    try
                    {
                        String importLocation = asImport.SchemaLocation;
                        Uri importSchemaLocation;
                        if (Uri.IsWellFormedUriString(importLocation, UriKind.Absolute))
                        {
                            importSchemaLocation = new Uri(importLocation, UriKind.RelativeOrAbsolute);
                        }
                        else
                        {
                            importSchemaLocation = new Uri(schemaLocation, importLocation);
                        }

                        var importSchema = LoadSchema(importSchemaLocation, null);
                        importAction(importSchema, importSchemaLocation);
                    }
                    catch (ConfigurationException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new ConfigurationException(
                            "Failed to read schema '" + asImport.SchemaLocation + "' : " + ex.Message, ex);
                    }
                }
            }
        }

        private void BuildTypeDictionary(XmlSchema xsModel)
        {
            // get top-level complex elements
            foreach (var oelement in xsModel.Items)
            {
                var complexType = oelement as XmlSchemaComplexType;
                if (complexType != null)
                {
                    var qualifiedName = complexType.QualifiedName;
                    if (Equals(qualifiedName, XmlQualifiedName.Empty))
                    {
                        qualifiedName = new XmlQualifiedName(
                            complexType.Name, xsModel.TargetNamespace);
                    }

                    _schemaTypeDictionary[qualifiedName] = complexType;
                }
            }
        }

        private void BuildElementDictionary(XmlSchema xsModel)
        {
            // Organize all of the elements
            foreach (var oelement in xsModel.Items)
            {
                var element = oelement as XmlSchemaElement;
                if (element != null)
                {
                    XmlQualifiedName elementName = element.QualifiedName;
                    if (Equals(elementName, XmlQualifiedName.Empty))
                    {
                        elementName = new XmlQualifiedName(
                            element.Name,
                            xsModel.TargetNamespace);
                    }

                    _schemaElementDictionary[elementName] = element;
                }
            }
        }

        /// <summary>
        /// Imports the namespaces.
        /// </summary>
        /// <param name="namespaces">The namespaces.</param>
        private void ImportNamespaces(IEnumerable<XmlQualifiedName> namespaces)
        {
            foreach (var @namespace in namespaces)
            {
                if (!_namespaceList.Contains(@namespace.Namespace))
                {
                    _namespaceList.Add(@namespace.Namespace);
                }
            }
        }

        /// <summary>
        /// Imports the namespaces.
        /// </summary>
        /// <param name="xsModel">The xs model.</param>
        private void ImportNamespaces(XmlSchema xsModel)
        {
            ImportNamespaces(xsModel.Namespaces.ToArray());
        }

        private void DetermineOptionalSimpleType(
            XmlSchema xsModel,
            XmlSchemaComplexType complexActualElement,
            out XmlSchemaSimpleType optionalSimpleType,
            out XmlQualifiedName optionalSimpleTypeName)
        {
            optionalSimpleType = null;
            optionalSimpleTypeName = null;

            // For complex types, the simple type information can be embedded as an extension
            // of the complex type.
            if (complexActualElement != null)
            {
                var contentModel = complexActualElement.ContentModel;
                if (contentModel is XmlSchemaSimpleContent)
                {
                    var simpleContentModel = (XmlSchemaSimpleContent)contentModel;
                    var simpleContentExtension = simpleContentModel.Content as XmlSchemaSimpleContentExtension;
                    if (simpleContentExtension != null)
                    {
                        var simpleContentBaseTypeName = simpleContentExtension.BaseTypeName;
                        if (!simpleContentBaseTypeName.IsEmpty)
                        {
                            var simpleType = ResolveSchemaType(xsModel, simpleContentBaseTypeName) as XmlSchemaSimpleType;
                            if (simpleType != null)
                            {
                                optionalSimpleType = simpleType;
                                optionalSimpleTypeName = simpleType.QualifiedName;
                            }
                        }
                    }
                }
            }
        }

        private SchemaElementComplex Process(
            XmlSchema xsModel,
            XmlQualifiedName complexElementName,
            XmlSchemaComplexType complexActualElement,
            bool isArray,
            ElementPathNode node)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug(
                    "Processing complex {0} {1} stack {2}",
                    complexElementName.Namespace,
                    complexElementName.Name,
                    node);
            }

            var attributes = new List<SchemaItemAttribute>();
            var simpleElements = new List<SchemaElementSimple>();
            var complexElements = new List<SchemaElementComplex>();

            XmlSchemaSimpleType optionalSimpleType = null;
            XmlQualifiedName optionalSimpleTypeName = null;

            DetermineOptionalSimpleType(
                xsModel,
                complexActualElement,
                out optionalSimpleType,
                out optionalSimpleTypeName
                );

            var complexElement = new SchemaElementComplex(
                complexElementName.Name,
                complexElementName.Namespace,
                attributes,
                complexElements,
                simpleElements,
                isArray,
                optionalSimpleType,
                optionalSimpleTypeName);

            // add attributes
            attributes.AddRange(GetAttributes(xsModel, complexActualElement));

            var complexParticles = GetContentModelParticles(
                xsModel, complexActualElement);
            complexElement = ProcessModelGroup(
                xsModel, complexParticles, simpleElements, complexElements, node,
                complexActualElement, complexElement);

            return complexElement;
        }

        internal static readonly SchemaItemAttribute[] EMPTY_SCHEMA_ATTRIBUTES = new SchemaItemAttribute[0];

        /// <summary>
        /// Gets the attributes from the complex type.  Searches embedded and extended
        /// content for additional attributes.
        /// </summary>
        /// <param name="xsModel">The xs model.</param>
        /// <param name="complexType">Type of the complex.</param>
        /// <returns></returns>
        internal IEnumerable<SchemaItemAttribute> GetAttributes(
            XmlSchema xsModel,
            XmlSchemaComplexType complexType)
        {
            if (complexType != null)
            {
                var attributes = GetAttributes(xsModel, complexType.Attributes);

                var contentModel = complexType.ContentModel;
                if (contentModel is XmlSchemaSimpleContent)
                {
                    var simpleContentModel = (XmlSchemaSimpleContent)contentModel;
                    var simpleContentExtension = simpleContentModel.Content as XmlSchemaSimpleContentExtension;
                    if (simpleContentExtension != null)
                    {
                        attributes = attributes.Concat(
                            GetAttributes(xsModel, simpleContentExtension.Attributes));
                    }
                }

                return attributes;
            }

            return EMPTY_SCHEMA_ATTRIBUTES;
        }

        /// <summary>
        /// Returns the attributes within a collection.  Most of the time, this will
        /// simply normalize the name and convert the structure into the resultant
        /// output type.
        /// </summary>
        /// <param name="xsModel">The xs model.</param>
        /// <param name="attributes">The attributes.</param>
        /// <returns></returns>
        internal IEnumerable<SchemaItemAttribute> GetAttributes(
            XmlSchema xsModel,
            XmlSchemaObjectCollection attributes)
        {
            foreach (var attr in attributes.Cast<XmlSchemaAttribute>())
            {
                var name = attr.QualifiedName;
                if (name.IsEmpty)
                {
                    if (attr.Form == XmlSchemaForm.Qualified)
                    {
                        name = new XmlQualifiedName(attr.Name, xsModel.TargetNamespace);
                    }
                    else
                    {
                        name = new XmlQualifiedName(attr.Name, null);
                    }
                }

                var schemaType = ResolveSchemaType(xsModel, attr.SchemaTypeName);
                var itemAttribute = new SchemaItemAttribute(
                    name.Namespace,
                    name.Name,
                    schemaType as XmlSchemaSimpleType,
                    attr.SchemaTypeName.Name);

                yield return itemAttribute;
            }
        }

        internal static readonly XmlSchemaObject[] EMPTY_SCHEMA_OBJECTS = new XmlSchemaObject[0];

        /// <summary>
        /// Gets the content model particles.
        /// </summary>
        /// <param name="xsModel">The xs model.</param>
        /// <param name="complexType">Type of the complex.</param>
        /// <returns></returns>
        internal IEnumerable<XmlSchemaObject> GetContentModelParticles(
            XmlSchema xsModel,
            XmlSchemaComplexType complexType)
        {
            if (complexType != null)
            {
                var complexGroupBase = complexType.Particle as XmlSchemaGroupBase;
                var contentModel = complexType.ContentModel;
                if (contentModel is XmlSchemaComplexContent)
                {
                    var complexContentExtension = contentModel.Content as XmlSchemaComplexContentExtension;
                    if (complexContentExtension != null)
                    {
                        return GetContentExtensionParticles(xsModel, complexContentExtension);
                    }
                }

                if (complexGroupBase != null)
                {
                    return complexGroupBase.Items.Cast<XmlSchemaObject>();
                }
            }

            return EMPTY_SCHEMA_OBJECTS;
        }

        /// <summary>
        /// Gets the content model particles associated with a content extension element.  These
        /// objects can be a little difficult because of the nesting of base types that occurs
        /// within them.  We use a cofunction to build an iterator that recursively up through
        /// the hierarchy.
        /// </summary>
        /// <param name="xsModel">The xs model.</param>
        /// <param name="contentExtension">The content extension.</param>
        /// <returns></returns>
        internal IEnumerable<XmlSchemaObject> GetContentExtensionParticles(
            XmlSchema xsModel,
            XmlSchemaComplexContentExtension contentExtension)
        {
            IEnumerable<XmlSchemaObject> result = EMPTY_SCHEMA_OBJECTS;

            var baseTypeName = contentExtension.BaseTypeName;
            if (baseTypeName.IsEmpty == false)
            {
                var baseType = ResolveSchemaType(xsModel, baseTypeName) as XmlSchemaComplexType;
                if (baseType != null)
                {
                    result = GetContentModelParticles(xsModel, baseType);
                }
            }

            var complexParticleGroup = contentExtension.Particle as XmlSchemaGroupBase;
            if (complexParticleGroup != null)
            {
                result = result.Concat(complexParticleGroup.Items.Cast<XmlSchemaObject>());
            }

            return result;
        }

        /// <summary>
        /// Processes the model group.
        /// </summary>
        /// <param name="xsModel">The schema model.</param>
        /// <param name="childParticles">The schema objects in this model group.</param>
        /// <param name="simpleElements">The simple elements.</param>
        /// <param name="complexElements">The complex elements.</param>
        /// <param name="node">The node.</param>
        /// <param name="complexActualElement">The complex actual element.</param>
        /// <param name="complexElement">The complex element.</param>
        /// <returns></returns>
        private SchemaElementComplex ProcessModelGroup(
            XmlSchema xsModel,
            IEnumerable<XmlSchemaObject> childParticles,
            IList<SchemaElementSimple> simpleElements,
            IList<SchemaElementComplex> complexElements,
            ElementPathNode node,
            XmlSchemaComplexType complexActualElement,
            SchemaElementComplex complexElement)
        {
            foreach (var childParticle in childParticles)
            {
                if (childParticle is XmlSchemaElement)
                {
                    var schemaElement = (XmlSchemaElement)childParticle;
                    var isArrayFlag = IsArray(schemaElement);

                    // the name for this element
                    XmlQualifiedName elementName;
                    // the type for this element ... this may take different paths
                    // depending upon how the type is provided to us.
                    XmlSchemaType schemaType;
                    XmlQualifiedName schemaTypeName;

                    if (schemaElement.RefName.IsEmpty)
                    {
                        elementName = schemaElement.QualifiedName;
                        if (Equals(elementName, XmlQualifiedName.Empty))
                            elementName = new XmlQualifiedName(
                                schemaElement.Name, xsModel.TargetNamespace);

                        schemaType = schemaElement.SchemaType;
                        schemaTypeName = schemaElement.SchemaTypeName;
                        if ((schemaType == null) && (!schemaTypeName.IsEmpty))
                        {
                            schemaType = ResolveSchemaType(xsModel, schemaTypeName);
                        }
                    }
                    else
                    {
                        // this element contains a reference to another element... the element will
                        // share the name of the reference and the type of the reference.  the reference
                        // type should be a complex type.
                        var referenceElement = ResolveElement(schemaElement.RefName);
                        var referenceElementType = referenceElement.SchemaType;

                        var elementNamespace = string.IsNullOrEmpty(schemaElement.RefName.Namespace)
                            ? xsModel.TargetNamespace
                            : schemaElement.RefName.Namespace;
                        elementName = new XmlQualifiedName(
                            schemaElement.RefName.Name, elementNamespace);

                        schemaType = referenceElementType;
                        schemaTypeName = referenceElement.SchemaTypeName;
                        // TODO
                    }

                    var simpleType = schemaType as XmlSchemaSimpleType;
                    if (simpleType != null)
                    {
                        var fractionDigits = GetFractionRestriction(simpleType);
                        var simpleElement = new SchemaElementSimple(
                            elementName.Name,
                            elementName.Namespace,
                            simpleType,
                            schemaTypeName.Name,
                            isArrayFlag,
                            fractionDigits);
                        simpleElements.Add(simpleElement);
                    }
                    else
                    {
                        var complexType = schemaType as XmlSchemaComplexType;
                        var newChild = node.AddChild(elementName);
                        if (newChild.DoesNameAlreadyExistInHierarchy())
                        {
                            continue;
                        }

                        complexActualElement = complexType;

                        SchemaElementComplex innerComplex = Process(
                            xsModel,
                            elementName,
                            complexActualElement,
                            isArrayFlag,
                            newChild
                        );

                        if (Log.IsDebugEnabled)
                        {
                            Log.Debug("Adding complex {0}", complexElement);
                        }
                        complexElements.Add(innerComplex);
                    }
                }

                ProcessModelGroup(
                    xsModel, childParticle, simpleElements, complexElements, node, complexActualElement,
                    complexElement);
            }

            return complexElement;
        }

        /// <summary>
        /// Processes the model group.
        /// </summary>
        /// <param name="xsModel">The schema model.</param>
        /// <param name="xsObject">The schema that represents the model group.</param>
        /// <param name="simpleElements">The simple elements.</param>
        /// <param name="complexElements">The complex elements.</param>
        /// <param name="node">The node.</param>
        /// <param name="complexActualElement">The complex actual element.</param>
        /// <param name="complexElement">The complex element.</param>
        /// <returns></returns>
        private SchemaElementComplex ProcessModelGroup(
            XmlSchema xsModel,
            XmlSchemaObject xsObject,
            IList<SchemaElementSimple> simpleElements,
            IList<SchemaElementComplex> complexElements,
            ElementPathNode node,
            XmlSchemaComplexType complexActualElement,
            SchemaElementComplex complexElement)
        {
            var xsGroup = xsObject as XmlSchemaGroupBase;
            if (xsGroup != null)
            {
                return ProcessModelGroup(
                    xsModel,
                    xsGroup.Items.Cast<XmlSchemaObject>(),
                    simpleElements,
                    complexElements,
                    node,
                    complexActualElement,
                    complexElement);
            }

            return complexElement;
        }

        private static bool IsArray(XmlSchemaElement element)
        {
            return element.MaxOccursString == "unbounded" || element.MaxOccurs > 1;
        }

        private static int? GetFractionRestriction(XmlSchemaSimpleType simpleType)
        {
            var simpleTypeRestriction = simpleType.Content as XmlSchemaSimpleTypeRestriction;
            if (simpleTypeRestriction != null)
            {
                foreach (XmlSchemaObject facet in simpleTypeRestriction.Facets)
                {
                }
            }
#if false


            if ((simpleType.getDefinedFacets() & XSSimpleType.FACET_FRACTIONDIGITS) != 0)
            {
                XSObjectList facets = simpleType.getFacets();
                Integer digits = null;
                for (int f = 0; f < facets.getLength(); f++)
                {
                    XSObject item = facets.item(f);
                    if (item
                    instanceof XSFacet)
                    {
                        XSFacet facet = (XSFacet) item;
                        if (facet.getFacetKind() == XSSimpleType.FACET_FRACTIONDIGITS)
                        {
                            try
                            {
                                digits = Integer.parseInt(facet.getLexicalFacetValue());
                            }
                            catch (Exception ex)
                            {
                                Log.warn(
                                    "Error parsing fraction facet value '" + facet.getLexicalFacetValue() + "' : " +
                                    ex.getMessage(), ex);
                            }
                        }
                    }
                }
                return digits;
            }
#endif
            return null;
        }
    }
}
