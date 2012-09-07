﻿using System;
using System.Diagnostics;
using Exceptron.Client;
using Exceptron.Client.Configuration;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace Exceptron.NLog
{
    /// <summary>
    /// <see cref="NLog"/> target for exceptron. Allows you to automatically report all
    /// exceptions logged to Nlog/>
    /// </summary>
    public class ExceptronTarget : Target
    {
        internal IExceptronClient ExceptronClient;

        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            var config = new ExceptronConfiguration
                {
                    ApiKey = ApiKey,
                    ThrowExceptions = LogManager.ThrowExceptions
                };

            ExceptronClient = new ExceptronClient(config);
        }

        /// <summary>
        /// exceptron API Key
        /// </summary>
        [RequiredParameter]
        public string ApiKey { get; set; }

        /// <summary>
        /// String that identifies the active user
        /// </summary>
        public Layout UserId { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            if (logEvent == null || logEvent.Exception == null) return;

            InternalLogger.Trace("Sending Exception to api.exceptron.com. Process Name: {0}", Process.GetCurrentProcess().ProcessName);

            try
            {
                var exceptionData = new ExceptionData
                {
                    Exception = logEvent.Exception,
                    Component = logEvent.LoggerName,
                    Message = logEvent.FormattedMessage,
                };

                if (UserId != null)
                {
                    exceptionData.UserId = UserId.Render(logEvent);
                }

                if (logEvent.Level <= LogLevel.Info)
                {
                    exceptionData.Severity = ExceptionSeverity.None;
                }
                else if (logEvent.Level <= LogLevel.Warn)
                {
                    exceptionData.Severity = ExceptionSeverity.Warning;
                }
                else if (logEvent.Level <= LogLevel.Error)
                {
                    exceptionData.Severity = ExceptionSeverity.Error;
                }
                else if (logEvent.Level <= LogLevel.Fatal)
                {
                    exceptionData.Severity = ExceptionSeverity.Fatal;
                }

                ExceptronClient.SubmitException(exceptionData);
            }
            catch (Exception e)
            {
                InternalLogger.Warn("Unable to report exception. {0}", e);
                throw;
            }
        }
    }
}
