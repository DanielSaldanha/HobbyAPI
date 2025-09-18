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

        [HttpPost("create")]
        public IActionResult CreateClient()
        {
            // gera novo GUID
            var clientId = Guid.NewGuid().ToString();

            // retorna para o frontend
            return Ok(new { clientId });
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
                updatedAt = DateOnly.FromDateTime(DateTime.Now)
            };

            await _context.Habits.AddAsync(TrueHabit);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(CreateHabit), new { id = habit.Id }, habit);
        }

        [HttpPost("logs")]
        public async Task<IActionResult> CreateLog(int id)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var limite = hoje.AddDays(-1);

            var res = await _context.Habits.FindAsync(id);
            var verify = await _context.HabitsLogs.FirstOrDefaultAsync(u => u.HabitId == res.Id);

            if(verify.date > limite)
            {
                //Criar uma rota para a limpeza de aglomerados

                //var list = await _context.Habits.ToListAsync();
                //var TrueValue = list.Select(u => new Logs
                //{
                //    amount = 0
                //});

                //por enquanto
                verify.amount = 0;
                await _context.SaveChangesAsync();
            }

            if(verify != null && verify.goalType == GoalType.Bool && verify.date == DateOnly.FromDateTime(DateTime.Now))
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
                amount = 1
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
                goal = u.goal == 0 && u.goalType == GoalType.Bool ? "false"
                      : u.goal == 1 && u.goalType == GoalType.Bool ? "true"
                      : u.goal.ToString(),
                //createdAt = u.createdAt,
                //updatedAt = u.updatedAt

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
                goal = habit.goal == 0 && habit.goalType == GoalType.Bool ? "false"
                      : habit.goal == 1 && habit.goalType == GoalType.Bool ? "true"
                      : habit.goal.ToString(),
                //createdAt = habit.createdAt,
                //updatedAt = habit.updatedAt
            };
            return Ok(response);
        }

        [HttpGet("stats/weekly")] // adicionar visibilidade de nome, identificar e separar bool e count
        public async Task<ActionResult> GetByWeekly()
        {
            int QuantiaDe_LogsSemanais = 0;
            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var limite = hoje.AddDays(-7);

            var habitos = await _context.HabitsLogs
                .Where(h => h.date >= limite)
                .ToListAsync();
            if(!habitos.Any())
            {
                return NotFound("nenhum log realizado nesta semana");
            }
            foreach(var i in habitos)
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
            return Ok(new { TrueValue , QuantiaDe_LogsSemanais });
        }

        [HttpPut("habits/{id}")]
        public async Task<IActionResult> PutHabit(int id,string name, string type, int goal)
        {

            if (type == "bool" && goal > 1 || goal < 0)
                return BadRequest("Você não pode botar valores imcompativeis em seu hábito");

            var habit = await _context.Habits.FindAsync(id);
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
