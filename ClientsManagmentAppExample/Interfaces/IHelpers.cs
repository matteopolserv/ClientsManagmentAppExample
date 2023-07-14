using ClientsManagmentAppExample.Models;

namespace ClientsManagmentAppExample.Interfaces
{
    public interface IHelpers
    {
        Task SendSMTPEmailAsync(string receiver, string subject, string body);
        Task<string> FormMessageBuilder(FormModel model);
        
        Task<string> DecryptString(string cipherText);
        Task<string> EncryptString(string text);
        Task<bool> CheckPassword(string text, string name);
    }
}
