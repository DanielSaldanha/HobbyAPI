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

        [TestCase("Gym", "caio", "bool", 10)]
        [TestCase("Drink Water", "john", "count", 8)]
        public async Task CreateHabit_ReturnsCreated(
            string name, string clientId, string goalType, int goal)
        {
            var dto = new DTO
            {
                name = name,
                clientId = clientId,
                goalType = goalType,
                goal = goal
            };

            var response = await _controller.CreateHabit(dto);

            var created = response as CreatedAtActionResult;
            Assert.IsNotNull(created);
            Assert.AreEqual(201, created.StatusCode);
        }

        [Test]
        public async Task CreateHabit_NullObject_ReturnsBadRequest()
        {
            DTO habit = null;

            var response = await _controller.CreateHabit(habit);
            var bad = response as BadRequestObjectResult;

            Assert.IsNotNull(bad);
            Assert.AreEqual(400, bad.StatusCode);
            Assert.AreEqual("Preencha para criar um hábito", bad.Value);
        }

        [Test]
        public async Task CreateHabit_DuplicateName_ReturnsBadRequest()
        {
            // Pré-salvar hábito existente
            _context.Habits.Add(new Habit
            {
                name = "Gym",
                goal = 5,
                goalType = GoalType.Bool,
                clientId = "caio",
                createdAt = DateOnly.FromDateTime(DateTime.Now),
                updatedAt = DateOnly.FromDateTime(DateTime.Now)
            });
            await _context.SaveChangesAsync();

            // Tentando recriar
            var dto = new DTO
            {
                name = "Gym",
                clientId = "caio",
                goalType = "bool",
                goal = 10
            };

            var response = await _controller.CreateHabit(dto);

            var bad = response as BadRequestObjectResult;

            Assert.IsNotNull(bad);
            Assert.AreEqual(400, bad.StatusCode);
            Assert.AreEqual("esta tarefa ja foi registrada por você", bad.Value);
        }

        [TestCase("invalidType")]
        [TestCase("xpto")]
        [TestCase("BOOL")]   // maiúsculo não é aceito
        public async Task CreateHabit_InvalidGoalType_ReturnsBadRequest(string invalidType)
        {
            var dto = new DTO
            {
                name = "Study",
                clientId = "caio",
                goalType = invalidType,
                goal = 1
            };

            var response = await _controller.CreateHabit(dto);

            var bad = response as BadRequestObjectResult;

            Assert.IsNotNull(bad);
            Assert.AreEqual(400, bad.StatusCode);
            Assert.AreEqual("goalType deve ser 'bool' ou 'count'.", bad.Value);
        }

        //TESTE DE CENÁRIO
        [Test]
        public async Task Scenario_UserCreatesHabit_ThenAttemptsDuplicate_FailsAsExpected()
        {
            // 1) Usuário cria hábito válido
            var firstHabit = new DTO
            {
                name = "Gym",
                clientId = "caio",
                goalType = "bool",
                goal = 10
            };

            var firstResponse = await _controller.CreateHabit(firstHabit);

            var created1 = firstResponse as CreatedAtActionResult;

            Assert.IsNotNull(created1, "Primeira criação deveria retornar CreatedAtActionResult");
            Assert.AreEqual(201, created1.StatusCode);

            // 2) Usuário tenta cadastrar o mesmo hábito
            var duplicateHabit = new DTO
            {
                name = "Gym",
                clientId = "caio",
                goalType = "bool",
                goal = 10
            };

            var secondResponse = await _controller.CreateHabit(duplicateHabit);
            var badRequest = secondResponse as BadRequestObjectResult;

            Assert.IsNotNull(badRequest, "O hábito duplicado deve gerar BadRequest");
            Assert.AreEqual(400, badRequest.StatusCode);
            Assert.AreEqual("esta tarefa ja foi registrada por você", badRequest.Value);

            // 3) Verificação final do estado do sistema
            var total = _context.Habits.Count();
            Assert.AreEqual(1, total, "Não deveria existir mais que 1 registro");
        }

    }
}
