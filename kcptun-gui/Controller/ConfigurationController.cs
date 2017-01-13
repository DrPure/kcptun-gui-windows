﻿using System;

using kcptun_gui.Model;

namespace kcptun_gui.Controller
{
    public class ConfigurationController
    {
        private MainController controller;

        private Configuration _config;

        public event EventHandler ConfigChanged;
        public event EventHandler EnableChanged;
        public event EventHandler VerboseChanged;
        public event EventHandler ServerIndexChanged;
        public event EventHandler KCPTunPathChanged;
        public event EventHandler StatisticsEnableChanged;
        public event EventHandler CheckGUIUpdateChanged;
        public event EventHandler CheckKCPTunUpdateChanged;
        public event EventHandler AutoUpgradeKCPTunChanged;
        public event EventHandler LanguageChanged;
        public event EventHandler SNMPConfigChanged;

        public ConfigurationController(MainController controller)
        {
            this.controller = controller;
            _config = Configuration.Load();
            if (_config.servers.Count == 0)
            {
                _config.servers.Add(Configuration.GetDefaultServer());
                _config.index = 0;
            }
        }

        public Server GetCurrentServer()
        {
            return _config.GetCurrentServer();
        }

        // always return copy
        public Configuration GetConfigurationCopy()
        {
            return Configuration.Load();
        }

        // always return current instance
        public Configuration GetCurrentConfiguration()
        {
            return _config;
        }

        public void ToggleEnable(bool enabled)
        {
            if (_config.enabled != enabled)
            {
                _config.enabled = enabled;
                SaveConfig(_config);
                if (EnableChanged != null)
                    EnableChanged.Invoke(this, new EventArgs());
            }
        }

        public void ToggleVerboseLogging(bool enabled)
        {
            if (_config.verbose != enabled)
            {
                _config.verbose = enabled;
                SaveConfig(_config);
                if (VerboseChanged != null)
                    VerboseChanged.Invoke(this, new EventArgs());
            }
        }

        public void ToggleCheckGUIUpdate(bool enabled)
        {
            if (_config.check_gui_update != enabled)
            {
                _config.check_gui_update = enabled;
                SaveConfig(_config);
                if (CheckGUIUpdateChanged != null)
                    CheckGUIUpdateChanged.Invoke(this, new EventArgs());
            }
        }

        public void ToggleCheckKCPTunUpdate(bool enabled)
        {
            if (_config.check_kcptun_update != enabled)
            {
                _config.check_kcptun_update = enabled;
                SaveConfig(_config);
                if (CheckKCPTunUpdateChanged != null)
                    CheckKCPTunUpdateChanged.Invoke(this, new EventArgs());
            }
        }

        public void ToggleAutoUpgradeKCPTun(bool enabled)
        {
            if (_config.auto_upgrade_kcptun != enabled)
            {
                _config.auto_upgrade_kcptun = enabled;
                SaveConfig(_config);
                if (AutoUpgradeKCPTunChanged != null)
                    AutoUpgradeKCPTunChanged.Invoke(this, new EventArgs());
            }
        }

        public void SelectServerIndex(int index)
        {
            if (_config.index != index)
            {
                _config.index = index;
                SaveConfig(_config);
                if (ServerIndexChanged != null)
                    ServerIndexChanged.Invoke(this, new EventArgs());
            }
        }

        public void SelectLanguage(I18N.Lang lang)
        {
            if (_config.language != lang.name)
            {
                _config.language = lang.name;
                SaveConfig(_config);
                if (LanguageChanged != null)
                    LanguageChanged.Invoke(this, new EventArgs());
            }
        }

        public void ChangeKCPTunPath(string kcptunPath)
        {
            if (_config.kcptun_path != kcptunPath)
            {
                _config.kcptun_path = kcptunPath;
                SaveConfig(_config);
                if (KCPTunPathChanged != null)
                    KCPTunPathChanged.Invoke(this, new EventArgs());
            }
        }

        public void ToggleStatisticsEnable(bool enabled)
        {
            if (_config.statistics_enabled != enabled)
            {
                _config.statistics_enabled = enabled;
                SaveConfig(_config);
                if (StatisticsEnableChanged != null)
                    StatisticsEnableChanged.Invoke(this, new EventArgs());
            }
        }

        public void ChangeSNMPConfig(SNMPConfiguration config)
        {
            if (!_config.snmp.Equals(config))
            {
                _config.snmp = config;
                SaveConfig(_config);
                if (SNMPConfigChanged != null)
                    SNMPConfigChanged.Invoke(this, new EventArgs());
            }
        }

        public void SaveConfig(Configuration config)
        {
            Configuration.Save(config);
            this._config = config;
            if (config.isDefault)
                config.isDefault = false;
            if (ConfigChanged != null)
                ConfigChanged.Invoke(this, new EventArgs());
        }
    }
}
