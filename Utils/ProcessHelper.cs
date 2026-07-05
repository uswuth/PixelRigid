using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PixelRigid.Utils
{
    internal static class ProcessHelper
    {
        private const string TaskName = "PixelRigid_Autostart";

        public static bool IsScheduledTaskRegistered()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = $"/Query /TN \"{TaskName}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            try
            {
                using (var p = Process.Start(psi))
                {
                    p.WaitForExit();
                    return p.ExitCode == 0;
                }
            }
            catch { return false; }
        }

        public static void RegisterScheduledTask(string exePath)
        {
            string xml = $@"<?xml version=""1.0"" encoding=""UTF-16""?>
<Task version=""1.2"" xmlns=""http://schemas.microsoft.com/windows/2004/02/mit/task"">
  <Triggers>
    <LogonTrigger>
      <Enabled>true</Enabled>
    </LogonTrigger>
  </Triggers>
  <Principals>
    <Principal id=""Author"">
      <LogonType>InteractiveToken</LogonType>
      <RunLevel>HighestAvailable</RunLevel>
    </Principal>
  </Principals>
  <Settings>
    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
    <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>
    <Priority>7</Priority>
  </Settings>
  <Actions>
    <Exec>
      <Command>{exePath}</Command>
    </Exec>
  </Actions>
</Task>";

            string tmp = Path.GetTempFileName() + ".xml";
            File.WriteAllText(tmp, xml, Encoding.Unicode);

            var psi = new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = $"/Create /TN \"{TaskName}\" /XML \"{tmp}\" /F",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var p = Process.Start(psi))
                p.WaitForExit();

            File.Delete(tmp);
        }

        public static void RemoveScheduledTask()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = $"/Delete /TN \"{TaskName}\" /F",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var p = Process.Start(psi))
                p.WaitForExit();
        }
    }
}