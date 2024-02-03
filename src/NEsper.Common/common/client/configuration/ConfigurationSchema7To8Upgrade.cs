///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.configuration
{
    /// <summary>
    ///     Helper to migrate schema version 7 XML configuration to schema version 8 XML configuration.
    /// </summary>
    public class ConfigurationSchema7To8Upgrade
    {
        /// <summary>
        ///     Convert a schema from the input stream
        /// </summary>
        /// <param name="inputStream">input stream for XML document text</param>
        /// <param name="name">for information purposes to name the document passed in</param>
        /// <returns>converted XML text</returns>
        /// <throws>ConfigurationException if the conversion failed</throws>
        public static string Upgrade(
            Stream inputStream,
            string name)
        {
            var document = ConfigurationParser.GetDocument(inputStream, name);

            try {
                UpgradeInternal(document);

                var result = PrettyPrint(document);
                result = result.RegexReplaceAll("esper-configuration-\\d-\\d.xsd", "esper-configuration-8.0.xsd");
                return result;
            }
            catch (Exception t) {
                throw new ConfigurationException("Failed to transform document " + name + ": " + t.Message, t);
            }
        }

        private static void UpgradeInternal(XmlDocument document)
        {
            var top = document.DocumentElement;
            if (top.Name != "esper-configuration") {
                throw new ConfigurationException("Expected root node 'esper-configuration'");
            }

            TrimWhitespace(top);

            var common = CreateIfNotFound("common", top, document);
            var compiler = CreateIfNotFound("compiler", top, document);
            var runtime = CreateIfNotFound("runtime", top, document);

            RemoveNodes("revision-event-type", top);
            RemoveNodes("plugin-event-representation", top);
            RemoveNodes("plugin-event-type", top);
            RemoveNodes("plugin-event-type-name-resolution", top);
            RemoveNodes("bytecodegen", top);

            MoveNodes("event-type-auto-name", top, common);
            MoveNodes("event-type", top, common);
            MoveNodes("variant-stream", top, common);
            MoveNodes("auto-import", top, common);
            MoveNodes("auto-import-annotations", top, common);
            MoveNodes("method-reference", top, common);
            MoveNodes("database-reference", top, common);
            MoveNodes("variable", top, common);

            var views = MoveNodes("plugin-view", top, compiler);
            var vdw = MoveNodes("plugin-virtualdw", top, compiler);
            var aggs = MoveNodes("plugin-aggregation-function", top, compiler);
            var aggsMF = MoveNodes("plugin-aggregation-multifunction", top, compiler);
            MoveNodes("plugin-singlerow-function", top, compiler);
            var guards = MoveNodes("plugin-pattern-guard", top, compiler);
            var observers = MoveNodes("plugin-pattern-observer", top, compiler);
            UpdateAttributeName("factory-class", "forge-class", views, vdw, aggs, aggsMF, guards, observers);

            MoveNodes("plugin-loader", top, runtime);

            HandleSettings(top, common, compiler, runtime);
        }

        private static void UpdateAttributeName(
            string oldName,
            string newName,
            params IList<XmlNode>[] nodes)
        {
            foreach (var list in nodes) {
                foreach (var node in list) {
                    var element = (XmlElement)node;
                    var value = element.GetAttribute(oldName);
                    if (value == null) {
                        continue;
                    }

                    element.RemoveAttribute(oldName);
                    element.SetAttribute(newName, value);
                }
            }
        }

        private static void HandleSettings(
            XmlElement top,
            XmlElement common,
            XmlElement compiler,
            XmlElement runtime)
        {
            var settings = FindNode("engine-settings", top);
            if (settings == null) {
                return;
            }

            settings.ParentNode.RemoveChild(settings);
            var defaults = FindNode("defaults", settings);
            defaults.ParentNode.RemoveChild(defaults);

            var enumerator = DOMElementEnumerator.Create(defaults.ChildNodes);
            while (enumerator.MoveNext()) {
                var element = enumerator.Current;
                var nodeName = element.Name;
                switch (nodeName) {
                    case "event-meta":
                        MoveChild(element, common);
                        break;

                    case "view-resources":
                        RemoveNodes("share-views", element);
                        RemoveNodes("allow-multiple-expiry-policy", element);
                        MoveChild(element, compiler);
                        break;

                    case "logging":
                        element.ParentNode.RemoveChild(element);
                        CloneMove(element, "query-plan,jdbc", common);
                        CloneMove(element, "code", compiler);
                        CloneMove(element, "execution-path,timer-debug,audit", runtime);
                        break;

                    case "stream-selection":
                    case "language":
                    case "scripts":
                        MoveChild(element, compiler);
                        break;

                    case "time-source":
                        element.ParentNode.RemoveChild(element);
                        CloneMove(element, "time-unit", common);
                        CloneMove(element, "time-source-type", runtime);
                        break;

                    case "expression":
                        element.ParentNode.RemoveChild(element);
                        var compilerExpr = CloneMove(element, "", compiler);
                        var runtimeExpr = CloneMove(element, "", runtime);
                        RemoveAttributes(compilerExpr, "self-subselect-preeval,time-zone");
                        RemoveAttributesAllBut(runtimeExpr, "self-subselect-preeval,time-zone");
                        break;

                    case "execution":
                        element.ParentNode.RemoveChild(element);
                        RemoveAttributes(element, "allow-isolated-service");
                        var commonExec = CloneMove(element, "", common);
                        var compilerExec = CloneMove(element, "", compiler);
                        var runtimeExec = CloneMove(element, "", runtime);
                        RemoveAttributesAllBut(commonExec, "threading-profile");
                        RemoveAttributesAllBut(compilerExec, "filter-service-max-filter-width");
                        RemoveAttributes(runtimeExec, "filter-service-max-filter-width,threading-profile");
                        break;

                    case "patterns":
                    case "match-recognize":
                    case "metrics-reporting":
                    case "exceptionHandling":
                    case "variables":
                    case "threading":
                    case "conditionHandling":
                        if (nodeName == "threading") {
                            RenameAttribute(element, "engine-fairlock", "runtime-fairlock");
                        }

                        if (nodeName == "metrics-reporting") {
                            RenameAttribute(element, "engine-interval", "runtime-interval");
                            RenameAttribute(element, "engine-metrics", "runtime-metrics");
                        }

                        MoveChild(element, runtime);
                        break;
                }
            }
        }

        private static void RenameAttribute(
            XmlElement element,
            string oldName,
            string newName)
        {
            var value = element.GetAttribute(oldName);
            if (string.IsNullOrEmpty(value)) {
                return;
            }

            element.RemoveAttribute(oldName);
            element.SetAttribute(newName, value);
        }

        private static void RemoveAttributesAllBut(
            XmlElement element,
            string allButCSV)
        {
            var names = ToSet(allButCSV);
            var attributes = element.Attributes;
            IList<string> removed = new List<string>();
            for (var i = 0; i < attributes.Count; i++) {
                var node = attributes.Item(i);
                if (!names.Contains(node.Name)) {
                    removed.Add(node.Name);
                }
            }

            foreach (var remove in removed) {
                attributes.RemoveNamedItem(remove);
            }
        }

        private static XmlElement CloneMove(
            XmlElement cloned,
            string allowedCSV,
            XmlElement target)
        {
            var clone = (XmlElement)cloned.CloneNode(true);
            var appended = (XmlElement)target.AppendChild(clone);
            RemoveNodesBut(allowedCSV, appended);
            return clone;
        }

        private static void MoveChild(
            XmlElement element,
            XmlElement newParent)
        {
            element.ParentNode.RemoveChild(element);
            newParent.AppendChild(element);
        }

        private static void RemoveAttributes(
            XmlElement element,
            string namesCSV)
        {
            var names = ToSet(namesCSV);
            foreach (var name in names) {
                var value = element.GetAttribute(name);
                if (value != null) {
                    element.RemoveAttribute(name);
                }
            }
        }

        private static IList<XmlNode> MoveNodes(
            string name,
            XmlElement from,
            XmlElement to)
        {
            var nodes = from.ChildNodes;

            IList<XmlNode> moved = new List<XmlNode>();
            for (var i = 0; i < nodes.Count; i++) {
                var node = nodes.Item(i);
                if (node.Name.Equals(name)) {
                    from.RemoveChild(node);
                    to.AppendChild(node);
                    moved.Add(node);
                }
            }

            return moved;
        }

        private static void RemoveNodes(
            string name,
            XmlElement parent)
        {
            var nodes = parent.ChildNodes;

            for (var i = 0; i < nodes.Count; i++) {
                var node = nodes.Item(i);
                if (node.Name.Equals(name)) {
                    parent.RemoveChild(node);
                }
            }
        }

        private static void RemoveNodesBut(
            string allowedCSV,
            XmlElement parent)
        {
            var allowed = ToSet(allowedCSV);
            var nodes = parent.ChildNodes;

            IList<XmlNode> toRemove = new List<XmlNode>();
            for (var i = 0; i < nodes.Count; i++) {
                var node = nodes.Item(i);
                var name = node.Name;
                if (!allowed.Contains(name)) {
                    toRemove.Add(node);
                }
            }

            foreach (var node in toRemove) {
                parent.RemoveChild(node);
            }
        }

        private static XmlElement CreateIfNotFound(
            string name,
            XmlElement parent,
            XmlDocument document)
        {
            var found = FindNode(name, parent);
            if (found != null) {
                return found;
            }

            var element = document.CreateElement(name);
            parent.AppendChild(element);
            return element;
        }

        private static XmlElement FindNode(
            string name,
            XmlElement parent)
        {
            var nodes = parent.ChildNodes;
            for (var i = 0; i < nodes.Count; i++) {
                var node = nodes.Item(i);
                if (node.Name.Equals(name)) {
                    if (!(node is XmlElement element)) {
                        throw new ConfigurationException("Unexpected non-element for name '" + name + "'");
                    }

                    return element;
                }
            }

            return null;
        }

        private static string PrettyPrint(XmlDocument document)
        {
            try {
                var stringWriter = new StringWriter();
                var xmlTextWriter = new XmlTextWriter(stringWriter);
                xmlTextWriter.Formatting = Formatting.Indented;
                xmlTextWriter.IndentChar = ' ';
                xmlTextWriter.Indentation = 4;

                document.WriteContentTo(xmlTextWriter);
                xmlTextWriter.Flush();

                return stringWriter.ToString();
            }
            catch (Exception t) {
                throw new ConfigurationException("Failed to pretty-print document: " + t.Message, t);
            }
        }

        private static void TrimWhitespace(XmlNode node)
        {
            var children = node.ChildNodes;
            for (var i = 0; i < children.Count; ++i) {
                var child = children.Item(i);
                if (child is XmlText xmlTextNode) {
                    xmlTextNode.Value = xmlTextNode.Value.Trim();
                }

                TrimWhitespace(child);
            }
        }

        private static ISet<string> ToSet(string csv)
        {
            return new HashSet<string>(csv.SplitCsv());
        }
    }
} // end of namespace