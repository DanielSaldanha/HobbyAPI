using HobbyAPI.Controllers;
using HobbyAPI.Data;
using HobbyAPI.Model;
using NUnit.Framework;
using Moq;
using Microsoft.EntityFrameworkCore;
using System.Formats.Asn1;
using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HobbyAPI.Tests
{
    [TestFixture]
    public class TestController
    {
        private HobbyController _controller;
        private Mock<DbSet<Habit>> _mockSet;
        private Mock<AppDbContext> _mockContext;


        [SetUp]
        public void Setup()
        {
            _mockSet = new Mock<DbSet<Habit>>();
            _mockContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            _controller = new HobbyController(_mockContext.Object);
        }

        [Test]
        public async Task CreateHabit_ReturnsCreated_WhenValid()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_CreateHabit")
                .Options;

            using var context = new AppDbContext(options);
            var controller = new HobbyController(context);

            var obj = new DTO
            {
                name = "beber vinho",
                goalType = "bool",
                goal = 0,
                clientId = "carlo"
            };

            // Act
            var result = await controller.CreateHabit(obj);

            // Assert
            var createdResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);

            var returnedHabit = createdResult.Value as DTO;
            Assert.IsNotNull(returnedHabit);
            Assert.AreEqual(obj.name, returnedHabit.name);
            Assert.AreEqual(obj.goalType, returnedHabit.goalType);
            Assert.AreEqual(obj.goal, returnedHabit.goal);

            // Extra: valida se realmente foi persistido em memória
            var habitInDb = await context.Habits.FirstOrDefaultAsync(h => h.name == "beber vinho");
            Assert.IsNotNull(habitInDb);
            Assert.AreEqual(obj.goal, habitInDb.goal);
        }


        [Test]
        public async Task GetByWeekly_ReturnsLast7DaysLogs()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_Weekly")
                .Options;

            using var context = new AppDbContext(options);

            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var limite = hoje.AddDays(-7);

            // Popula dados fake
            context.HabitsLogs.AddRange(
                new Logs { Id = 1, HabitId = 1, name = "Gym", date = hoje, goalType = GoalType.Bool, amount = 1 , clientId = "carlo"},
                new Logs { Id = 2, HabitId = 1, name = "Gym", date = hoje.AddDays(-3), goalType = GoalType.Count, amount = 5 , clientId = "carlo" },
                new Logs { Id = 3, HabitId = 1, name = "Gym", date = hoje.AddDays(-10), goalType = GoalType.Bool, amount = 0, clientId = "carlo" } // fora da janela
            );
            await context.SaveChangesAsync();

            var controller = new HobbyController(context);

            // Act
            var result = await controller.GetByWeekly("carlo");

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var logs = okResult.Value as IEnumerable<DTOLogs>;
            Assert.IsNotNull(logs);

            // só deve conter os últimos 7 dias
            Assert.AreEqual(2, logs.Count());
            Assert.IsTrue(logs.All(l => l.date >= limite));
        }

        [Test]
        public async Task GetByWeekly_ReturnsNUll()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_Weekly")
                .Options;

            using var context = new AppDbContext(options);
            var controller = new HobbyController(context);

            // Act
            var result = await controller.GetByWeekly("carlo");

            // Assert
            var NullResult = result as NotFoundObjectResult;
            //Assert.IsNotNull(NullResult);
            Assert.AreEqual(404, NullResult.StatusCode);
            Assert.AreEqual("nenhum log realizado nesta semana", NullResult.Value);

        }

        [Test]
        public async Task CreateLog_ReturnsOk_WhenLogIsCreated()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_CreateLog_Success")
                .Options;

            using var context = new AppDbContext(options);
            var controller = new HobbyController(context);

            // Um habito existente que ainda não tem um log
            var habit = new Habit
            {
                name = "Beber água",
                goalType = GoalType.Count,
                goal = 1,
                clientId = "carlo"
            };

            context.Habits.Add(habit);// Já existente na tabela habits
            await context.SaveChangesAsync();

            // Act
            var result = await controller.CreateLog(habit.Id, "carlo");

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("parabens por ter cumprido esta missão", okResult.Value);

            // Verifica se realmente salvou no banco
            //Assert.AreEqual(1, context.HabitsLogs.Count());
        }

        [Test]
        public async Task CreateLog_ReturnsBadRequest_WhenLogAlreadyExistsToday()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_CreateLog_Duplicate")
                .Options;

            using var context = new AppDbContext(options);
            var controller = new HobbyController(context);
            // Um habito existente que ainda não tem um log
            var habit = new Habit
            {
                name = "Beber água",
                goalType = GoalType.Bool,
                goal = 1,
                clientId = "carlo"
            };

            context.Habits.Add(habit);// Já existente na tabela habits
            await context.SaveChangesAsync();

            // Adiciona log para hoje
            context.HabitsLogs.Add(new Logs// ja existente na tabel habitslogs
            {
                HabitId = habit.Id,
                name = habit.name,
                date = DateOnly.FromDateTime(DateTime.Now),
                goalType = GoalType.Bool,
                amount = habit.goal,
                clientId = "carlo"
            });
            await context.SaveChangesAsync();

            // Act
            var result = await controller.CreateLog(habit.Id, "carlo");

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Você não pode fazer dois logs deste mesmo Hábito por dia", badRequestResult.Value);

            // Verifica se não adicionou log duplicado
            Assert.AreEqual(1, context.HabitsLogs.Count());
        }

    }
}
