using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DattingApp.API.Helpers;
using DattingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DattingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            _context = context;

        }
        public void Add<T>(T entity) where T: class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T: class
        {
            _context.Remove(entity);
        }

        public async Task<Like> GetLike(int userId, int recepientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recepientId);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            var photo = await _context.Photos.Where(u => u.UserId == userId).FirstOrDefaultAsync(p => p.IsMain);
            return photo;
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
            return photo;
        }

        public async Task<User> GetUser(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.OrderByDescending(u => u.LastActive).AsQueryable();
            users = users.Where(u => u.Id != userParams.UserId);
            users = users.Where(u => u.Gender == userParams.Gender);

            if(userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Now.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Now.AddYears(-userParams.MinAge);

                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }

            if(userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, true);
                users = users.Where(u => userLikers.Contains(u.Id));
            }

            if(userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, false);
                users = users.Where(u => userLikees.Contains(u.Id));
            }

            if(!string.IsNullOrWhiteSpace(userParams.OrderBy))
            {
                switch(userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Create);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }
            
            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if(likers)
            {
                return user.Likers.Where(u => u.LikeeId == id).Select(u => u.LikerId);
            }
            else
            {
                return user.Likees.Where(u => u.LikerId == id).Select(u => u.LikeeId);
            }
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages.AsQueryable();
            
            switch(messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(x => x.RecepientId == messageParams.UserId && !x.RecepientDeleted);
                    break;
                case "Outbox":
                    messages = messages.Where(x => x.SenderId == messageParams.UserId && !x.SenderDeleted);
                    break;
                default:
                    messages = messages.Where(x => x.RecepientId == messageParams.UserId && !x.RecepientDeleted && !x.IsRead);
                    break;
            }

            messages = messages.OrderByDescending(x => x.MessageSent);

            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recepientId)
        {
            var messages = await _context.Messages
                            .Where(u => (u.RecepientId  == userId && !u.RecepientDeleted && u.SenderId == recepientId) ||
                            (u.RecepientId == recepientId && u.SenderId == userId && !u.SenderDeleted))
                            .OrderByDescending(u => u.MessageSent)
                            .ToListAsync();
            
            return messages;
        }
    }
}