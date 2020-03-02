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
        #endregion

        #region Constructors
        public MongoStorage(IMongoClient client)
        {
            this.client = client;
            InitCollections();
        }
        #endregion

        #region Service Methods
        private void InitCollections()
        {
            var database = client.GetDatabase("test");

            Register();
            tokens = database.GetCollection<UserToken>("Tokens");
            users = database.GetCollection<User>("Users");
            photos = database.GetCollection<Photo>("Photos");
            trips = database.GetCollection<Trip>("Trips");
            places = database.GetCollection<Place>("Places");
            goods = database.GetCollection<Good>("Goods");
            goals = database.GetCollection<Goal>("Goals");
            //CreateIndexes();
        }

        private void Register()
        {
            RegisterUserModels();

            RegisterPhotoModel();

            RegisterTripModel();

            RegisterPlaceModel();

            RegisterGoodModel();

            RegisterGoalModel();

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
                Password = password
            };

            if(await FindUserEmail(email))
            {
                throw new ArgumentException("Such email exists!");
            }

            await users.InsertOneAsync(user);

            return await FindUser(user.Email, user.Password);
        }

        public async Task<UserToken> FindUser(string email, string password)
        {
            if(!await FindUserEmail(email))
            {
                throw new ArgumentException("Such email doesn't exist!");
            }

            var filter = new BsonDocument("$and", new BsonArray{    
                new BsonDocument("Email", email),
                new BsonDocument("Password", password)
            });

            var gotusers = await users.Find(filter).ToListAsync(); 

            if(gotusers.Count != 1)
            {
                throw new ArgumentException("Such account doesn't exist!");
            }

            UserToken token = new UserToken()
            {
                UserId = gotusers[0].Id,
                Token = TokenGenerator()
            };

            await tokens.InsertOneAsync(token);
            return token;
        }

        public async Task<bool> FindUserEmail(string email)
        {
            var filter = new BsonDocument("Email", email);
            var count = await users.CountDocumentsAsync(filter);

            return !(count == 0);
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
            if (BsonClassMap.IsClassMapRegistered(typeof(Photo)) == false)
            {
                BsonClassMap.RegisterClassMap<Photo>(entity =>
                {
                    entity.MapIdProperty(e => e.UserId)
                        .SetIsRequired(true)
                        .SetSerializer(new GuidSerializer(BsonType.String))
                        .SetElementName("UserId");
                    entity.MapIdProperty(e => e.Base64)
                        .SetIsRequired(true)
                        .SetElementName("Base64");
                    entity.MapIdProperty(e => e.Id)
                        .SetIsRequired(true)
                        .SetSerializer(new GuidSerializer(BsonType.String));
                    entity.SetIdMember(entity.GetMemberMap(e => e.Id));
                });
            }
        }

        public async Task<Photo> UploadPhoto(string Base64, Guid UserId)
        {
            Photo photo = new Photo()
            {
                UserId = UserId,
                Base64 = Base64,
                Id = Guid.NewGuid()
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
                    entity.MapIdProperty(e => e.Photos)
                        .SetIsRequired(false)
                        .SetElementName("Photos");
                    entity.MapIdProperty(e => e.Places)
                        .SetIsRequired(false)
                        .SetElementName("Places");
                    entity.MapIdProperty(e => e.Goods)
                        .SetIsRequired(false)
                        .SetElementName("Goods");
                    entity.MapIdProperty(e => e.Goals)
                        .SetIsRequired(false)
                        .SetElementName("Goals");
                    entity.MapIdProperty(e => e.ToDate)
                        .SetIsRequired(false)
                        .SetElementName("ToDate");
                    entity.MapIdProperty(e => e.FromDate)
                        .SetIsRequired(false)
                        .SetElementName("FromDate");
                    entity.MapIdProperty(e => e.UserId)
                        .SetIsRequired(true)
                        .SetElementName("UserId")
                        .SetSerializer(new GuidSerializer(BsonType.String));
                    entity.MapIdProperty(e => e.Id)
                        .SetIsRequired(true)
                        .SetSerializer(new GuidSerializer(BsonType.String));
                    entity.SetIdMember(entity.GetMemberMap(e => e.Id));
                });
            }
        }

        public async Task<Trip> UpsertTrip(Trip trip)
        {
            if (trip == null) return null;

            var filter = Builders<Trip>.Filter
                .Eq(entity => entity.Id, trip.Id);
            var definition = Builders<Trip>.Update
                .Combine(
                Builders<Trip>.Update.Set(e => e.Name, trip.Name),
                Builders<Trip>.Update.Set(e => e.Photos, trip.Photos),
                Builders<Trip>.Update.Set(e => e.Places, trip.Places),
                Builders<Trip>.Update.Set(e => e.Goals, trip.Goals),
                Builders<Trip>.Update.Set(e => e.Goods, trip.Goods),
                Builders<Trip>.Update.Set(e => e.TextField, trip.TextField),
                Builders<Trip>.Update.Set(e => e.UserId, trip.UserId),
                Builders<Trip>.Update.Set(e => e.FromDate, trip.FromDate),
                Builders<Trip>.Update.Set(e => e.ToDate, trip.ToDate));
            var options = new FindOneAndUpdateOptions<Trip, Trip>()
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            return await trips.FindOneAndUpdateAsync(filter, definition, options).ConfigureAwait(false);

        }

        public async Task<Trip> ReadTrip(Guid Id)
        {
            var filter = Builders<Trip>.Filter.Eq(x => x.Id, Id);
            var gottrips = await trips.Find(filter).ToListAsync();

            if (gottrips.Count == 1)
            {
                return gottrips[0];
            }

            return null;
        }

        public async Task<Guid> DeleteTrip(Guid Id)
        {
            var filter = Builders<Trip>.Filter.Eq(ed => ed.Id, Id);
            await trips.DeleteOneAsync(filter).ConfigureAwait(false);
            return Id;
        }

        public async Task<Guid[]> GetAllTrips(Guid UserId)
        {
            var filter = Builders<Trip>.Filter.Eq(x => x.UserId, UserId);
            var gotids = (await trips.Find(filter).ToListAsync()).Select(x => x.Id).ToList();

            return gotids.ToArray();
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
                    entity.MapIdProperty(e => e.Photos)
                        .SetIsRequired(false)
                        .SetElementName("Photos");
                    entity.MapIdProperty(e => e.UserId)
                        .SetIsRequired(true)
                        .SetElementName("UserId")
                        .SetSerializer(new GuidSerializer(BsonType.String));
                    entity.MapIdProperty(e => e.Id)
                        .SetIsRequired(true)
                        .SetSerializer(new GuidSerializer(BsonType.String));
                    entity.SetIdMember(entity.GetMemberMap(e => e.Id));
                });
            }
        }

        public async Task<Place> UpsertPlace(Place place)
        {
            if (place == null) return null;

            var filter = Builders<Place>.Filter
                .Eq(entity => entity.Id, place.Id);
            var definition = Builders<Place>.Update
                .Combine(
                Builders<Place>.Update.Set(e => e.Name, place.Name),
                Builders<Place>.Update.Set(e => e.Photos, place.Photos),
                Builders<Place>.Update.Set(e => e.Adress, place.Adress),
                Builders<Place>.Update.Set(e => e.Description, place.Description),
                Builders<Place>.Update.Set(e => e.UserId, place.UserId));
            var options = new FindOneAndUpdateOptions<Place, Place>()
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            return await places.FindOneAndUpdateAsync(filter, definition, options).ConfigureAwait(false);
        }

        public async Task<Place> ReadPlace(Guid Id)
        {
            var filter = Builders<Place>.Filter.Eq(x => x.Id, Id);
            var gotplaces = await places.Find(filter).ToListAsync();

            if (gotplaces.Count == 1)
            {
                return gotplaces[0];
            }

            return null;
        }

        public async Task<Guid> DeletePlace(Guid Id)
        {
            var filter = Builders<Place>.Filter.Eq(ed => ed.Id, Id);
            await places.DeleteOneAsync(filter).ConfigureAwait(false);
            return Id;
        }

        public async Task<Guid[]> GetAllPlaces(Guid UserId)
        {
            var filter = Builders<Place>.Filter.Eq(x => x.UserId, UserId);
            var gotids = (await places.Find(filter).ToListAsync()).Select(x => x.Id).ToList();

            return gotids.ToArray();
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
                    entity.MapIdProperty(e => e.Id)
                        .SetIsRequired(true)
                        .SetSerializer(new GuidSerializer(BsonType.String));
                    entity.SetIdMember(entity.GetMemberMap(e => e.Id));
                });
            }
        }

        public async Task<Goal> UpsertGoal(Goal goal)
        {
            if (goal == null) return null;

            var filter = Builders<Goal>.Filter
                .Eq(entity => entity.Id, goal.Id);
            var definition = Builders<Goal>.Update
                .Combine(
                Builders<Goal>.Update.Set(e => e.Name, goal.Name),
                Builders<Goal>.Update.Set(e => e.IsDone, goal.IsDone),
                Builders<Goal>.Update.Set(e => e.Description, goal.Description),
                Builders<Goal>.Update.Set(e => e.UserId, goal.UserId));
            var options = new FindOneAndUpdateOptions<Goal, Goal>()
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            return await goals.FindOneAndUpdateAsync(filter, definition, options).ConfigureAwait(false);
        }

        public async Task<Goal> ReadGoal(Guid Id)
        {
            var filter = Builders<Goal>.Filter.Eq(x => x.Id, Id);
            var gotgoals = await goals.Find(filter).ToListAsync();

            if (gotgoals.Count == 1)
            {
                return gotgoals[0];
            }

            return null;
        }

        public async Task<Guid> DeleteGoal(Guid Id)
        {
            var filter = Builders<Goal>.Filter.Eq(ed => ed.Id, Id);
            await goals.DeleteOneAsync(filter).ConfigureAwait(false);
            return Id;
        }

        public async Task<Guid[]> GetAllGoals(Guid UserId)
        {
            var filter = Builders<Goal>.Filter.Eq(x => x.UserId, UserId);
            var gotids = (await goals.Find(filter).ToListAsync()).Select(x => x.Id).ToList();

            return gotids.ToArray();
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
                    entity.MapIdProperty(e => e.Id)
                        .SetIsRequired(true)
                        .SetSerializer(new GuidSerializer(BsonType.String));
                    entity.SetIdMember(entity.GetMemberMap(e => e.Id));
                });
            }
        }
        public async Task<Good> UpsertGood(Good good)
        {
            if (good == null) return null;

            var filter = Builders<Good>.Filter
                .Eq(entity => entity.Id, good.Id);
            var definition = Builders<Good>.Update
                .Combine(
                Builders<Good>.Update.Set(e => e.Name, good.Name),
                Builders<Good>.Update.Set(e => e.IsTook, good.IsTook),
                Builders<Good>.Update.Set(e => e.Description, good.Description),
                Builders<Good>.Update.Set(e => e.UserId, good.UserId));
            var options = new FindOneAndUpdateOptions<Good, Good>()
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            return await goods.FindOneAndUpdateAsync(filter, definition, options).ConfigureAwait(false);
        }

        public async Task<Good> ReadGood(Guid Id)
        {
            var filter = Builders<Good>.Filter.Eq(x => x.Id, Id);
            var gotgoods = await goods.Find(filter).ToListAsync();

            if (gotgoods.Count == 1)
            {
                return gotgoods[0];
            }

            return null;
        }

        public async Task<Guid> DeleteGood(Guid Id)
        {
            var filter = Builders<Good>.Filter.Eq(ed => ed.Id, Id);
            await goods.DeleteOneAsync(filter).ConfigureAwait(false);
            return Id;
        }

        public async Task<Guid[]> GetAllGoods(Guid UserId)
        {
            var filter = Builders<Good>.Filter.Eq(x => x.UserId, UserId);
            var gotids = (await goods.Find(filter).ToListAsync()).Select(x => x.Id).ToList();

            return gotids.ToArray();
        }
        #endregion

    }
}
