using HobbyAPI.Data;
using HobbyAPI.Model;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.ComponentModel;

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
            if (res != null)
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
                updatedAt = DateOnly.FromDateTime(DateTime.Now),
                clientId = habit.clientId
            };

            await _context.Habits.AddAsync(TrueHabit);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(CreateHabit), new { id = habit.Id }, habit);
        }

        [HttpPost("logs")]
        public async Task<IActionResult> CreateLog(int id, string clientId)
        {
            var res = await _context.Habits.FirstOrDefaultAsync
                (u => u.Id == id && u.clientId == clientId);
            if(res == null)
            {
                return NotFound("você não possui essa tarefa");
            }
            var verify = await _context.HabitsLogs.FirstOrDefaultAsync(
                u => u.HabitId == res.Id && u.clientId == clientId);

            if (verify != null && verify.goalType == GoalType.Bool && verify.date == DateOnly.FromDateTime(DateTime.Now))
            {
                return BadRequest("Você não pode fazer dois logs deste mesmo Hábito por dia");
            }
            if(verify != null)
            {
                verify.date = DateOnly.FromDateTime(DateTime.Now);
                verify.amount = verify.amount + 1;
                await _context.SaveChangesAsync();
                return Ok("parabens por ter cumprido esta missão");
            }
            var log = new Logs
            {
                HabitId = res.Id,
                name = res.name,
                date = DateOnly.FromDateTime(DateTime.Now),
                goalType = res.goalType,
                amount = 1,
                clientId = clientId
            };
            await _context.HabitsLogs.AddAsync(log);
            await _context.SaveChangesAsync();
            return Ok("parabens por ter cumprido esta missão");
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
            var habit = await _context.Habits.FindAsync(id);
            
            if (habit == null)
            {
                return NotFound("falha ao visualizar hábitos");
            }

            if(habit.clientId != clientId)
            {
                return NotFound("Você não fez essa tarefa");
            }

            NewDTO response = new NewDTO
            {
                Id = habit.Id,
                name = habit.name,
                goalType = habit.goalType == GoalType.Bool ? "bool" : "count", // Convertendo enum para string
                goal = habit.goal == 0 && habit.goalType == GoalType.Bool ? "false"
                      : habit.goal == 1 && habit.goalType == GoalType.Bool ? "true"
                      : habit.goal.ToString(),
            };
            return Ok(response);
        }

        [HttpGet("stats/weekly")]
        public async Task<ActionResult> GetByWeekly(string clientId)
        {
            int QuantiaDe_LogsSemanais = 0;
            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var limite = hoje.AddDays(-7);

            var habitos = await _context.HabitsLogs
                .Where(h => h.date >= limite && h.clientId == clientId)
                .ToListAsync();
            if(!habitos.Any())
            {
                return NotFound("nenhum log realizado nesta semana");
            }

            foreach (var i in habitos)
            {
                QuantiaDe_LogsSemanais++;
            }
            var TrueValue = habitos.Select(u => new DTOLogs
            {
                Id = u.Id,
                HabitId = u.HabitId,
                name = u.name,
                date = u.date,
                goalType = u.goalType == GoalType.Bool ? "Bool" : "Count", 
                amount = u.amount == 0 && u.goalType == GoalType.Bool ? "false"
                       : u.amount == 1 && u.goalType == GoalType.Bool  ? "true"
                       : u.amount.ToString()
            });
            return Ok(TrueValue);
        }

        [HttpPut("habits/{id}")]
        public async Task<IActionResult> PutHabit(int id,string clientId,string name, string type, int goal)
        {

            if (type == "bool" && goal > 1 || goal < 0)
                return BadRequest("Você não pode botar valores imcompativeis em seu hábito");

            var habit = await _context.Habits.FirstOrDefaultAsync(u => u.Id == id && u.clientId == clientId);
            if(habit == null) return NotFound("usuario não encontrado");

            habit.name = name;

            if (type == "bool") habit.goalType = GoalType.Bool;
            else if (type == "count") habit.goalType = GoalType.Count;

            habit.goal = goal;

            habit.updatedAt = DateOnly.FromDateTime(DateTime.Now);
            
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("habits/{id}")]
        public async Task<IActionResult> DeleteHabit(int? id, string clientId)
        {
            if(id == null)
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
