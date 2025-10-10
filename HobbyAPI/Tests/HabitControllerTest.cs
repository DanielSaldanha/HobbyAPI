using HobbyAPI.Controllers;
using HobbyAPI.Data;
using HobbyAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace HobbyAPI.Tests
{
    public class HabitControllerTest
    {
        private HabitController _controller;
        private AppDbContext _context;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_CreateLog_Success")
                .Options;

            _context = new AppDbContext(options);
            _controller = new HabitController(_context);
        }

        [Test]
        public async Task CreateHabit_returnsCreated()
        {
            //arrange
            var dto = new DTO { name = "Gym", clientId = "caio", goalType = "bool", goal = 19 };

            //act
            var res = await _controller.CreateHabit(dto);
            //assert
            var Created = res as CreatedAtActionResult;
            Assert.IsNotNull(Created);
            Assert.AreEqual(201, Created.StatusCode);
        }
    }
}
