using NiceHashMiner.Configs;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NiceHashMinerLegacy.Extensions;

namespace NiceHashMiner.Miners
{
    /// <summary>
    /// Miner variant that gets speed values from logfile and must be terminated
    /// </summary>
    public abstract class MinerLogBench : Miner
    {
        protected int BenchmarkTimeWait = 120;

        protected string PrimaryLookForStart;
        protected string SecondaryLookForStart;

        protected double DevFee;

        protected MinerLogBench(string name) :
            base(name)
        { }

        /// <summary>
        /// Thread routine for miners that cannot be scheduled to stop and need speed data read from command line
        /// </summary>
        /// <param name="commandLine"></param>
        protected override void BenchmarkThreadRoutine(object commandLine)
        {
            CleanOldLogs();

            BenchmarkSignalQuit = false;
            BenchmarkSignalHanged = false;
            BenchmarkSignalFinnished = false;
            BenchmarkException = null;

            Thread.Sleep(ConfigManager.GeneralConfig.MinerRestartDelayMS);

            try
            {
                Helpers.ConsolePrint("BENCHMARK", "Benchmark starts");
                Helpers.ConsolePrint(MinerTag(), "Benchmark should end in : " + BenchmarkTimeWait + " seconds");
                BenchmarkHandle = BenchmarkStartProcess((string)commandLine);
                BenchmarkHandle.WaitForExit(BenchmarkTimeWait + 2);
                var benchmarkTimer = new Stopwatch();
                benchmarkTimer.Reset();
                benchmarkTimer.Start();
                //BenchmarkThreadRoutineStartSettup();
                // wait a little longer then the benchmark routine if exit false throw
                //var timeoutTime = BenchmarkTimeoutInSeconds(BenchmarkTimeInSeconds);
                //var exitSucces = BenchmarkHandle.WaitForExit(timeoutTime * 1000);
                // don't use wait for it breaks everything
                BenchmarkProcessStatus = BenchmarkProcessStatus.Running;
                var keepRunning = true;
                while (keepRunning && IsActiveProcess(BenchmarkHandle.Id))
                {
                    //string outdata = BenchmarkHandle.StandardOutput.ReadLine();
                    //BenchmarkOutputErrorDataReceivedImpl(outdata);
                    // terminate process situations
                    if (benchmarkTimer.Elapsed.TotalSeconds >= (BenchmarkTimeWait + 2)
                        || BenchmarkSignalQuit
                        || BenchmarkSignalFinnished
                        || BenchmarkSignalHanged
                        || BenchmarkSignalTimedout
                        || BenchmarkException != null)
                    {
                        var imageName = MinerExeName.Replace(".exe", "");
                        // maybe will have to KILL process
                        KillProspectorClaymoreMinerBase(imageName);
                        if (BenchmarkSignalTimedout)
                        {
                            throw new Exception("Benchmark timedout");
                        }

                        if (BenchmarkException != null)
                        {
                            throw BenchmarkException;
                        }

                        if (BenchmarkSignalQuit)
                        {
                            throw new Exception("Termined by user request");
                        }

                        if (BenchmarkSignalFinnished)
                        {
                            break;
                        }

                        keepRunning = false;
                        break;
                    }

                    // wait a second reduce CPU load
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                BenchmarkThreadRoutineCatch(ex);
            }
            finally
            {
                BenchmarkAlgorithm.BenchmarkSpeed = 0;
                // find latest log file
                var latestLogFile = "";
                var dirInfo = new DirectoryInfo(WorkingDirectory);
                foreach (var file in dirInfo.GetFiles(GetLogFileName()))
                {
                    latestLogFile = file.Name;
                    break;
                }

                BenchmarkHandle?.WaitForExit(10000);

                // read file log
                string[] lines;
                if (File.Exists(WorkingDirectory + latestLogFile))
                {
                    lines = File.ReadAllLines(WorkingDirectory + latestLogFile);
                    ProcessBenchLines(lines);
                }
                else
                {
                    lines = new string[0];
                }

                BenchmarkThreadRoutineFinish(lines);
            }
        }

        protected void CleanOldLogs()
        {
            // clean old logs
            try
            {
                var dirInfo = new DirectoryInfo(WorkingDirectory);
                var deleteContains = GetLogFileName();
                if (dirInfo.Exists)
                {
                    foreach (var file in dirInfo.GetFiles())
                    {
                        if (file.Name.Contains(deleteContains))
                        {
                            file.Delete();
                        }
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// When parallel benchmarking each device needs its own log files, so this uniquely identifies for the setup
        /// </summary>
        protected string GetDeviceID()
        {
            var ids = MiningSetup.MiningPairs.Select(x => x.Device.ID);
            var idStr = string.Join(",", ids);

            if (!IsMultiType) return idStr;

            // Miners that use multiple dev types need to also discriminate based on that
            var types = MiningSetup.MiningPairs.Select(x => (int)x.Device.DeviceType);
            return $"{string.Join(",", types)}-{idStr}";
        }

        protected string GetLogFileName()
        {
            return $"{GetDeviceID()}_log.txt";
        }

        protected virtual void ProcessBenchLines(IEnumerable<string> lines)
        {
            var primarySpeedSum = 0d;
            var primarySpeedCount = 0;
            var secondarySpeedSum = 0d;
            var secondarySpeedCount = 0;

            // Iterate over non-null lowercased lines
            foreach (var line in lines.Where(l => l != null).Select(l => l.ToLower()))
            {
                if (line.Contains(PrimaryLookForStart))
                {
                    if (!line.TryGetHashrateAfter(PrimaryLookForStart, out var primary))
                        continue;

                    primarySpeedSum += primary;
                    primarySpeedCount++;
                }
                else if (!string.IsNullOrWhiteSpace(SecondaryLookForStart) &&
                         line.Contains(SecondaryLookForStart))
                {
                    if (!line.TryGetHashrateAfter(SecondaryLookForStart, out var secondary))
                        continue;

                    secondarySpeedSum += secondary;
                    secondarySpeedCount++;
                }
            }

            var primarySpeed = primarySpeedCount > 0 ? primarySpeedSum / primarySpeedCount : 0;
            var secondarySpeed = secondarySpeedCount > 0 ? secondarySpeedSum / secondarySpeedCount : 0;

            ProcessBenchResults(primarySpeed * (100 - DevFee), secondarySpeed * (100 - DevFee));
        }

        protected virtual void ProcessBenchResults(double primarySpeed, double secondarySpeed)
        {
            BenchmarkAlgorithm.BenchmarkSpeed = primarySpeed;
        }

        protected override void BenchmarkOutputErrorDataReceived(object sender, DataReceivedEventArgs e)
        { }

        private static bool IsActiveProcess(int pid)
        {
            try
            {
                return Process.GetProcessById(pid) != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
