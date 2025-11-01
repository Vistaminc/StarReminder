using System;
using System.Reflection;

namespace MediaDetectionSystem.Models
{
    /// <summary>
    /// 版本信息管理类 - 自动从程序集读取版本号
    /// </summary>
    public static class VersionInfo
    {
        private static string? _version;
        private static string? _fullVersion;
        private static Version? _assemblyVersion;

        /// <summary>
        /// 获取主版本号（格式：1.2.1）
        /// </summary>
        public static string Version
        {
            get
            {
                if (_version == null)
                {
                    _version = Assembly.GetExecutingAssembly()
                        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                        ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
                    
                    // 如果版本号包含 "+" 或 "-"（如 1.2.1+abc），只取前面的部分
                    if (_version.Contains("+") || _version.Contains("-"))
                    {
                        var parts = _version.Split(new[] { '+', '-' }, StringSplitOptions.RemoveEmptyEntries);
                        _version = parts[0];
                    }
                }
                return _version;
            }
        }

        /// <summary>
        /// 获取完整版本号（包含所有信息）
        /// </summary>
        public static string FullVersion
        {
            get
            {
                if (_fullVersion == null)
                {
                    _fullVersion = Assembly.GetExecutingAssembly()
                        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                        ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
                }
                return _fullVersion;
            }
        }

        /// <summary>
        /// 获取版本号对象
        /// </summary>
        public static Version AssemblyVersion
        {
            get
            {
                if (_assemblyVersion == null)
                {
                    _assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);
                }
                return _assemblyVersion;
            }
        }

        /// <summary>
        /// 获取格式化后的版本字符串（用于显示）
        /// </summary>
        public static string DisplayVersion => $"v{Version}";

        /// <summary>
        /// 获取程序集名称
        /// </summary>
        public static string AssemblyName => Assembly.GetExecutingAssembly().GetName().Name ?? "StarReminder";

        /// <summary>
        /// 强制刷新版本信息（通常不需要调用）
        /// </summary>
        public static void Refresh()
        {
            _version = null;
            _fullVersion = null;
            _assemblyVersion = null;
        }
    }
}

