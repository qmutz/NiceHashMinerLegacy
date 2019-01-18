using Newtonsoft.Json;
using NiceHashMiner.Devices;
using NiceHashMiner.Miners.Grouping;
using NiceHashMiner.Miners.Parsing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Algorithms;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners
{
    public abstract class ClaymoreBaseMiner : MinerLogBench
    {
        protected double ApiReadMult = 1;
        protected AlgorithmType SecondaryAlgorithmType = AlgorithmType.NONE;

        // CD intensity tuning
        protected const int defaultIntensity = 30;

        private IEnumerable<MiningPair> SortedMiningPairs => MiningSetup.MiningPairs
            .OrderByDescending(pair => pair.Device.DeviceType)
            .ThenBy(pair => pair.Device.IDByBus);

        protected ClaymoreBaseMiner(string minerDeviceName)
            : base(minerDeviceName)
        {
            ConectionType = NhmConectionType.STRATUM_SSL;
            IsKillAllUsedMinerProcs = true;
        }

        // return true if a secondary algo is being used
        public bool IsDual()
        {
            return (SecondaryAlgorithmType != AlgorithmType.NONE);
        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000 * 5; // 5 minute max, whole waiting time 75seconds
        }

        private class JsonApiResponse
        {
#pragma warning disable IDE1006 // Naming Styles
            public List<string> result { get; set; }
            public int id { get; set; }
            public object error { get; set; }
#pragma warning restore IDE1006 // Naming Styles
        }

        public override async Task<ApiData> GetSummaryAsync()
        {
            CurrentMinerReadStatus = MinerApiReadStatus.NONE;
            var ad = new SplitApiData(MiningSetup);

            JsonApiResponse resp = null;
            try
            {
                var bytesToSend = Encoding.ASCII.GetBytes("{\"id\":0,\"jsonrpc\":\"2.0\",\"method\":\"miner_getstat1\"}\n");
                using (var client = new TcpClient("127.0.0.1", ApiPort))
                using (var nwStream = client.GetStream())
                {
                    await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                    var bytesToRead = new byte[client.ReceiveBufferSize];
                    var bytesRead = await nwStream.ReadAsync(bytesToRead, 0, client.ReceiveBufferSize);
                    var respStr = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                    resp = JsonConvert.DeserializeObject<JsonApiResponse>(respStr, Globals.JsonSettings);
                }
                //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", respStr);
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint(MinerTag(), "GetSummary exception: " + ex.Message);
            }

            if (resp != null && resp.error == null)
            {
                //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", "resp != null && resp.error == null");
                if (resp.result != null && resp.result.Count > 4)
                {
                    //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", "resp.result != null && resp.result.Count > 4");
                    var speeds = resp.result[3].Split(';');
                    var secondarySpeeds = resp.result[5].Split(';');

                    var sortedDevs = SortedMiningPairs.Select(p => p.Device.Index).ToList();

                    for (var i = 0; i < speeds.Length; i++)
                    {
                        //Helpers.ConsolePrint("ClaymoreZcashMiner API back:", "foreach (var speed in speeds) {");
                        var tmpSpeed = 0d;
                        var tmpSecSpeed = 0d;
                        try
                        {
                            tmpSpeed = double.Parse(speeds[i], CultureInfo.InvariantCulture);
                            if (IsDual()) 
                            {
                                tmpSecSpeed = double.Parse(secondarySpeeds[i], CultureInfo.InvariantCulture);
                            }
                        }
                        catch
                        { }

                        if (sortedDevs.Count > i)
                        {
                            ad.Speeds[sortedDevs[i]] = tmpSpeed * ApiReadMult;
                            if (IsDual())
                            {
                                ad.SecondarySpeeds[sortedDevs[i]] = tmpSecSpeed * ApiReadMult;
                            }
                        } 
                    }

                    CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;
                }

                if (ad.Speed == 0)
                {
                    CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                }

                // some clayomre miners have this issue reporting negative speeds in that case restart miner
                if (ad.Speed < 0)
                {
                    Helpers.ConsolePrint(MinerTag(), "Reporting negative speeds will restart...");
                    Restart();
                }
            }

            return ad;
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            ShutdownMiner();
        }

        protected virtual string DeviceCommand(int amdCount = 1)
        {
            return " -di ";
        }

        // This method now overridden in ClaymoreCryptoNightMiner 
        // Following logic for ClaymoreDual and ClaymoreZcash
        protected override string GetDevicesCommandString()
        {
            // First by device type (AMD then NV), then by bus ID index
            var sortedMinerPairs = SortedMiningPairs.ToList();
            var extraParams = ExtraLaunchParametersParser.ParseForMiningPairs(sortedMinerPairs, DeviceType.AMD);

            var ids = new List<string>();
            var intensities = new List<string>();

            var amdDeviceCount = ComputeDeviceManager.Query.AmdDevices.Count;
            Helpers.ConsolePrint("ClaymoreIndexing", $"Found {amdDeviceCount} AMD devices");

            foreach (var mPair in sortedMinerPairs)
            {
                var id = mPair.Device.IDByBus;
                if (id < 0)
                {
                    // should never happen
                    Helpers.ConsolePrint("ClaymoreIndexing", "ID by Bus too low: " + id + " skipping device");
                    continue;
                }

                if (mPair.Device.DeviceType == DeviceType.NVIDIA)
                {
                    Helpers.ConsolePrint("ClaymoreIndexing", "NVIDIA device increasing index by " + amdDeviceCount);
                    id += amdDeviceCount;
                }

                if (id > 9)
                {
                    // New >10 GPU support in CD9.8
                    if (id < 36)
                    {
                        // CD supports 0-9 and a-z indexes, so 36 GPUs
                        var idchar = (char) (id + 87); // 10 = 97(a), 11 - 98(b), etc
                        ids.Add(idchar.ToString());
                    }
                    else
                    {
                        Helpers.ConsolePrint("ClaymoreIndexing", "ID " + id + " too high, ignoring");
                    }
                }
                else
                {
                    ids.Add(id.ToString());
                }

                if (mPair.Algorithm is DualAlgorithm algo && algo.TuningEnabled)
                {
                    intensities.Add(algo.CurrentIntensity.ToString());
                }
            }

            var deviceStringCommand = DeviceCommand(amdDeviceCount) + string.Join("", ids);
            var intensityStringCommand = "";
            if (intensities.Count > 0)
            {
                intensityStringCommand = " -dcri " + string.Join(",", intensities);
            }

            return deviceStringCommand + intensityStringCommand + extraParams;
        }

        // benchmark stuff

        protected override void BenchmarkThreadRoutine(object commandLine)
        {
            if (BenchmarkAlgorithm is DualAlgorithm dualBenchAlgo && dualBenchAlgo.TuningEnabled)
            {
                var stepsLeft = (int) Math.Ceiling((double) (dualBenchAlgo.TuningEnd - dualBenchAlgo.CurrentIntensity) /
                                                   (dualBenchAlgo.TuningInterval)) + 1;
                Helpers.ConsolePrint("CDTUING", "{0} tuning steps remain, should complete in {1} seconds", stepsLeft,
                    stepsLeft * BenchmarkTimeWait);
                Helpers.ConsolePrint("CDTUNING",
                    $"Starting benchmark for intensity {dualBenchAlgo.CurrentIntensity} out of {dualBenchAlgo.TuningEnd}");
            }

            base.BenchmarkThreadRoutine(commandLine);
        }
    }
}
