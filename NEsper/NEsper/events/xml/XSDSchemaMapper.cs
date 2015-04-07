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
using System.Net;
using System.Xml;
using System.Xml.Schema;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;


namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// Helper class for mapping a XSD schema model to an internal representation.
    /// </summary>
    public class XSDSchemaMapper
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Loads a schema from the provided Uri.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="schemaText">The schema text.</param>
        /// <returns></returns>
        public static XmlSchema LoadSchema(Uri uri, String schemaText)
        {
            if (uri == null) {
                var stringReader = new StringReader(schemaText);
                var schema = XmlSchema.Read(stringReader, null);
                if (schema == null) {
                    throw new ConfigurationException("Failed to read schema from schemaText");
                }

                return schema;
            }

            using (var client = new WebClient()) {
                using (var resourceStream = client.OpenRead(uri)) {
                    var schema = XmlSchema.Read(resourceStream, null);
                    if (schema == null) {
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
        /// <param name="maxRecusiveDepth">depth of maximal recursive element</param>
        /// <returns>model</returns>
        public static SchemaModel LoadAndMap(String schemaResource, String schemaText, int maxRecusiveDepth)
        {
            // Load schema
            try {
                var schemaLocation = string.IsNullOrEmpty(schemaResource) ? null : ResourceManager.ResolveResourceURL(schemaResource);
                var schema = LoadSchema(schemaLocation, schemaText);
                return Map(schema, schemaLocation, maxRecusiveDepth);
            }
            catch (ConfigurationException) {
                throw;
            }
            catch (Exception ex) {
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
        private readonly IList<string> _namesspaceList =
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
            if (schemaType == null) {
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
            if (schemaType == null) {
                schemaType = XmlSchemaSimpleType.GetBuiltInComplexType(name);
                if (schemaType == null) {
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
            mapper.Import(xsModel, schemaLocation, maxRecursiveDepth);
            return mapper.CreateModel();
        }

        /// <summary>
        /// Creates the model.
        /// </summary>
        /// <returns></returns>
        private SchemaModel CreateModel()
        {
            return new SchemaModel(_components, _namesspaceList);
        }

        /// <summary>
        /// Imports the specified schema.
        /// </summary>
        /// <param name="xsModel">The xs model.</param>
        /// <param name="schemaLocation">The schema location.</param>
        /// <param name="maxRecursiveDepth">The max recursive depth.</param>
        private void Import(XmlSchema xsModel, Uri schemaLocation, int maxRecursiveDepth)
        {
            // get namespaces
            var namespaces = xsModel.Namespaces;
            var namespacesArray = namespaces.ToArray();

            for (int i = 0; i < namespacesArray.Length; i++)
            {
                var @namespace = namespacesArray[i];
                if (! _namesspaceList.Contains(@namespace.Namespace)) {
                    _namesspaceList.Add(@namespace.Namespace);
                }
            }

            foreach (var include in xsModel.Includes) {
                var asImport = include as XmlSchemaImport;
                if (asImport != null) {
                    try {
                        String importLocation = asImport.SchemaLocation;
                        Uri importSchemaLocation;
                        if (Uri.IsWellFormedUriString(importLocation, UriKind.Absolute)) {
                            importSchemaLocation = new Uri(importLocation, UriKind.RelativeOrAbsolute);
                        }
                        else {
                            importSchemaLocation = new Uri(schemaLocation, importLocation);
                        }

                        var importSchema = LoadSchema(importSchemaLocation, null);
                        Import(importSchema,
                               importSchemaLocation,
                               maxRecursiveDepth - 1);
                    }
                    catch (ConfigurationException) {
                        throw;
                    }
                    catch (Exception ex) {
                        throw new ConfigurationException(
                            "Failed to read schema '" + asImport.SchemaLocation + "' : " + ex.Message, ex);
                    }
                }
            }

            // Organize all of the elements
            foreach (var oelement in xsModel.Items) {
                var element = oelement as XmlSchemaElement;
                if (element != null) {
                    XmlQualifiedName elementName = element.QualifiedName;
                    if (Equals(elementName, XmlQualifiedName.Empty)) {
                        elementName = new XmlQualifiedName(
                            element.Name,
                            xsModel.TargetNamespace);
                    }

                    _schemaElementDictionary[elementName] = element;
                }
            }

            // get top-level complex elements
            foreach (var oelement in xsModel.Items) {
                var complexType = oelement as XmlSchemaComplexType;
                if (complexType != null) {
                    var qualifiedName = complexType.QualifiedName;
                    if (Equals(qualifiedName, XmlQualifiedName.Empty)) {
                        qualifiedName = new XmlQualifiedName(
                            complexType.Name, xsModel.TargetNamespace);                        
                    }

                    _schemaTypeDictionary[qualifiedName] = complexType;
                }
            }

            foreach (var oelement in xsModel.Items) {
                var element = oelement as XmlSchemaElement;
                if (element == null)
                    continue;

                var schemaType = element.SchemaType;
                if (schemaType == null) {
                    var schemaTypeName = element.SchemaTypeName;
                    if (!Equals(schemaTypeName, XmlQualifiedName.Empty)) {
                        schemaType = ResolveSchemaType(xsModel, schemaTypeName);
                    }
                }

                if (!(schemaType is XmlSchemaComplexType)) {
                    continue;
                }

                var name = element.Name;
                var nameNamespaceStack = new Stack<NamespaceNamePair>();
                var namespaceArray = element.Namespaces.ToArray();
                var @namespace = namespaceArray.Length > 0 ? namespaceArray[0].Namespace : xsModel.TargetNamespace;
                var nameNamespace = new NamespaceNamePair(@namespace, name);
                nameNamespaceStack.Push(nameNamespace);

                if (Log.IsDebugEnabled) {
                    Log.Debug("Processing component " + @namespace + " " + name);
                }

                var complexElement = ProcessComplexElement(
                    xsModel,
                    name,
                    @namespace,
                    element,
                    (XmlSchemaComplexType) schemaType,
                    false,
                    nameNamespaceStack,
                    maxRecursiveDepth);

                if (Log.IsDebugEnabled) {
                    Log.Debug("Adding component " + @namespace + " " + name);
                }

                _components.Add(complexElement);
            }
        }

        /// <summary>
        /// Processes the complex element.
        /// </summary>
        /// <param name="xsModel">The schema model.</param>
        /// <param name="complexElementName">Name of the complex element.</param>
        /// <param name="complexElementNamespace">The complex element namespace.</param>
        /// <param name="complexActualElement">The complex actual element.</param>
        /// <param name="complexType">Type of the complex.</param>
        /// <param name="isArray">if set to <c>true</c> [is array].</param>
        /// <param name="nameNamespaceStack">The name namespace stack.</param>
        /// <param name="maxRecursiveDepth">The max recursive depth.</param>
        /// <returns></returns>
        private SchemaElementComplex ProcessComplexElement(XmlSchema xsModel,
                                                           String complexElementName,
                                                           String complexElementNamespace,
                                                           XmlSchemaElement complexActualElement,
                                                           XmlSchemaComplexType complexType,
                                                           bool isArray,
                                                           Stack<NamespaceNamePair> nameNamespaceStack,
                                                           int maxRecursiveDepth)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("Processing complex {0} {1} stack {2}",
                    complexElementNamespace,
                    complexElementName,
                    nameNamespaceStack);
            }

            // Obtain the actual complex schema type
            //var complexType = (XmlSchemaComplexType)complexActualElement.SchemaType;
            // Represents the mapping of element attributes
            var attributes = new List<SchemaItemAttribute>();
            // Represents the mapping of child elements that are simple
            var simpleElements = new List<SchemaElementSimple>();
            // Represents the mapping of child elements that are complex
            var complexElements = new List<SchemaElementComplex>();
            // Represents the complex element - the above are encapsulated in the structure
            var complexElement = new SchemaElementComplex(
                complexElementName,
                complexElementNamespace,
                attributes,
                complexElements,
                simpleElements,
                isArray);

            // Map the schema attributes into internal form
            if (complexType != null) {
                var attrs = complexType.Attributes;
                foreach (var uattr in attrs) {
                    var attr = (XmlSchemaAttribute) uattr;
                    var name = attr.QualifiedName;
                    if (Equals(name, XmlQualifiedName.Empty))
                    {
                        name = new XmlQualifiedName(attr.Name, null);
                    }

                    var schemaType = ResolveSchemaType(xsModel, attr.SchemaTypeName);

                    var itemAttribute = new SchemaItemAttribute(
                        name.Namespace,
                        name.Name,
                        schemaType as XmlSchemaSimpleType,
                        attr.SchemaTypeName.Name);
                    attributes.Add(itemAttribute);
                }

                var contentModel = complexType.ContentModel;
                if (contentModel is XmlSchemaSimpleContent)
                {
                    XmlSchemaSimpleContent simpleContent = (XmlSchemaSimpleContent)contentModel;
                    if (simpleContent.Content is XmlSchemaSimpleContentExtension)
                    {
                        XmlSchemaSimpleContentExtension extension = (XmlSchemaSimpleContentExtension)simpleContent.Content;
                        foreach (var eattr in extension.Attributes)
                        {
                            if (eattr is XmlSchemaAttribute)
                            {
                                XmlSchemaAttribute sattr = (XmlSchemaAttribute)eattr;
                                XmlQualifiedName sqname = sattr.QualifiedName;
                                if (Equals(sqname, XmlQualifiedName.Empty)) {
                                    sqname = new XmlQualifiedName(sattr.Name, xsModel.TargetNamespace);
                                }

                                XmlSchemaSimpleType simpleType = ResolveSchemaType(xsModel, sattr.SchemaTypeName) as XmlSchemaSimpleType;
                                SchemaItemAttribute itemAttribute = new SchemaItemAttribute(
                                    sqname.Namespace, 
                                    sqname.Name,
                                    simpleType,
                                    sattr.SchemaTypeName.Name);
                                attributes.Add(itemAttribute);
                            }
                            else if (eattr is XmlSchemaAttributeGroupRef)
                            {
                            }
                        }

                        XmlQualifiedName optionalSimpleTypeName = extension.BaseTypeName;
                        if (!Equals(optionalSimpleTypeName, XmlQualifiedName.Empty))
                        {
                            XmlSchemaSimpleType optionalSimpleType = XmlSchemaSimpleType.GetBuiltInSimpleType(optionalSimpleTypeName);
                            complexElement.OptionalSimpleType = optionalSimpleType;
                            complexElement.OptionalSimpleTypeName = optionalSimpleTypeName;
                        }
                    }
                }

                var complexParticle = complexType.Particle;
                if (complexParticle is XmlSchemaGroupBase)
                {
                    XmlSchemaGroupBase particleGroup = (XmlSchemaGroupBase) complexParticle;
                    foreach (var artifact in particleGroup.Items) {
                        XmlSchemaElement myComplexElement = null;
                        XmlQualifiedName myComplexElementName = null;

                        if (artifact is XmlSchemaElement) {
                            var schemaElement = (XmlSchemaElement) artifact;
                            var isArrayFlag = IsArray(schemaElement);
                            var refName = schemaElement.RefName;

                            // Resolve complex elements that are a child of the sequence.  Complex
                            // elements come in one of two forms... the first is through reference
                            // the second is a direct child.  Of course you can have simple types
                            // too.

                            if (Equals(refName, XmlQualifiedName.Empty)) {
                                var schemaTypeName = schemaElement.SchemaTypeName;
                                if (!Equals(schemaTypeName, XmlQualifiedName.Empty))
                                {
                                    var schemaType = ResolveSchemaType(xsModel, schemaTypeName);
                                    if (schemaType is XmlSchemaSimpleType)
                                    {
                                        var simpleElementName = schemaElement.QualifiedName;
                                        if (Equals(simpleElementName, XmlQualifiedName.Empty))
                                            simpleElementName = new XmlQualifiedName(schemaElement.Name, xsModel.TargetNamespace);

                                        var fractionDigits = GetFractionRestriction((XmlSchemaSimpleType) schemaType);
                                        var simpleElement = new SchemaElementSimple(
                                            simpleElementName.Name,
                                            simpleElementName.Namespace,
                                            (XmlSchemaSimpleType) schemaType,
                                            schemaTypeName.Name,
                                            isArrayFlag,
                                            fractionDigits);
                                        simpleElements.Add(simpleElement);
                                    } else {
                                        myComplexElement = schemaElement;
                                        myComplexElement.SchemaType = schemaType;
                                        myComplexElementName = schemaElement.QualifiedName;
                                        if (Equals(myComplexElementName, XmlQualifiedName.Empty))
                                        {
                                            myComplexElementName = new XmlQualifiedName(
                                                myComplexElement.Name, xsModel.TargetNamespace);
                                        }
                                    }
                                } else {
                                    myComplexElement = schemaElement;
                                    myComplexElementName =
                                        !Equals(schemaElement.QualifiedName, XmlQualifiedName.Empty)
                                            ? schemaElement.QualifiedName
                                            : new XmlQualifiedName(schemaElement.Name, xsModel.TargetNamespace);
                                }
                            } else {
                                myComplexElement = ResolveElement(refName);
                                myComplexElementName = refName;
                                if (myComplexElementName.Namespace == null) {
                                    myComplexElementName = new XmlQualifiedName(refName.Name, xsModel.TargetNamespace);
                                }
                            }

                            if (myComplexElement != null)
                            {
                                if (myComplexElement.SchemaType == null) {
                                    if (!Equals(myComplexElement.SchemaTypeName, XmlQualifiedName.Empty)) {
                                        myComplexElement.SchemaType = ResolveSchemaType(xsModel, myComplexElement.SchemaTypeName);
                                    }
                                }

                                if (myComplexElement.SchemaType is XmlSchemaSimpleType)
                                {
                                    XmlSchemaSimpleType simpleSchemaType = (XmlSchemaSimpleType) myComplexElement.SchemaType;
                                    SchemaElementSimple innerSimple =
                                        new SchemaElementSimple(
                                            myComplexElementName.Name,
                                            myComplexElementName.Namespace,
                                            simpleSchemaType,
                                            simpleSchemaType.Name,
                                            isArrayFlag,
                                            GetFractionRestriction(simpleSchemaType));

                                    if (Log.IsDebugEnabled)
                                    {
                                        Log.Debug("Adding simple " + innerSimple);
                                    }

                                    simpleElements.Add(innerSimple);
                                }
                                else
                                {
                                    // Reference to complex type
                                    NamespaceNamePair nameNamespace = new NamespaceNamePair(
                                        myComplexElementName.Namespace,
                                        myComplexElementName.Name);
                                    nameNamespaceStack.Push(nameNamespace);

                                    // if the stack contains
                                    if (maxRecursiveDepth != Int32.MaxValue) {
                                        int containsCount = 0;
                                        foreach (NamespaceNamePair pair in nameNamespaceStack) {
                                            if (Equals(nameNamespace, pair)) {
                                                containsCount++;
                                            }
                                        }

                                        if (containsCount >= maxRecursiveDepth) {
                                            continue;
                                        }
                                    }

                                    SchemaElementComplex innerComplex = ProcessComplexElement(
                                        xsModel,
                                        myComplexElementName.Name,
                                        myComplexElementName.Namespace,
                                        myComplexElement,
                                        (XmlSchemaComplexType) myComplexElement.SchemaType,
                                        isArrayFlag,
                                        nameNamespaceStack,
                                        maxRecursiveDepth);

                                    nameNamespaceStack.Pop();

                                    if (Log.IsDebugEnabled)
                                    {
                                        Log.Debug("Adding complex " + complexElement);
                                    }

                                    complexElements.Add(innerComplex);
                                }
                            }
                        }
                    }
                }
                else if (complexParticle is XmlSchemaGroupRef) {
                    var groupRefParticle = (XmlSchemaGroupRef) complexParticle;
                }
            }

            return complexElement;
        }

        private static bool IsArray(XmlSchemaElement element)
        {
            return element.MaxOccursString == "unbounded" || element.MaxOccurs > 1;
        }

        private static bool IsArray(XmlSchemaParticle particle)
        {
            return particle.MaxOccursString == "unbounded" || particle.MaxOccurs > 1; 
        }

        private static int? GetFractionRestriction(XmlSchemaSimpleType simpleType)
        {
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
                            catch (RuntimeException ex)
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
