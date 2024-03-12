using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPrice.Methods
{
    public class ConsoleModify
    {
        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Got unhandeld exception!");
            Exception exception = (Exception)e.ExceptionObject;
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string full_dir = path + "logs";
                if (!Directory.Exists(full_dir)) Directory.CreateDirectory(full_dir);
                var guid = Guid.NewGuid().ToString();
                string total_file_path = $"{path}logs\\{guid}.txt";
                //File.Create(total_file_path);
                File.AppendAllText(total_file_path, exception.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't save a error: " + ex.Message);
               
            }
            var applicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            Process.Start(applicationPath);
            Environment.Exit(Environment.ExitCode);
        }
    }
}
