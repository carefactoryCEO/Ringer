using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ringer.Models;
using SQLite;

namespace Ringer.Data
{
    public class RingerDatabase
    {
        private SQLiteAsyncConnection _database;

        public RingerDatabase(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Message>().Wait();

        }

        public Task<List<Message>> GetMessagesAsync()
        {
            return _database.Table<Message>().ToListAsync();
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

        public Task<int> DeleteMessageAsync(Message message)
        {
            return _database.DeleteAsync(message);
        }

        public Task ResetDbAsync()
        {
            return _database.DropTableAsync<Message>();
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
