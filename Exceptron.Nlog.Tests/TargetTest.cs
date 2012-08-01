using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Exceptron.Client;
using Exceptron.NLog;
using Moq;
using NLog;
using NLog.Config;
using NUnit.Framework;

namespace Exceptron.Nlog.Tests
{
    [TestFixture]
    public class TargetTest
    {
        private Logger _testLogger = LogManager.GetCurrentClassLogger();
        private ExceptronTarget _etTarget = new ExceptronTarget();
        private Mock<IExceptronClient> _exceptronClient = new Mock<IExceptronClient>();

        [SetUp]
        public void Setup()
        {
            LogManager.Configuration = new LoggingConfiguration();
            LogManager.ThrowExceptions = true;
            _etTarget = new ExceptronTarget
                {
                    ApiKey = "FAKE",
                };

            LogManager.Configuration.AddTarget(Guid.NewGuid().ToString(), _etTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, _etTarget));

            LogManager.ReconfigExistingLoggers();

            _etTarget._exceptronClient = _exceptronClient.Object;
        }

        [Test]
        public void null_user_id_should_not_break_logger()
        {
            _testLogger.FatalException("Test", GetException());

            _exceptronClient.Verify(c => c.SubmitException(It.IsAny<ExceptionData>()), Times.Once());
        }

        [TestCase("")]
        [TestCase(null)]
        public void api_key_should_be_required(string apiKey)
        {
            LogManager.Configuration = new LoggingConfiguration();
            LogManager.ThrowExceptions = true;
            _etTarget = new ExceptronTarget();
            _etTarget.ApiKey = apiKey;
            LogManager.Configuration.AddTarget(Guid.NewGuid().ToString(), _etTarget);
            LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, _etTarget));

            Assert.Throws<NLogConfigurationException>(LogManager.ReconfigExistingLoggers);
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        private static void ThrowsException()
        {
            throw new Exception("Test Exception");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DebuggerStepThrough]
        private static Exception GetException()
        {
            try
            {
                ThrowsException();
            }
            catch (Exception e)
            {

                return e;
            }

            return null;
        }

    }
}
