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
        Task<UserToken> FindUserByToken(string token);

        #region Auth
        Task<UserToken> FindUser(string email, string password);

        Task<bool> FindUserEmail(string email);

        Task<UserToken> AddUser(string username, string email, string password);
        #endregion

        #region Photo
        Task<Photo> UploadPhoto(string Base64, Guid UserId);

        Task<Photo> GetPhoto(Guid Id);
        #endregion

        #region Trip

        Task<Trip> UpsertTrip(Trip trip);
        Task<Trip> ReadTrip(Guid Id);
        Task<Guid> DeleteTrip(Guid Id);
        Task<Guid[]> GetAllTrips(Guid UserId);

        #endregion
    }
}
