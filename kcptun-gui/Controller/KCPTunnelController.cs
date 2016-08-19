﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;

using kcptun_gui.Model;
using kcptun_gui.Properties;
using kcptun_gui.Util;

namespace kcptun_gui.Controller
{
    public class KCPTunnelController
    {
        const string FILENAME = "kcptun-client.exe";

        private MainController controller;

        private MyProcess _process;
        private Server _server;

        static KCPTunnelController()
        {
            try
            {
                if (Environment.Is64BitOperatingSystem)
                    FileManager.UncompressFile(Utils.GetTempPath(FILENAME), Resources.client_windows_amd64_exe);
                else
                    FileManager.UncompressFile(Utils.GetTempPath(FILENAME), Resources.client_windows_386_exe);
            }
            catch (IOException e)
            {
                Logging.LogUsefulException(e);
            }
        }

        public Server Server
        {
            get
            {
                return _server;
            }
            set
            {
                _server = value;
            }
        }

        public bool IsRunning
        {
            get
            {
                return _process != null;
            }
        }

        public event EventHandler Started;
        public event EventHandler Stoped;

        public KCPTunnelController(MainController controller)
        {
            this.controller = controller;
        }

        public string GetKCPTunPath()
        {
            Configuration config = controller.ConfigController.GetCurrentConfiguration();
            if (string.IsNullOrEmpty(config.kcptun_path))
                return Utils.GetTempPath(FILENAME);
            else
                return config.kcptun_path;
        }

        public void Start()
        {
            if (IsRunning)
                throw new Exception("Kcptun running");
            if (_server == null)
                throw new Exception("No Server");

            try
            {
                string filename = GetKCPTunPath();
                Console.WriteLine($"Executable: {filename}");
                MyProcess p = new MyProcess(_server);
                _process = p;
                p.StartInfo.FileName = filename;
                p.StartInfo.Arguments = BuildArguments(_server);
                p.StartInfo.WorkingDirectory = Utils.GetTempPath();
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.ErrorDataReceived += OnProcessErrorDataReceived;
                p.OutputDataReceived += OnProcessOutputDataReceived;
                p.Exited += OnProcessExited;
                p.EnableRaisingEvents = true;
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                Console.WriteLine("kcptun started - " + p.server.FriendlyName());

                if (Started != null)
                    Started.Invoke(this, new EventArgs());
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
            }
        }

        public void Stop()
        {
            MyProcess p = _process;
            if (p != null)
            {
                _process = null;
                KillProcess(p);
                p.Dispose();

                Console.WriteLine("kcptun stoped - " + p.server.FriendlyName());

                if (Stoped != null)
                    Stoped.Invoke(this, new EventArgs());
            }
        }

        private void WriteToLogFile(MyProcess process, string s)
        {
            if (s != null)
            {
                using (StringReader sr = new StringReader(s))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        Console.WriteLine(process.server.FriendlyName() + " - " + line);
                    }
                }
            }
        }

        private void OnProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (controller.ConfigController.GetCurrentConfiguration().verbose)
                WriteToLogFile(sender as MyProcess, e.Data);
        }

        private void OnProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (controller.ConfigController.GetCurrentConfiguration().verbose)
                WriteToLogFile(sender as MyProcess, e.Data);
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            if (sender == _process)
                Stop();
        }

        private static void KillProcess(Process p)
        {
            try
            {
                if (!p.HasExited)
                {
                    p.CloseMainWindow();
                    p.WaitForExit(100);
                    if (!p.HasExited)
                    {
                        p.Kill();
                        p.WaitForExit();
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
            }
        }

        public static void KillAll()
        {
            Process[] processes = Process.GetProcessesByName("kcptun-client");
            foreach (Process p in processes)
            {
                KillProcess(p);
            }
        }

        public string GetKcptunVersion()
        {
            string version = "";
            try
            {
                string filename = GetKCPTunPath();
                Console.WriteLine($"Executable: {filename}");
                Process p = new Process();
                // Configure the process using the StartInfo properties.
                p.StartInfo.FileName = filename;
                p.StartInfo.Arguments = "-v";
                p.StartInfo.WorkingDirectory = Utils.GetTempPath();
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();
                int count = 300;
                while (!p.HasExited && count > 0)
                {
                    p.WaitForExit(100);
                    count--;
                }
                if (count == 0)
                {
                    Console.WriteLine("Can't get kcptun version.");
                    return "";
                }
                version = p.StandardOutput.ReadToEnd();
            }
            catch(Exception e)
            {
                Logging.LogUsefulException(e);
            }
            return version;
        }

        public static string BuildArguments(Server server)
        {
            StringBuilder arguments = new StringBuilder();
            if (server.mode == kcptun_mode.manual_all)
            {
                arguments.Append($" -l \"{server.localaddr}\"");
                arguments.Append($" -r \"{server.remoteaddr}\"");
                arguments.Append($" {server.extend_arguments}");
            }
            else
            {
                arguments.Append($" -l \"{server.localaddr}\"");
                arguments.Append($" -r \"{server.remoteaddr}\"");
                arguments.Append($" --crypt {server.crypt}");
                if (server.crypt != kcptun_crypt.none)
                    arguments.Append($" --key \"{server.key}\"");
                arguments.Append($" --mode \"{server.mode}\"");
                arguments.Append($" --conn {server.conn}");
                arguments.Append($" --mtu {server.mtu}");
                arguments.Append($" --sndwnd {server.sndwnd}");
                arguments.Append($" --rcvwnd {server.rcvwnd}");
                if (server.nocomp)
                    arguments.Append($" --nocomp");
                arguments.Append($" --datashard {server.datashard}");
                arguments.Append($" --parityshard {server.parityshard}");
                arguments.Append($" --dscp {server.dscp}");
                if (server.mode == kcptun_mode.manual)
                {
                    arguments.Append($" --nodelay {server.nodelay}");
                    arguments.Append($" --resend {server.resend}");
                    arguments.Append($" --nc {server.nc}");
                    arguments.Append($" --interval {server.interval}");
                }
                else
                {
                    /*do nothing*/
                }
                if (!string.IsNullOrEmpty(server.extend_arguments))
                    arguments.Append($" {server.extend_arguments}");
            }

            return arguments.ToString().Trim();
        }

        class MyProcess: Process
        {
            public Server server { get; private set; }

            public MyProcess(Server server)
            {
                this.server = server;
            }
        }
    }
}
