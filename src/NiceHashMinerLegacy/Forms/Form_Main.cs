using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Devices.Querying;
using NiceHashMiner.Forms;
using NiceHashMiner.Miners;
using NiceHashMiner.Miners.IdleChecking;
using NiceHashMiner.Stats;
using NiceHashMiner.Switching;
using NiceHashMiner.Utils;
using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Threading;
using System.Windows.Forms;
using static NiceHashMiner.Translations;
using Timer = System.Windows.Forms.Timer;

namespace NiceHashMiner
{
    public partial class Form_Main : Form, Form_Loading.IAfterInitializationCaller
    {
        private Timer _minerStatsCheck;
        private Timer _startupTimer;
        private Timer _idleCheck;

        private bool _showWarningNiceHashData;
        private bool _demoMode;

        private Form_Loading _loadingScreen;
        private Form_Benchmark _benchmarkForm;

        private bool _isDeviceDetectionInitialized = false;

        private bool _isManuallyStarted = false;

        private CudaDeviceChecker _cudaChecker;

        public Form_Main()
        {
            InitializeComponent();
            CenterToScreen();

            Width = ConfigManager.GeneralConfig.MainFormSize.X;
            Height = ConfigManager.GeneralConfig.MainFormSize.Y;
            Icon = Properties.Resources.logo;

            InitLocalization();

            // Log the computer's amount of Total RAM and Page File Size
            var moc = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem").Get();
            foreach (ManagementObject mo in moc)
            {
                var totalRam = long.Parse(mo["TotalVisibleMemorySize"].ToString()) / 1024;
                var pageFileSize = (long.Parse(mo["TotalVirtualMemorySize"].ToString()) / 1024) - totalRam;
                Helpers.ConsolePrint("NICEHASH", "Total RAM: " + totalRam + "MB");
                Helpers.ConsolePrint("NICEHASH", "Page File Size: " + pageFileSize + "MB");
            }

            Text += ApplicationStateManager.Title;

            InitMainConfigGuiData();
        

            FormHelpers.TranslateFormControls(this);
        }

        private void InitLocalization()
        {
            MessageBoxManager.Unregister();
            MessageBoxManager.Yes = Tr("&Yes");
            MessageBoxManager.No = Tr("&No");
            MessageBoxManager.OK = Tr("&OK");
            MessageBoxManager.Cancel = Tr("&Cancel");
            MessageBoxManager.Retry = Tr("&Retry");
            MessageBoxManager.Register();

            //todo make this dinamically
            {
                comboBoxLocation.Items[0] = Tr("Europe - Amsterdam");
                comboBoxLocation.Items[1] = Tr("USA - San Jose");
                comboBoxLocation.Items[2] = Tr("China - Hong Kong");
                comboBoxLocation.Items[3] = Tr("Japan - Tokyo");
                comboBoxLocation.Items[4] = Tr("India - Chennai");
                comboBoxLocation.Items[5] = Tr("Brazil - Sao Paulo");
            }

            //??? doesn't get translated if we don't translate it directly????
            toolStripStatusLabelGlobalRateText.Text = Tr("Global rate:");

            toolStripStatusLabelBTCDayText.Text =
                "BTC/" + Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());
            toolStripStatusLabelBalanceText.Text = (ExchangeRateApi.ActiveDisplayCurrency + "/") +
                                                   Tr(
                                                       ConfigManager.GeneralConfig.TimeUnit.ToString()) + "     " +
                                                   Tr("Balance") + ":";

            devicesListViewEnableControl1.InitLocale();
        }

        // InitMainConfigGuiData gets called after settings are changed and whatnot but this is a crude and tightly coupled way of doing things
        private void InitMainConfigGuiData()
        {
            if (ConfigManager.GeneralConfig.ServiceLocation >= 0 &&
                ConfigManager.GeneralConfig.ServiceLocation < comboBoxLocation.Items.Count)
                comboBoxLocation.SelectedIndex = ConfigManager.GeneralConfig.ServiceLocation;
            else
                comboBoxLocation.SelectedIndex = 0;

            textBoxBTCAddress.Text = ConfigManager.GeneralConfig.BitcoinAddress;
            textBoxWorkerName.Text = ConfigManager.GeneralConfig.WorkerName;

            _showWarningNiceHashData = true;
            _demoMode = false;

            // init active display currency after config load
            ExchangeRateApi.ActiveDisplayCurrency = ConfigManager.GeneralConfig.DisplayCurrency;

            // init factor for Time Unit
            TimeFactor.UpdateTimeUnit(ConfigManager.GeneralConfig.TimeUnit);

            toolStripStatusLabelBalanceDollarValue.Text = "(" + ExchangeRateApi.ActiveDisplayCurrency + ")";
            toolStripStatusLabelBalanceText.Text = (ExchangeRateApi.ActiveDisplayCurrency + "/") +
                                                   Tr(
                                                       ConfigManager.GeneralConfig.TimeUnit.ToString()) + "     " +
                                                   Tr("Balance") + ":";
            BalanceCallback(null, null); // update currency changes

            if (_isDeviceDetectionInitialized)
            {
                devicesListViewEnableControl1.ResetComputeDevices(AvailableDevices.Devices);
            }

            devicesListViewEnableControl1.SetPayingColumns();
        }

        public void AfterLoadComplete()
        {
            _loadingScreen = null;
            Enabled = true;

            IdleCheckManager.StartIdleCheck(ConfigManager.GeneralConfig.IdleCheckType, IdleCheck);
        }
        
        private void IdleCheck(object sender, IdleChangedEventArgs e)
        {
            if (!ConfigManager.GeneralConfig.StartMiningWhenIdle || _isManuallyStarted) return;

            if (_minerStatsCheck.Enabled)
            {
                if (!e.IsIdle)
                {
                    StopMining();
                    Helpers.ConsolePrint("NICEHASH", "Resumed from idling");
                }
            }
            else
            {
                if (_benchmarkForm == null && e.IsIdle)
                {
                    Helpers.ConsolePrint("NICEHASH", "Entering idling state");
                    if (StartMining(false) != StartMiningReturnType.StartMining)
                    {
                        StopMining();
                    }
                }
            }
        }

        // This is a single shot _benchmarkTimer
        private async void StartupTimer_Tick(object sender, EventArgs e)
        {
            _startupTimer.Stop();
            _startupTimer = null;

            // Internals Init
            // TODO add loading step
            MinersSettingsManager.Init();

            if (!Helpers.Is45NetOrHigher())
            {
                MessageBox.Show(Tr("NiceHash Miner Legacy requires .NET Framework 4.5 or higher to work properly. Please install Microsoft .NET Framework 4.5"),
                    Tr("Warning!"),
                    MessageBoxButtons.OK);

                Close();
                return;
            }

            if (!Helpers.Is64BitOperatingSystem)
            {
                MessageBox.Show(Tr("NiceHash Miner Legacy supports only x64 platforms. You will not be able to use NiceHash Miner Legacy with x86"),
                    Tr("Warning!"),
                    MessageBoxButtons.OK);

                Close();
                return;
            }

            // 3rdparty miners check scope #1
            {
                // check if setting set
                if (ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.NOT_SET)
                {
                    // Show TOS
                    Form tos = new Form_3rdParty_TOS();
                    tos.ShowDialog(this);
                }
            }

            // Query Available ComputeDevices
            ComputeDeviceManager.OnProgressUpdate += _loadingScreen.SetMessageAndIncrementStep;
            var query = await ComputeDeviceManager.QueryDevicesAsync();
            ComputeDeviceManager.OnProgressUpdate -= _loadingScreen.SetMessageAndIncrementStep;
            ShowQueryWarnings(query);

            _isDeviceDetectionInitialized = true;

            /////////////////////////////////////////////
            /////// from here on we have our devices and Miners initialized
            ConfigManager.AfterDeviceQueryInitialization();
            _loadingScreen.IncreaseLoadCounterAndMessage(Tr("Saving config..."));

            // All devices settup should be initialized in AllDevices
            devicesListViewEnableControl1.ResetComputeDevices(AvailableDevices.Devices);
            // set properties after
            devicesListViewEnableControl1.SaveToGeneralConfig = true;

            _loadingScreen.IncreaseLoadCounterAndMessage(
                Tr("Checking for latest version..."));

            _minerStatsCheck = new Timer();
            _minerStatsCheck.Tick += MinerStatsCheck_Tick;
            _minerStatsCheck.Interval = ConfigManager.GeneralConfig.MinerAPIQueryInterval * 1000;

            _loadingScreen.IncreaseLoadCounterAndMessage(Tr("Getting NiceHash SMA information..."));
            // Init ws connection
            NiceHashStats.OnBalanceUpdate += BalanceCallback;
            NiceHashStats.OnConnectionLost += ConnectionLostCallback;
            NiceHashStats.OnVersionBurn += VersionBurnCallback;
            NiceHashStats.OnExchangeUpdate += ExchangeCallback;
            NiceHashStats.StartConnection(Links.NhmSocketAddress);

            // increase timeout
            if (Globals.IsFirstNetworkCheckTimeout)
            {
                while (!Helpers.WebRequestTestGoogle() && Globals.FirstNetworkCheckTimeoutTries > 0)
                {
                    --Globals.FirstNetworkCheckTimeoutTries;
                }
            }

            _loadingScreen.IncreaseLoadCounterAndMessage(Tr("Getting Bitcoin exchange rate..."));

            _loadingScreen.IncreaseLoadCounterAndMessage(
                Tr("Setting environment variables..."));
            Helpers.SetDefaultEnvironmentVariables();

            _loadingScreen.IncreaseLoadCounterAndMessage(
                Tr("Setting Windows error reporting..."));

            Helpers.DisableWindowsErrorReporting(ConfigManager.GeneralConfig.DisableWindowsErrorReporting);

            _loadingScreen.IncreaseLoadCounter();
            if (ConfigManager.GeneralConfig.NVIDIAP0State)
            {
                _loadingScreen.SetInfoMsg(Tr("Changing all supported NVIDIA GPUs to P0 state..."));
                Helpers.SetNvidiaP0State();
            }

            _loadingScreen.FinishLoad();

            var runVCRed = !MinersExistanceChecker.IsMinersBinsInit() && !ConfigManager.GeneralConfig.DownloadInit;
            // standard miners check scope
            {
                // check if download needed
                if (!MinersExistanceChecker.IsMinersBinsInit() && !ConfigManager.GeneralConfig.DownloadInit)
                {
                    var downloadUnzipForm =
                        new Form_Loading(new MinersDownloader(MinersDownloadManager.StandardDlSetup));
                    SetChildFormCenter(downloadUnzipForm);
                    downloadUnzipForm.ShowDialog();
                }
                // check if files are mising
                if (!MinersExistanceChecker.IsMinersBinsInit())
                {
                    var result = MessageBox.Show(Tr("There are missing files from last Miners Initialization. Please make sure that your anti-virus is not blocking the application. NiceHash Miner Legacy might not work properly without missing files. Click Yes to reinitialize NiceHash Miner Legacy to try to fix this issue."),
                        Tr("Warning!"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        ConfigManager.GeneralConfig.DownloadInit = false;
                        ConfigManager.GeneralConfigFileCommit();
                        var pHandle = new Process
                        {
                            StartInfo =
                            {
                                FileName = Application.ExecutablePath
                            }
                        };
                        pHandle.Start();
                        Close();
                        return;
                    }
                }
                else if (!ConfigManager.GeneralConfig.DownloadInit)
                {
                    // all good
                    ConfigManager.GeneralConfig.DownloadInit = true;
                    ConfigManager.GeneralConfigFileCommit();
                }
            }
            // 3rdparty miners check scope #2
            {
                // check if download needed
                if (ConfigManager.GeneralConfig.Use3rdPartyMiners == Use3rdPartyMiners.YES)
                {
                    if (!MinersExistanceChecker.IsMiners3rdPartyBinsInit() &&
                        !ConfigManager.GeneralConfig.DownloadInit3rdParty)
                    {
                        var download3rdPartyUnzipForm =
                            new Form_Loading(new MinersDownloader(MinersDownloadManager.ThirdPartyDlSetup));
                        SetChildFormCenter(download3rdPartyUnzipForm);
                        download3rdPartyUnzipForm.ShowDialog();
                    }
                    // check if files are mising
                    if (!MinersExistanceChecker.IsMiners3rdPartyBinsInit())
                    {
                        var result = MessageBox.Show(Tr("There are missing files from last Miners Initialization. Please make sure that your anti-virus is not blocking the application. NiceHash Miner Legacy might not work properly without missing files. Click Yes to reinitialize NiceHash Miner Legacy to try to fix this issue."),
                            Tr("Warning!"),
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (result == DialogResult.Yes)
                        {
                            ConfigManager.GeneralConfig.DownloadInit3rdParty = false;
                            ConfigManager.GeneralConfigFileCommit();
                            var pHandle = new Process
                            {
                                StartInfo =
                                {
                                    FileName = Application.ExecutablePath
                                }
                            };
                            pHandle.Start();
                            Close();
                            return;
                        }
                    }
                    else if (!ConfigManager.GeneralConfig.DownloadInit3rdParty)
                    {
                        // all good
                        ConfigManager.GeneralConfig.DownloadInit3rdParty = true;
                        ConfigManager.GeneralConfigFileCommit();
                    }
                }
            }

            if (runVCRed)
            {
                Helpers.InstallVcRedist();
            }


            if (ConfigManager.GeneralConfig.AutoStartMining)
            {
                // well this is started manually as we want it to start at runtime
                _isManuallyStarted = true;
                if (StartMining(false) != StartMiningReturnType.StartMining)
                {
                    _isManuallyStarted = false;
                    StopMining();
                }
            }

            // Register callbacks
            ApplicationStateManager.OnVersionUpdate += OnVersionUpdate;
            ApplicationStateManager.BtcAddressChanged += OnBtcAddressChanged;
            ApplicationStateManager.WorkerNameChanged += OnWorkerNameChanged;
            ApplicationStateManager.ServiceLocationChanged += OnServiceLocationChanged;
        }

        private void ShowQueryWarnings(QueryResult query)
        {
            if (query.FailedMinNVDriver)
            {
                MessageBox.Show(string.Format(
                        Tr(
                            "We have detected that your system has Nvidia GPUs, but your driver is older than {0}. In order for NiceHash Miner Legacy to work correctly you should upgrade your drivers to recommended {1} or newer. If you still see this warning after updating the driver please uninstall all your Nvidia drivers and make a clean install of the latest official driver from http://www.nvidia.com."),
                        query.MinDriverString,
                        query.RecommendedDriverString),
                    Tr("Nvidia Recomended driver"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (query.FailedRecommendedNVDriver)
            {
                MessageBox.Show(string.Format(
                        Tr(
                            "We have detected that your Nvidia Driver is older than {0}{1}. We recommend you to update to {2} or newer."),
                        query.RecommendedDriverString,
                        query.CurrentDriverString,
                        query.RecommendedDriverString),
                    Tr("Nvidia Recomended driver"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (query.NoDevices)
            {
                var result = MessageBox.Show(Tr("No supported devices are found. Select the OK button for help or cancel to continue."),
                    Tr("No Supported Devices"),
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.OK)
                {
                    Process.Start(Links.NhmNoDevHelp);
                }
            }

            if (query.FailedRamCheck)
            {
                MessageBox.Show(Tr("NiceHash Miner Legacy recommends increasing virtual memory size so that all algorithms would work fine."),
                    Tr("Warning!"),
                    MessageBoxButtons.OK);
            }

            if (query.FailedVidControllerStatus)
            {
                var msg = Tr("We have detected a Video Controller that is not working properly. NiceHash Miner Legacy will not be able to use this Video Controller for mining. We advise you to restart your computer, or reinstall your Video Controller drivers.");
                msg += '\n' + query.FailedVidControllerInfo;
                MessageBox.Show(msg,
                    Tr("Warning! Video Controller not operating correctly"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (query.FailedAmdDriverCheck)
            {
                var warningDialog = new DriverVersionConfirmationDialog();
                warningDialog.ShowDialog();
            }

            if (query.FailedCpu64Bit)
            {
                MessageBox.Show(Tr("NiceHash Miner Legacy works only on 64-bit version of OS for CPU mining. CPU mining will be disabled."),
                    Tr("Warning!"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (query.FailedCpuCount)
            {
                MessageBox.Show(Tr("NiceHash Miner Legacy does not support more than 64 virtual cores. CPU mining will be disabled."),
                    Tr("Warning!"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetChildFormCenter(Form form)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(Location.X + (Width - form.Width) / 2, Location.Y + (Height - form.Height) / 2);
        }

        private void Form_Main_Shown(object sender, EventArgs e)
        {
            // general loading indicator
            const int totalLoadSteps = 11;
            _loadingScreen = new Form_Loading(this,
                Tr("Loading, please wait..."),
                Tr("Querying CPU devices..."), totalLoadSteps);
            SetChildFormCenter(_loadingScreen);
            _loadingScreen.Show();

            _startupTimer = new Timer();
            _startupTimer.Tick += StartupTimer_Tick;
            _startupTimer.Interval = 200;
            _startupTimer.Start();
        }

        // TODO: Move this and its timer
        private static async void MinerStatsCheck_Tick(object sender, EventArgs e)
        {
            await MinersManager.MinerStatsCheck();
        }



        public void UpdateGlobalRate()
        {
            var totalRate = MinersManager.GetTotalRate();

            if (ConfigManager.GeneralConfig.AutoScaleBTCValues && totalRate < 0.1)
            {
                toolStripStatusLabelBTCDayText.Text =
                    "mBTC/" + Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());
                toolStripStatusLabelGlobalRateValue.Text =
                    (totalRate * 1000 * TimeFactor.TimeUnit).ToString("F5", CultureInfo.InvariantCulture);
            }
            else
            {
                toolStripStatusLabelBTCDayText.Text =
                    "BTC/" + Tr(ConfigManager.GeneralConfig.TimeUnit.ToString());
                toolStripStatusLabelGlobalRateValue.Text =
                    (totalRate * TimeFactor.TimeUnit).ToString("F6", CultureInfo.InvariantCulture);
            }

            toolStripStatusLabelBTCDayValue.Text = ExchangeRateApi
                .ConvertToActiveCurrency((totalRate * TimeFactor.TimeUnit * ExchangeRateApi.GetUsdExchangeRate()))
                .ToString("F2", CultureInfo.InvariantCulture);
            toolStripStatusLabelBalanceText.Text = (ExchangeRateApi.ActiveDisplayCurrency + "/") +
                                                   Tr(
                                                       ConfigManager.GeneralConfig.TimeUnit.ToString()) + "     " +
                                                   Tr("Balance") + ":";
        }


        private void BalanceCallback(object sender, EventArgs e)
        {
            Helpers.ConsolePrint("NICEHASH", "Balance update");
            var balance = NiceHashStats.Balance;
            if (balance > 0)
            {
                if (ConfigManager.GeneralConfig.AutoScaleBTCValues && balance < 0.1)
                {
                    toolStripStatusLabelBalanceBTCCode.Text = "mBTC";
                    toolStripStatusLabelBalanceBTCValue.Text =
                        (balance * 1000).ToString("F5", CultureInfo.InvariantCulture);
                }
                else
                {
                    toolStripStatusLabelBalanceBTCCode.Text = "BTC";
                    toolStripStatusLabelBalanceBTCValue.Text = balance.ToString("F6", CultureInfo.InvariantCulture);
                }
                
                var amount = (balance * ExchangeRateApi.GetUsdExchangeRate());
                amount = ExchangeRateApi.ConvertToActiveCurrency(amount);
                toolStripStatusLabelBalanceDollarText.Text = amount.ToString("F2", CultureInfo.InvariantCulture);
                toolStripStatusLabelBalanceDollarValue.Text = $"({ExchangeRateApi.ActiveDisplayCurrency})";
            }
        }

        private void ExchangeCallback(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker) UpdateExchange);
            }
            else
            {
                UpdateExchange();
            }
        }

        private void UpdateExchange()
        {
            var br = ExchangeRateApi.GetUsdExchangeRate();
            var currencyRate = Tr("N/A");
            if (br > 0)
            {
                currencyRate = ExchangeRateApi.ConvertToActiveCurrency(br).ToString("F2");
            }

            toolTip1.SetToolTip(statusStrip1, $"1 BTC = {currencyRate} {ExchangeRateApi.ActiveDisplayCurrency}");

            Helpers.ConsolePrint("NICEHASH",
                "Current Bitcoin rate: " + br.ToString("F2", CultureInfo.InvariantCulture));
        }

        private void VersionBurnCallback(object sender, SocketEventArgs e)
        {
            BeginInvoke((Action) (() =>
            {
                StopMining();
                _benchmarkForm?.StopBenchmark();
                var dialogResult = MessageBox.Show(e.Message, Tr("Error!"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }));
        }


        private void ConnectionLostCallback(object sender, EventArgs e)
        {
            if (!NHSmaData.HasData && ConfigManager.GeneralConfig.ShowInternetConnectionWarning &&
                _showWarningNiceHashData)
            {
                _showWarningNiceHashData = false;
                var dialogResult = MessageBox.Show(Tr("NiceHash Miner Legacy requires internet connection to run. Please ensure that you are connected to the internet before running NiceHash Miner Legacy. Would you like to continue?"),
                    Tr("Check internet connection"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (dialogResult == DialogResult.Yes)
                    return;
                if (dialogResult == DialogResult.No)
                    Application.Exit();
            }
        }

        private bool VerifyMiningAddress(bool showError)
        {
            if (!BitcoinAddress.ValidateBitcoinAddress(textBoxBTCAddress.Text.Trim()) && showError)
            {
                var result = MessageBox.Show(Tr("Invalid Bitcoin address!\n\nPlease enter a valid Bitcoin address or choose Yes to create one."),
                    Tr("Error!"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (result == DialogResult.Yes)
                    Process.Start(Links.NhmBtcWalletFaq);

                textBoxBTCAddress.Focus();
                return false;
            }
            if (!BitcoinAddress.ValidateWorkerName(textBoxWorkerName.Text.Trim()) && showError)
            {
                var result = MessageBox.Show(Tr("Invalid workername!\n\nPlease enter a valid workername (Aa-Zz, 0-9, up to 15 character long)."),
                    Tr("Error!"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                textBoxWorkerName.Focus();
                return false;
            }

            return true;
        }

        private void LinkLabelCheckStats_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!VerifyMiningAddress(true)) return;

            Process.Start(Links.CheckStats + textBoxBTCAddress.Text.Trim());
        }


        private void LinkLabelChooseBTCWallet_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(Links.NhmBtcWalletFaq);
        }

        private void LinkLabelNewVersion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ApplicationStateManager.VisitNewVersionUrl();
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            MinersManager.StopAllMiners();

            MessageBoxManager.Unregister();
        }

        private void ButtonBenchmark_Click(object sender, EventArgs e)
        {
            ConfigManager.GeneralConfig.ServiceLocation = comboBoxLocation.SelectedIndex;

            _benchmarkForm = new Form_Benchmark();
            SetChildFormCenter(_benchmarkForm);
            _benchmarkForm.ShowDialog();
            var startMining = _benchmarkForm.StartMining;
            _benchmarkForm = null;

            InitMainConfigGuiData();
            if (startMining)
            {
                ButtonStartMining_Click(null, null);
            }
        }


        private void ButtonSettings_Click(object sender, EventArgs e)
        {
            var settings = new Form_Settings();
            SetChildFormCenter(settings);
            settings.ShowDialog();

            if (settings.IsChange && settings.IsChangeSaved && settings.IsRestartNeeded)
            {
                MessageBox.Show(
                    Tr("Settings change requires NiceHash Miner Legacy to restart."),
                    Tr("Restart Notice"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                var pHandle = new Process
                {
                    StartInfo =
                    {
                        FileName = Application.ExecutablePath
                    }
                };
                pHandle.Start();
                Close();
            }
            else if (settings.IsChange && settings.IsChangeSaved)
            {
                InitLocalization();
                InitMainConfigGuiData();
                AfterLoadComplete();
            }
        }

        private void ButtonStartMining_Click(object sender, EventArgs e)
        {
            _isManuallyStarted = true;
            if (StartMining(true) == StartMiningReturnType.ShowNoMining)
            {
                _isManuallyStarted = false;
                StopMining();
                MessageBox.Show(Tr("NiceHash Miner Legacy cannot start mining. Make sure you have at least one enabled device that has at least one enabled and benchmarked algorithm."),
                    Tr("Warning!"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void ButtonStopMining_Click(object sender, EventArgs e)
        {
            _isManuallyStarted = false;
            StopMining();
        }

        private void ButtonLogo_Click(object sender, EventArgs e)
        {
            Process.Start(Links.VisitUrl);
        }

        private void ButtonHelp_Click(object sender, EventArgs e)
        {
            Process.Start(Links.NhmHelp);
        }

        private void ToolStripStatusLabel10_Click(object sender, EventArgs e)
        {
            Process.Start(Links.NhmPayingFaq);
        }

        private void ToolStripStatusLabel10_MouseHover(object sender, EventArgs e)
        {
            statusStrip1.Cursor = Cursors.Hand;
        }

        private void ToolStripStatusLabel10_MouseLeave(object sender, EventArgs e)
        {
            statusStrip1.Cursor = Cursors.Default;
        }

        private void textBoxBTCAddress_Leave(object sender, EventArgs e)
        {
            var trimmedBtcText = textBoxBTCAddress.Text.Trim();
            var result = ApplicationStateManager.SetBTCIfValidOrDifferent(trimmedBtcText);
            // TODO GUI stuff get back to this
            switch (result)
            {
                case ApplicationStateManager.SetResult.INVALID:
                    //var dialogResult = MessageBox.Show(International.GetText("Form_Main_msgbox_InvalidBTCAddressMsg"),
                    //International.GetText("Error_with_Exclamation"),
                    //MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                    //if (dialogResult == DialogResult.Yes)
                    //    Process.Start(Links.NhmBtcWalletFaq);

                    //textBoxBTCAddress.Focus();
                    break;
                case ApplicationStateManager.SetResult.CHANGED:
                    break;
                case ApplicationStateManager.SetResult.NOTHING_TO_CHANGE:
                    break;
            }
        }

        private void textBoxWorkerName_Leave(object sender, EventArgs e)
        {
            var trimmedWorkerNameText = textBoxWorkerName.Text.Trim();
            var result = ApplicationStateManager.SetWorkerIfValidOrDifferent(trimmedWorkerNameText);
            // TODO GUI stuff get back to this
            switch (result)
            {
                case ApplicationStateManager.SetResult.INVALID:
                    // TODO workername invalid handling
                    break;
                case ApplicationStateManager.SetResult.CHANGED:
                    break;
                case ApplicationStateManager.SetResult.NOTHING_TO_CHANGE:
                    break;
            }
        }

        private void comboBoxLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            var locationIndex = comboBoxLocation.SelectedIndex;
            var result = ApplicationStateManager.SetServiceLocationIfValidOrDifferent(locationIndex);
            // TODO GUI stuff get back to this, here we can't really break anything
            switch (result)
            {
                case ApplicationStateManager.SetResult.INVALID:
                    break;
                case ApplicationStateManager.SetResult.CHANGED:
                    break;
                case ApplicationStateManager.SetResult.NOTHING_TO_CHANGE:
                    break;
            }
        }

        // Minimize to system tray if MinimizeToTray is set to true
        private void Form1_Resize(object sender, EventArgs e)
        {
            notifyIcon1.Icon = Properties.Resources.logo;
            notifyIcon1.Text = Application.ProductName + " v" + Application.ProductVersion +
                               "\nDouble-click to restore..";

            if (ConfigManager.GeneralConfig.MinimizeToTray && FormWindowState.Minimized == WindowState)
            {
                notifyIcon1.Visible = true;
                Hide();
            }
        }

        // Restore NiceHashMiner from the system tray
        private void NotifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        ///////////////////////////////////////
        // Miner control functions
        private enum StartMiningReturnType
        {
            StartMining,
            ShowNoMining,
            IgnoreMsg
        }

        // TODO this will be moved outside of GUI code, replace textBoxBTCAddress.Text with ConfigManager.GeneralConfig.BitcoinAddress
        private StartMiningReturnType StartMining(bool showWarnings)
        {
            if (ConfigManager.GeneralConfig.BitcoinAddress.Equals(""))
            {
                if (showWarnings)
                {
                    var result = MessageBox.Show(Tr("You have not entered a bitcoin address. NiceHash Miner Legacy will start mining in DEMO mode. In the DEMO mode, you can test run the miner and be able see how much you can earn using your computer. Would you like to continue in DEMO mode?\n\nDISCLAIMER: YOU WILL NOT EARN ANYTHING DURING DEMO MODE!"),
                        Tr("Start mining in DEMO mode?"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        _demoMode = true;
                        labelDemoMode.Visible = true;
                    }
                    else
                    {
                        return StartMiningReturnType.IgnoreMsg;
                    }
                }
                else
                {
                    return StartMiningReturnType.IgnoreMsg;
                }
            }
            else if (!VerifyMiningAddress(true)) return StartMiningReturnType.IgnoreMsg;

            var hasData = NHSmaData.HasData;

            if (!showWarnings)
            {
                for (var i = 0; i < 10; i++)
                {
                    if (hasData) break;
                    Thread.Sleep(1000);
                    hasData = NHSmaData.HasData;
                    Helpers.ConsolePrint("NICEHASH", $"After {i}s has data: {hasData}");
                }
            }

            if (!hasData)
            {
                Helpers.ConsolePrint("NICEHASH", "No data received within timeout");
                if (showWarnings)
                {
                    MessageBox.Show(Tr("Unable to get NiceHash profitability data. If you are connected to internet, try again later."),
                        Tr("Error!"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return StartMiningReturnType.IgnoreMsg;
            }


            // Check if there are unbenchmakred algorithms
            var isBenchInit = true;
            foreach (var cdev in AvailableDevices.Devices)
            {
                if (cdev.Enabled)
                {
                    if (cdev.GetAlgorithmSettings().Where(algo => algo.Enabled).Any(algo => algo.BenchmarkSpeed == 0))
                    {
                        isBenchInit = false;
                    }
                }
            }
            // Check if the user has run benchmark first
            if (!isBenchInit)
            {
                var result = DialogResult.No;
                if (showWarnings)
                {
                    result = MessageBox.Show(Tr("There are unbenchmarked algorithms for selected enabled devices. Click Yes to benchmark and start mining, No to skip benchmark and continue mining, Cancel to abort"),
                        Tr("Warning!"),
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                }
                if (result == DialogResult.Yes)
                {
                    _benchmarkForm = new Form_Benchmark(
                        BenchmarkPerformanceType.Standard,
                        true);
                    SetChildFormCenter(_benchmarkForm);
                    _benchmarkForm.ShowDialog();
                    _benchmarkForm = null;
                    InitMainConfigGuiData();
                }
                else if (result == DialogResult.No)
                {
                    // check devices without benchmarks
                    foreach (var cdev in AvailableDevices.Devices)
                    {
                        if (cdev.Enabled)
                        {
                            var enabled = cdev.GetAlgorithmSettings().Any(algo => algo.BenchmarkSpeed > 0);
                            cdev.Enabled = enabled;
                        }
                    }
                }
                else
                {
                    return StartMiningReturnType.IgnoreMsg;
                }
            }

            textBoxBTCAddress.Enabled = false;
            textBoxWorkerName.Enabled = false;
            comboBoxLocation.Enabled = false;
            buttonBenchmark.Enabled = false;
            buttonStartMining.Enabled = false;
            buttonSettings.Enabled = false;
            devicesListViewEnableControl1.SetIsMining(true);
            buttonStopMining.Enabled = true;

            var btcAdress = _demoMode ? Globals.DemoUser : ConfigManager.GeneralConfig.BitcoinAddress;
            var isMining = MinersManager.StartInitialize(devicesListViewEnableControl1, StratumService.MiningLocations[comboBoxLocation.SelectedIndex],
                textBoxWorkerName.Text.Trim(), btcAdress);

            if (!_demoMode) ConfigManager.GeneralConfigFileCommit();

            //_isSmaUpdated = true; // Always check profits on mining start
            //_smaMinerCheck.Interval = 100;
            //_smaMinerCheck.Start();
            _minerStatsCheck.Start();

            // TODO move this
            if (_cudaChecker == null && ConfigManager.GeneralConfig.RunScriptOnCUDA_GPU_Lost)
            {
                _cudaChecker = new CudaDeviceChecker();
                _cudaChecker.Start();
            }

            return isMining ? StartMiningReturnType.StartMining : StartMiningReturnType.ShowNoMining;
        }

        private void StopMining()
        {
            _minerStatsCheck.Stop();
            _cudaChecker?.Stop();

            MinersManager.StopAllMiners();

            textBoxBTCAddress.Enabled = true;
            textBoxWorkerName.Enabled = true;
            comboBoxLocation.Enabled = true;
            buttonBenchmark.Enabled = true;
            buttonStartMining.Enabled = true;
            buttonSettings.Enabled = true;
            devicesListViewEnableControl1.SetIsMining(false);
            buttonStopMining.Enabled = false;

            if (_demoMode)
            {
                _demoMode = false;
                labelDemoMode.Visible = false;
            }

            UpdateGlobalRate();
        }

        private void Form_Main_ResizeEnd(object sender, EventArgs e)
        {
            ConfigManager.GeneralConfig.MainFormSize.X = Width;
            ConfigManager.GeneralConfig.MainFormSize.Y = Height;
        }

        private void TextBoxBTCAddress_Enter(object sender, EventArgs e)
        {
            //var btc = ConfigManager.GeneralConfig.BitcoinAddress.Trim();
            //if (btc == "")
            //{
            //    var loginForm = new LoginForm();
            //    this.SetChildFormCenter(loginForm);
            //    loginForm.ShowDialog();
            //    if (BitcoinAddress.ValidateBitcoinAddress(loginForm.Btc))
            //    {
            //        ConfigManager.GeneralConfig.BitcoinAddress = loginForm.Btc;
            //        ConfigManager.GeneralConfigFileCommit();
            //        this.textBoxBTCAddress.Text = loginForm.Btc;
            //    }
            //}
        }

        #region State callbacks

        private void OnServiceLocationChanged(object sender, int e)
        {
            FormHelpers.SafeInvoke(this, () => { comboBoxLocation.SelectedIndex = e; });
        }

        private void OnWorkerNameChanged(object sender, string e)
        {
            FormHelpers.SafeUpdateTextbox(textBoxWorkerName, e);
        }

        private void OnBtcAddressChanged(object sender, string e)
        {
            FormHelpers.SafeUpdateTextbox(textBoxBTCAddress, e);
        }

        private void OnVersionUpdate(object sender, Version version)
        {
            // Trying to keep GUI-specific stuff (such as version update label translation) out of business code
            var displayNewVer = string.Format(Tr("IMPORTANT! New version v{0} has\r\nbeen released. Click here to download it."), version);
            FormHelpers.SafeInvoke(this, () => { linkLabelNewVersion.Text = displayNewVer; });
        }

        #endregion
    }
}
