using HobbyAPI.Data;
using HobbyAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HobbyAPI.Controllers
{
    public class LogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LogsController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost("logs")]
        public async Task<IActionResult> CreateLog(int id, string clientId)
        {
            var res = await _context.Habits.FirstOrDefaultAsync
                (u => u.Id == id && u.clientId == clientId);
            if (res == null)
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

            if (verify != null)
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
                return NoContent();
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
            return CreatedAtAction(nameof(CreateLog), new { id = log.Id }, log);
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
                return NoContent();
            }

            if (badgeAL.date < limite)
            {
                badgeAL.consistency = 0;
                await _context.SaveChangesAsync();
                return BadRequest("Você não conseguiu manter sua constancia");
            }

            badgeAL.consistency = badgeAL.consistency + 1;
            badgeAL.date = DateOnly.FromDateTime(DateTime.Now);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(CreateBadge), new { id = badgeAL.Id }, badgeAL);
        }
        [HttpGet("stats/weekly")]
        public async Task<ActionResult> GetByWeekly(string clientId)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Now);
            var limite = hoje.AddDays(-7);

            var habitos = await _context.HabitsLogs
                .Where(h => h.date >= limite && h.clientId == clientId)
                .ToListAsync();
            if (!habitos.Any())
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

            if (badge == "bronze")
            {
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

        [HttpPut("verifyAmount")]
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

            return NoContent();
        }

    }
}
