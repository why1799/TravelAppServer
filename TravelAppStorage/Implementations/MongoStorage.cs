using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using TravelAppModels;
using TravelAppStorage.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using TravelAppModels.Models;
using Microsoft.Extensions.Options;
using TravelAppStorage.Settings;
using TravelAppStorage.Encryption;

namespace TravelAppStorage.Implementations
{
    public class MongoStorage : IStorage
    {
        #region Data
        private IMongoClient client;
        private IMongoCollection<UserToken> tokens;
        private IMongoCollection<User> users;
        private IMongoCollection<Photo> photos;
        private IMongoCollection<Trip> trips;
        private IMongoCollection<Place> places;
        private IMongoCollection<Good> goods;
        private IMongoCollection<Goal> goals;
        private IMongoCollection<Category> categories;
        private IMongoCollection<Purchase> purchases;
        #endregion

        #region Constructors
        public MongoStorage(IOptions<DBConnection> options)
        {
            var connection = options.Value;
            this.client = new MongoClient(connection.StringDBConnection);
            InitCollections(connection.DataBaseName);
        }
        #endregion

        #region Service Methods
        private void InitCollections(string DataBaseName)
        {
            var database = client.GetDatabase(DataBaseName);

            Register();
            tokens = database.GetCollection<UserToken>("Tokens");
            users = database.GetCollection<User>("Users");
            //photos = database.GetCollection<Photo>("Photos");
            trips = database.GetCollection<Trip>("Trips");
            places = database.GetCollection<Place>("Places");
            goods = database.GetCollection<Good>("Goods");
            goals = database.GetCollection<Goal>("Goals");
            categories = database.GetCollection<Category>("Categories");
            purchases = database.GetCollection<Purchase>("Purchases");
            //CreateIndexes();
        }

        private void Register()
        {
            RegisterUserModels();
            //RegisterPhotoModel();
            RegisterTripModel();
            RegisterPlaceModel();
            RegisterGoodModel();
            RegisterGoalModel();
            RegisterCategoryModel();
            RegisterPurchaseModel();
        }
        #endregion

        public async Task<UserToken> FindUserByToken(string token)
        {
            var filter = Builders<UserToken>.Filter.Eq(x => x.Token, token);
            var gottokens = await tokens.Find(filter).ToListAsync();

            if (gottokens.Count != 1)
            {
                throw new ArgumentException("Such account doesn't exist!");
            }

            return gottokens[0];
        }

        #region Auth

        public void RegisterUserModels()
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(User)) == false)
            {
                BsonClassMap.RegisterClassMap<User>(entity =>
                {
                    entity.MapProperty(e => e.Email)
                        .SetIsRequired(true)
                        .SetElementName("Email");
                    entity.MapProperty(e => e.Password)
                        .SetIsRequired(true)
                        .SetElementName("Password");
                    entity.MapProperty(e => e.Username)
                        .SetIsRequired(true)
                        .SetElementName("Username");
                    entity.MapIdProperty(e => e.Id)
                        .SetIsRequired(true)
                        .SetSerializer(new GuidSerializer(BsonType.String));
                    entity.SetIdMember(entity.GetMemberMap(e => e.Id));
                });
            }

            if (BsonClassMap.IsClassMapRegistered(typeof(UserToken)) == false)
            {
                BsonClassMap.RegisterClassMap<UserToken>(entity =>
                {
                    entity.MapIdProperty(e => e.UserId)
                        .SetIsRequired(true)
                        .SetSerializer(new GuidSerializer(BsonType.String))
                        .SetElementName("UserId");
                    entity.MapProperty(e => e.Token)
                        .SetIsRequired(true)
                        .SetElementName("Token");
                    entity.SetIdMember(entity.GetMemberMap(e => e.Token));
                });
            }
        }

        public async Task<UserToken> AddUser(string username, string email, string password)
        {
            User user = new User()
            {
                Id = Guid.NewGuid(),
                Email = email,
                Username = username,
                Password = EncryptionData.HashData(password)
            };

            if ((await FindUserEmail(email)) != null)
            {
                throw new ArgumentException("Such email exists!");
            }

            await users.InsertOneAsync(user);

            return await FindUser(user.Email, password);
        }

        public async Task<UserToken> FindUser(string email, string password)
        {
            var user = await FindUserEmail(email);

            if (user == null)
            {
                throw new ArgumentException("Such email doesn't exist!");
            }

            if(!EncryptionData.VerifyHashedData(user.Password, password))
            {
                throw new ArgumentException("Such account doesn't exist!");
            }

            UserToken token = new UserToken()
            {
                UserId = user.Id,
                Token = TokenGenerator()
            };

            await tokens.InsertOneAsync(token);
            return token;
        }
        public async Task<User> FindUserEmail(string email)
        {
            var filter = new BsonDocument("Email", email);
            var user = await users.Find(filter).ToListAsync();

            if(user.Count != 1)
            {
                return null;
            }

            return user[0];
        }

        public async Task<bool> CheckToken(string token, Guid UserId)
        {
            var filter = Builders<UserToken>.Filter.And(Builders<UserToken>.Filter.Eq(x => x.Token, token), Builders<UserToken>.Filter.Eq(x => x.UserId, UserId));
            var gotusertokens = await tokens.Find(filter).ToListAsync();

            return gotusertokens.Count == 1;
        }

        private string TokenGenerator()
        {
            var list = new List<byte>();
            list.AddRange(GuidToList());
            list.AddRange(GuidToList());
            list.AddRange(GuidToList());

            return new string(Convert.ToBase64String(list.ToArray()).Where(x => x >= '0' && x <= '9' || x >= 'a' && x <= 'z' || x >= 'A' && x <= 'Z').Select(y => y).ToList().ToArray()) ;
        }

        private List<byte> GuidToList()
        {
            var guid = Guid.NewGuid();
            return guid.ToByteArray().ToList();
        }


        #endregion

        #region Photo

        public void RegisterPhotoModel()
        {
            //if (BsonClassMap.IsClassMapRegistered(typeof(Photo)) == false)
            //{
            //    BsonClassMap.RegisterClassMap<Photo>(entity =>
            //    {
            //        entity.MapIdProperty(e => e.UserId)
            //            .SetIsRequired(true)
            //            .SetSerializer(new GuidSerializer(BsonType.String))
            //            .SetElementName("UserId");
            //        entity.MapIdProperty(e => e.Base64)
            //            .SetIsRequired(true)
            //            .SetElementName("Base64");
            //        entity.MapIdProperty(e => e.LastUpdate)
            //            .SetIsRequired(false)
            //            .SetElementName("LastUpdate").SetDefaultValue(new DateTime(2020, 04, 26).Ticks);
            //        entity.MapIdProperty(e => e.Id)
            //            .SetIsRequired(true)
            //            .SetSerializer(new GuidSerializer(BsonType.String));
            //        entity.SetIdMember(entity.GetMemberMap(e => e.Id));
            //    });
            //}
        }

        public async Task<Photo> UploadPhoto(string Base64, Guid UserId)
        {
            Photo photo = new Photo()
            {
                UserId = UserId,
                //Base64 = Base64,
                Id = Guid.NewGuid(),
                //LastUpdate = DateTime.UtcNow.Ticks
        };

            await photos.InsertOneAsync(photo);
            return photo;
        }

        public async Task<Photo> GetPhoto(Guid Id)
        {
            var filter = Builders<Photo>.Filter.Eq(x => x.Id, Id);
            var gottokens = await photos.Find(filter).ToListAsync();

            if(gottokens.Count == 1)
            {
                return gottokens[0];
            }

            return null;
        }

        #endregion

        #region Trip

        public void RegisterTripModel()
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(Trip)) == false)
            {
                BsonClassMap.RegisterClassMap<Trip>(entity =>
                {
                    entity.MapIdProperty(e => e.Name)
                        .SetIsRequired(true)
                        .SetElementName("Name");
                    entity.MapIdProperty(e => e.TextField)
                        .SetIsRequired(false)
                        .SetElementName("TextField");
                    entity.MapIdProperty(e => e.PhotoIds)
                        .SetIsRequired(false)
                        .SetElementName("PhotoIds");
                    entity.MapIdProperty(e => e.FileIds)
                        .SetIsRequired(false)
                        .SetElementName("FileIds");
                    entity.MapIdProperty(e => e.PlaceIds)
                        .SetIsRequired(false)
                        .SetElementName("PlaceIds");
                    entity.MapIdProperty(e => e.GoodIds)
                        .SetIsRequired(false)
                        .SetElementName("GoodIds");
                    entity.MapIdProperty(e => e.GoalIds)
                        .SetIsRequired(false)
                        .SetElementName("GoalIds");
                    entity.MapIdProperty(e => e.PurchaseIds)
                        .SetIsRequired(false)
                        .SetElementName("PurchaseIds");
                    entity.MapIdProperty(e => e.ToDate)
                        .SetIsRequired(false)
                        .SetElementName("ToDate");
                    entity.MapIdProperty(e => e.FromDate)
                        .SetIsRequired(false)
                        .SetElementName("FromDate");
                    entity.MapIdProperty(e => e.Notes)
                        .SetIsRequired(false)
                        .SetElementName("Notes");
                    entity.MapIdProperty(e => e.UserId)
                        .SetIsRequired(true)
                        .SetElementName("UserId")
                        .SetSerializer(new GuidSerializer(BsonType.String));
                    entity.MapIdProperty(e => e.LastUpdate)
                       .SetIsRequired(true)
                       .SetElementName("LastUpdate");
                    entity.MapIdProperty(e => e.IsDeleted)
                       .SetIsRequired(true)
                       .SetElementName("IsDeleted");
                    entity.MapIdProperty(e => e.Id)
                        .SetIsRequired(true)
                        .SetSerializer(new GuidSerializer(BsonType.String));
                    entity.SetIdMember(entity.GetMemberMap(e => e.Id));
                });
            }
        }

        public async Task<Trip> UpsertTrip(Trip trip, Guid UserId)
        {
            if (trip == null) return null;

            var filter = Builders<Trip>.Filter.And(
                Builders<Trip>.Filter.Eq(entity => entity.Id, trip.Id),
                Builders<Trip>.Filter.Eq(entity => entity.UserId, UserId));

            var definition = Builders<Trip>.Update
                .Combine(
                Builders<Trip>.Update.Set(e => e.Name, trip.Name),
                Builders<Trip>.Update.Set(e => e.PhotoIds, trip.PhotoIds),
                Builders<Trip>.Update.Set(e => e.FileIds, trip.FileIds),
                Builders<Trip>.Update.Set(e => e.PlaceIds, trip.PlaceIds),
                Builders<Trip>.Update.Set(e => e.GoalIds, trip.GoalIds),
                Builders<Trip>.Update.Set(e => e.GoodIds, trip.GoodIds),
                Builders<Trip>.Update.Set(e => e.PurchaseIds, trip.PurchaseIds),
                Builders<Trip>.Update.Set(e => e.TextField, trip.TextField),
                Builders<Trip>.Update.Set(e => e.Notes, trip.Notes),
                Builders<Trip>.Update.Set(e => e.UserId, UserId),
                Builders<Trip>.Update.Set(e => e.FromDate, trip.FromDate),
                Builders<Trip>.Update.Set(e => e.ToDate, trip.ToDate),
                Builders<Trip>.Update.Set(e => e.LastUpdate, DateTime.UtcNow.Ticks),
                Builders<Trip>.Update.Set(e => e.IsDeleted, false));

            var options = new FindOneAndUpdateOptions<Trip, Trip>()
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            return await trips.FindOneAndUpdateAsync(filter, definition, options).ConfigureAwait(false);

        }

        public async Task<Trip> ReadTrip(Guid Id, Guid UserId)
        {
            var filter = Builders<Trip>.Filter.And(
                Builders<Trip>.Filter.Eq(x => x.Id, Id),
                Builders<Trip>.Filter.Eq(x => x.UserId, UserId),
                Builders<Trip>.Filter.Eq(x => x.IsDeleted, false));
            var gottrips = await trips.Find(filter).ToListAsync();

            if (gottrips.Count == 1)
            {
                return gottrips[0];
            }

            return null;
        }
        public async Task<Trip[]> ReadManyTrips(Guid[] Ids, Guid UserId)
        {
            var filter = Builders<Trip>.Filter.And(
                Builders<Trip>.Filter.Where(x => Ids.Contains(x.Id)),
                Builders<Trip>.Filter.Eq(x => x.UserId, UserId),
                Builders<Trip>.Filter.Eq(x => x.IsDeleted, false));

            var gottrips = await trips.Find(filter).ToListAsync();

            var returntrips = new List<Trip>();

            foreach (var id in Ids)
            {
                var got = gottrips.FirstOrDefault(x => x.Id == id);
                if (got != null)
                {
                    returntrips.Add(got);
                }
            }

            return returntrips.ToArray();
        }

        public async Task<Guid> DeleteTrip(Guid Id, Guid UserId)
        {
            var filter = Builders<Trip>.Filter.And(
                Builders<Trip>.Filter.Eq(entity => entity.Id, Id),
                Builders<Trip>.Filter.Eq(entity => entity.UserId, UserId));

            var update = Builders<Trip>.Update
                .Combine(
                Builders<Trip>.Update.Set(e => e.LastUpdate, DateTime.UtcNow.Ticks),
                Builders<Trip>.Update.Set(e => e.IsDeleted, true));
            var result = await trips.UpdateOneAsync(filter, update).ConfigureAwait(false);

            //await trips.DeleteOneAsync(filter).ConfigureAwait(false);
            return Id;
        }

        public async Task<Guid[]> DeleteManyTrips(Guid[] Ids, Guid UserId)
        {
            var filter = Builders<Trip>.Filter.And(
                Builders<Trip>.Filter.Where(x => Ids.Contains(x.Id)),
                Builders<Trip>.Filter.Eq(entity => entity.UserId, UserId));

            var update = Builders<Trip>.Update
                .Combine(
                Builders<Trip>.Update.Set(e => e.LastUpdate, DateTime.UtcNow.Ticks),
                Builders<Trip>.Update.Set(e => e.IsDeleted, true));
            var result = await trips.UpdateManyAsync(filter, update).ConfigureAwait(false);

            //await trips.DeleteOneAsync(filter).ConfigureAwait(false);
            return Ids;
        }

        public async Task<Guid[]> GetAllTripIds(Guid UserId)
        {
            var filter = Builders<Trip>.Filter.And(
                Builders<Trip>.Filter.Eq(x => x.UserId, UserId),
                Builders<Trip>.Filter.Eq(x => x.IsDeleted, false));
            var gotids = (await trips.Find(filter).ToListAsync()).Select(x => x.Id).ToList();

            return gotids.ToArray();
        }

        public async Task<Trip[]> GetAllTrips(Guid UserId, long time = 0, bool IsDeleted = false)
        {
            var filter = Builders<Trip>.Filter.And(
                Builders<Trip>.Filter.Eq(x => x.UserId, UserId),
                Builders<Trip>.Filter.Gt(x => x.LastUpdate, time),
                Builders<Trip>.Filter.Eq(x => x.IsDeleted, IsDeleted));
            var got = await trips.Find(filter).ToListAsync();

            return got.ToArray();
        }

        #endregion

        #region Place

        public void RegisterPlaceModel()
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(Place)) == false)
            {
                BsonClassMap.RegisterClassMap<Place>(entity =>
                {
                    entity.MapIdProperty(e => e.Name)
                        .SetIsRequired(true)
                        .SetElementName("Name");
                    entity.MapIdProperty(e => e.Description)
                        .SetIsRequired(false)
                        .SetElementName("Description");
                    entity.MapIdProperty(e => e.Adress)
                        .SetIsRequired(true)
                        .SetElementName("Adress");
                    entity.MapIdProperty(e => e.PhotoIds)
                        .SetIsRequired(false)
                        .SetElementName("Photos");
                    entity.MapIdProperty(e => e.IsVisited)
                       .SetIsRequired(true)
                       .SetElementName("IsVisited");
                    entity.MapIdProperty(e => e.Date)
                       .SetIsRequired(false)
                       .SetElementName("Date");
                    entity.MapIdProperty(e => e.UserId)
                        .SetIsRequired(true)
                        .SetElementName("UserId")
                        .SetSerializer(new GuidSerializer(BsonType.String));
                    entity.MapIdProperty(e => e.LastUpdate)
                       .SetIsRequired(true)
                       .SetElementName("LastUpdate");
                    entity.MapIdProperty(e => e.IsDeleted)
                       .SetIsRequired(true)
                       .SetElementName("IsDeleted");
                    entity.MapIdProperty(e => e.Id)
                        .SetIsRequired(true)
                        .SetSerializer(new GuidSerializer(BsonType.String));
                    entity.SetIdMember(entity.GetMemberMap(e => e.Id));
                });
            }
        }

        public async Task<Place> UpsertPlace(Place place, Guid UserId)
        {
            if (place == null) return null;

            var filter = Builders<Place>.Filter.And(
                Builders<Place>.Filter.Eq(entity => entity.Id, place.Id),
                Builders<Place>.Filter.Eq(entity => entity.UserId, UserId));

            var definition = Builders<Place>.Update
                .Combine(
                Builders<Place>.Update.Set(e => e.Name, place.Name),
                Builders<Place>.Update.Set(e => e.PhotoIds, place.PhotoIds),
                Builders<Place>.Update.Set(e => e.Adress, place.Adress),
                Builders<Place>.Update.Set(e => e.Description, place.Description),
                Builders<Place>.Update.Set(e => e.Date, place.Date),
                Builders<Place>.Update.Set(e => e.IsVisited, place.IsVisited),
                Builders<Place>.Update.Set(e => e.UserId, UserId),
                Builders<Place>.Update.Set(e => e.LastUpdate, DateTime.UtcNow.Ticks),  
                Builders<Place>.Update.Set(e => e.IsDeleted, false));
            var options = new FindOneAndUpdateOptions<Place, Place>()
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            return await places.FindOneAndUpdateAsync(filter, definition, options).ConfigureAwait(false);
        }

        public async Task<Place> ReadPlace(Guid Id, Guid UserId)
        {
            var filter = Builders<Place>.Filter.And(
               Builders<Place>.Filter.Eq(x => x.Id, Id),
               Builders<Place>.Filter.Eq(x => x.UserId, UserId),
               Builders<Place>.Filter.Eq(x => x.IsDeleted, false));
            var gotplaces = await places.Find(filter).ToListAsync();

            if (gotplaces.Count == 1)
            {
                return gotplaces[0];
            }

            return null;
        }
        public async Task<Place[]> ReadManyPlaces(Guid[] Ids, Guid UserId)
        {
            var filter = Builders<Place>.Filter.And(
               Builders<Place>.Filter.Where(x => Ids.Contains(x.Id)),
               Builders<Place>.Filter.Eq(x => x.UserId, UserId),
               Builders<Place>.Filter.Eq(x => x.IsDeleted, false));
            var gotplaces = await places.Find(filter).ToListAsync();

            var returnplaces = new List<Place>();

            foreach (var id in Ids)
            {
                var got = gotplaces.FirstOrDefault(x => x.Id == id);
                if (got != null)
                {
                    returnplaces.Add(got);
                }
            }

            return returnplaces.ToArray();
        }

        public async Task<Guid> DeletePlace(Guid Id, bool deletefromtrip, Guid UserId)
        {
            var filter = Builders<Place>.Filter.And(
               Builders<Place>.Filter.Eq(x => x.Id, Id),
               Builders<Place>.Filter.Eq(x => x.UserId, UserId));
            var update = Builders<Place>.Update
                .Combine(
                Builders<Place>.Update.Set(e => e.LastUpdate, DateTime.UtcNow.Ticks),
                Builders<Place>.Update.Set(e => e.IsDeleted, true));
            var result = await places.UpdateOneAsync(filter, update).ConfigureAwait(false);
            //await places.DeleteOneAsync(filter).ConfigureAwait(false);

            if(deletefromtrip)
            {
                var tripfilter = Builders<Trip>.Filter.And(
                    Builders<Trip>.Filter.Eq(ed => ed.UserId, UserId),
                    Builders<Trip>.Filter.Where(x => x.PlaceIds.Contains(Id)));
                var trips = await this.trips.Find(tripfilter).ToListAsync();

                foreach(var trip in trips)
                {
                    var placeids = trip.PlaceIds.ToList();
                    placeids.Remove(Id);
                    trip.PlaceIds = placeids.ToArray();
                    await UpsertTrip(trip, UserId);
                }
            }

            return Id;
        }


        public async Task<Guid[]> DeleteManyPlaces(Guid[] Ids, bool deletefromtrip, Guid UserId)
        {
            var filter = Builders<Place>.Filter.And(
               Builders<Place>.Filter.Where(entity => Ids.Contains(entity.Id)),
               Builders<Place>.Filter.Eq(x => x.UserId, UserId));
            var update = Builders<Place>.Update
                .Combine(
                Builders<Place>.Update.Set(e => e.LastUpdate, DateTime.UtcNow.Ticks),
                Builders<Place>.Update.Set(e => e.IsDeleted, true));
            var result = await places.UpdateManyAsync(filter, update).ConfigureAwait(false);
            //await places.DeleteOneAsync(filter).ConfigureAwait(false);

            if (deletefromtrip)
            {
                foreach (var Id in Ids)
                {
                    var tripfilter = Builders<Trip>.Filter.And(
                    Builders<Trip>.Filter.Eq(ed => ed.UserId, UserId),
                    Builders<Trip>.Filter.Where(x => x.PlaceIds.Contains(Id)));
                    var trips = await this.trips.Find(tripfilter).ToListAsync();

                    foreach (var trip in trips)
                    {
                        var placeids = trip.PlaceIds.ToList();
                        placeids.Remove(Id);
                        trip.PlaceIds = placeids.ToArray();
                        await UpsertTrip(trip, UserId);
                    }
                }
            }

            return Ids;
        }

        public async Task<Guid[]> GetAllPlaceIds(Guid UserId)
        {
            var filter = Builders<Place>.Filter.And(
                Builders<Place>.Filter.Eq(x => x.UserId, UserId),
                Builders<Place>.Filter.Eq(x => x.IsDeleted, false));
            var gotids = (await places.Find(filter).ToListAsync()).Select(x => x.Id).ToList();

            return gotids.ToArray();
        }

        public async Task<Place[]> GetAllPlaces(Guid UserId, long time = 0, bool IsDeleted = false)
        {
            var filter = Builders<Place>.Filter.And(
               Builders<Place>.Filter.Eq(x => x.UserId, UserId),
               Builders<Place>.Filter.Gt(x => x.LastUpdate, time),
               Builders<Place>.Filter.Eq(x => x.IsDeleted, IsDeleted));
            var got = await places.Find(filter).ToListAsync();

            return got.ToArray();
        }
        #endregion

        #region Goal

        public void RegisterGoalModel()
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(Goal)) == false)
            {
                BsonClassMap.RegisterClassMap<Goal>(entity =>
                {
                    entity.MapIdProperty(e => e.Name)
                        .SetIsRequired(true)
                        .SetElementName("Name");
                    entity.MapIdProperty(e => e.Description)
                        .SetIsRequired(false)
                        .SetElementName("Description");
                    entity.MapIdProperty(e => e.IsDone)
                        .SetIsRequired(true)
                        .SetElementName("IsDone");
                    entity.MapIdProperty(e => e.UserId)
                        .SetIsRequired(true)
                        .SetElementName("UserId")
                        .SetSerializer(new GuidSerializer(BsonType.String));
                    entity.MapIdProperty(e => e.LastUpdate)
                       .SetIsRequired(true)
                       .SetElementName("LastUpdate");
                    entity.MapIdProperty(e => e.IsDeleted)
                       .SetIsRequired(true)
                       .SetElementName("IsDeleted");
                    entity.MapIdProperty(e => e.Id)
                        .SetIsRequired(true)
                        .SetSerializer(new GuidSerializer(BsonType.String));
                    entity.SetIdMember(entity.GetMemberMap(e => e.Id));
                });
            }
        }

        public async Task<Goal> UpsertGoal(Goal goal, Guid UserId)
        {
            if (goal == null) return null;

            var filter = Builders<Goal>.Filter.And(
                Builders<Goal>.Filter.Eq(entity => entity.Id, goal.Id),
                Builders<Goal>.Filter.Eq(entity => entity.UserId, UserId));

            var definition = Builders<Goal>.Update
                .Combine(
                Builders<Goal>.Update.Set(e => e.Name, goal.Name),
                Builders<Goal>.Update.Set(e => e.IsDone, goal.IsDone),
                Builders<Goal>.Update.Set(e => e.Description, goal.Description),
                Builders<Goal>.Update.Set(e => e.UserId, UserId),
                Builders<Goal>.Update.Set(e => e.LastUpdate, DateTime.UtcNow.Ticks),
                Builders<Goal>.Update.Set(e => e.IsDeleted, false));
            var options = new FindOneAndUpdateOptions<Goal, Goal>()
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            return await goals.FindOneAndUpdateAsync(filter, definition, options).ConfigureAwait(false);
        }

        public async Task<Goal> ReadGoal(Guid Id, Guid UserId)
        {
            var filter = Builders<Goal>.Filter.And(
               Builders<Goal>.Filter.Eq(x => x.Id, Id),
               Builders<Goal>.Filter.Eq(x => x.UserId, UserId),
               Builders<Goal>.Filter.Eq(x => x.IsDeleted, false));
            var gotgoals = await goals.Find(filter).ToListAsync();

            if (gotgoals.Count == 1)
            {
                return gotgoals[0];
            }

            return null;
        }
        public async Task<Goal[]> ReadManyGoals(Guid[] Ids, Guid UserId)
        {
            var filter = Builders<Goal>.Filter.And(
               Builders<Goal>.Filter.Where(x => Ids.Contains(x.Id)),
               Builders<Goal>.Filter.Eq(x => x.UserId, UserId),
               Builders<Goal>.Filter.Eq(x => x.IsDeleted, false));
            var gotgoals = await goals.Find(filter).ToListAsync();

            var returngoals = new List<Goal>();

            foreach (var id in Ids)
            {
                var got = gotgoals.FirstOrDefault(x => x.Id == id);
                if (got != null)
                {
                    returngoals.Add(got);
                }
            }

            return returngoals.ToArray();
        }

        public async Task<Guid> DeleteGoal(Guid Id, bool deletefromtrip, Guid UserId)
        {
            var filter = Builders<Goal>.Filter.And(
                Builders<Goal>.Filter.Eq(entity => entity.Id, Id),
                Builders<Goal>.Filter.Eq(entity => entity.UserId, UserId));
            var update = Builders<Goal>.Update
                .Combine(
                Builders<Goal>.Update.Set(e => e.LastUpdate, DateTime.UtcNow.Ticks),
                Builders<Goal>.Update.Set(e => e.IsDeleted, true));
            var result = await goals.UpdateOneAsync(filter, update).ConfigureAwait(false);
            //await goals.DeleteOneAsync(filter).ConfigureAwait(false);

            if (deletefromtrip)
            {
                var tripfilter = Builders<Trip>.Filter.And(
                    Builders<Trip>.Filter.Eq(ed => ed.UserId, UserId),
                    Builders<Trip>.Filter.Where(x => x.GoalIds.Contains(Id)));
                var trips = await this.trips.Find(tripfilter).ToListAsync();

                foreach (var trip in trips)
                {
                    var goalids = trip.GoalIds.ToList();
                    goalids.Remove(Id);
                    trip.GoalIds = goalids.ToArray();
                    await UpsertTrip(trip, UserId);
                }
            }

            return Id;
        }

        public async Task<Guid[]> DeleteManyGoals(Guid[] Ids, bool deletefromtrip, Guid UserId)
        {
            var filter = Builders<Goal>.Filter.And(
                Builders<Goal>.Filter.Where(entity => Ids.Contains(entity.Id)),
                Builders<Goal>.Filter.Eq(entity => entity.UserId, UserId));
            var update = Builders<Goal>.Update
                .Combine(
                Builders<Goal>.Update.Set(e => e.LastUpdate, DateTime.UtcNow.Ticks),
                Builders<Goal>.Update.Set(e => e.IsDeleted, true));
            var result = await goals.UpdateManyAsync(filter, update).ConfigureAwait(false);
            //await goals.DeleteOneAsync(filter).ConfigureAwait(false);

            if (deletefromtrip)
            {
                foreach (var Id in Ids)
                {
                    var tripfilter = Builders<Trip>.Filter.And(
                    Builders<Trip>.Filter.Eq(ed => ed.UserId, UserId),
                    Builders<Trip>.Filter.Where(x => x.GoalIds.Contains(Id)));
                    var trips = await this.trips.Find(tripfilter).ToListAsync();

                    foreach (var trip in trips)
                    {
                        var goalids = trip.GoalIds.ToList();
                        goalids.Remove(Id);
                        trip.GoalIds = goalids.ToArray();
                        await UpsertTrip(trip, UserId);
                    }
                }
            }

            return Ids;
        }

        public async Task<Guid[]> GetAllGoalIds(Guid UserId)
        {
            var filter = Builders<Goal>.Filter.And(
                Builders<Goal>.Filter.Eq(x => x.UserId, UserId),
                Builders<Goal>.Filter.Eq(x => x.IsDeleted, false));
            var gotids = (await goals.Find(filter).ToListAsync()).Select(x => x.Id).ToList();

            return gotids.ToArray();
        }

        public async Task<Goal[]> GetAllGoals(Guid UserId, long time = 0, bool IsDeleted = false)
        {
            var filter = Builders<Goal>.Filter.And(
               Builders<Goal>.Filter.Eq(x => x.UserId, UserId),
               Builders<Goal>.Filter.Gt(x => x.LastUpdate, time),
               Builders<Goal>.Filter.Eq(x => x.IsDeleted, IsDeleted));
            var got = await goals.Find(filter).ToListAsync();

            return got.ToArray();
        }
        #endregion

        #region Good
        public void RegisterGoodModel()
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(Good)) == false)
            {
                BsonClassMap.RegisterClassMap<Good>(entity =>
                {
                    entity.MapIdProperty(e => e.Name)
                        .SetIsRequired(true)
                        .SetElementName("Name");
                    entity.MapIdProperty(e => e.Description)
                        .SetIsRequired(false)
                        .SetElementName("Description");
                    entity.MapIdProperty(e => e.IsTook)
                        .SetIsRequired(true)
                        .SetElementName("IsTook");
                    entity.MapIdProperty(e => e.UserId)
                        .SetIsRequired(true)
                        .SetElementName("UserId")
                        .SetSerializer(new GuidSerializer(BsonType.String));
                    entity.MapIdProperty(e => e.LastUpdate)
                       .SetIsRequired(true)
                       .SetElementName("LastUpdate");
                    entity.MapIdProperty(e => e.IsDeleted)
                       .SetIsRequired(true)
                       .SetElementName("IsDeleted");
                    entity.MapIdProperty(e => e.Count)
                       .SetIsRequired(true)
                       .SetElementName("Count");
                    entity.MapIdProperty(e => e.Id)
                        .SetIsRequired(true)
                        .SetSerializer(new GuidSerializer(BsonType.String));
                    entity.SetIdMember(entity.GetMemberMap(e => e.Id));
                });
            }
        }
        public async Task<Good> UpsertGood(Good good, Guid UserId)
        {
            if (good == null) return null;

            var filter = Builders<Good>.Filter.And(
                Builders<Good>.Filter.Eq(entity => entity.Id, good.Id),
                Builders<Good>.Filter.Eq(entity => entity.UserId, UserId));

            var definition = Builders<Good>.Update
                .Combine(
                Builders<Good>.Update.Set(e => e.Name, good.Name),
                Builders<Good>.Update.Set(e => e.IsTook, good.IsTook),
                Builders<Good>.Update.Set(e => e.Description, good.Description),
                Builders<Good>.Update.Set(e => e.UserId, UserId),
                Builders<Good>.Update.Set(e => e.LastUpdate, DateTime.UtcNow.Ticks),
                Builders<Good>.Update.Set(e => e.IsDeleted, false),
                Builders<Good>.Update.Set(e => e.Count, good.Count));
            var options = new FindOneAndUpdateOptions<Good, Good>()
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            return await goods.FindOneAndUpdateAsync(filter, definition, options).ConfigureAwait(false);
        }

        public async Task<Good> ReadGood(Guid Id, Guid UserId)
        {
            var filter = Builders<Good>.Filter.And(
               Builders<Good>.Filter.Eq(x => x.Id, Id),
               Builders<Good>.Filter.Eq(x => x.UserId, UserId),
               Builders<Good>.Filter.Eq(x => x.IsDeleted, false));
            var gotgoods = await goods.Find(filter).ToListAsync();

            if (gotgoods.Count == 1)
            {
                return gotgoods[0];
            }

            return null;
        }

        public async Task<Good[]> ReadManyGoods(Guid[] Ids, Guid UserId)
        {
            var filter = Builders<Good>.Filter.And(
               Builders<Good>.Filter.Where(x => Ids.Contains(x.Id)),
               Builders<Good>.Filter.Eq(x => x.UserId, UserId),
               Builders<Good>.Filter.Eq(x => x.IsDeleted, false));
            var gotgoods = await goods.Find(filter).ToListAsync();

            var returngoods = new List<Good>();

            foreach (var id in Ids)
            {
                var got = gotgoods.FirstOrDefault(x => x.Id == id);
                if (got != null)
                {
                    returngoods.Add(got);
                }
            }

            return returngoods.ToArray();
        }

        public async Task<Guid> DeleteGood(Guid Id, bool deletefromtrip, Guid UserId)
        {
            var filter = Builders<Good>.Filter.And(
                Builders<Good>.Filter.Eq(entity => entity.Id, Id),
                Builders<Good>.Filter.Eq(entity => entity.UserId, UserId));
            var update = Builders<Good>.Update
                .Combine(
                Builders<Good>.Update.Set(e => e.LastUpdate, DateTime.UtcNow.Ticks),
                Builders<Good>.Update.Set(e => e.IsDeleted, true));
            var result = await goods.UpdateOneAsync(filter, update).ConfigureAwait(false);
            //await goods.DeleteOneAsync(filter).ConfigureAwait(false);

            if (deletefromtrip)
            {
                var tripfilter = Builders<Trip>.Filter.And(
                    Builders<Trip>.Filter.Eq(ed => ed.UserId, UserId),
                    Builders<Trip>.Filter.Where(x => x.GoodIds.Contains(Id)));
                var trips = await this.trips.Find(tripfilter).ToListAsync();

                foreach (var trip in trips)
                {
                    var goodids = trip.GoodIds.ToList();
                    goodids.Remove(Id);
                    trip.GoodIds = goodids.ToArray();
                    await UpsertTrip(trip, UserId);
                }
            }

            return Id;
        }

        public async Task<Guid[]> DeleteManyGoods(Guid[] Ids, bool deletefromtrip, Guid UserId)
        {
            var filter = Builders<Good>.Filter.And(
                Builders<Good>.Filter.Where(entity => Ids.Contains(entity.Id)),
                Builders<Good>.Filter.Eq(entity => entity.UserId, UserId));
            var update = Builders<Good>.Update
                .Combine(
                Builders<Good>.Update.Set(e => e.LastUpdate, DateTime.UtcNow.Ticks),
                Builders<Good>.Update.Set(e => e.IsDeleted, true));
            var result = await goods.UpdateManyAsync(filter, update).ConfigureAwait(false);
            //await goods.DeleteOneAsync(filter).ConfigureAwait(false);

            if (deletefromtrip)
            {
                foreach (var Id in Ids)
                {
                    var tripfilter = Builders<Trip>.Filter.And(
                    Builders<Trip>.Filter.Eq(ed => ed.UserId, UserId),
                    Builders<Trip>.Filter.Where(x => x.GoodIds.Contains(Id)));
                    var trips = await this.trips.Find(tripfilter).ToListAsync();

                    foreach (var trip in trips)
                    {
                        var goodids = trip.GoodIds.ToList();
                        goodids.Remove(Id);
                        trip.GoodIds = goodids.ToArray();
                        await UpsertTrip(trip, UserId);
                    }
                }
            }

            return Ids;
        }

        public async Task<Guid[]> GetAllGoodIds(Guid UserId)
        {
            var filter = Builders<Good>.Filter.And(
                Builders<Good>.Filter.Eq(x => x.UserId, UserId),
                Builders<Good>.Filter.Eq(x => x.IsDeleted, false));
            var gotids = (await goods.Find(filter).ToListAsync()).Select(x => x.Id).ToList();

            return gotids.ToArray();
        }

        public async Task<Good[]> GetAllGoods(Guid UserId, long time = 0, bool IsDeleted = false)
        {
            var filter = Builders<Good>.Filter.And(
                Builders<Good>.Filter.Eq(x => x.UserId, UserId),
                Builders<Good>.Filter.Gt(x => x.LastUpdate, time),
                Builders<Good>.Filter.Eq(x => x.IsDeleted, IsDeleted));
            var got = await goods.Find(filter).ToListAsync();

            return got.ToArray();
        }
        #endregion

        #region Category
        public void RegisterCategoryModel()
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(Category)) == false)
            {
                BsonClassMap.RegisterClassMap<Category>(entity =>
                {
                    entity.MapIdProperty(e => e.Name)
                        .SetIsRequired(true)
                        .SetElementName("Name");
                    entity.MapIdProperty(e => e.Description)
                        .SetIsRequired(false)
                        .SetElementName("Description");
                    entity.MapIdProperty(e => e.Id)
                        .SetIsRequired(true)
                        .SetSerializer(new GuidSerializer(BsonType.String));
                    entity.SetIdMember(entity.GetMemberMap(e => e.Id));
                });
            }
        }
        public async Task<Category> UpsertCategory(Category category)
        {
            if (category == null) return null;

            var filter = Builders<Category>.Filter
                .Eq(entity => entity.Id, category.Id);
            var definition = Builders<Category>.Update
                .Combine(
                Builders<Category>.Update.Set(e => e.Name, category.Name),
                Builders<Category>.Update.Set(e => e.Description, category.Description));
            var options = new FindOneAndUpdateOptions<Category, Category>()
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            return await categories.FindOneAndUpdateAsync(filter, definition, options).ConfigureAwait(false);
        }

        public async Task<Category> ReadCategory(Guid Id)
        {
            var filter = Builders<Category>.Filter.Eq(x => x.Id, Id);
            var gotcategories = await categories.Find(filter).ToListAsync();

            if (gotcategories.Count == 1)
            {
                return gotcategories[0];
            }

            return null;
        }

        public async Task<Guid> DeleteCategory(Guid Id)
        {
            var filter = Builders<Category>.Filter.Eq(ed => ed.Id, Id);
            await categories.DeleteOneAsync(filter).ConfigureAwait(false);
            return Id;
        }

        public async Task<Guid[]> GetAllCategoryIds()
        {
            var filter = Builders<Category>.Filter.Empty;
            var gotids = (await categories.Find(filter).ToListAsync()).Select(x => x.Id).ToList();

            return gotids.ToArray();
        }

        public async Task<Category[]> GetAllCategories()
        {
            var filter = Builders<Category>.Filter.Empty;
            var gotcategories = (await categories.Find(filter).ToListAsync());

            return gotcategories.ToArray();
        }
        #endregion

        #region Purchase

        public void RegisterPurchaseModel()
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(Purchase)) == false)
            {
                BsonClassMap.RegisterClassMap<Purchase>(entity =>
                {
                    entity.MapIdProperty(e => e.Name)
                        .SetIsRequired(true)
                        .SetElementName("Name");
                    entity.MapIdProperty(e => e.Description)
                        .SetIsRequired(false)
                        .SetElementName("Description");
                    entity.MapIdProperty(e => e.IsBought)
                        .SetIsRequired(true)
                        .SetElementName("IsBought");
                    entity.MapIdProperty(e => e.Price)
                        .SetIsRequired(true)
                        .SetElementName("Price");
                    entity.MapIdProperty(e => e.CategoryId)
                        .SetIsRequired(true)
                        .SetElementName("CategoryId")
                        .SetSerializer(new GuidSerializer(BsonType.String));
                    entity.MapIdProperty(e => e.UserId)
                        .SetIsRequired(true)
                        .SetElementName("UserId")
                        .SetSerializer(new GuidSerializer(BsonType.String));
                    entity.MapIdProperty(e => e.LastUpdate)
                       .SetIsRequired(true)
                       .SetElementName("LastUpdate");
                    entity.MapIdProperty(e => e.IsDeleted)
                       .SetIsRequired(true)
                       .SetElementName("IsDeleted");
                    entity.MapIdProperty(e => e.Id)
                        .SetIsRequired(true)
                        .SetSerializer(new GuidSerializer(BsonType.String));
                    entity.SetIdMember(entity.GetMemberMap(e => e.Id));
                });
            }
        }

        public async Task<Purchase> UpsertPurchase(Purchase purchase, Guid UserId)
        {
            if (purchase == null) return null;

            var filter = Builders<Purchase>.Filter.And(
                Builders<Purchase>.Filter.Eq(entity => entity.Id, purchase.Id),
                Builders<Purchase>.Filter.Eq(entity => entity.UserId, UserId));

            var definition = Builders<Purchase>.Update
                .Combine(
                Builders<Purchase>.Update.Set(e => e.Name, purchase.Name),
                Builders<Purchase>.Update.Set(e => e.Description, purchase.Description),
                Builders<Purchase>.Update.Set(e => e.CategoryId, purchase.CategoryId),
                Builders<Purchase>.Update.Set(e => e.Price, purchase.Price),
                Builders<Purchase>.Update.Set(e => e.IsBought, purchase.IsBought),
                Builders<Purchase>.Update.Set(e => e.UserId, UserId),
                Builders<Purchase>.Update.Set(e => e.LastUpdate, DateTime.UtcNow.Ticks),
                Builders<Purchase>.Update.Set(e => e.IsDeleted, false));
            var options = new FindOneAndUpdateOptions<Purchase, Purchase>()
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            return await purchases.FindOneAndUpdateAsync(filter, definition, options).ConfigureAwait(false);
        }

        public async Task<Purchase> ReadPurchase(Guid Id, Guid UserId)
        {
            var filter = Builders<Purchase>.Filter.And(
               Builders<Purchase>.Filter.Eq(x => x.Id, Id),
               Builders<Purchase>.Filter.Eq(x => x.UserId, UserId),
               Builders<Purchase>.Filter.Eq(x => x.IsDeleted, false));
            var gotpurchases = await purchases.Find(filter).ToListAsync();

            if (gotpurchases.Count == 1)
            {
                return gotpurchases[0];
            }

            return null;
        }

        public async Task<Purchase[]> ReadManyPurchases(Guid[] Ids, Guid UserId)
        {
            var filter = Builders<Purchase>.Filter.And(
               Builders<Purchase>.Filter.Where(x => Ids.Contains(x.Id)),
               Builders<Purchase>.Filter.Eq(x => x.UserId, UserId),
               Builders<Purchase>.Filter.Eq(x => x.IsDeleted, false));
            var gotpurchases = await purchases.Find(filter).ToListAsync();

            var returnpurchases = new List<Purchase>();
            
            foreach(var id in Ids)
            {
                var got = gotpurchases.FirstOrDefault(x => x.Id == id);
                if(got != null)
                {
                    returnpurchases.Add(got);
                }
            }

            return returnpurchases.ToArray();
        }

        public async Task<Guid> DeletePurchase(Guid Id, bool deletefromtrip, Guid UserId)
        {
            var filter = Builders<Purchase>.Filter.And(
                Builders<Purchase>.Filter.Eq(entity => entity.Id, Id),
                Builders<Purchase>.Filter.Eq(entity => entity.UserId, UserId));
            var update = Builders<Purchase>.Update
               .Combine(
               Builders<Purchase>.Update.Set(e => e.LastUpdate, DateTime.UtcNow.Ticks),
               Builders<Purchase>.Update.Set(e => e.IsDeleted, true));
            var result = await purchases.UpdateOneAsync(filter, update).ConfigureAwait(false);
            //await purchases.DeleteOneAsync(filter).ConfigureAwait(false);

            if (deletefromtrip)
            {
                var tripfilter = Builders<Trip>.Filter.And(
                    Builders<Trip>.Filter.Eq(ed => ed.UserId, UserId),
                    Builders<Trip>.Filter.Where(x => x.PurchaseIds.Contains(Id)));
                var trips = await this.trips.Find(tripfilter).ToListAsync();

                foreach (var trip in trips)
                {
                    var purchaseids = trip.PurchaseIds.ToList();
                    purchaseids.Remove(Id);
                    trip.PurchaseIds = purchaseids.ToArray();
                    await UpsertTrip(trip, UserId);
                }
            }

            return Id;
        }

        public async Task<Guid[]> DeleteManyPurchases(Guid[] Ids, bool deletefromtrip, Guid UserId)
        {
            var filter = Builders<Purchase>.Filter.And(
                Builders<Purchase>.Filter.Where(entity => Ids.Contains(entity.Id)),
                Builders<Purchase>.Filter.Eq(entity => entity.UserId, UserId));
            var update = Builders<Purchase>.Update
               .Combine(
               Builders<Purchase>.Update.Set(e => e.LastUpdate, DateTime.UtcNow.Ticks),
               Builders<Purchase>.Update.Set(e => e.IsDeleted, true));
            var result = await purchases.UpdateManyAsync(filter, update).ConfigureAwait(false);
            //await purchases.DeleteOneAsync(filter).ConfigureAwait(false);

            if (deletefromtrip)
            {
                foreach (var Id in Ids)
                {
                    var tripfilter = Builders<Trip>.Filter.And(
                        Builders<Trip>.Filter.Eq(ed => ed.UserId, UserId),
                        Builders<Trip>.Filter.Where(x => x.PurchaseIds.Contains(Id)));
                    var trips = await this.trips.Find(tripfilter).ToListAsync();

                    foreach (var trip in trips)
                    {
                        var purchaseids = trip.PurchaseIds.ToList();
                        purchaseids.Remove(Id);
                        trip.PurchaseIds = purchaseids.ToArray();
                        await UpsertTrip(trip, UserId);
                    }
                }
            }

            return Ids;
        }

        public async Task<Guid[]> GetAllPurchaseIds(Guid UserId)
        {
            var filter = Builders<Purchase>.Filter.And(
                Builders<Purchase>.Filter.Eq(x => x.UserId, UserId),
                Builders<Purchase>.Filter.Eq(x => x.IsDeleted, false));
            var gotids = (await purchases.Find(filter).ToListAsync()).Select(x => x.Id).ToList();

            return gotids.ToArray();
        }

        public async Task<Purchase[]> GetAllPurchases(Guid UserId, long time = 0, bool IsDeleted = false)
        {
            var filter = Builders<Purchase>.Filter.And(
                Builders<Purchase>.Filter.Eq(x => x.UserId, UserId),
                Builders<Purchase>.Filter.Gt(x => x.LastUpdate, time),
                Builders<Purchase>.Filter.Eq(x => x.IsDeleted, IsDeleted));
            var got = await purchases.Find(filter).ToListAsync();

            return got.ToArray();
        }
        #endregion
    }
}
