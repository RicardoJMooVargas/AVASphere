using MongoDB.Driver;
using VYAACentralInforApi.Domain.System;
using VYAACentralInforApi.Domain.System.Interfaces;

namespace VYAACentralInforApi.Infrastructure.System
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<Users> _usersCollection;

        public UserRepository(MongoDbContext context)
        {
            _usersCollection = context.Users;
        }

        public async Task<IEnumerable<Users>> GetAllUsersAsync()
        {
            return await _usersCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Users?> GetUserByIdAsync(string id)
        {
            return await _usersCollection.Find(x => x.IdUsers == id).FirstOrDefaultAsync();
        }

        public async Task<Users?> GetUserByUserNameAsync(string userName)
        {
            return await _usersCollection.Find(x => x.UserName == userName).FirstOrDefaultAsync();
        }

        public async Task<Users> CreateUserAsync(Users user)
        {
            await _usersCollection.InsertOneAsync(user);
            return user;
        }

        public async Task<Users> UpdateUserAsync(Users user)
        {
            await _usersCollection.ReplaceOneAsync(x => x.IdUsers == user.IdUsers, user);
            return user;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var result = await _usersCollection.DeleteOneAsync(x => x.IdUsers == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> UserExistsAsync(string userName)
        {
            var count = await _usersCollection.CountDocumentsAsync(x => x.UserName == userName);
            return count > 0;
        }
    }
}