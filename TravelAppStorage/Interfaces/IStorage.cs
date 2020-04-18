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

        Task<bool> CheckToken(string token, Guid UserId);
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

        #region Place

        Task<Place> UpsertPlace(Place place);
        Task<Place> ReadPlace(Guid Id);
        Task<Guid> DeletePlace(Guid Id, bool deletefromtrip);
        Task<Guid[]> GetAllPlaces(Guid UserId);

        #endregion

        #region Goal

        Task<Goal> UpsertGoal(Goal goal);
        Task<Goal> ReadGoal(Guid Id);
        Task<Guid> DeleteGoal(Guid Id, bool deletefromtrip);
        Task<Guid[]> GetAllGoals(Guid UserId);

        #endregion

        #region Good

        Task<Good> UpsertGood(Good good);
        Task<Good> ReadGood(Guid Id);
        Task<Guid> DeleteGood(Guid Id, bool deletefromtrip);
        Task<Guid[]> GetAllGoods(Guid UserId);

        #endregion

        #region Category

        Task<Category> UpsertCategory(Category category);
        Task<Category> ReadCategory(Guid Id);
        Task<Guid> DeleteCategory(Guid Id);
        Task<Guid[]> GetAllCategoryIds();
        Task<Category[]> GetAllCategories();

        #endregion

        #region Purchase

        Task<Purchase> UpsertPurchase(Purchase purchase);
        Task<Purchase> ReadPurchase(Guid Id);
        Task<Guid> DeletePurchase(Guid Id, bool deletefromtrip);
        Task<Guid[]> GetAllPurchases(Guid UserId);

        #endregion
    }
}
