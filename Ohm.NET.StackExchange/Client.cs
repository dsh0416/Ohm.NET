using System;
using System.Threading.Tasks;
using StackExchange.Redis;
using Ohm.NET.Services;
using System.Collections.Generic;

namespace Ohm.NET.StackExchange
{
    public class Client : IRedisConnectionProvider
    {
        private ConnectionMultiplexer Connection;
        private IDatabase Database;
        private const int DefaultTimeout = 10;
        private List<Tuple<string, string[]>> CommandBuffer = new List<Tuple<string, string[]>>();

        public Client(string url="127.0.0.1:6379", int timeout=DefaultTimeout) {
            Connection = TaskSync(ConnectionMultiplexer.ConnectAsync(url), timeout);
            Database = Connection.GetDatabase();
        }

        public RedisRawResult Call(string command, params string[] arguments) =>
        TaskSync(CallAsync(command, arguments));

        public Task<RedisRawResult> CallAsync(string command, params string[] arguments)
        {
            var task = Database.ExecuteAsync(command, arguments);
            return task.ContinueWith(t1 =>
            {
                var result = t1.Result;
                // TODO: Translate result
                return new RedisRawResult();
            });
        }

        public void Clear() => CommandBuffer.Clear();

        public RedisRawResult[] Commit() => 
        TaskSync(CommitAsync());

        public Task<RedisRawResult[]> CommitAsync()
        {
            var tasks = new List<Task<RedisRawResult>>();
            CommandBuffer.ForEach((tuple) => {
                tasks.Add(CallAsync(tuple.Item1, tuple.Item2));
            });
            return Task<RedisRawResult[]>.Factory.ContinueWhenAll(
                tasks.ToArray(), args =>
            {
                var results = new List<RedisRawResult>();
                foreach(var arg in args) {
                    results.Add(arg.Result);
                }
                Clear();
                return results.ToArray();
            });
        }

        public void Configure(string url)
        {
            Connection = TaskSync(ConnectionMultiplexer.ConnectAsync(url), DefaultTimeout);
            Database = Connection.GetDatabase();
        }

        public void Configure(string url, int timeout)
        {
            Connection = TaskSync(ConnectionMultiplexer.ConnectAsync(url), timeout);
            Database = Connection.GetDatabase();
        }

        public void Queue(string command, params string[] arguments)
        {
            CommandBuffer.Add(new Tuple<string, string[]>(command, arguments));
        }

        public void Quit()
        {
            QuitAsync().Wait();
        }

        public Task QuitAsync() {
            return CallAsync("QUIT");
        }

        public void Timeout()
        {
            throw new System.NotImplementedException();
        }

        private T TaskSync<T>(Task<T> task, Nullable<int> timeout = null)
        {
            if (timeout.HasValue) {
                if (task.Wait(timeout.GetValueOrDefault()))
                    return task.Result;
                else
                    throw new System.TimeoutException();
            }
            task.Wait();
            return task.Result;
        }
    }
}
