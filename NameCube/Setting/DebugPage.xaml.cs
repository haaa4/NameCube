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

namespace NameCube.Setting
{
    /// <summary>
    /// DebugPage.xaml 的交互逻辑
    /// </summary>
    public partial class DebugPage : Page
    {
        public DebugPage()
        {
            InitializeComponent();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariables.json.AllSettings.debug = false;
            GlobalVariables.SaveJson();
        }
        //空引用异常
        public class NullReferenceCrash
        {
            public static void TriggerNullReference()
            {
                string text = null;
                int length = text.Length;
            }

            public static void TriggerNullReferenceInMethod()
            {
                List<string> list = null;
                list.Add("test");
            }
        }
        //索引越界异常
        public class IndexOutOfRangeCrash
        {
            public static void TriggerIndexOutOfRange()
            {
                int[] numbers = { 1, 2, 3 };
                // 访问不存在的索引
                for (int i = 0; i <= numbers.Length; i++) 
                {
                    Console.WriteLine(numbers[i]);
                }
            }

            public static void TriggerListIndexOutOfRange()
            {
                List<string> items = new List<string> { "A", "B", "C" };
                // 列表索引越界
                string item = items[10];
            }
        }
        //堆栈溢出异常
        public class StackOverflowCrash
        {
            private static int counter = 0;

            public static void TriggerStackOverflow()
            {
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
            public static void TriggerOutOfMemory()
            {
                List<byte[]> memoryHog = new List<byte[]>();

                try
                {
                    while (true)
                    {
                        // 不断分配大内存块
                        byte[] largeArray = new byte[100000000]; // 100MB
                        memoryHog.Add(largeArray);
                        Console.WriteLine($"已分配: {memoryHog.Count * 100} MB");
                    }
                }
                catch (OutOfMemoryException ex)
                {
                    Console.WriteLine($"内存不足: {ex.Message}");
                    throw; // 重新抛出异常
                }
            }
        }
        //多线程竞争条件异常
        public class ThreadingCrash
        {
            private static List<int> sharedList = new List<int>();
            private static bool shouldRun = true;

            public static void TriggerRaceCondition()
            {
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
            NullReferenceCrash.TriggerNullReference();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NullReferenceCrash.TriggerNullReferenceInMethod();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            IndexOutOfRangeCrash.TriggerIndexOutOfRange();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            IndexOutOfRangeCrash.TriggerListIndexOutOfRange();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            StackOverflowCrash.TriggerStackOverflow();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            OutOfMemoryCrash.TriggerOutOfMemory();
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            ThreadingCrash.TriggerRaceCondition();
        }
    }
}
