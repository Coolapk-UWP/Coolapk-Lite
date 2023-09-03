using CoolapkLite.Models.Network;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.ApplicationModel;

#if !CANARY
using System.Collections.Generic;
#endif

namespace CoolapkLite.Helpers
{
    public static class UpdateHelper
    {
#if CANARY
        private const string DevOps_API = "https://dev.azure.com/{0}/{1}/_apis/pipelines/{2}/runs";

        public static Task<UpdateInfo> CheckUpdateAsync(string organization, string project, uint pipelineId, bool isBackground = false)
        {
            PackageVersion currentVersion = Package.Current.Id.Version;
            return CheckUpdateAsync(organization, project, pipelineId, currentVersion, isBackground);
        }

        public static async Task<UpdateInfo> CheckUpdateAsync(string organization, string project, uint pipelineId, PackageVersion currentVersion, bool isBackground = false)
        {
            if (string.IsNullOrEmpty(organization))
            {
                throw new ArgumentNullException(nameof(organization));
            }

            if (string.IsNullOrEmpty(project))
            {
                throw new ArgumentNullException(nameof(project));
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string url = string.Format(DevOps_API, organization, project, pipelineId);
                    HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    RunsInfo result = JsonConvert.DeserializeObject<RunsInfo>(responseBody);

                    if (result != null)
                    {
                        RunInfo run = result.Value.FirstOrDefault((x) => x.Result == "succeeded");

                        if (run != null)
                        {
                            SystemVersionInfo newVersionInfo = GetAsVersionInfo(run.CreatedDate, run.ID);
                            int major = currentVersion.Major <= 0 ? 0 : currentVersion.Major;
                            int minor = currentVersion.Minor <= 0 ? 0 : currentVersion.Minor;
                            int build = currentVersion.Build <= 0 ? 0 : currentVersion.Build;
                            int revision = currentVersion.Revision <= 0 ? 0 : currentVersion.Revision;

                            SystemVersionInfo currentVersionInfo = new SystemVersionInfo(major, minor, build, revision);

                            return new UpdateInfo
                            {
                                CreatedAt = Convert.ToDateTime(run?.CreatedDate),
                                PublishedAt = Convert.ToDateTime(run?.FinishedDate),
                                ReleaseUrl = run?.Links?.Web?.Href,
                                IsExistNewVersion = newVersionInfo > currentVersionInfo,
                                Version = newVersionInfo
                            };
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                SettingsHelper.LogManager.GetLogger(nameof(UpdateHelper)).Error(e.ExceptionToMessage(), e);
                if (!isBackground) { UIHelper.ShowHttpExceptionMessage(e); }
            }

            return null;
        }

        private static SystemVersionInfo GetAsVersionInfo(DateTime dateTime, int id)
        {
            int major = int.Parse(dateTime.ToString("yy"));
            int minor = int.Parse(dateTime.ToString("MMdd"));
            return new SystemVersionInfo(major, minor, id);
        }
#else
        private const string KKPP_API = "https://v2.kkpp.cc/repos/{0}/{1}/releases/latest";
        private const string GITHUB_API = "https://api.github.com/repos/{0}/{1}/releases/latest";

        public static Task<UpdateInfo> CheckUpdateAsync(string username, string repository, bool isBackground = false)
        {
            PackageVersion currentVersion = Package.Current.Id.Version;
            return CheckUpdateAsync(username, repository, currentVersion, isBackground);
        }

        public static async Task<UpdateInfo> CheckUpdateAsync(string username, string repository, PackageVersion currentVersion, bool isBackground = false)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (string.IsNullOrEmpty(repository))
            {
                throw new ArgumentNullException(nameof(repository));
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", username);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    string url = string.Format(GITHUB_API, username, repository);
                    HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    UpdateInfo result = JsonConvert.DeserializeObject<UpdateInfo>(responseBody);

                    if (result != null)
                    {
                        SystemVersionInfo newVersionInfo = GetAsVersionInfo(result.TagName);
                        int major = currentVersion.Major <= 0 ? 0 : currentVersion.Major;
                        int minor = currentVersion.Minor <= 0 ? 0 : currentVersion.Minor;
                        int build = currentVersion.Build <= 0 ? 0 : currentVersion.Build;
                        int revision = currentVersion.Revision <= 0 ? 0 : currentVersion.Revision;

                        SystemVersionInfo currentVersionInfo = new SystemVersionInfo(major, minor, build, revision);

                        result.IsExistNewVersion = newVersionInfo > currentVersionInfo;
                        result.Version = newVersionInfo;

                        return result;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                SettingsHelper.LogManager.GetLogger(nameof(UpdateHelper)).Error(e.ExceptionToMessage(), e);
                if (!isBackground) { UIHelper.ShowHttpExceptionMessage(e); }
            }

            return null;
        }

        private static SystemVersionInfo GetAsVersionInfo(string version)
        {
            List<int> numbs = GetVersionNumbers(version).Split('.').Select(int.Parse).ToList();
            return numbs.Count <= 1
                ? new SystemVersionInfo(numbs[0], 0, 0, 0)
                : numbs.Count <= 2
                    ? new SystemVersionInfo(numbs[0], numbs[1], 0, 0)
                    : numbs.Count <= 3
                        ? new SystemVersionInfo(numbs[0], numbs[1], numbs[2], 0)
                        : new SystemVersionInfo(numbs[0], numbs[1], numbs[2], numbs[3]);
        }

        private static string GetVersionNumbers(string version)
        {
            string allowedChars = "01234567890.";
            return new string(version.Where(allowedChars.Contains).ToArray());
        }
#endif
    }
}
