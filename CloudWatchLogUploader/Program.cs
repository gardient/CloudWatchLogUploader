using System;
using Amazon.CloudWatchLogs;

namespace CloudWatchLogUploader
{
    class Program
    {
        static void Main(string[] args)
        {
            var opt = new CommandOptions();
            if (CommandLine.Parser.Default.ParseArguments(args, opt))
            {
                DebugLogger.Debug = opt.Debug;

                var client = new AmazonCloudWatchLogsClient();
                var logGroup = new LogGroupHelper(client).GetLogGroup(opt.LogGroup);
                var logStreamHelper = new LogStreamHelper(client);
                var logStream = logStreamHelper.GetLogStream(logGroup, opt.LogStream);
                logStreamHelper.UploadLogs(logGroup, logStream, opt.OverwriteStream, opt.InputFilePath);

                Console.Write("Upload complete, press any key to continue...");
                Console.ReadKey();
            }
        }
    }
}
