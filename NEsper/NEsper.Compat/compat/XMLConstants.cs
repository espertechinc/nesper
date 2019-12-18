///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


namespace com.espertech.esper.compat
{
    /// <summary>
    /// Contains constants for XML processing.
    /// </summary>

    public class XMLConstants
    {
        /// <summary>
        /// Namespace URI to use to represent that there is no Namespace.
        /// <para/>
        /// Defined by the Namespace specification to be "".
        /// <para/>
        /// <a href="http://www.w3.org/TR/REC-xml-names/#defaulting">Namespaces in XML, 5.2 Namespace Defaulting</a>
        /// </summary>
        public const string NULL_NS_URI = "";

        /// <summary>
        /// Prefix to use to represent the default XML Namespace.
        /// <para/>
        /// Defined by the XML specification to be "".
        /// <para/>
        /// <a href="http://www.w3.org/TR/REC-xml-names/#ns-qualnames"> Namespaces in XML, 3. Qualified Names</a>
        /// </summary>
        public const string DEFAULT_NS_PREFIX = "";

        /// <summary>
        /// The official XML Namespace name URI.
        /// <para/>
        /// Defined by the XML specification to be
        /// <code>http://www.w3.org/XML/1998/namespace</code>.
        /// <para/>
        /// <a href="http://www.w3.org/TR/REC-xml-names/#ns-qualnames"> Namespaces in XML, 3. Qualified Names</a>
        /// </summary>
        public const string XML_NS_URI =
            "http://www.w3.org/XML/1998/namespace";

        /// <summary>
        /// The official XML Namespace prefix.
        /// <para/>
        /// Defined by the XML specification to be <code>xml</code>.
        /// <para/>
        /// <a href="http://www.w3.org/TR/REC-xml-names/#ns-qualnames"> Namespaces in XML, 3. Qualified Names</a>
        /// </summary>
        public const string XML_NS_PREFIX = "xml";

        /// <summary>
        /// The official XML attribute used for specifying XML Namespace declarations,
        /// <see cref="XMLNS_ATTRIBUTE"/>, Namespace name URI.
        /// <para/>
        /// Defined by the XML specification to be
        /// <code>http://www.w3.org/2000/xmlns/</code>.
        /// <a href="http://www.w3.org/TR/REC-xml-names/#ns-qualnames"> Namespaces in XML, 3. Qualified Names</a>
        /// <a href="http://www.w3.org/XML/xml-names-19990114-errata/"> Namespaces in XML Errata</a>
        /// </summary>
        public const string XMLNS_ATTRIBUTE_NS_URI =
            "http://www.w3.org/2000/xmlns/";

        /// <summary>
        /// The official XML attribute used for specifying XML Namespace declarations.
        /// <para/>
        /// It is <strong><em>NOT</em></strong> valid to use as a prefix.  Defined by the
        /// XML specification to be <code>xmlns</code>.
        /// <para/>
        /// <a href="http://www.w3.org/TR/REC-xml-names/#ns-qualnames"> Namespaces in XML, 3. Qualified Names</a>
        /// </summary>
        public const string XMLNS_ATTRIBUTE = "xmlns";

        /// <summary>
        /// W3C XML Schema Namespace URI.
        /// <para/>
        /// Defined to be <code>http://www.w3.org/2001/XMLSchema</code>.
        /// <para/>
        /// <a href="http://www.w3.org/TR/xmlschema-1/#Instance_Document_Constructions"> XML Schema Part 1: Structures, 2.6 Schema-Related Markup in Documents Being Validated</a>
        /// </summary>
        public const string W3C_XML_SCHEMA_NS_URI = "http://www.w3.org/2001/XMLSchema";

        /// <summary>
        /// W3C XML Schema Instance Namespace URI.
        /// <para/>
        /// Defined to be <code>http://www.w3.org/2001/XMLSchema-instance</code>.
        /// <para/>
        /// <a href="http://www.w3.org/TR/xmlschema-1/#Instance_Document_Constructions"> XML Schema Part 1: Structures, 2.6 Schema-Related Markup in Documents Being Validated</a>
        /// </summary>
        public const string W3C_XML_SCHEMA_INSTANCE_NS_URI =
            "http://www.w3.org/2001/XMLSchema-instance";

        /// <summary>
        /// W3C XPath Datatype Namespace URI.
        /// <para/>
        /// Defined to be "<code>http://www.w3.org/2003/11/xpath-datatypes</code>".
        /// <para/>
        /// <a href="http://www.w3.org/TR/xpath-datamodel">XQuery 1.0 and XPath 2.0 Data Model</a>
        /// </summary>
        public const string W3C_XPATH_DATATYPE_NS_URI = "http://www.w3.org/2003/11/xpath-datatypes";

        /// <summary>
        /// XML Document Type Declaration Namespace URI as an arbitrary value.
        /// <para/>
        /// Since not formally defined by any existing standard, arbitrarily define to be
        /// <code>http://www.w3.org/TR/REC-xml</code>.
        /// <para/>
        /// </summary>
        public const string XML_DTD_NS_URI = "http://www.w3.org/TR/REC-xml";

        /// <summary>
        /// RELAX NG Namespace URI.
        /// <para/>
        /// Defined to be <code>http://relaxng.org/ns/structure/1.0</code>.
        /// <para/>
        /// <a href="http://relaxng.org/spec-20011203.html">RELAX NG Specification</a>
        /// </summary>
        public const string RELAXNG_NS_URI = "http://relaxng.org/ns/structure/1.0";

        /// <summary>
        /// Feature for secure processing. 
        /// <list>
        /// <item>
        /// <code>true</code> instructs the
        /// implementation to process XML securely. This may set limits on XML constructs to
        /// avoid conditions such as denial of service attacks.
        /// </item>
        /// <item>
        /// <code>false</code>
        /// instructs the implementation to process XML acording the letter of the XML
        /// specifications ingoring security issues such as limits on XML constructs to avoid
        /// conditions such as denial of service attacks.
        /// </item>
        /// </list>
        /// </summary>
        public const string FEATURE_SECURE_PROCESSING = "http://javax.xml.XMLConstants/feature/secure-processing";
    }
}
