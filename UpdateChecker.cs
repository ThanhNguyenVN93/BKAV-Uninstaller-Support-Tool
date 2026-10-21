using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    /// <summary>Kiểm tra bản phát hành mới nhất trên GitHub Releases của repo.</summary>
    internal static class UpdateChecker
    {
        private const string RepoUrl = "https://github.com/ThanhNguyenVN93/BKAV-Uninstaller-Support-Tool";
        private const string ReleasesApiUrl =
            "https://api.github.com/repos/ThanhNguyenVN93/BKAV-Uninstaller-Support-Tool/releases/latest";

        internal sealed class UpdateInfo
        {
            public Version Version;
            public string TagName;
            public string HtmlUrl;
        }

        /// <summary>Trả về thông tin bản mới nếu có, hoặc null nếu không xác định được / không có mạng.</summary>
        internal static async Task<UpdateInfo> CheckAsync()
        {
            using (var http = new HttpClient { Timeout = TimeSpan.FromSeconds(6) })
            {
                // GitHub API bắt buộc phải có User-Agent, nếu không sẽ trả 403.
                http.DefaultRequestHeaders.UserAgent.ParseAdd("BKAV-Uninstaller-Tool-UpdateCheck");

                string json = await http.GetStringAsync(ReleasesApiUrl).ConfigureAwait(false);

                var tagMatch = Regex.Match(json, "\"tag_name\"\\s*:\\s*\"([^\"]+)\"");
                if (!tagMatch.Success) return null;

                string tag = tagMatch.Groups[1].Value;
                string versionText = tag.TrimStart('v', 'V');
                if (!Version.TryParse(versionText, out var version)) return null;

                var urlMatch = Regex.Match(json, "\"html_url\"\\s*:\\s*\"([^\"]+)\"");

                return new UpdateInfo
                {
                    Version = version,
                    TagName = tag,
                    HtmlUrl = urlMatch.Success ? urlMatch.Groups[1].Value : RepoUrl + "/releases/latest"
                };
            }
        }
    }
}
