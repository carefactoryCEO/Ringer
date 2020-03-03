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
        Task<Message> GetLocallySavedLastServerMessageAsync(string currentRoomId);
        Task<int> GetLocallySavedLastServerMessageIdAsync(string currentRoomId);
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
        public async Task<int> SaveMessageAsync(Message message, bool update = false)
        {
            if (update)
                return await _database.UpdateAsync(message);

            _ = await _database.InsertAsync(message);

            return message.Id;
        }
        public Task ResetMessagesAsync()
        {
            return _database.DeleteAllAsync<Message>();
        }
        public async Task<int> GetLocallySavedLastServerMessageIdAsync(string currentRoomId)
        {
            var message = await GetLocallySavedLastServerMessageAsync(currentRoomId);
            return message?.ServerId ?? 0;
        }
        public Task<Message> GetLocallySavedLastServerMessageAsync(string currentRoomId)
        {
            return _database.Table<Message>()
                .Where(m => m.RoomId == currentRoomId)
                .OrderByDescending(m => m.ServerId)
                .FirstOrDefaultAsync();
        }
    }
}
