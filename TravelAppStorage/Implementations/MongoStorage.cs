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
            //CreateIndexes();
        }

        private void Register()
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

            return Convert.ToBase64String(list.ToArray());
        }

        private List<byte> GuidToList()
        {
            var guid = Guid.NewGuid();
            return guid.ToByteArray().ToList();
        }


        #endregion

        #region Photo

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
    }
}
