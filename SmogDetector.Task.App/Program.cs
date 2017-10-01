using System;
using System.Linq;
using Autofac;
using log4net;
using SmogDetector.Task.App.Commands;

namespace SmogDetector.Task.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            GlobalContext.Properties["LogName"] = args.Length > 0 ? args[0] : "";
            var logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            logger.Debug("Job started");
            logger.Debug("Parameters: " + string.Join(" ", args));
            var container = Bootstrapper.Configure();
            using (var scope = container.BeginLifetimeScope())
            {
                try
                {
                    var parameters = args.Skip(1).ToList();
                    if (args.Length > 0)
                    {
                        var commandName = args[0];
                        if (scope.IsRegisteredWithName(commandName, typeof(ICommand)))
                        {
                            scope.ResolveNamed<ICommand>(commandName).Execute(parameters);
                        }
                        else
                        {
                            scope.ResolveNamed<ICommand>("help").Execute(parameters);
                        }
                    }
                    else
                    {
                        scope.ResolveNamed<ICommand>("help").Execute(parameters);
                        logger.Error("No command specified");
                    }
                }
                catch (Exception e)
                {
                    logger.ErrorFormat(
                        "Job error {0} Message: {1}{0}Inner Error: {2}{0}Stacktrace: "
                              + e.StackTrace, Environment.NewLine, e.Message, e.InnerException?.Message ?? "");
                    throw;
                }
                finally
                {
                    logger.Debug("Job ended");
                }
            }
        }
    }
}
