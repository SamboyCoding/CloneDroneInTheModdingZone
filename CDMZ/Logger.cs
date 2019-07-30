using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;

namespace CDMZ
{
    public sealed class Logger
    {
        #region Static Stuff
        private static readonly string LogFilePath = Path.Combine(Application.dataPath, "cdmz.log");
        private static bool _init = false;
        private static string _log;
        #endregion
        
        private readonly string _logPrefix;

        internal Logger(string prefix)
        {
            _logPrefix = prefix;
        }

        private static void Write(string message)
        {
            if(!_init && File.Exists(LogFilePath)) File.Delete(LogFilePath);
            _init = true;

            _log += $"[{DateTime.Now}] {message}\n";
            File.AppendAllText(LogFilePath, $"[{DateTime.Now}] {message}\n");
        }
        
        public void Debug(string message)
        {
            Write($"[Debug] [{_logPrefix}] {message}");
        }

        public void Info(string message)
        {
            Write($"[Info] [{_logPrefix}] {message}");
        }
        
        public void Warning(string message)
        {
            Write($"[Warn] [{_logPrefix}] {message}");
        }
        
        public void Error(string message)
        {
            Write($"[Error] [{_logPrefix}] {message}");
        }

        public void DumpCurrentStack()
        {
            ExceptionActual("\n" + new StackTrace(), "Debug stack print");
        }
        
        public void Exception(Exception ex, string message = "Unexpected Error")
        {
            //Another quirk of c#: If an exception is never thrown it will not have a stack.
            if (string.IsNullOrEmpty(ex.StackTrace))
            {
                try
                {
                    throw ex;
                }
                catch (Exception e)
                {
                    ex = e;
                } 
            }
                
            ExceptionActual(ex?.ToString(), message);
        }

        private void ExceptionActual(string ex, string message)
        {
            Write($"[Exception] [{_logPrefix}] {message}: {ex}");
        }
    }
}