using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudWatchLogUploader
{
    internal class LogGroupHelper : BaseHelper
    {
        public LogGroupHelper(AmazonCloudWatchLogsClient client) : base(client)
        {
        }

        public LogGroup GetLogGroup(string logGroup = null)
        {
            List<LogGroup> allGroups = new List<LogGroup>();
            DescribeLogGroupsResponse lgResponse = null;
            do
            {
                DebugLogger.WriteLine("Getting logGroups...");
                lgResponse = client.DescribeLogGroups(new DescribeLogGroupsRequest { NextToken = (lgResponse != null ? lgResponse.NextToken : null) });
                allGroups.AddRange(lgResponse.LogGroups);
                DebugLogger.WriteLine("Got logGroups, have {0}, {1} more pages", allGroups.Count, (!string.IsNullOrWhiteSpace(lgResponse.NextToken) ? "still" : "no"));
            } while (!string.IsNullOrWhiteSpace(lgResponse.NextToken));

            if (string.IsNullOrWhiteSpace(logGroup) || logGroup[logGroup.Length - 1] == '*')
            {
                if (!string.IsNullOrWhiteSpace(logGroup))
                {
                    if (logGroup.EndsWith("*"))
                        logGroup = logGroup.Substring(0, logGroup.Length - 1);
                    allGroups = allGroups.Where(x => x.LogGroupName.StartsWith(logGroup)).ToList();
                }

                for (int i = 0, len = allGroups.Count; i < len; ++i)
                    Console.WriteLine(i + ") " + allGroups[i].LogGroupName);
                int num = ReadIntBetween("Choose log group (-1 to create one): ", -1, allGroups.Count - 1);

                if (num == -1)
                {
                    if (logGroup[logGroup.Length - 1] == '*')
                        return CreateLogGroup();
                }

                Console.Clear();
                Console.WriteLine("You choose LogGroup: " + allGroups[num].LogGroupName);
                return allGroups[num];
            }

            var lg = allGroups.FirstOrDefault(x => x.LogGroupName == logGroup);
            if (lg == null)
            {
                Console.WriteLine("The log group '" + logGroup + "' does not exist.");
                lg = CreateLogGroup();
            }

            Console.WriteLine("You choose LogGroup: " + lg.LogGroupName);
            return lg;
        }

        private LogGroup CreateLogGroup(string lgName = null)
        {
            if (string.IsNullOrWhiteSpace(lgName))
            {
                lgName = GetLogGroupNameToCreate();
            }
            else
            {
                Console.WriteLine("Do you want to create the log group [Y/N] (default: N): " + lgName);
                if (!GetYesOrNo())
                {
                    lgName = GetLogGroupNameToCreate();
                }
            }

            client.CreateLogGroup(new CreateLogGroupRequest(lgName));

            return client.DescribeLogGroups(new DescribeLogGroupsRequest { LogGroupNamePrefix = lgName }).LogGroups.FirstOrDefault();
        }

        private string GetLogGroupNameToCreate()
        {
            Console.Write("Input log group name (Ctrl+C to abort): ");
            return Console.ReadLine();
        }
    }
}
