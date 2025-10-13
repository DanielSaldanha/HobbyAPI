using HobbyAPI.Data;
using HobbyAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HobbyAPI.Controllers
{
    public class HabitController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HabitController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("habits")]
        public async Task<IActionResult> CreateHabit([FromBody] DTO habit)
        {
            if (habit == null)
            {
                return BadRequest("Preencha para criar um hábito");
            }
            var res = await _context.Habits.FirstOrDefaultAsync(x => x.name == habit.name);
            if (res != null)
            {
                return BadRequest("esta tarefa ja foi registrada por você");
            }


            // Validação do goalType
            GoalType goalType;

            if (habit.goalType == "bool")
            {
                goalType = GoalType.Bool;
            }
            else if (habit.goalType == "count")
            {
                goalType = GoalType.Count;
            }
            else
            {
                return BadRequest("goalType deve ser 'bool' ou 'count'.");
            }

            Habit TrueHabit = new Habit
            {
                name = habit.name,
                goal = habit.goal,
                goalType = goalType, // Atribuindo o goalType corretamente
                createdAt = DateOnly.FromDateTime(DateTime.Now),
                updatedAt = DateOnly.FromDateTime(DateTime.Now),
                clientId = habit.clientId
            };

            await _context.Habits.AddAsync(TrueHabit);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(CreateHabit), new { id = habit.Id }, habit);
        }

        [HttpGet("habits")]
        public async Task<ActionResult> GetAll(string clientId)
        {
            var response = await _context.Habits
           .Where(h => h.clientId == clientId)
           .ToListAsync();

            if (response == null || !response.Any())
            {
                return NotFound("dados não achados");
            }
            var TrueValue = response.Select(u => new NewDTO
            {
                Id = u.Id,
                name = u.name,
                goalType = u.goalType == GoalType.Bool ? "bool" : "count",
                goal = u.goal == 0 && u.goalType == GoalType.Bool ? "false"
                      : u.goal == 1 && u.goalType == GoalType.Bool ? "true"
                      : u.goal.ToString(),
            }).ToList();

            return Ok(TrueValue);
        }

        [HttpGet("habits/{id}")]
        public async Task<ActionResult> GetById(int id, string clientId)
        {
            var habit = await _context.Habits.FirstOrDefaultAsync
                (u => u.Id == id && u.clientId == clientId);

            if (habit == null)
            {
                return NotFound("falha ao visualizar hábitos");
            }

            if (habit.clientId != clientId)
            {
                return NotFound("Você não fez essa tarefa");
            }

            //NewDTO response = new NewDTO
            //{
            //    Id = habit.Id,
            //    name = habit.name,
            //    goalType = habit.goalType == GoalType.Bool ? "bool" : "count", // Convertendo enum para string
            //    goal = habit.goal == 0 && habit.goalType == GoalType.Bool ? "false"
            //          : habit.goal == 1 && habit.goalType == GoalType.Bool ? "true"
            //          : habit.goal.ToString(),
            //};
            return Ok(habit);
        }

        [HttpPut("habits/{id}")]
        public async Task<IActionResult> PutHabit([FromBody] DTO dto)
        {

            if (dto.goalType == "bool" && dto.goal > 1 || dto.goal < 0)
                return BadRequest("Você não pode botar valores imcompativeis em seu hábito");

            var habit = await _context.Habits.FirstOrDefaultAsync(u => u.Id == dto.Id && u.clientId == dto.clientId);
            if (habit == null) return NotFound("usuario não encontrado");

            habit.name = dto.name;

            if (dto.goalType == "bool") habit.goalType = GoalType.Bool;
            else if (dto.goalType == "count") habit.goalType = GoalType.Count;

            habit.goal = dto.goal;

            habit.updatedAt = DateOnly.FromDateTime(DateTime.Now);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("habits/{id}")]
        public async Task<IActionResult> DeleteHabit(int? id, string clientId)
        {
            if (id == null)
            {
                return BadRequest("Você precisa colocar o Id da tarefa");
            }
            var res = await _context.Habits.FirstOrDefaultAsync(u => u.Id == id && u.clientId == clientId);
            if (res == null)
            {
                return BadRequest("falha ao encontrar habito");
            }
            _context.Habits.Remove(res);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
