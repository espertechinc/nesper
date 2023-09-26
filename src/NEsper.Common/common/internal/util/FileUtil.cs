///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.common.@internal.util
{
    public class FileUtil
    {
        public static string ReadTextFile(string file)
        {
            return File.ReadAllText(file);
        }

        public static string LinesToText(IList<string> lines)
        {
            var writer = new StringWriter();
            foreach (var line in lines) {
                writer.Write(line);
                writer.Write(Environment.NewLine);
            }

            return writer.ToString();
        }

        public static IList<string> ReadFile(TextReader reader)
        {
            var list = new List<string>();

            string text;
            // repeat until all lines is read
            while ((text = reader.ReadLine()) != null) {
                list.Add(text);
            }

            return list;
        }
    }
} // end of namespace