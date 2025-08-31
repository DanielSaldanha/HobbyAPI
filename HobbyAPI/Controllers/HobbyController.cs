using HobbyAPI.Data;
using HobbyAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> CriaHabito([FromBody] DTO habit)
        {
            if (habit == null)
            {
                return BadRequest("Preencha para criar um hábito");
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
                goalType = goalType // Atribuindo o goalType corretamente
            };

            await _context.Habits.AddAsync(TrueHabit);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(CriaHabito), new { id = TrueHabit.Id }, TrueHabit);
        }

        [HttpGet("habits")]
        public async Task<ActionResult> VerHabitos()
        {
            var habit = await _context.Habits.ToListAsync();
            if(habit == null)
            {
                return BadRequest("falha ao vizualizar hábitos");
            }

            Habit response = new Habit
            {
                Id = habit.Id,
                name = habit.name,
                goalType = habit.goalType == GoalType.Bool ? "bool" : "count", // Convertendo enum para string
                goal = habit.goal
            };

            return Ok(response);
        }

        [HttpPut("habits/{id}")]
        public async Task<IActionResult> MudarHábito([FromBody] Habit habit)
        {
            _context.Entry(habit).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("habits/{id}")]
        public async Task<IActionResult> DeletarHabito(int? id)
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
