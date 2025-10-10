using HobbyAPI.Controllers;
using HobbyAPI.Data;
using HobbyAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace HobbyAPI.Tests
{
    public class LogsControllerTest
    {
        private LogsController _controller;
        private AppDbContext _context;
        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_CreateLog_Success")
            .Options;

            _context = new AppDbContext(options);
            _controller = new LogsController(_context);
        }

        [Test]
        public async Task CreateLog_returnsBadRequest()
        {
            //arrange
            var dto = new Habit { Id = 2,name = "Gym", clientId = "caio", goalType = GoalType.Bool, goal = 19};
            _context.Habits.Add(dto);
            await _context.SaveChangesAsync();

            var log = new Logs { Id = 2, HabitId = 2, name = "Gym", clientId = "caio", goalType = GoalType.Bool,
                amount = 4, date = DateOnly.FromDateTime(DateTime.Now) };
            _context.HabitsLogs.Add(log);
            await _context.SaveChangesAsync();
            //act
            var res = await _controller.CreateLog(2,"caio");
            //assert
            var BadReq = res as BadRequestObjectResult;
            Assert.IsNotNull(BadReq);
            Assert.AreEqual(400,BadReq.StatusCode);
            Assert.AreEqual("Você não pode fazer dois logs deste mesmo Hábito por dia", BadReq.Value);
        }
        [Test]
        public async Task WeeklyStats_returnsOk()
        {
            //arrange
            var log = new Logs
            {
                Id = 2,
                HabitId = 2,
                name = "Gym",
                clientId = "caio",
                goalType = GoalType.Bool,
                amount = 4,
                date = DateOnly.FromDateTime(DateTime.Now)
            };
            _context.HabitsLogs.Add(log);
            await _context.SaveChangesAsync();
            //act
            var res = await _controller.GetByWeekly("caio");
            //assert
            var Ok = res as OkObjectResult;
            Assert.IsNotNull(Ok);
            Assert.AreEqual(200, Ok.StatusCode);
        }
    }
}
