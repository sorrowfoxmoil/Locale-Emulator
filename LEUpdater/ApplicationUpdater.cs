﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Amemiya.Net;

namespace LEUpdater
{
    internal static class ApplicationUpdater
    {
        private static string url = string.Empty;

        internal static void CheckApplicationUpdate(string version, NotifyIcon notifyIcon)
        {
            string url = string.Format(@"http://service.watashi.me/le/check.php?ver={0}&lang={1}",
                                       version,
                                       CultureInfo.CurrentUICulture.LCID);

            try
            {
                var client = new WebClientEx(10 * 1000);
                MemoryStream stream = client.DownloadDataStream(url);

                var xmlContent = new XmlDocument();
                xmlContent.Load(stream);

                ProcessUpdate(xmlContent, notifyIcon);
            }
            catch (Exception)
            {
                notifyIcon.Visible = false;
                Environment.Exit(0);
            }
        }

        private static void ProcessUpdate(XmlDocument xmlContent, NotifyIcon notifyIcon)
        {
            string newVer = xmlContent.SelectSingleNode(@"/VersionInfo/Version/text()").Value;

            if (CompareVersion(Application.ProductVersion, newVer))
            {
                try
                {
                    string version = xmlContent.SelectSingleNode(@"/VersionInfo/Version/text()").Value;
                    string date = xmlContent.SelectSingleNode(@"/VersionInfo/Date/text()").Value;
                    url = xmlContent.SelectSingleNode(@"/VersionInfo/Url/text()").Value;
                    string note = xmlContent.SelectSingleNode(@"/VersionInfo/Note/text()").Value;

                    notifyIcon.BalloonTipClicked += (sender, e) =>
                                                    {
                                                        Process.Start(url);

                                                        notifyIcon.Visible = false;
                                                        Environment.Exit(0);
                                                    };
                    notifyIcon.BalloonTipClosed += (sender, e) =>
                                                   {
                                                       notifyIcon.Visible = false;
                                                       Environment.Exit(0);
                                                   };

                    notifyIcon.ShowBalloonTip(0,
                                              String.Format("New Version {0} Available", version),
                                              String.Format("{0}\r\n" +
                                                            "\r\n" +
                                                            "Click here to open download page.",
                                                            note),
                                              ToolTipIcon.Info);
                }
                catch (Exception)
                {
                    notifyIcon.Visible = false;
                    Environment.Exit(0);
                }
            }
            else
            {
                notifyIcon.Visible = false;
                Environment.Exit(0);
            }
        }

        /// <summary>
        ///     If ver2 is bigger than ver1, return true.
        /// </summary>
        /// <param name="oldVer"></param>
        /// <param name="newVer"></param>
        /// <returns></returns>
        private static bool CompareVersion(string oldVer, string newVer)
        {
            var versionOld = new Version(oldVer);
            var versionNew = new Version(newVer);

            return versionOld < versionNew;
        }
    }
}