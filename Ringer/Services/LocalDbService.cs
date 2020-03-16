using System.Collections.Generic;
using System.Threading.Tasks;
using Ringer.Helpers;
using Ringer.Models;
using SQLite;

namespace Ringer.Services
{
    public interface ILocalDbService
    {
        Task<List<MessageModel>> GetAllAsync(bool desc = false);
        Task ResetMessagesAsync();
        Task<MessageModel> GetMessageAsync(int id);
        Task<MessageModel> SaveMessageAsync(MessageModel message, bool update = false);
        Task<MessageModel> GetLocallySavedLastServerMessageAsync(string currentRoomId);
        Task<int> GetLocallySavedLastServerMessageIdAsync(string currentRoomId);
        Task<MessageModel> GetLastMessageAsync(string roomId);
        Task<MessageModel> GetLastMessageAsync(MessageModel message);
        Task<List<MessageModel>> GetMessagesAsync(int skip = 0, int take = 50, bool initial = false);
        Task<MessageModel> GetSentMessageAsync(string roomId);
    }

    public class LocalDbService : ILocalDbService
    {
        private SQLiteAsyncConnection _database;

        public LocalDbService()
        {
            _database = new SQLiteAsyncConnection(Constants.DbPath);
            _database.CreateTableAsync<MessageModel>().Wait();
        }

        public async Task<MessageModel> GetSentMessageAsync(string roomId)
        {
            return await _database.Table<MessageModel>()
                .OrderByDescending(m => m.Id)
                .FirstOrDefaultAsync(m => m.RoomId == roomId && m.ServerId == -1);
        }

        public async Task<MessageModel> GetLastMessageAsync(string roomId)
        {
            return await _database.Table<MessageModel>()
                .Where(m => m.RoomId == roomId)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<MessageModel> GetLastMessageAsync(MessageModel message)
        {
            return await _database.Table<MessageModel>()
                .Where(m => m.RoomId == message.RoomId && m.Id < message.Id)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<MessageModel>> GetMessagesAsync(int skip = 0, int take = 50, bool initial = false)
        {
            List<MessageModel> messageModels =
                await _database.Table<MessageModel>()
                    .Where(m => m.RoomId == App.RoomId)
                    .OrderByDescending(m => m.Id)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();

            if (initial)
                messageModels.Sort((m1, m2) => m1.Id.CompareTo(m2.Id));

            return messageModels;
        }


        public async Task<List<MessageModel>> GetAllAsync(bool desc = false)
        {
            List<MessageModel> messageModels = desc ?
                await _database.Table<MessageModel>().Where(m => m.RoomId == App.RoomId).OrderByDescending(m => m.Id).ToListAsync() :
                await _database.Table<MessageModel>().Where(m => m.RoomId == App.RoomId).ToListAsync();

            return messageModels;
        }
        public async Task<MessageModel> GetMessageAsync(int id)
        {
            MessageModel message = await _database.Table<MessageModel>()
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();

            return message;
        }
        public async Task<MessageModel> SaveMessageAsync(MessageModel message, bool update = false)
        {
            if (update)
            {
                await _database.UpdateAsync(message);
            }
            else
            {
                await _database.InsertAsync(message);
            }

            return message;
        }
        public Task ResetMessagesAsync()
        {
            return _database.DeleteAllAsync<MessageModel>();
        }
        public async Task<int> GetLocallySavedLastServerMessageIdAsync(string currentRoomId)
        {
            var message = await GetLocallySavedLastServerMessageAsync(currentRoomId);
            return message?.ServerId ?? -1;
        }
        public Task<MessageModel> GetLocallySavedLastServerMessageAsync(string currentRoomId)
        {
            return _database.Table<MessageModel>()
                .Where(m => m.RoomId == currentRoomId)
                .OrderByDescending(m => m.ServerId)
                .FirstOrDefaultAsync();
        }
    }
}
