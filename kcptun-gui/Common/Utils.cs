﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using System.Net;
using System.Net.NetworkInformation;
using kcptun_gui.Controller;
using System.Drawing;
using Microsoft.Win32;
using System.Net.Sockets;

namespace kcptun_gui.Common
{
    public class Utils
    {
        private static string TempPath = null;

        public static string GetTempPath()
        {
            if (TempPath == null)
            {
                if (File.Exists(Path.Combine(Application.StartupPath, "kcptun_portable_mode.txt")) ||
                    File.Exists(Path.Combine(Application.StartupPath, "shadowsocks_portable_mode.txt")))
                    try
                    {
                        TempPath = Path.Combine(Application.StartupPath, "temp");
                        if (!Directory.Exists(TempPath))
                            Directory.CreateDirectory(TempPath);
                    }
                    catch (Exception e)
                    {
                        TempPath = Path.GetTempPath();
                        Logging.LogUsefulException(e);
                    }
                else
                    TempPath = Path.GetTempPath();
            }
            return TempPath;
        }

        public static string GetTempPath(string filename)
        {
            return Path.Combine(GetTempPath(), filename);
        }

        public static string UnGzip(byte[] buf)
        {
            byte[] buffer = new byte[1024];
            int n;
            using (MemoryStream sb = new MemoryStream())
            {
                using (GZipStream input = new GZipStream(new MemoryStream(buf),
                                                         CompressionMode.Decompress,
                                                         false))
                {
                    while ((n = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        sb.Write(buffer, 0, n);
                    }
                }
                return System.Text.Encoding.UTF8.GetString(sb.ToArray());
            }
        }

        public static int GetFreePort(ProtocolType protocol, int defaultPort = 12948)
        {
            try
            {
                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] activeEndPoints = protocol == ProtocolType.Tcp ? properties.GetActiveTcpListeners() : properties.GetActiveUdpListeners();

                List<int> usedPorts = new List<int>();
                foreach (IPEndPoint endPoint in activeEndPoints)
                {
                    usedPorts.Add(endPoint.Port);
                }
                for (int port = defaultPort; port <= 65535; port++)
                {
                    if (!usedPorts.Contains(port))
                    {
                        return port;
                    }
                }
            }
            catch (Exception e)
            {
                // in case access denied
                Logging.LogUsefulException(e);
                return defaultPort;
            }
            throw new Exception("No free port found.");
        }

        public static bool CheckIfTcpPortInUse(int port)
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CheckIfUdpPortInUse(int port)
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveUdpListeners();

            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    return true;
                }
            }
            return false;
        }

        public static string FormatSize(long n)
        {
            TrafficSize size = new TrafficSize(n);
            return size.ToString();
        }

        public static Bitmap ToGray(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    Color color = bmp.GetPixel(i, j);
                    int gray = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                    Color newColor = Color.FromArgb(color.A, gray, gray, gray);
                    bmp.SetPixel(i, j, newColor);
                }
            }
            return bmp;
        }

        private static Color flyBlue = Color.FromArgb(25, 125, 191);

        public static Bitmap ToBlue(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    Color color = bmp.GetPixel(i, j);
                    // Muliply with flyBlue
                    int red = color.R * flyBlue.R / 255;
                    int green = color.G * flyBlue.G / 255;
                    int blue = color.B * flyBlue.B / 255;
                    Color newColor = Color.FromArgb(color.A, red, green, blue);
                    bmp.SetPixel(i, j, newColor);
                }
            }
            return bmp;
        }

        public static RegistryKey OpenUserRegKey(string name, bool writable)
        {
            // we are building x86 binary for both x86 and x64, which will
            // cause problem when opening registry key
            // detect operating system instead of CPU
            RegistryKey userKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser,
                Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
            userKey = userKey.OpenSubKey(name, writable);
            return userKey;
        }

        public static IPEndPoint ToEndPoint(string address)
        {
            int index = address.LastIndexOf(':');
            string host = address.Substring(0, index);
            string port = address.Substring(index + 1);
            IPAddress ipAddress;
            if (string.IsNullOrEmpty(host))
                ipAddress = IPAddress.Any;
            else if (!IPAddress.TryParse(host, out ipAddress))
                ipAddress = Dns.GetHostEntry(host).AddressList[0];
            IPEndPoint ep = new IPEndPoint(ipAddress, Convert.ToInt32(port));
            return ep;
        }
    }
}
