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

        private void SetupMockData(List<Habit> data)
        {
            var queryableData = data.AsQueryable();
            _mockSet.As<IQueryable<Habit>>().Setup(m => m.Provider).Returns(queryableData.Provider);
            _mockSet.As<IQueryable<Habit>>().Setup(m => m.Expression).Returns(queryableData.Expression);
            _mockSet.As<IQueryable<Habit>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
            _mockSet.As<IQueryable<Habit>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator());

            _mockContext.Setup(c => c.Habits).Returns(_mockSet.Object);
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TesteDb")
            .Options;

            _mockContext = new Mock<AppDbContext>(options);

        }

        [Test]
        public async Task Create_ReturnsOk()
        {
            // Arrange
            var obj = new DTO
            {
                name = "beber vinho",
                goalType = "bool",
                goal = 0
            };

            // Configurando o mock para simular a adição de um item
            _mockSet.Setup(m => m.AddAsync(It.IsAny<Habit>(), default)).ReturnsAsync((Habit item, CancellationToken token) =>
            {
                // Aqui você deve simular que o ID é atribuído
                item.Id = (int)(new Random().Next(2, 100)); // Atribuindo um ID aleatório. Pode ser alterado conforme a lógica desejada.
                return new EntityEntry<Habit>(null); // Retornar um EntityEntry simulando um banco. item
            });

            // Simula SaveChangesAsync
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

            _mockContext.Setup(c => c.Habits).Returns(_mockSet.Object);

            // Act
            var result = await _controller.CreateHabit(obj);

            // Assert
            var createdResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);

            var returnedHabit = createdResult.Value as DTO;
            Assert.IsNotNull(returnedHabit);
            Assert.AreEqual(obj.name, returnedHabit.name);
            Assert.AreEqual(obj.goalType, returnedHabit.goalType);
            Assert.AreEqual(obj.goal, returnedHabit.goal);
        }
    }
}
