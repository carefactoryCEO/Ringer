using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ringer.Core.Models;
using Ringer.HubServer.Data;

namespace Ringer.HubServer.Services
{
    public interface IUserService
    {
        Task<User> LogInAsync(string email, string password);
        Task<User> CreateAsync(User user, string password);
        Task DeleteAsync(int id);
        IEnumerable<User> GetAll();
        ValueTask<User> GetByIdAsync(int id);
        Task<User> UpdateAsync(User user, string password = null);
    }

    public class UserService : IUserService
    {
        private readonly RingerDbContext _context;

        public UserService(RingerDbContext context)
        {
            _context = context;
        }

        public async Task<User> LogInAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;

            var user = await _context.Users
                .Include(u => u.Devices)
                .SingleOrDefaultAsync(u => u.Email == email).ConfigureAwait(false);

            // check if username exists
            if (user == null)
                return null;

            // check if password is correct
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // authentication successful
            return user;
        }

        public async Task<User> CreateAsync(User user, string password)
        {
            // validation
            if (string.IsNullOrWhiteSpace(password))
                throw new Exception("비밀번호는 필수항목입니다.");

            if (_context.Users.Any(u => u.Email == user.Email))
                throw new Exception($"{user.Email}은 이미 사용중입니다.");

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.CreatedAt = user.UpdatedAt = DateTime.UtcNow;


            _context.Users.Add(user);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return user;
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id).ConfigureAwait(false);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users;
        }

        public ValueTask<User> GetByIdAsync(int id)
        {
            return _context.Users.FindAsync(id);
        }

        public async Task<User> UpdateAsync(User userParam, string password = null)
        {
            var user = await _context.Users.FindAsync(userParam.Id).ConfigureAwait(false);

            if (user == null)
                throw new Exception("User not found");

            // update email if it has changed
            if (!string.IsNullOrWhiteSpace(userParam.Email) && userParam.Email != user.Email)
            {
                // throw error if the new username is already taken
                if (_context.Users.Any(u => u.Email == userParam.Email))
                    throw new Exception("Username " + userParam.Email + " is already taken");

                user.Email = userParam.Email;
            }

            // update user properties if provided
            if (!string.IsNullOrWhiteSpace(userParam.Name))
                user.Name = userParam.Name;

            user.UserType = userParam.UserType;

            if (!string.IsNullOrWhiteSpace(userParam.PhoneNumber))
                user.PhoneNumber = userParam.PhoneNumber;

            // update password if provided
            if (!string.IsNullOrWhiteSpace(password))
            {
                CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return user;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(password));

            using var hmac = new System.Security.Cryptography.HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(password));
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", nameof(storedHash));
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", nameof(storedSalt));

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
    }
}
