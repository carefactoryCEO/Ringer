using System.Collections.Generic;
using System.Threading.Tasks;
using Ringer.Helpers;
using Ringer.Models;
using SQLite;

namespace Ringer.Services
{
    public interface ILocalDbService
    {
        Task<List<Message>> GetMessagesAsync(bool desc);
        Task<Message> GetMessageAsync(int id);
        Task<int> SaveMessageAsync(Message message, bool update = false);
        Task ResetMessagesAsync();
        Task<Message> GetLastMessage(string currentRoomId);
        Task<int> GetLastMessageIndexAsync(string currentRoomId);
    }

    public class LocalDbService : ILocalDbService
    {
        private SQLiteAsyncConnection _database;

        public LocalDbService()
        {
            _database = new SQLiteAsyncConnection(Constants.DbPath);
            _database.CreateTableAsync<Message>().Wait();
        }

        public Task<List<Message>> GetMessagesAsync(bool desc = false)
        {
            if (desc)
                return _database.Table<Message>().Where(m => m.RoomId == App.CurrentRoomId).OrderByDescending(m => m.Id).ToListAsync();

            return _database.Table<Message>().Where(m => m.RoomId == App.CurrentRoomId).ToListAsync();
        }
        public Task<Message> GetMessageAsync(int id)
        {
            return _database.Table<Message>()
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();
        }
        public Task<int> SaveMessageAsync(Message message, bool update = false)
        {
            if (update)
                return _database.UpdateAsync(message);

            return _database.InsertAsync(message);
        }
        public Task ResetMessagesAsync()
        {
            return _database.DeleteAllAsync<Message>();
        }
        public async Task<int> GetLastMessageIndexAsync(string currentRoomId)
        {
            var message = await GetLastMessage(currentRoomId);
            return message?.Id ?? 0;
        }
        public Task<Message> GetLastMessage(string currentRoomId)
        {
            return _database.Table<Message>()
                .Where(m => m.RoomId == currentRoomId)
                .OrderByDescending(m => m.Id)
                .FirstOrDefaultAsync();
        }
    }
}
