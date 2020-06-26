using System.Threading.Tasks;

namespace UserAPI.RabbitMQ.Producer
{
    public interface IUserAPIRabbitRPCService
    {
        Task<T> PublishRabbitMessageWaitForResponseAsync<T>(string method, object requestModel);
    }
}
