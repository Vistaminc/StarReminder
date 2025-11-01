using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MediaDetectionSystem.Models;

namespace MediaDetectionSystem.Services
{
    /// <summary>
    /// 更新检查器
    /// </summary>
    public class UpdateChecker
    {
        private const string UPDATE_CHECK_URL = "https://api.github.com/repos/vistaminc/StarReminder/releases/latest";
        private readonly HttpClient _httpClient;
        private readonly string _currentVersion;

        public UpdateChecker()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "StarReminder");
            
            // 使用 VersionInfo 统一获取当前版本
            _currentVersion = VersionInfo.Version;
        }

        /// <summary>
        /// 检查是否有新版本
        /// </summary>
        /// <returns>返回更新信息，如果没有更新则返回null</returns>
        public async Task<UpdateInfo?> CheckForUpdatesAsync()
        {
            try
            {
                Debug.WriteLine($"[UpdateChecker] 当前版本: {_currentVersion}");
                Debug.WriteLine($"[UpdateChecker] 检查更新中...");

                var response = await _httpClient.GetStringAsync(UPDATE_CHECK_URL);
                var releaseInfo = JsonSerializer.Deserialize<GitHubRelease>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (releaseInfo == null)
                {
                    Debug.WriteLine($"[UpdateChecker] 无法解析更新信息");
                    return null;
                }

                var latestVersion = releaseInfo.TagName?.TrimStart('v') ?? "0.0.0";
                Debug.WriteLine($"[UpdateChecker] 最新版本: {latestVersion}");

                if (IsNewerVersion(latestVersion, _currentVersion))
                {
                    Debug.WriteLine($"[UpdateChecker] 发现新版本: {latestVersion}");
                    return new UpdateInfo
                    {
                        LatestVersion = latestVersion,
                        CurrentVersion = _currentVersion,
                        ReleaseNotes = releaseInfo.Body ?? "",
                        DownloadUrl = releaseInfo.HtmlUrl ?? "",
                        PublishedAt = releaseInfo.PublishedAt
                    };
                }

                Debug.WriteLine($"[UpdateChecker] 当前已是最新版本");
                return null;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[UpdateChecker] 网络请求失败: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateChecker] 检查更新失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 比较版本号
        /// </summary>
        private bool IsNewerVersion(string latestVersion, string currentVersion)
        {
            try
            {
                var latest = new Version(latestVersion);
                var current = new Version(currentVersion);
                return latest > current;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 打开下载页面
        /// </summary>
        public void OpenDownloadPage(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateChecker] 打开下载页面失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// GitHub Release 信息
    /// </summary>
    public class GitHubRelease
    {
        public string? TagName { get; set; }
        public string? Name { get; set; }
        public string? Body { get; set; }
        public string? HtmlUrl { get; set; }
        public DateTime PublishedAt { get; set; }
    }

    /// <summary>
    /// 更新信息
    /// </summary>
    public class UpdateInfo
    {
        public string LatestVersion { get; set; } = string.Empty;
        public string CurrentVersion { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
    }
}


