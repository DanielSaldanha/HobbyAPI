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
                if (habit.goal != 1 || habit.goal != 0)
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

            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var limite = hoje.AddDays(-1);

            if (verify != null && verify.goalType == GoalType.Bool
                && verify.date == DateOnly.FromDateTime(DateTime.Now))
            {
                return BadRequest("Você não pode fazer dois logs deste mesmo Hábito por dia");
            }

            if(verify != null)
            {
                verify.date = DateOnly.FromDateTime(DateTime.Now);
                verify.amount = verify.amount + 1;

                //configurando material de medalha
                var badge = new Badge
                {
                    habitid = res.Id,
                    clientId = res.clientId,
                    name = res.name,
                    starter = 1,
                    consistency = 1,
                    date = DateOnly.FromDateTime(DateTime.Now)
                };
                //salvar verificação de medalhas
                await _context.Badges.AddAsync(badge);
                //salve todas as mudanças
                await _context.SaveChangesAsync();
                return Ok("parabens por ter cumprido esta missão");
            }
            //configurando material do log
            var log = new Logs
            {
                HabitId = res.Id,
                name = res.name,
                date = DateOnly.FromDateTime(DateTime.Now),
                goalType = res.goalType,
                amount = 1,
                clientId = clientId
            };
            //configurando material de medalha
            var Badge = new Badge
            {
                habitid = res.Id,
                clientId = res.clientId,
                name = res.name,
                starter = 1,
                consistency = 1,
                date = DateOnly.FromDateTime(DateTime.Now)
            };
            //salvar verificação de medalhas
            await _context.Badges.AddAsync(Badge);

            // salvar log
            await _context.HabitsLogs.AddAsync(log);
            //salvar
            await _context.SaveChangesAsync();
            return Ok("parabens por ter cumprido esta missão");
        }

        [HttpPost("ClaimBadges")]
        public async Task<IActionResult> CreateBadge(int id, string clientId)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var limite = hoje.AddDays(-1);
            var semana = hoje.AddDays(-7);

            // badge already loged
            var badgeAL = await _context.Badges.FirstOrDefaultAsync(h => h.habitid == id && h.clientId == clientId);
            if (badgeAL == null)
            {
                return NotFound("Você não possui tal tarefa");
            }

            if (badgeAL.date > limite)
            {
                return BadRequest("Você já garantiu sua constancia por hoje");
            }

            var habitos = await _context.HabitsLogs
            .Where(h => h.date >= semana && h.clientId == clientId).ToListAsync();
            int index = 0;
            foreach (var i in habitos) index++;

            if (badgeAL.consistency >= 3 && index >= 10)
            {
                badgeAL.consistency = badgeAL.consistency + 1;
                badgeAL.date = DateOnly.FromDateTime(DateTime.Now);
                badgeAL.badge = Badg3.Bronze;
                await _context.SaveChangesAsync();
                return Ok("parabens você ganhou uma medalha");
            }

            if(badgeAL.date < limite)
            {
                badgeAL.consistency = 0;
                await _context.SaveChangesAsync();
                return BadRequest("Você não conseguiu manter sua constancia");
            }

            badgeAL.consistency = badgeAL.consistency + 1;
            badgeAL.date = DateOnly.FromDateTime(DateTime.Now);
            await _context.SaveChangesAsync();
            return Ok("efetuado com sucesso");
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
            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var limite = hoje.AddDays(-7);

            var habitos = await _context.HabitsLogs
                .Where(h => h.date >= limite && h.clientId == clientId)
                .ToListAsync();
            if(!habitos.Any())
            {
                return NotFound("nenhum log realizado nesta semana");
            }

            var TrueValue = habitos.Select(u => new DTOLogs
            {
                Id = u.Id,
                HabitId = u.HabitId,
                name = u.name,
                date = u.date,
                goalType = u.goalType == GoalType.Bool ? "Bool" : "Count", 
                amount = u.amount.ToString()
            });
            return Ok(TrueValue);
        }

        [HttpGet("badges")]
        public async Task<ActionResult> GetByBadge(string badge, string clientId)
        {

            if (badge == "bronze"){
                var res = await _context.Badges.Where(h => h.badge == Badg3.Bronze
                && h.clientId == clientId).ToListAsync();
                return Ok(res); 
            }

            if (badge == "prata")
            {
                var res = await _context.Badges.Where(h => h.badge == Badg3.Silver
                && h.clientId == clientId).ToListAsync();
                return Ok(res);
            }
            
            if (badge == "ouro")
            {
                var res = await _context.Badges.Where(h => h.badge == Badg3.Gold 
                && h.clientId == clientId).ToListAsync();
                return Ok(res);
            }

            return NotFound("você não possui esse tipo de medalhas");
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

        [HttpPut("VerifyAmount")]
        public async Task<IActionResult> VerifyAmountsFromLogs(string clientId)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var limite = hoje.AddDays(-1);

            var habitos = await _context.HabitsLogs
                .Where(h => h.clientId == clientId)
                .ToListAsync();

            if (!habitos.Any())
            {
                return NotFound("nenhum log realizado nesta semana");
            }

            foreach (var i in habitos)
            {
                if (i.date <= limite)
                {
                    i.amount = 0;
                    i.date = DateOnly.FromDateTime(DateTime.Now);
                }
            }

            await _context.SaveChangesAsync();

            return Ok("efetuado com seucesso");
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
