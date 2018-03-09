using System.Threading.Tasks;

namespace Ohm.NET.Services
{
    public interface IRedisConnectionProvider
    {
        void Configure(string url);
        RedisRawResult Call(string command, params string[] arguments);
        Task<RedisRawResult> CallAsync(string command, params string[] arguments);
        void Queue(string command, params string[] arguments);
        void Clear();
        RedisRawResult[] Commit();
        Task<RedisRawResult[]> CommitAsync();
        void Quit();
        Task QuitAsync();
        int Timeout { get; set; }
    }
}
