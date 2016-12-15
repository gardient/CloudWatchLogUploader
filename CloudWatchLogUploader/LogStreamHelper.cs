using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace CloudWatchLogUploader
{
    internal class LogStreamHelper : BaseHelper
    {
        public LogStreamHelper(AmazonCloudWatchLogsClient client) : base(client)
        {
        }

        public LogStream GetLogStream(LogGroup logGroup, string logStream = null)
        {
            List<LogStream> allStreams = new List<LogStream>();
            DescribeLogStreamsResponse lsResponse = null;
            do
            {
                DebugLogger.WriteLine("Getting logStreams...");
                lsResponse = client.DescribeLogStreams(new DescribeLogStreamsRequest
                {
                    NextToken = (lsResponse != null ? lsResponse.NextToken : null),
                    LogGroupName = logGroup.LogGroupName
                });
                allStreams.AddRange(lsResponse.LogStreams);
                DebugLogger.WriteLine("Got logStreams, have {0}, {1} more pages", allStreams.Count, (!string.IsNullOrWhiteSpace(lsResponse.NextToken) ? "still" : "no"));
            } while (!string.IsNullOrWhiteSpace(lsResponse.NextToken));

            if (string.IsNullOrWhiteSpace(logStream) || logStream[logStream.Length - 1] == '*')
            {
                if (!string.IsNullOrWhiteSpace(logStream))
                {
                    logStream = logStream.Substring(0, logStream.Length - 1);
                    allStreams = allStreams.Where(x => x.LogStreamName.StartsWith(logStream)).ToList();
                }

                allStreams = allStreams.OrderByDescending(x => x.CreationTime).ToList();

                for (int i = 0, len = allStreams.Count; i < len; ++i)
                    Console.WriteLine(i + ") " + allStreams[i].LogStreamName);
                int num = ReadIntBetween("Choose log stream: ", -1, allStreams.Count - 1);

                if (num == -1)
                {
                    if (logStream[logStream.Length - 1] == '*')
                        return CreateLogStream(logGroup);
                }

                Console.Clear();
                Console.WriteLine("You choose LogGroup: " + logGroup.LogGroupName + Environment.NewLine + "You choose LogStream: " + allStreams[num].LogStreamName);
                return allStreams[num];
            }

            var ls = allStreams.FirstOrDefault(x => x.LogStreamName == logStream);
            if (ls == null)
            {
                Console.WriteLine("The log stream '" + logGroup + "' does not exist.");
                ls = CreateLogStream(logGroup);
            }

            Console.WriteLine("You choose LogStream: " + ls.LogStreamName);
            return ls;
        }

        public void UploadLogs(LogGroup logGroup, LogStream logStream, bool overwriteStream, string inputFilePath = null)
        {
            string input = inputFilePath;
            while (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Choose input file: ");
                var possibleInput = Console.ReadLine();
                if (File.Exists(possibleInput) && !File.GetAttributes(possibleInput).HasFlag(FileAttributes.Directory))
                    input = possibleInput;
                else
                    Console.WriteLine(possibleInput + "doesn't exist or directory");
            }

            Console.Clear();
            Console.WriteLine(string.Format("Log stream: {1}{0}Log group: {2}{0}Overwrite stream: {3}{0}Input file: {4}{0}Do you want to start the upload? [Y/N] (default:N)", Environment.NewLine, logGroup.LogGroupName, logStream.LogStreamName, overwriteStream, input));
            if (!GetYesOrNo())
                return;

            using(var sr = new StreamReader(input))
            {
                List<InputLogEvent> events = new List<InputLogEvent>();

                while (!sr.EndOfStream)
                {
                    events.Add(new InputLogEvent { Message = sr.ReadLine(), Timestamp = DateTime.UtcNow });
                }

                client.PutLogEvents(new PutLogEventsRequest(logGroup.LogGroupName, logStream.LogStreamName, events));
            }
        }

        private LogStream CreateLogStream(LogGroup lg, string lsName = null)
        {
            if (string.IsNullOrWhiteSpace(lsName))
            {
                lsName = GetLogStreamNameToCreate();
            }
            else
            {
                Console.WriteLine("Do you want to create the log group [Y/N] (default: N): " + lsName);
                if (!GetYesOrNo())
                {
                    lsName = GetLogStreamNameToCreate();
                }
            }

            client.CreateLogStream(new CreateLogStreamRequest(lg.LogGroupName, lsName));

            return client.DescribeLogStreams(new DescribeLogStreamsRequest { LogStreamNamePrefix = lsName, LogGroupName = lg.LogGroupName }).LogStreams.FirstOrDefault();
        }

        private string GetLogStreamNameToCreate()
        {
            Console.Write("Input log stream name (Ctrl+C to abort): ");
            return Console.ReadLine();
        }
    }
}
