﻿using Bogus;
using FluentAssertions;
using MatchDayApp.Domain.Common.Helpers;
using MatchDayApp.Domain.Entities;
using MatchDayApp.Domain.Entities.Enum;
using MatchDayApp.Domain.Repository;
using MatchDayApp.Domain.Specification.UserSpec;
using MatchDayApp.Infra.Data.Data;
using MatchDayApp.Infra.Data.Repositories;
using MatchDayApp.UnitTest.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.Ordering;

namespace MatchDayApp.UnitTest.Persistence
{
    [Trait("Repositories", "UserRepository")]
    public class UserRepositoryTest
    {
        private readonly MatchDayAppContext _memoryDb;
        private readonly IUserRepository _userRepository;

        private readonly Faker<User> _fakeUser;

        public UserRepositoryTest()
        {
            var configServices = ServicesConfiguration.Configure();

            _memoryDb = configServices
                .GetRequiredService<MatchDayAppContext>()
                .SeedFakeUserData();

            _userRepository = new UserRepository(_memoryDb);

            string salt = SecurePasswordHasher.CreateSalt(8);
            _fakeUser = new Faker<User>()
                .RuleFor(u => u.FirstName, f => f.Person.FirstName)
                .RuleFor(u => u.LastName, f => f.Person.LastName)
                .RuleFor(u => u.Email, f => f.Person.Email)
                .RuleFor(u => u.Username, f => f.UniqueIndex + f.Person.UserName)
                .RuleFor(u => u.Password, f => SecurePasswordHasher.GenerateHash(f.Internet.Password(), salt))
                .RuleFor(u => u.Salt, salt)
                .RuleFor(u => u.UserType, UserType.Player);
        }

        [Fact, Order(1)]
        public void ShouldCanConnectInMemoryDatabase()
        {
            _memoryDb.Database.IsInMemory().Should().BeTrue();
            _memoryDb.Database.CanConnect().Should().BeTrue();
        }

        [Fact, Order(2)]
        public async Task ListAllAsync_User_AllUsersRegistered()
        {
            var users = await _userRepository
                .ListAllAsync();

            users.Should()
                .NotBeNull()
                .And.HaveCount(3)
                .And.SatisfyRespectively(
                    user1 =>
                    {
                        user1.Username.Should().Be("test1");
                        user1.Email.Should().Be("test1@email.com");
                        user1.UserType.Should().Be(UserType.SoccerCourtOwner);
                    },
                    user2 =>
                    {
                        user2.Username.Should().Be("test2");
                        user2.Email.Should().Be("test2@email.com");
                        user2.UserType.Should().Be(UserType.TeamOwner);
                    },
                    user3 =>
                    {
                        user3.Username.Should().Be("test3");
                        user3.Email.Should().Be("test3@email.com");
                        user3.UserType.Should().Be(UserType.SoccerCourtOwner);
                    });
        }

        [Fact, Order(3)]
        public async Task GetByIdAsync_User_OneUserWithSameId()
        {
            var userTestId = _memoryDb.Users.First().Id;
            var expectedUser = new
            {
                Id = userTestId,
                FirstName = "Test",
                LastName = "One",
                Username = "test1",
                Email = "test1@email.com",
                UserType = UserType.SoccerCourtOwner
            };

            var user = await _userRepository
                .GetByIdAsync(userTestId);

            user.Should().BeEquivalentTo(expectedUser, options =>
                options.ExcludingMissingMembers());
        }

        [Fact, Order(4)]
        public async Task GetByIdAsync_User_NullWithUserIdNotRegistered()
        {
            var invalidId = Guid.NewGuid();

            var user = await _userRepository
                .GetByIdAsync(invalidId);

            user.Should().BeNull();
        }

        [Fact, Order(5)]
        public async Task GetAsync_User_GetUserWithMatchTheUserTypeSpecification()
        {
            var expectedUser = new
            {
                FirstName = "Test",
                LastName = "Two",
                Username = "test2",
                Email = "test2@email.com",
                UserType = UserType.TeamOwner
            };

            var spec = new UserContainEmailOrUsernameSpecification("test2@email.com");
            var user = (await _userRepository.GetAsync(spec)).FirstOrDefault();

            user.Should().BeEquivalentTo(expectedUser, options =>
                options.ExcludingMissingMembers());
        }

        [Fact, Order(6)]
        public async Task GetAsync_User_GetUserWithMatchThePredicate()
        {
            var expectedUser = new
            {
                FirstName = "Test",
                LastName = "Two",
                Username = "test2",
                Email = "test2@email.com",
                UserType = UserType.TeamOwner
            };

            var user = (await _userRepository.GetAsync(u => u.Username == "test2")).FirstOrDefault();

            user.Should().BeEquivalentTo(expectedUser, options =>
                options.ExcludingMissingMembers());
        }

        [Fact, Order(7)]
        public async Task GetAsync_User_GetUsersWithMatchThePredicateAndOrdernedByDescendingByUsername()
        {
            var users = await _userRepository
                .GetAsync(null, x => x.OrderByDescending(u => u.Username), "", true);

            users.Should()
                .HaveCount(3)
                .And.SatisfyRespectively(
                user1 =>
                {
                    user1.Username.Should().Be("test3");
                    user1.Email.Should().Be("test3@email.com");
                },
                user2 =>
                {
                    user2.Username.Should().Be("test2");
                    user2.Email.Should().Be("test2@email.com");
                },
                user3 =>
                {
                    user3.Username.Should().Be("test1");
                    user3.Email.Should().Be("test1@email.com");
                });
        }

        [Fact, Order(8)]
        public async Task CountAsync_User_CountTotalUsersMatchedTheSpecification()
        {
            const int expectedTotalCount = 2;

            var spec = new UserContainUserTypeSpecification(UserType.SoccerCourtOwner);
            var usersCount = await _userRepository.CountAsync(spec);

            usersCount.Should().Be(expectedTotalCount);
        }

        [Fact, Order(9)]
        public async Task AddRangeAsync_User_AddedListUsers()
        {
            var result = await _userRepository
                .AddRangeAsync(_fakeUser.Generate(5));

            result.Should().NotBeNull()
                .And.HaveCount(5);
            _memoryDb.Users.Should().HaveCount(8);
        }

        [Fact, Order(10)]
        public async Task SaveAsync_User_UpdatedUser()
        {
            var userToUpdate = _memoryDb.Users.Last();

            userToUpdate.Email = "test.updated@email.com";
            userToUpdate.LastName = "Updated";

            var result = await _userRepository
                .SaveAsync(userToUpdate);

            result.Should().NotBeNull()
                .And.BeOfType<User>();
            result.LastName.Should().Be("Updated");
            result.Email.Should().Be("test.updated@email.com");
        }

        [Fact, Order(11)]
        public async Task DeleteAsync_User_UpdatedUser()
        {
            var userToDelete = _memoryDb.Users.Last();

            await _userRepository
                .DeleteAsync(userToDelete);

            var deletedUser = await _userRepository
                .GetByIdAsync(userToDelete.Id);

            deletedUser.Should().BeNull();
            _memoryDb.Users.Should().HaveCount(2);
        }
    }
}
