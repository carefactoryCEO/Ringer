using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Ringer.Helpers;
using Ringer.Models;
using SQLite;

namespace Ringer.Services
{
    public interface ILocalDbService
    {
        Task<List<MessageModel>> GetMessagesAsync(int take = 50, int skip = 0);
        Task<MessageModel> GetLastMessageAsync(string roomId);
        Task<MessageModel> GetSentMessageAsync(string roomId);
        Task<MessageModel> SaveMessageAsync(MessageModel message);
        Task<MessageModel> UpdateMessageAsync(MessageModel message);
        Task ResetMessagesAsync();
        Task<MessageModel> GetMessageAsync(int id);
    }

    public class LocalDbService : ILocalDbService
    {
        private readonly SQLiteAsyncConnection _database;
        private AsyncTableQuery<MessageModel> _messageTable;

        public LocalDbService()
        {
            Utilities.Trace(Constants.DbPath);
            _database = new SQLiteAsyncConnection(Constants.DbPath);
            _database.CreateTableAsync<MessageModel>().Wait();
            _messageTable = _database.Table<MessageModel>();
        }

        public async Task<List<MessageModel>> GetMessagesAsync(int take = 50, int skip = 0)
        {
            // 날짜의 역순으로 skip만큼 띄고, take만큼 선택
            List<MessageModel> messageModels =
                await _messageTable
                    .Where(m => m.RoomId == App.RoomId)
                    .OrderByDescending(m => m.CreatedAt)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();

            // 날짜순 정렬
            messageModels.Reverse();

            return messageModels;
        }
        public Task<MessageModel> GetSentMessageAsync(string roomId)
        {
            return _messageTable
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync(m => m.RoomId == roomId && m.ServerId == -1);
        }
        public Task<MessageModel> GetLastMessageAsync(string roomId)
        {
            return _messageTable
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync(m => m.RoomId == roomId);
        }
        public async Task<MessageModel> SaveMessageAsync(MessageModel message)
        {
            if (await _database.InsertAsync(message) > 0)
                return message;

            return null;
        }
        public async Task<MessageModel> UpdateMessageAsync(MessageModel message)
        {
            if (await _database.UpdateAsync(message) > 0)
                return message;

            return null;
        }
        public Task ResetMessagesAsync()
        {
            return _database.DeleteAllAsync<MessageModel>();
        }

        public Task<MessageModel> GetMessageAsync(int id)
        {
            return _messageTable.FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
