///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace com.espertech.esper.compat.threading.locks
{
    public class TelemetryEngine
    {
        private readonly IDictionary<string, TelemetryLockCategory> _categoryDictionary =
            new Dictionary<string, TelemetryLockCategory>();

        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <value>The categories.</value>
        public IEnumerable<TelemetryLockCategory> Categories => _categoryDictionary.Values;

        /// <summary>
        /// Gets the category dictionary.
        /// </summary>
        /// <value>The category dictionary.</value>
        public IDictionary<string, TelemetryLockCategory> CategoryDictionary => _categoryDictionary;

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public TelemetryLockCategory GetCategory(string name)
        {
            TelemetryLockCategory lockCategory;
            if (!_categoryDictionary.TryGetValue(name, out lockCategory)) {
                lockCategory = new TelemetryLockCategory(name);
                _categoryDictionary[name] = lockCategory;
            }

            return lockCategory;
        }

        /// <summary>
        /// Dumps telemetry information to a textWriter.
        /// </summary>
        /// <param name="textWriter">The text writer.</param>
        public void DumpTo(TextWriter textWriter)
        {
            var xmlWriterSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
            var xmlWriter = XmlWriter.Create(textWriter, xmlWriterSettings);

            xmlWriter.WriteStartDocument(true);
            xmlWriter.WriteStartElement("telemetry");

            // find all events that belong together - blending category if
            // necessary

            foreach (var category in _categoryDictionary.Values)
            {
                xmlWriter.WriteStartElement("category");
                xmlWriter.WriteAttributeString("name", category.Name);

                foreach (var tEvent in category.Events) {
                    long tta = tEvent.AcquireTime - tEvent.RequestTime;
                    long ttp = tEvent.ReleaseTime - tEvent.AcquireTime;

                    xmlWriter.WriteStartElement("event");
                    xmlWriter.WriteAttributeString("id", tEvent.Id);
                    xmlWriter.WriteAttributeString("request", tEvent.RequestTime.ToString());
                    xmlWriter.WriteAttributeString("acquire", tEvent.AcquireTime.ToString());
                    xmlWriter.WriteAttributeString("release", tEvent.ReleaseTime.ToString());
                    xmlWriter.WriteAttributeString("tta", tta.ToString());
                    xmlWriter.WriteAttributeString("ttp", ttp.ToString());
                    xmlWriter.WriteStartElement("stack");
                    xmlWriter.WriteString(tEvent.StackTrace.ToString());
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Flush();
        }

        /// <summary>
        /// Dumps telemetry information to file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public void DumpToFile(string filename)
        {
            using(var writer = File.CreateText(filename)) {
                DumpTo(writer);
                writer.Flush();
                writer.Close();
            }
        }
    }
}
