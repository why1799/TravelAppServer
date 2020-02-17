using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TravelAppModels;
using TravelAppModels.Models;

namespace TravelAppStorage.Interfaces
{
    public interface IStorage
    {
        Task<UserToken> FindUser(string email, string password);

        Task<bool> FindUserEmail(string email);

        Task<UserToken> AddUser(string username, string email, string password); 
    }
}
