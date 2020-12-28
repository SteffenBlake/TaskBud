using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskBud.Business.Services.Abstractions;

namespace TaskBud.Website.Controllers
{
    [Authorize]
    [Route("history")]
    public class HistoryController : Controller
    {
        private IHistoryManager HistoryManager { get; }
        public HistoryController(IHistoryManager historyManager)
        {
            HistoryManager = historyManager ?? throw new ArgumentNullException(nameof(historyManager));
        }

        [HttpGet("index")]
        public IActionResult Index(int limit = 40)
        {
            var data = HistoryManager.Index(User, limit);
            return View(data);
        }

        [HttpGet("{historyId}")]
        public IActionResult Read(string historyId)
        {
            var data = HistoryManager.Read(User, historyId);
            return PartialView("_HistoryItem", data);
        }

        [HttpPost("{historyId}/undo")]
        public async Task<IActionResult> Undo(string historyId)
        {
            var data = await HistoryManager.UndoAsync(User, historyId);
            return PartialView("_HistoryItem", data);
        }

        [HttpPost("{historyId}/redo")]
        public async Task<IActionResult> Redo(string historyId)
        {
            var data = await HistoryManager.RedoAsync(User, historyId);
            return PartialView("_HistoryItem", data);
        }
    }
}
