///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;

using NUnit.Framework.Internal;

using com.espertech.esper.client;

namespace NEsper.Tests.Runner
{
    /// <summary>
    /// This application is an nunit tester application.  It is primarily
    /// here for devs who want to run a profiler or code coverage tool over
    /// the unit tests but can't do so under nunit.  Additionally, some
    /// unit tests may fail under certain conditions (such as specific
    /// sequences in which tests are run).  This is here to help those
    /// make those types of problems easier to track down.
    /// </summary>

    class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args"></param>
        [STAThread]
        static void Main(string[] args)
        {
            //Common.Logging.LogManager.Adapter
            //log4net.Config.XmlConfigurator.Configure();

            ServiceManager.Services.AddService(new SettingsService());
            ServiceManager.Services.AddService(new DomainManager());
            ServiceManager.Services.AddService(new ProjectService());
            ServiceManager.Services.InitializeServices();

            TextReader inReader = Console.In;
            TextWriter outWriter = Console.Out;
            TextWriter errorWriter = Console.Error;

            EventListener testListener = new EventCollector(inReader, outWriter, errorWriter);
            
            try
            {
                var assembly = typeof(TestConfiguration).Assembly.Location;
                var framework = RuntimeFramework.CurrentFramework;

                var testPackage = new TestPackage(assembly);
                testPackage.TestName = null;
                testPackage.Settings["DomainUsage"] = DomainUsage.Single;
                testPackage.Settings["ProcessModel"] = ProcessModel.Single;
                testPackage.Settings["ShadowCopyFiles"] = false;
                testPackage.Settings["UseThreadedRunner"] = true;
                testPackage.Settings["DefaultTimeout"] = 0;
                testPackage.Settings["RuntimeFramework"] = framework;

                var testFilter = TestFilter.Empty;

                if (args.Length > 0)
                {
                    var nameFilter = new SimpleNameFilter();
                    nameFilter.Add(args[0]);
                    testFilter = nameFilter;
                    Console.WriteLine("Using SimpleNameFilter");
                }

                using (var testRunner = new DefaultTestRunnerFactory().MakeTestRunner(testPackage))
                {
                    testRunner.Load(testPackage);

                    Console.Error.WriteLine("{0}: Testing begins", DateTime.Now.Ticks);
                    testRunner.Run(testListener, testFilter, true, LoggingThreshold.Off);
                    Console.Error.WriteLine("{0}: Testing ends", DateTime.Now.Ticks);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(">> Error");
                Console.WriteLine(e);
            }

            //Console.ReadLine();
        }

        [Serializable]
        class EventCollector : EventListener
        {
            private readonly TextReader _inReader;
            private readonly TextWriter _outWriter;
            private TextWriter _errWriter;

            public EventCollector(TextReader inReader, TextWriter outWriter, TextWriter errWriter)
            {
                _inReader = inReader;
                _outWriter = outWriter;
                _errWriter = errWriter;
            }

            public void DisplayThreadStatistics()
            {
                ProcessThreadCollection threads = Process.GetCurrentProcess().Threads;
                _outWriter.WriteLine("ThreadCount: {0}", threads.Count);
                foreach (ProcessThread thread in threads)
                {
                    _outWriter.WriteLine("\tThread: {0,8} | {1,10} | {2,8} | {3,8} | {4,8}",
                                      thread.Id,
                                      thread.StartTime.Ticks,
                                      thread.PrivilegedProcessorTime,
                                      thread.UserProcessorTime,
                                      thread.TotalProcessorTime);
                }
            }

            public void RunStarted(string name, int testCount)
            {
                _outWriter.WriteLine("{0}: Run {1} started",
                    DateTime.Now.Ticks,
                    name);
            }

            public void RunFinished(TestResult result)
            {
                _outWriter.WriteLine("{0}: Run {1} finished",
                                  DateTime.Now.Ticks,
                                  result.Name);
            }

            public void RunFinished(Exception exception)
            {
                _outWriter.WriteLine("{0}: Run finished - {1}",
                                  DateTime.Now.Ticks,
                                  exception.Message);
            }

            public void TestStarted(TestName testName)
            {
                _outWriter.WriteLine("{0}: Test {1} started ... ",
                    DateTime.Now.Ticks, testName.FullName);
            }

            public void TestFinished(TestResult result)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                if (result.IsSuccess)
                {
                    _outWriter.WriteLine("{0}: Test {1} Passed In {2:F5}",
                        DateTime.Now.Ticks,
                        result.Name,
                        result.Time);
                    DisplayThreadStatistics();
                }
                else
                {
                    _outWriter.WriteLine("{0}: Test {1} Failed - {2:F5}",
                                          DateTime.Now.Ticks,
                                          result.Name,
                                          result.Time);

                    _outWriter.WriteLine("Failed");
                    _outWriter.WriteLine(result.Message);
                    _outWriter.WriteLine(result.StackTrace);
                    DisplayThreadStatistics();
                }

                if (result.HasResults)
                {
                    var subResults = result.Results;
                    foreach (var subResult in subResults)
                    {
                        _outWriter.WriteLine(subResult);
                    }
                }

                _outWriter.Flush();
            }

            public void TestOutput(TestOutput testOutput)
            {
            }

            public void SuiteStarted(TestName testName)
            {
                _outWriter.WriteLine("{0}: Suite started - {1}",
                                  DateTime.Now.Ticks,
                                  testName);
            }

            public void SuiteFinished(TestResult result)
            {
                _outWriter.WriteLine("{0}: Suite finished - {1}",
                     DateTime.Now.Ticks,
                     result.Name);

                //if (result.Name == "espertech")
                //{
                //    _outWriter.WriteLine("Waiting for input");
                //    _inReader.ReadLine();
                //}
            }

            public void UnhandledException(Exception exception)
            {
            }
        }
    }
}
