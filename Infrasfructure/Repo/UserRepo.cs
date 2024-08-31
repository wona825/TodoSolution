using System.Net;
using Application.Contracts;
using Infrasfructure.Data;
using Infrasfructure.Error;
using Microsoft.EntityFrameworkCore;

namespace Infrasfructure.Repo
{
    internal class UserRepo : IUser
    {
        private readonly AppDbContext _appDbContext;

        public UserRepo(AppDbContext appDbContext)
        {
            this._appDbContext = appDbContext;
        }


        /// <summary>
        /// 유저명 중복 체크 메소드 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task<bool> CheckUserNameDuplicationAsync(string? userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new CustomException(HttpStatusCode.BadRequest, "Missing user name.");
            }

            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            return user != null;
        }


        /// <summary>
        /// 유저 소프트 삭제 메소드 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="CustomException"></exception>
        public async Task SoftDeleteUserAsync(int userId)
        {
            var user = await _appDbContext.Users
                .Include(u => u.Todos)
                .Include(u => u.Token)
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new CustomException(HttpStatusCode.NotFound, "User not found.");

            if (user.DisabledAt != null)
            {
                throw new CustomException(HttpStatusCode.BadRequest, "User is already disabled.");
            }

            var currentTime = DateTime.Now;

            user.DisabledAt = currentTime;
            foreach (var todo in user.Todos)
            {
                todo.DisabledAt = currentTime;
            }

            if (user.Token != null)
            {
                _appDbContext.Tokens.Remove(user.Token);
            }

            await _appDbContext.SaveChangesAsync();
        }
    }
}

