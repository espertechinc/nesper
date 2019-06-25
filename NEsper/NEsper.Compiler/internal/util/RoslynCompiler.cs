///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.compiler.@internal.util
{
    public class RoslynCompiler
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected internal static void Compile(CodegenClass clazz, IDictionary<string, byte[]> classes, bool withCodeLogging)
        {
            string code = CodegenClassGenerator.Compile(clazz);

#if false
            try
            {
                string optionalFileName = null;
                if (Boolean.GetBoolean(ICookable.SYSTEM_PROPERTY_SOURCE_DEBUGGING_ENABLE))
                {
                    string dirName = System.GetProperty(ICookable.SYSTEM_PROPERTY_SOURCE_DEBUGGING_DIR);
                    if (dirName == null)
                    {
                        dirName = System.GetProperty("java.io.tmpdir");
                    }
                    var file = new FileInfo(dirName, clazz.ClassName + ".java");
                    if (!file.Exists())
                    {
                        bool created = file.CreateNewFile();
                        if (!created)
                        {
                            throw new RuntimeException("Failed to created file '" + file + "'");
                        }
                    }

                    FileWriter writer = null;
                    try
                    {
                        writer = new FileWriter(file);
                        PrintWriter print = new PrintWriter(writer);
                        print.Write(code);
                        print.Close();
                    }
                    catch (IOException ex)
                    {
                        throw new RuntimeException("Failed to write to file '" + file + "'");
                    }
                    finally
                    {
                        if (writer != null)
                        {
                            writer.Close();
                        }
                    }

                    file.DeleteOnExit();
                    optionalFileName = file.AbsolutePath;
                }

                org.codehaus.janino.Scanner scanner = new Scanner(optionalFileName, new ByteArrayInputStream(
                        code.GetBytes("UTF-8")), "UTF-8");

                ByteArrayProvidingClassLoader cl = new ByteArrayProvidingClassLoader(classes);
                UnitCompiler unitCompiler = new UnitCompiler(
                        new Parser(scanner).ParseCompilationUnit(),
                        new ClassLoaderIClassLoader(cl));
                ClassFile[] classFiles = unitCompiler.CompileUnit(true, true, true);
                for (int i = 0; i < classFiles.Length; i++)
                {
                    classes.Put(classFiles[i].ThisClassName, classFiles[i].ToByteArray());
                }

                if (withCodeLogging)
                {
                    Log.Info("Code:\n" + CodeWithLineNum(code));
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to compile: " + ex.Message + "\ncode:" + CodeWithLineNum(code));
                throw new RuntimeException(ex);
            }
#else
            throw new NotImplementedException();
#endif
        }
    }
} // end of namespace