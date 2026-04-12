using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
<<<<<<< HEAD
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
=======
using System.Text.Json;
using System.Windows;
>>>>>>> c69be5c4950bc482a4a0fd3c6e85e97a8d570b2d

namespace NameCube.Setting
{
    /// <summary>
    /// ThanksWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ThanksWindow : Window
    {
        public class NuGetPackage
        {
            public string Name { get; set; }
            public string Version { get; set; }
        }
<<<<<<< HEAD
=======

>>>>>>> c69be5c4950bc482a4a0fd3c6e85e97a8d570b2d
        public ThanksWindow()
        {
            InitializeComponent();
            Loaded += CreditsWindow_Loaded;
        }
<<<<<<< HEAD
=======

>>>>>>> c69be5c4950bc482a4a0fd3c6e85e97a8d570b2d
        private async void CreditsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var packages = await System.Threading.Tasks.Task.Run(() => LoadPackagesFromCreditsFile());
            PackagesListView.ItemsSource = packages;
        }

        private List<NuGetPackage> LoadPackagesFromCreditsFile()
        {
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string jsonPath = Path.Combine(exeDir, "credits.json");

            if (!File.Exists(jsonPath))
            {
                return new List<NuGetPackage>
                {
                    new NuGetPackage { Name = "未找到致谢文件", Version = "请重新生成项目" }
                };
            }

            try
            {
                string rawContent = File.ReadAllText(jsonPath);
                // 关键：找到第一个 '{' 字符，之前的文本全部忽略
                int startIdx = rawContent.IndexOf('{');
                if (startIdx == -1)
                    throw new InvalidOperationException("credits.json 中未找到有效的 JSON 起始符号 '{'");

                string jsonContent = rawContent.Substring(startIdx);
                using JsonDocument doc = JsonDocument.Parse(jsonContent);
                JsonElement root = doc.RootElement;

                var packages = new List<NuGetPackage>();

                // 解析 JSON 结构（根据您提供的实际格式）
                if (root.TryGetProperty("projects", out JsonElement projects))
                {
                    foreach (JsonElement project in projects.EnumerateArray())
                    {
                        if (project.TryGetProperty("frameworks", out JsonElement frameworks))
                        {
                            foreach (JsonElement framework in frameworks.EnumerateArray())
                            {
                                if (framework.TryGetProperty("topLevelPackages", out JsonElement topLevelPackages))
                                {
                                    foreach (JsonElement pkg in topLevelPackages.EnumerateArray())
                                    {
                                        // 注意：实际字段名是 "id" 和 "resolvedVersion"，不是 "name"
                                        string name = pkg.GetProperty("id").GetString();
                                        string version = pkg.GetProperty("resolvedVersion").GetString();
                                        if (!string.IsNullOrEmpty(name))
                                        {
                                            packages.Add(new NuGetPackage { Name = name, Version = version });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return packages;
            }
            catch (Exception ex)
            {
                Log.Debug($"解析 credits.json 失败: {ex.Message}");
                return new List<NuGetPackage>
                {
                    new NuGetPackage { Name = "解析错误", Version = ex.Message }
                };
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> c69be5c4950bc482a4a0fd3c6e85e97a8d570b2d
