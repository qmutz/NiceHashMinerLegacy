using NiceHashMiner.Configs;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NiceHashMiner.Interfaces;

namespace NiceHashMiner.Miners
{
    /// <summary>
    /// Miner variant that gets speed values from standard output.
    /// </summary>
    public abstract class MinerStandardBench : Miner
    {
        private Stopwatch _benchmarkTimeOutStopWatch;
        private readonly List<string> _benchLines = new List<string>();
        private CancellationToken _cancelToken;

        protected Action OnNotExitAction;
        protected int BenchWaitFactor = 1;

        protected MinerStandardBench(string name) :
            base(name)
        { }

        protected override async Task<(bool, string)> BenchmarkThreadRoutine(string commandLine, CancellationToken cancelToken)
        {
            BenchmarkSignalHanged = false;
            BenchmarkSignalFinnished = false;
            BenchmarkException = null;
            _cancelToken = cancelToken;
            
            await Task.Delay(ConfigManager.GeneralConfig.MinerRestartDelayMS * BenchWaitFactor, cancelToken);

            try
            {
                Helpers.ConsolePrint("BENCHMARK", "Benchmark starts");
                BenchmarkHandle = BenchmarkStartProcess(commandLine);

                BenchmarkThreadRoutineStartSettup();
                // wait a little longer then the benchmark routine if exit false throw
                //var timeoutTime = BenchmarkTimeoutInSeconds(BenchmarkTimeInSeconds);
                //var exitSucces = BenchmarkHandle.WaitForExit(timeoutTime * 1000);
                // don't use wait for it breaks everything
                BenchmarkProcessStatus = BenchmarkProcessStatus.Running;
                
                var exited = await Task.Run(() => 
                        BenchmarkHandle.WaitForExit((BenchmarkTimeoutInSeconds(BenchmarkTimeInSeconds) + 20) * 1000),
                    cancelToken
                    );

                // No duplicate code in Sgminer.cs
                if (!exited && OnNotExitAction != null)
                {
                    await Task.Run(OnNotExitAction);
                }

                if (BenchmarkSignalTimedout && !TimeoutStandard)
                {
                    throw new TimeoutException("Benchmark timedout");
                }

                if (BenchmarkException != null)
                {
                    throw BenchmarkException;
                }

                if (cancelToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(cancelToken);
                }

                if (BenchmarkSignalHanged || !exited)
                {
                    throw new Exception("Miner is not responding");
                }

                if (BenchmarkSignalFinnished)
                {
                    //break;
                }
            }
            catch (Exception ex)
            {
                BenchmarkThreadRoutineCatch(ex);
            }

            FinishUpBenchmark();

            return await BenchmarkThreadRoutineFinish(_benchLines);
        }

        protected abstract bool BenchmarkParseLine(string outdata);

        protected abstract void BenchmarkOutputErrorDataReceivedImpl(string outdata);

        protected virtual void FinishUpBenchmark() { }

        protected override void BenchmarkOutputErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (_benchmarkTimeOutStopWatch == null)
            {
                _benchmarkTimeOutStopWatch = new Stopwatch();
                _benchmarkTimeOutStopWatch.Start();
            }
            else if (_benchmarkTimeOutStopWatch.Elapsed.TotalSeconds >
                     BenchmarkTimeoutInSeconds(BenchmarkTimeInSeconds))
            {
                _benchmarkTimeOutStopWatch.Stop();
                BenchmarkSignalTimedout = true;
            }

            var outdata = e.Data;
            if (e.Data != null)
            {
                BenchmarkOutputErrorDataReceivedImpl(outdata);
            }

            // terminate process situations
            if (_cancelToken.IsCancellationRequested
                || BenchmarkSignalFinnished
                || BenchmarkSignalHanged
                || BenchmarkSignalTimedout
                || BenchmarkException != null)
            {
                EndBenchmarkProcces();
            }
        }

        public override Task<(bool, string)> BenchmarkStart(int time, CancellationToken cancelToken)
        {
            _benchmarkTimeOutStopWatch = null;
            _benchLines.Clear();

            return base.BenchmarkStart(time, cancelToken);
        }

        protected void CheckOutdata(string outdata)
        {
            //Helpers.ConsolePrint("BENCHMARK" + benchmarkLogPath, outdata);
            _benchLines.Add(outdata);
            // ccminer, cpuminer
            if (outdata.Contains("Cuda error"))
                BenchmarkException = new Exception("CUDA error");
            if (outdata.Contains("is not supported"))
                BenchmarkException = new Exception("N/A");
            if (outdata.Contains("illegal memory access"))
                BenchmarkException = new Exception("CUDA error");
            if (outdata.Contains("unknown error"))
                BenchmarkException = new Exception("Unknown error");
            if (outdata.Contains("No servers could be used! Exiting."))
                BenchmarkException = new Exception("No pools or work can be used for benchmarking");
            //if (outdata.Contains("error") || outdata.Contains("Error"))
            //    BenchmarkException = new Exception("Unknown error #2");
            // Ethminer
            if (outdata.Contains("No GPU device with sufficient memory was found"))
                BenchmarkException = new Exception("[daggerhashimoto] No GPU device with sufficient memory was found.");
            // xmr-stak
            if (outdata.Contains("Press any key to exit"))
                BenchmarkException = new Exception("Xmr-Stak erred, check its logs");

            // lastly parse data
            if (BenchmarkParseLine(outdata))
            {
                BenchmarkSignalFinnished = true;
            }
        }
    }
}
