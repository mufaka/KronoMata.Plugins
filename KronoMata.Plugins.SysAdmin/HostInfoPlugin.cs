using Hardware.Info;
using KronoMata.Public;
using System.Text;

namespace KronoMata.Plugins.SysAdmin
{
    public class HostInfoPlugin : IPlugin
    {
        public string Name { get { return "Host Info Plugin"; } }

        public string Description { get { return "Gets host information system running the KronoMata Agent."; } }

        public string Version { get { return "1.2"; } }

        private void AppendBooleanParameter(List<PluginParameter> parameters, string name, string description)
        {
            parameters.Add(new PluginParameter()
            {
                Name = name,
                Description = description,
                DataType = ConfigurationDataType.Boolean,
                IsRequired = false
            });
        }

        public List<PluginParameter> Parameters
        {
            get
            {
                var parameters = new List<PluginParameter>();

                AppendBooleanParameter(parameters, "Get OS", "Returns Operation System Information.");
                AppendBooleanParameter(parameters, "Get CPU", "Returns CPU List");
                AppendBooleanParameter(parameters, "Get Memory List", "Returns Memory List");
                AppendBooleanParameter(parameters, "Get Memory Status", "Returns Memory Status");
                AppendBooleanParameter(parameters, "Get Drives", "Returns Drive List");

                return parameters;
            }
        }

        public List<PluginResult> Execute(Dictionary<string, string> systemConfig, Dictionary<string, string> pluginConfig)
        {
            var log = new List<PluginResult>();

            try
            {
                var hardwareInfo = new HardwareInfo(useAsteriskInWMI: false);
                var logBuffer = new StringBuilder();

                if (pluginConfig.ContainsKey("Get OS") && pluginConfig["Get OS"] == "True")
                {
                    hardwareInfo.RefreshOperatingSystem();
                    logBuffer.AppendLine("Operating System");
                    logBuffer.AppendLine("----------------");
                    logBuffer.AppendLine(hardwareInfo.OperatingSystem.ToString());
                    logBuffer.AppendLine();
                }

                if (pluginConfig.ContainsKey("Get CPU") && pluginConfig["Get CPU"] == "True")
                {
                    hardwareInfo.RefreshCPUList(includePercentProcessorTime: false);
                    logBuffer.AppendLine("CPU Information");
                    logBuffer.AppendLine("-------------");

                    foreach (CPU cpu in hardwareInfo.CpuList)
                    {
                        logBuffer.AppendLine(cpu.ToString());

                        foreach (CpuCore cpuCore in cpu.CpuCoreList)
                        {
                            logBuffer.AppendLine($"\t{cpuCore.ToString()}");
                        }
                    }
                    logBuffer.AppendLine();
                }

                if (pluginConfig.ContainsKey("Get Memory List") && pluginConfig["Get Memory List"] == "True")
                {
                    hardwareInfo.RefreshMemoryList();
                    logBuffer.AppendLine("Memory List");
                    logBuffer.AppendLine("-----------");

                    foreach (Memory memory in hardwareInfo.MemoryList)
                    {
                        logBuffer.AppendLine(memory.ToString());
                    }
                    logBuffer.AppendLine();
                }

                if (pluginConfig.ContainsKey("Get Memory Status") && pluginConfig["Get Memory Status"] == "True")
                {
                    hardwareInfo.RefreshMemoryStatus();
                    logBuffer.AppendLine("Memory Status");
                    logBuffer.AppendLine("-------------");
                    logBuffer.AppendLine(hardwareInfo.MemoryStatus.ToString());
                    logBuffer.AppendLine();
                }

                if (pluginConfig.ContainsKey("Get Drives") && pluginConfig["Get Drives"] == "True")
                {
                    hardwareInfo.RefreshDriveList();

                    logBuffer.AppendLine("Drive Information");
                    logBuffer.AppendLine("-----------------");

                    foreach (Drive drive in hardwareInfo.DriveList)
                    {
                        logBuffer.AppendLine(drive.ToString());

                        foreach (Partition partition in drive.PartitionList)
                        {
                            logBuffer.AppendLine($"\t{partition.ToString()}");

                            foreach (Volume volume in partition.VolumeList)
                            {
                                logBuffer.AppendLine($"\t\t{volume.ToString()}");
                            }
                        }
                    }

                    logBuffer.AppendLine();
                }

                var detail = "No information found.";
                if (logBuffer.Length > 0)
                {
                    detail = logBuffer.ToString();
                }

                log.Add(new PluginResult()
                {
                    IsError = false,
                    Message = $"{Environment.MachineName} Host Information",
                    Detail = detail
                });

            }
            catch (Exception ex)
            {
                log.Add(new PluginResult()
                {
                    IsError = true,
                    Message = ex.Message,
                    Detail = ex.StackTrace ?? String.Empty
                });
            }

            return log;
        }
    }
}
