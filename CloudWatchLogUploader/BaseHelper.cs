using System;
using Amazon.CloudWatchLogs;

namespace CloudWatchLogUploader
{
    internal abstract class BaseHelper
    {
        protected readonly AmazonCloudWatchLogsClient client;
        public BaseHelper(AmazonCloudWatchLogsClient client)
        {
            this.client = client;
        }

        protected bool GetYesOrNo()
        {
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
            } while (key.Key == ConsoleKey.Enter || key.Key == ConsoleKey.Y || key.Key == ConsoleKey.N);

            switch (key.Key)
            {
                case ConsoleKey.Y:
                    return true;
                case ConsoleKey.N:
                default:
                    return false;
            }
        }

        protected int ReadIntBetween(string message, int min, int max)
        {
            Console.Write(message);
            int num;
            while (!int.TryParse(Console.ReadLine(), out num) && num >= min && num <= max)
                Console.Write(Environment.NewLine + "Please enter an integer between " + min + " and " + max);
            return num;
        }
    }
}
