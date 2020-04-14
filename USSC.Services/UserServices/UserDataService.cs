﻿using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using USSC.Infrastructure.Models;
using USSC.Infrastructure.Services;
using USSC.Services.UserServices.Interfaces;

namespace USSC.Services.UserServices
{
    public class UserDataService : IUserDataService
    {
        private readonly IApplicationDataService _applicationData;
        private readonly ILogger<UserDataService> _logger;

        public UserDataService(IApplicationDataService applicationData, ILogger<UserDataService> logger)
        {
            _applicationData = applicationData;
            _logger = logger;
        }

        public async Task<User> GetUserData(string email)
        {
            var user = await _applicationData.Data.Users.FindUserByEmail(email);

            return user;
        }

        public async Task<User> GetUserData(int id)
        {
            var user = await _applicationData.Data.Users.FindUserById(id);

            return user;
        }

        public async Task<IEnumerable<string>> GetUserRoles(int userId)
        {
            var roles = await _applicationData.Data.Users.GetUserRoles(userId);

            return roles.Select(r => r.Name);
        }

        public IEnumerable<string> GetAllRoles()
        {
            var allRoles = _applicationData.Data.Roles
                .GetAll()
                .Select(r => r.Name);

            return allRoles;
        }

        public IEnumerable<User> GetAllUsers()
        {
            var users = _applicationData.Data.Users.GetAll();

            return users;
        }

        public async Task<User> EditUser(int userId, string email, string name, string lastName, string password,
            IEnumerable<string> roles)
        {
            _logger.LogInformation($"Edit user {name} {lastName} - {email}");
            var editUser = _applicationData.Data.Users.GetSingleOrDefault(u => u.Id == userId);

            editUser.Email = email;
            editUser.Name = name;
            editUser.LastName = lastName;

            if (password != null)
            {
                editUser.Password = Helpers.ComputeHash(password);
            }

            _applicationData.Data.Users.RemoveUserRoles(userId);
            await _applicationData.Data.SaveChangesAsync();

            await AddRoles(editUser, roles);

            _logger.LogInformation($"User {name} {lastName} - {email} edited successfully");

            return editUser;
        }

        private async Task AddRoles(User user, IEnumerable<string> roles)
        {
            _logger.LogInformation($"Adding roles for user {user.Name} {user.LastName} - {user.Email}");
            var userEntity = await _applicationData.Data.Users.FindUserById(user.Id);

            foreach (var role in roles)
            {
                var roleEntity = await _applicationData.Data.Roles.FindRole(role);
                _applicationData.Data.Users.AssignRole(roleEntity, userEntity);
            }

            await _applicationData.Data.SaveChangesAsync();
            _logger.LogInformation($"Roles Added for user {user.Name} {user.LastName} - {user.Email} successfully");
        }

        public async Task DeleteUser(int userId)
        {
            var user = await _applicationData.Data.Users.FindUserById(userId);
            _logger.LogInformation($"Delete user {user.Name} {user.LastName} - {user.Email}");

            _applicationData.Data.Users.DeleteUser(userId);
            await _applicationData.Data.SaveChangesAsync();

            _logger.LogInformation($"User {user.Name} {user.LastName} - {user.Email} deleted successfully");
        }
    }
}
