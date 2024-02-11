using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace OLabWebAPI.Services;

public class CounterUpdateService : Hub
{
  /// <summary>
  /// 
  /// </summary>
  /// <param name="message"></param>
  /// <returns></returns>
  public async Task SendMessage(string message)
  {
    await Clients.All.SendAsync("newMessage", "anonymous", message);
  }
}