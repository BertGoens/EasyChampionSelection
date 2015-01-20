using EasyChampionSelection.ECS.Serialization;
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
                    f.Write(FormalizeError(ex, additional));
                }
            } catch(Exception) {
            }
        }

        public static string FormalizeError(Exception ex, string additional = "") {
            string res = "Error Report for " + StaticSerializer.AppName + Environment.NewLine;

            res += "Version " + Assembly.GetExecutingAssembly().GetName().Version + Environment.NewLine;
            if(!String.IsNullOrWhiteSpace(additional)) {
                res += "Developer information: " + Environment.NewLine +
                    additional + Environment.NewLine;
            }

            res += "Exception information:" + Environment.NewLine;
            res += ex.ToString() + Environment.NewLine;
            res += "Inner Exception information:" + Environment.NewLine;
            res += ex.InnerException + Environment.NewLine;

            return res;
        }
    }
}
