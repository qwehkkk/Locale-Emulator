﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using LECommonLibrary;

namespace LEGUI
{
    /// <summary>
    ///     Interaction logic for AppConfig.xaml
    /// </summary>
    public partial class AppConfig
    {
        private readonly List<CultureInfo> _cultureInfos = new List<CultureInfo>();
        private readonly List<TimeZoneInfo> _timezones = new List<TimeZoneInfo>();

        public AppConfig()
        {
            InitializeComponent();

            // Region.
            _cultureInfos =
                CultureInfo.GetCultures(CultureTypes.AllCultures)
                           .OrderBy(i => i.DisplayName)
                           .ToList();
            cbLocation.ItemsSource = _cultureInfos.Select(c => c.DisplayName);
            cbLocation.SelectedIndex = _cultureInfos.FindIndex(c => c.Name == "ja-JP");

            //Timezone.
            _timezones = TimeZoneInfo.GetSystemTimeZones().ToList();
            cbTimezone.ItemsSource = _timezones.Select(t => t.DisplayName);
            cbTimezone.SelectedIndex = _timezones.FindIndex(tz => tz.Id == "Tokyo Standard Time");

            //Font.
            cbDefaultFont.Text = "MS Gothic";

            // Load exists config.
            LEProfile[] configs = LEConfig.GetProfiles(App.StandaloneFilePath);
            if (configs.Length > 0)
            {
                LEProfile conf = configs[0];

                if (!String.IsNullOrEmpty(conf.Parameter))
                {
                    tbAppParameter.FontStyle = FontStyles.Normal;
                    tbAppParameter.Text = conf.Parameter;
                }
                cbTimezone.SelectedIndex = _timezones.FindIndex(tz => tz.Id == conf.Timezone);
                cbDefaultFont.Text = conf.DefaultFont;
                cbLocation.SelectedIndex = _cultureInfos.FindIndex(ci => ci.Name == conf.Location);

                cbStartAsSuspend.IsChecked = conf.RunWithSuspend;
            }
        }

        private void bSaveAppSetting_Click(object sender, RoutedEventArgs e)
        {
            var crt = new LEProfile
                {
                    Name = Path.GetFileName(App.StandaloneFilePath),
                    Guid = Guid.NewGuid().ToString(),
                    ShowInMainMenu = false,
                    Parameter =
                        I18n.GetString("EnterArgument") == tbAppParameter.Text ? String.Empty : tbAppParameter.Text,
                    DefaultFont = cbDefaultFont.Text,
                    Location = _cultureInfos[cbLocation.SelectedIndex].Name,
                    Timezone = _timezones[cbTimezone.SelectedIndex].Id,
                    RunWithSuspend = cbStartAsSuspend.IsChecked != null && (bool)cbStartAsSuspend.IsChecked
                };

            LEConfig.SaveApplicationConfigFile(App.StandaloneFilePath, crt);

            //Run the application.
            Process.Start(
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "LEProc.exe"),
                string.Format("-run \"{0}\"", App.StandaloneFilePath.Replace(".le.config", "")));

            Application.Current.Shutdown();
        }

        private void bDeleteAppSetting_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.No == MessageBox.Show(I18n.GetString("ConfirmDel"), "", MessageBoxButton.YesNo))
                return;

            if (File.Exists(App.StandaloneFilePath))
                File.Delete(App.StandaloneFilePath);

            Application.Current.Shutdown();
        }
    }
}