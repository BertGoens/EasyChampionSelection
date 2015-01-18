using System;
using System.IO;
using System.Reflection;

namespace EasyChampionSelection.ECS {
    public class StaticErrorLogger {

        /// <summary>
        /// Prepares an error report and saves it to a text file, or shows it on-screen if creating the file fails.
        /// </summary>
        /// <param name="exceptionString">The exception information (usually provided by ex.ToString()).</param>
        /// <param name="innerExceptionString">The inner exception information (if any).</param>
        /// <param name="additional">Any additional information provided by the developer.</param>
        public static void WriteErrorReport(Exception ex, string additional = "") {
            try {
                using(StreamWriter f = new StreamWriter(StaticSerializer.FullPath_ErrorFile, true)) {
                    f.WriteAsync(FormalizeError(ex, additional));
                }
            } catch(Exception) {
            }
        }

        public static string FormalizeError(Exception ex, string additional = "") {
            string versionString = "Version " + Assembly.GetExecutingAssembly().GetName().Version;

            string res = "Error Report for " + StaticSerializer.AppName;

            res += versionString + "\n";
            if(!String.IsNullOrWhiteSpace(additional)) {
                res += "Developer information: \n" + additional + "\n";
            }

            res += "Exception information: \n";
            res += ex.ToString() + "\n";
            res += "Inner Exception information: \n";
            res += ex.InnerException + "\n";

            return res;
        }
    }
}
