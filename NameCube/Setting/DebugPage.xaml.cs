using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NameCube.FirstUse;
using Serilog; // 添加Serilog引用

namespace NameCube.Setting
{
    /// <summary>
    /// DebugPage.xaml 的交互逻辑
    /// </summary>
    public partial class DebugPage : Page
    {
        private static readonly ILogger _logger = Log.ForContext<DebugPage>(); // 添加Serilog日志实例

        public DebugPage()
        {
            InitializeComponent();
            _logger.Warning("调试页面被访问");
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("退出调试模式");
            GlobalVariablesData.config.AllSettings.debug = false;
            GlobalVariablesData.SaveConfig();
        }

        //空引用异常
        public class NullReferenceCrash
        {
            private static readonly ILogger _logger = Log.ForContext<NullReferenceCrash>();

            public static void TriggerNullReference()
            {
                _logger.Warning("触发空引用异常");
                string text = null;
                int length = text.Length;
            }

            public static void TriggerNullReferenceInMethod()
            {
                _logger.Warning("触发方法中的空引用异常");
                List<string> list = null;
                list.Add("test");
            }
        }

        //索引越界异常
        public class IndexOutOfRangeCrash
        {
            private static readonly ILogger _logger = Log.ForContext<IndexOutOfRangeCrash>();

            public static void TriggerIndexOutOfRange()
            {
                _logger.Warning("触发索引越界异常");
                int[] numbers = { 1, 2, 3 };
                // 访问不存在的索引
                for (int i = 0; i <= numbers.Length; i++)
                {
                    Console.WriteLine(numbers[i]);
                }
            }

            public static void TriggerListIndexOutOfRange()
            {
                _logger.Warning("触发列表索引越界异常");
                List<string> items = new List<string> { "A", "B", "C" };
                // 列表索引越界
                string item = items[10];
            }
        }

        //堆栈溢出异常
        public class StackOverflowCrash
        {
            private static readonly ILogger _logger = Log.ForContext<StackOverflowCrash>();
            private static int counter = 0;

            public static void TriggerStackOverflow()
            {
                _logger.Warning("触发堆栈溢出异常");
                counter++;
                // 无限递归导致堆栈溢出
                TriggerStackOverflow();
            }

            public static void MethodA()
            {
                MethodB();
            }

            public static void MethodB()
            {
                MethodA();
            }
        }

        //内存不足异常
        public class OutOfMemoryCrash
        {
            private static readonly ILogger _logger = Log.ForContext<OutOfMemoryCrash>();

            public static void TriggerOutOfMemory()
            {
                _logger.Error("触发内存不足异常");
                List<byte[]> memoryHog = new List<byte[]>();

                try
                {
                    while (true)
                    {
                        // 不断分配大内存块
                        byte[] largeArray = new byte[100000000]; // 100MB
                        memoryHog.Add(largeArray);
                        _logger.Debug($"已分配: {memoryHog.Count * 100} MB");
                    }
                }
                catch (OutOfMemoryException ex)
                {
                    _logger.Error(ex, "内存不足异常");
                    throw; // 重新抛出异常
                }
            }
        }

        //多线程竞争条件异常
        public class ThreadingCrash
        {
            private static readonly ILogger _logger = Log.ForContext<ThreadingCrash>();
            private static List<int> sharedList = new List<int>();
            private static bool shouldRun = true;

            public static void TriggerRaceCondition()
            {
                _logger.Warning("触发多线程竞争条件异常");

                // 线程1：不断添加元素
                Thread thread1 = new Thread(() =>
                {
                    while (shouldRun)
                    {
                        sharedList.Add(DateTime.Now.Millisecond);
                        Thread.Sleep(1);
                    }
                });

                // 线程2：不断遍历和修改
                Thread thread2 = new Thread(() =>
                {
                    while (shouldRun)
                    {
                        try
                        {
                            foreach (var item in sharedList)
                            {
                                // 在遍历过程中集合被修改会导致异常
                                Console.WriteLine(item);
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            _logger.Error("多线程竞争条件异常发生");
                            shouldRun = false;
                            throw;
                        }
                        Thread.Sleep(1);
                    }
                });

                thread1.Start();
                thread2.Start();

                Thread.Sleep(1000);
                shouldRun = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _logger.Warning("用户点击触发空引用异常按钮");
            NullReferenceCrash.TriggerNullReference();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _logger.Warning("用户点击触发方法中的空引用异常按钮");
            NullReferenceCrash.TriggerNullReferenceInMethod();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _logger.Warning("用户点击触发索引越界异常按钮");
            IndexOutOfRangeCrash.TriggerIndexOutOfRange();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            _logger.Warning("用户点击触发列表索引越界异常按钮");
            IndexOutOfRangeCrash.TriggerListIndexOutOfRange();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            _logger.Warning("用户点击触发堆栈溢出异常按钮");
            StackOverflowCrash.TriggerStackOverflow();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            _logger.Warning("用户点击触发内存不足异常按钮");
            OutOfMemoryCrash.TriggerOutOfMemory();
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            _logger.Warning("用户点击触发多线程竞争条件异常按钮");
            ThreadingCrash.TriggerRaceCondition();
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            NewVersionWindow newVersionWindow = new NewVersionWindow();
            newVersionWindow.Show();
        }
    }
}