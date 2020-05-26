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

        Task<Trip> UpsertTrip(Trip trip, Guid UserId);
        Task<Trip> ReadTrip(Guid Id, Guid UserId);
        Task<Trip[]> ReadManyTrips(Guid[] Ids, Guid UserId);
        Task<Guid> DeleteTrip(Guid Id, Guid UserId);
        Task<Guid[]> DeleteManyTrips(Guid[] Ids, Guid UserId);
        Task<Guid[]> GetAllTripIds(Guid UserId);
        Task<Trip[]> GetAllTrips(Guid UserId, long time = 0, bool IsDeleted = false);
        #endregion

        #region Place

        Task<Place> UpsertPlace(Place place, Guid UserId);
        Task<Place> ReadPlace(Guid Id, Guid UserId);
        Task<Guid> DeletePlace(Guid Id, bool deletefromtrip, Guid UserId);
        Task<Guid[]> DeleteManyPlaces(Guid[] Ids, bool deletefromtrip, Guid UserId);
        Task<Guid[]> GetAllPlaceIds(Guid UserId);
        Task<Place[]> GetAllPlaces(Guid UserId, long time = 0, bool IsDeleted = false);

        #endregion

        #region Goal

        Task<Goal> UpsertGoal(Goal goal, Guid UserId);
        Task<Goal> ReadGoal(Guid Id, Guid UserId);
        Task<Guid> DeleteGoal(Guid Id, bool deletefromtrip, Guid UserId);
        Task<Guid[]> DeleteManyGoals(Guid[] Ids, bool deletefromtrip, Guid UserId);
        Task<Guid[]> GetAllGoalIds(Guid UserId);
        Task<Goal[]> GetAllGoals(Guid UserId, long time = 0, bool IsDeleted = false);
        #endregion

        #region Good

        Task<Good> UpsertGood(Good good, Guid UserId);
        Task<Good> ReadGood(Guid Id, Guid UserId);
        Task<Guid> DeleteGood(Guid Id, bool deletefromtrip, Guid UserId);
        Task<Guid[]> DeleteManyGoods(Guid[] Ids, bool deletefromtrip, Guid UserId);
        Task<Guid[]> GetAllGoodIds(Guid UserId);
        Task<Good[]> GetAllGoods(Guid UserId, long time = 0, bool IsDeleted = false);

        #endregion

        #region Category

        Task<Category> UpsertCategory(Category category);
        Task<Category> ReadCategory(Guid Id);
        Task<Guid> DeleteCategory(Guid Id);
        Task<Guid[]> GetAllCategoryIds();
        Task<Category[]> GetAllCategories();

        #endregion

        #region Purchase

        Task<Purchase> UpsertPurchase(Purchase purchase, Guid UserId);
        Task<Purchase> ReadPurchase(Guid Id, Guid UserId);
        Task<Guid> DeletePurchase(Guid Id, bool deletefromtrip, Guid UserId);
        Task<Guid[]> DeleteManyPurchases(Guid[] Ids, bool deletefromtrip, Guid UserId);
        Task<Guid[]> GetAllPurchaseIds(Guid UserId);
        Task<Purchase[]> GetAllPurchases(Guid UserId, long time = 0, bool IsDeleted = false);

        #endregion
    }
}
