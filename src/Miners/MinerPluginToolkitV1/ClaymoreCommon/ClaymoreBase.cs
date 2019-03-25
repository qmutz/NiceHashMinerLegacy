using MinerPlugin;
using MinerPluginToolkitV1.ExtraLaunchParameters;
using Newtonsoft.Json;
using NiceHashMinerLegacy.Common;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static NiceHashMinerLegacy.Common.StratumServiceHelpers;


namespace MinerPluginToolkitV1.ClaymoreCommon
{
   public class ClaymoreBase : MinerBase
    {
        private int _apiPort;
        protected readonly string _uuid;

        // this is second algorithm - if this is null only dagger is being mined
        private AlgorithmType _algorithmDualType;

        private string _devices;
        private string _extraLaunchParameters = "";

        // command line parts
        private string _platform;

        public ClaymoreBase(string uuid)
        {
            _uuid = uuid;
        }

        private static int GetPlatformIDForType(DeviceType type)
        {
            switch (type)
            {
                case DeviceType.AMD:
                    return 1;
                case DeviceType.NVIDIA:
                    return 2;
                default:
                    return 3;
            }
        }

        protected virtual string DualAlgoName
        {
            get
            {
                switch (_algorithmDualType)
                {
                    case AlgorithmType.DaggerDecred:
                        return "dcr";
                    case AlgorithmType.DaggerBlake2s:
                        return "b2s";
                    case AlgorithmType.DaggerKeccak:
                        return "kc";
                    default:
                        return "";
                }
            }
        }

        private double DevFee
        {
            get
            {
                return 1.0;
            }
        }

        private double DualDevFee
        {
            get
            {
                return 0.0;
            }
        }

        public bool IsDual()
        {
            return (_algorithmDualType != AlgorithmType.NONE);
        }

        private class JsonApiResponse
        {
#pragma warning disable IDE1006 // Naming Styles
            public List<string> result { get; set; }
            public int id { get; set; }
            public object error { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var ad = new ApiData();

            JsonApiResponse resp = null;
            try
            {
                var bytesToSend = Encoding.ASCII.GetBytes("{\"id\":0,\"jsonrpc\":\"2.0\",\"method\":\"miner_getstat1\"}\n");
                using (var client = new TcpClient("127.0.0.1", _apiPort))
                using (var nwStream = client.GetStream())
                {
                    await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                    var bytesToRead = new byte[client.ReceiveBufferSize];
                    var bytesRead = await nwStream.ReadAsync(bytesToRead, 0, client.ReceiveBufferSize);
                    var respStr = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                    resp = JsonConvert.DeserializeObject<JsonApiResponse>(respStr);
                }
                //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", respStr);
                if (resp != null && resp.error == null)
                {
                    //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", "resp != null && resp.error == null");
                    if (resp.result != null && resp.result.Count > 4)
                    {
                        //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", "resp.result != null && resp.result.Count > 4");
                        var speeds = resp.result[3].Split(';');
                        var secondarySpeeds = (IsDual()) ? resp.result[5].Split(';') : new string[0];
                        var primarySpeed = 0d;
                        var secondarySpeed = 0d;
                        foreach (var speed in speeds)
                        {
                            //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", "foreach (var speed in speeds) {");
                            double tmpSpeed;
                            try
                            {
                                tmpSpeed = double.Parse(speed, CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                                tmpSpeed = 0;
                            }

                            primarySpeed += tmpSpeed;
                        }

                        foreach (var speed in secondarySpeeds)
                        {
                            double tmpSpeed;
                            try
                            {
                                tmpSpeed = double.Parse(speed, CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                                tmpSpeed = 0;
                            }

                            secondarySpeed += tmpSpeed;
                        }
                        var totalPrimary = new List<(AlgorithmType, double)>();
                        var totalSecondary = new List<(AlgorithmType, double)>();
                        totalPrimary.Add((AlgorithmType.DaggerHashimoto, primarySpeed));
                        totalSecondary.Add((_algorithmDualType, secondarySpeed));

                        ad.AlgorithmSpeedsTotal = totalPrimary;
                        ad.AlgorithmSecondarySpeedsTotal = totalSecondary;
                        //ad.Speed *= ApiReadMult;
                        //ad.SecondarySpeed *= ApiReadMult;
                    }
                }
            }
            catch (Exception ex)
            {
                //Helpers.ConsolePrint(MinerTag(), "GetSummary exception: " + ex.Message);
            }
            return ad;
        }

        protected override void Init()
        {
            bool ok;
            (_algorithmDualType, ok) = MinerToolkit.GetAlgorithmDualType(_miningPairs);
            if (!ok) throw new InvalidOperationException("Invalid mining initialization");
            // all good continue on

            // Order pairs and parse ELP
            var orderedMiningPairs = _miningPairs.ToList();
            orderedMiningPairs.Sort((a, b) => a.device.ID.CompareTo(b.device.ID));
            _devices = string.Join("", orderedMiningPairs.Select(p => p.device.ID));
            _platform = $"{GetPlatformIDForType(orderedMiningPairs.First().device.DeviceType)}";

            if (MinerOptionsPackage != null)
            {
                // TODO add ignore temperature checks
                var generalParams = Parser.Parse(orderedMiningPairs, MinerOptionsPackage.GeneralOptions);
                var temperatureParams = Parser.Parse(orderedMiningPairs, MinerOptionsPackage.TemperatureOptions);
                _extraLaunchParameters = $"{generalParams} {temperatureParams}".Trim();
            }
        }

        public async override Task<(double speed, bool ok, string msg)> StartBenchmark(CancellationToken stop, BenchmarkPerformanceType benchmarkType = BenchmarkPerformanceType.Standard)
        {
            var benchmarkTime = 90; // in seconds
            switch (benchmarkType)
            {
                case BenchmarkPerformanceType.Quick:
                    benchmarkTime = 60;
                    break;
                case BenchmarkPerformanceType.Standard:
                    benchmarkTime = 90;
                    break;
                case BenchmarkPerformanceType.Precise:
                    benchmarkTime = 180;
                    break;
            }

            var commandLine = CreateCommandLine(MinerToolkit.DemoUser) + "-benchmark 1";
            var (binPath, binCwd) = GetBinAndCwdPaths();
            var bp = new BenchmarkProcess(binPath, binCwd, commandLine);

            var benchHashes = 0d;
            var benchIters = 0;
            var benchHashResult = 0d;  // Not too sure what this is..
            var after = $"GPU{_devices}";
            var targetBenchIters = Math.Max(1, (int)Math.Floor(benchmarkTime / 20d));

            bp.CheckData = (string data) =>
            {
                var hasHashRate = data.Contains(after);

                if (!hasHashRate) return (benchHashResult, false);

                var (hashrate, found) = data.TryGetHashrateAfter(after);

                benchHashes += hashrate;
                benchIters++;

                benchHashResult = (benchHashes / benchIters) * (1 - DevFee * 0.01);

                return (benchHashResult, benchIters >= targetBenchIters);
            };

            var benchmarkTimeout = TimeSpan.FromSeconds(benchmarkTime + 10);
            var benchmarkWait = TimeSpan.FromMilliseconds(500);
            var t = MinerToolkit.WaitBenchmarkResult(bp, benchmarkTimeout, benchmarkWait, stop);
            return await t;
        }

        protected override (string binPath, string binCwd) GetBinAndCwdPaths()
        {
            var pluginRoot = Path.Combine(Paths.MinerPluginsPath(), _uuid);
            var pluginRootBins = Path.Combine(pluginRoot, "bins");
            var binPath = Path.Combine(pluginRootBins, "EthDcrMiner64.exe");
            var binCwd = pluginRootBins;
            return (binPath, binCwd);
        }

        private string CreateCommandLine(string username)
        {
            var urlFirst = GetLocationUrl(AlgorithmType.DaggerHashimoto, _miningLocation, NhmConectionType.STRATUM_TCP);
            var cmd = "";
            if (_algorithmDualType == AlgorithmType.NONE) //noDual
            {
                cmd = $"-di {_devices} -platform {_platform} -epool {urlFirst} -ewal {username} -esm 3 -epsw x -allpools 1 -dbg -1 {_extraLaunchParameters} -wd 0";
            }
            else
            {
                var urlSecond = GetLocationUrl(_algorithmDualType, _miningLocation, NhmConectionType.STRATUM_TCP);
                cmd = $"-di {_devices} -platform {_platform} -epool {urlFirst} -ewal {username} -esm 3 -epsw x -allpools 1 -dcoin {DualAlgoName} -dpool {urlSecond} -dwal {username} -dpsw x -dbg -1 {_extraLaunchParameters} -wd 0";
            }
            
            return cmd;
        }

        protected override string MiningCreateCommandLine()
        {
            _apiPort = MinersApiPortsManager.GetAvaliablePortInRange();
            return CreateCommandLine(_username) + $" -mport 127.0.0.1:-{_apiPort}";
        }
    }
}
