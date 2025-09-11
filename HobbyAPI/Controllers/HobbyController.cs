using HobbyAPI.Data;
using HobbyAPI.Model;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace HobbyAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class HobbyController : ControllerBase
    {
        private readonly AppDbContext _context;
        public HobbyController(AppDbContext context)
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
            if(res != null)
            {
                return BadRequest("esta tarefa ja foi registrada por você");
            }


                // Validação do goalType
                GoalType goalType;

            if (habit.goalType == "bool")
            {
                if (habit.goal > 1)
                {
                    return BadRequest("Esse valor é imcompativel com goal");
                }
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
                interactedAt = DateOnly.FromDateTime(DateTime.Now)
            };

            await _context.Habits.AddAsync(TrueHabit);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(CreateHabit), new { id = habit.Id }, habit);
        }

        [HttpPost("logs")]
        public async Task<IActionResult> CreateLog(int id)
        {
            var res = await _context.Habits.FindAsync(id);
            var log = new Logs
            {
                HabitId = res.Id,
                date = DateOnly.FromDateTime(DateTime.Now),
                amount = res.goal
            };
            await _context.HabitsLogs.AddAsync(log);
            await _context.SaveChangesAsync();
            return Ok("parabens por ter cumprido esta missão");
        }

        [HttpGet("habits")]
        public async Task<ActionResult> GetAll()
        {
            var response = await _context.Habits.ToListAsync();
            if (response == null || !response.Any())
            {
                return NotFound("dados não achados");
            }

            var TrueValue = response.Select(u => new NewDTO
            {
                Id = u.Id,
                name = u.name,
                goalType = u.goalType == GoalType.Bool ? "bool" : "count",
                goal = u.goal == 0 ? "false" : u.goal == 1 ? "true" : u.goal.ToString(),
                createdAt = u.createdAt,
                interactedAt = u.interactedAt

            }).ToList();

            return Ok(TrueValue);
        }


        [HttpGet("habits/{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            var habit = await _context.Habits.FindAsync(id);
            if (habit == null)
            {
                return BadRequest("falha ao visualizar hábitos");
            }

            NewDTO response = new NewDTO
            {
                Id = habit.Id,
                name = habit.name,
                goalType = habit.goalType == GoalType.Bool ? "bool" : "count", // Convertendo enum para string
                goal = habit.goal == 0 ? "false" : habit.goal == 1 ? "true" : habit.goal.ToString(),
                createdAt = habit.createdAt,
                interactedAt = habit.interactedAt
            };
            return Ok(response);
        }

        [HttpGet("/stats/weekly")] // adicionar visibilidade de nome, identificar e separar bool e count
        public async Task<ActionResult> GetByWeekly()
        {
            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var limite = hoje.AddDays(-7);

            var habitos = await _context.HabitsLogs
                .Where(h => h.date >= limite)
                .ToListAsync();

            var TrueValue = habitos.Select(u => new DTOLogs
            {
                HabitId = u.Id,
                date = u.date,
                amount = u.amount == 0 ? "false" : u.amount == 1 ? "true" : u.amount.ToString()
            });

            return Ok(TrueValue);
        }

        [HttpPut("habits/{id}")]
        public async Task<IActionResult> PutHabit([FromBody] DTO dto)
        {
            GoalType goalType;

            if (dto.goalType == "bool")
            {
                goalType = GoalType.Bool;
            }
            else if (dto.goalType == "count")
            {
                goalType = GoalType.Count;
            }
            else
            {
                return BadRequest("goalType deve ser 'bool' ou 'count'.");
            }
            var habit = new Habit 
            {
                name = dto.name,
                goalType = goalType,
                goal = dto.goal
            };

            _context.Entry(habit).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("habits/{id}")]
        public async Task<IActionResult> DeleteHabit(int? id)
        {
            if(id == null)
            {
                return BadRequest("Você precisa colocar o Id da tarefa");
            }
            var res = await _context.Habits.FindAsync(id);
            if(res == null)
            {
                return BadRequest("falha ao encontrar habito");
            }
            _context.Habits.Remove(res);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
