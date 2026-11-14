using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDeskWeb.Data;
using HelpDeskWeb.Models;

namespace HelpDeskWeb.Controllers
{
    public class TicketsController : Controller
    {
        private const int PageSize = 5;
        private static readonly string[] AllowedStatuses =
            { "New", "In Progress", "Resolved", "Closed" };

        private readonly AppDbContext _context;

        public TicketsController(AppDbContext context)
        {
            _context = context;
        }

        // Bütün müraciətlərin siyahısı (səhifələmə ilə)
        public async Task<IActionResult> Index(int page = 1)
        {
            var all = await _context.Tickets.ToListAsync();

            var totalCount = all.Count;
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
            page = Math.Clamp(page, 1, totalPages);

            var items = all
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;

            return View(items);
        }

        // Statusun siyahıdan sürətli dəyişdirilməsi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(
            int id, string status, int page = 1)
        {
            if (!AllowedStatuses.Contains(status))
                return BadRequest();

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
                return NotFound();

            ticket.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { page });
        }

        // Statistika səhifəsi
        public async Task<IActionResult> Stats()
        {
            var all = await _context.Tickets.ToListAsync();

            ViewBag.Total = all.Count;
            ViewBag.ByCategory = all
                .GroupBy(x => x.Category)
                .ToDictionary(g => g.Key, g => g.Count());
            ViewBag.ByPriority = all
                .GroupBy(x => x.Priority)
                .ToDictionary(g => g.Key, g => g.Count());
            ViewBag.ByStatus = all
                .GroupBy(x => x.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            return View();
        }

        // CSV-yə eksport
        public async Task<IActionResult> ExportCsv()
        {
            var all = (await _context.Tickets.ToListAsync())
                .OrderByDescending(x => x.CreatedAt);

            var sb = new StringBuilder();
            sb.AppendLine("Id,Ad,Email,Basliq,Kateqoriya,Prioritet,Status,Tarix");

            foreach (var t in all)
            {
                sb.AppendLine(string.Join(",",
                    t.Id,
                    Csv(t.ReporterName),
                    Csv(t.ReporterEmail),
                    Csv(t.Title),
                    Csv(t.Category),
                    Csv(t.Priority),
                    Csv(t.Status),
                    Csv(t.CreatedAt.ToString("yyyy-MM-dd HH:mm"))));
            }

            var bytes = Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(sb.ToString()))
                .ToArray();

            return File(bytes, "text/csv", "muracietler.csv");
        }

        private static string Csv(string value)
        {
            value ??= "";
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        // Yeni müraciət səhifəsi
        public IActionResult Create()
        {
            return View();
        }

        // Yeni müraciətin yadda saxlanılması
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ticket ticket)
        {
            if (!ModelState.IsValid)
            {
                return View(ticket);
            }

            ticket.CreatedAt = DateTime.Now;
            ticket.Status = "New";

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Müraciətin detalları
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(x => x.Id == id);

            if (ticket == null)
                return NotFound();

            return View(ticket);
        }

        // Redaktə səhifəsi
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var ticket = await _context.Tickets
                .FindAsync(id);

            if (ticket == null)
                return NotFound();

            return View(ticket);
        }

        // Redaktənin yadda saxlanılması
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, Ticket ticket)
        {
            if (id != ticket.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(ticket);

            var existing = await _context.Tickets.FindAsync(id);

            if (existing == null)
                return NotFound();

            existing.ReporterName = ticket.ReporterName;
            existing.ReporterEmail = ticket.ReporterEmail;
            existing.Title = ticket.Title;
            existing.Description = ticket.Description;
            existing.Category = ticket.Category;
            existing.Priority = ticket.Priority;
            existing.Status = ticket.Status;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Silmə səhifəsi
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(x => x.Id == id);

            if (ticket == null)
                return NotFound();

            return View(ticket);
        }

        // Silmə əməliyyatı
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);

            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
