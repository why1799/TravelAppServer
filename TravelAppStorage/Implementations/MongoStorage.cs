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
        }
        #endregion

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
    }
}
