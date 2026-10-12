using LibrarySystem.Data;
using LibrarySystem.Models;
using LibrarySystem.Services;
using LibrarySystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers
{
    [Authorize(Roles = "Reception,Manager")]
    public class ItemTransfersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly INotificationService _notifications;

        public ItemTransfersController(
    ApplicationDbContext context,
    UserManager<IdentityUser> userManager,
    INotificationService notifications)
        {
            _context = context;
            _userManager = userManager;
            _notifications = notifications;
        }

        // GET: ItemTransfers
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var staffBranch = await _context.StaffBranches
                .AsNoTracking()
                .FirstOrDefaultAsync(sb => sb.UserId == user.Id);

            if (staffBranch == null)
                return Forbid();

            var transfers = await _context.ItemTransfers
                .Include(t => t.Item)
                .Include(t => t.FromBranch)
                .Include(t => t.ToBranch)
                .Include(t => t.RequestedByUser)
                .Where(t =>
                    t.FromBranchId == staffBranch.BranchId ||
                    t.ToBranchId == staffBranch.BranchId)
                .OrderByDescending(t => t.RequestedDate)
                .Select(t => new ItemTransferIndexViewModel
                {
                    Id = t.Id,

                    FromBranchId = t.FromBranchId,
                    ToBranchId = t.ToBranchId,

                    ItemLibraryCode = t.Item.LibraryCode,
                    ItemName = t.Item.Name,

                    FromBranchName = t.FromBranch.Name,
                    ToBranchName = t.ToBranch.Name,

                    RequestedByUserEmail =
                        t.RequestedByUser.Email ??
                        t.RequestedByUser.UserName ??
                        "",

                    Status = t.Status.ToString(),

                    RequestedDate = t.RequestedDate,
                    CompletedDate = t.CompletedDate,

                    CanApprove =
                        t.Status == TransferStatus.Pending &&
                        t.FromBranchId == staffBranch.BranchId,

                    CanReject =
                        t.Status == TransferStatus.Pending &&
                        t.FromBranchId == staffBranch.BranchId,

                    CanReceive =
                        t.Status == TransferStatus.InTransit &&
                        t.ToBranchId == staffBranch.BranchId
                })
                .ToListAsync();

            var viewModel = new ItemTransfersViewModel
            {
                OutgoingRequests = transfers
                    .Where(t =>
                        t.ToBranchId == staffBranch.BranchId &&
                        (t.Status == TransferStatus.Pending.ToString() ||
                         t.Status == TransferStatus.InTransit.ToString()))
                    .OrderByDescending(t => t.RequestedDate)
                    .ToList(),

                IncomingTransferRequests = transfers
                    .Where(t =>
                        t.FromBranchId == staffBranch.BranchId &&
                        (t.Status == TransferStatus.Pending.ToString() ||
                         t.Status == TransferStatus.InTransit.ToString()))
                    .OrderByDescending(t => t.RequestedDate)
                    .ToList(),

                HistoricalTransfers = transfers
                    .Where(t =>
                        t.Status == TransferStatus.Completed.ToString() ||
                        t.Status == TransferStatus.Rejected.ToString())
                    .OrderByDescending(t => t.RequestedDate)
                    .ToList()
            };

            return View(viewModel);
        }

        // GET: ItemTransfers/Create
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var staffBranch = await _context.StaffBranches
                .Include(sb => sb.Branch)
                .AsNoTracking()
                .FirstOrDefaultAsync(sb => sb.UserId == user.Id);

            if (staffBranch == null)
                return Forbid();

            var viewModel = new ItemTransferCreateViewModel
            {
                CurrentBranchName = staffBranch.Branch.Name,
                Items = await GetTransferableItemsAsync(staffBranch.BranchId)
            };

            return View(viewModel);
        }

        // POST: ItemTransfers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemTransferCreateViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var staffBranch = await _context.StaffBranches
                .Include(sb => sb.Branch)
                .FirstOrDefaultAsync(sb => sb.UserId == user.Id);

            if (staffBranch == null)
                return Forbid();

            var item = await _context.Items
                .Include(i => i.Branch)
                .FirstOrDefaultAsync(i => i.Id == viewModel.ItemId);

            if (item == null)
            {
                ModelState.AddModelError(
                    "ItemId",
                    "The selected item could not be found.");
            }
            else
            {
                if (item.BranchId == staffBranch.BranchId)
                {
                    ModelState.AddModelError(
                        "ItemId",
                        "You can only request items from another branch.");
                }

                if (item.Status != ItemStatus.Available)
                {
                    ModelState.AddModelError(
                        "ItemId",
                        "Only available items can be requested for transfer.");
                }

                if (item.OnOrder)
                {
                    ModelState.AddModelError(
                        "ItemId",
                        "Items currently on order cannot be requested for transfer.");
                }

                var activeTransferExists = await _context.ItemTransfers
                    .AnyAsync(t =>
                        t.ItemId == item.Id &&
                        (t.Status == TransferStatus.Pending ||
                         t.Status == TransferStatus.InTransit));

                if (activeTransferExists)
                {
                    ModelState.AddModelError(
                        "ItemId",
                        "This item already has an active transfer.");
                }
            }

            if (!ModelState.IsValid)
            {
                viewModel.CurrentBranchName = staffBranch.Branch.Name;
                viewModel.Items =
                    await GetTransferableItemsAsync(staffBranch.BranchId);

                return View(viewModel);
            }

            var transfer = new ItemTransfer
            {
                ItemId = item!.Id,
                FromBranchId = item.BranchId,
                ToBranchId = staffBranch.BranchId,
                RequestedByUserId = user.Id,
                Status = TransferStatus.Pending,
                RequestedDate = DateTime.Now
            };

            _context.ItemTransfers.Add(transfer);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Transfer request created for {item.Name} from " +
                $"{item.Branch.Name} to {staffBranch.Branch.Name}.";

            return RedirectToAction(nameof(Index));
        }

        // POST: ItemTransfers/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var staffBranch = await _context.StaffBranches
                .FirstOrDefaultAsync(sb => sb.UserId == user.Id);

            if (staffBranch == null)
                return Forbid();

            var transfer = await _context.ItemTransfers
    .Include(t => t.Item).ThenInclude(i => i.Branch)
    .FirstOrDefaultAsync(t => t.Id == id);

            if (transfer == null)
                return NotFound();

            if (transfer.FromBranchId != staffBranch.BranchId)
                return Forbid();

            if (transfer.Status != TransferStatus.Pending)
            {
                TempData["ErrorMessage"] =
                    "Only pending transfers can be approved.";

                return RedirectToAction(nameof(Index));
            }

            if (transfer.Item.BranchId != transfer.FromBranchId)
            {
                TempData["ErrorMessage"] =
                    "The item is no longer held by the source branch.";

                return RedirectToAction(nameof(Index));
            }

            if (transfer.Item.Status != ItemStatus.Available)
            {
                TempData["ErrorMessage"] =
                    "The item is no longer available for transfer.";

                return RedirectToAction(nameof(Index));
            }

            transfer.Status = TransferStatus.InTransit;
            transfer.Item.Status = ItemStatus.InTransit;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Transfer approved and marked as In Transit.";

            return RedirectToAction(nameof(Index));
        }

        // POST: ItemTransfers/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var staffBranch = await _context.StaffBranches
                .FirstOrDefaultAsync(sb => sb.UserId == user.Id);

            if (staffBranch == null)
                return Forbid();

            var transfer = await _context.ItemTransfers
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transfer == null)
                return NotFound();

            if (transfer.FromBranchId != staffBranch.BranchId)
                return Forbid();

            if (transfer.Status != TransferStatus.Pending)
            {
                TempData["ErrorMessage"] =
                    "Only pending transfers can be rejected.";

                return RedirectToAction(nameof(Index));
            }

            transfer.Status = TransferStatus.Rejected;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Transfer request rejected.";

            return RedirectToAction(nameof(Index));
        }

        // POST: ItemTransfers/Receive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Receive(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var staffBranch = await _context.StaffBranches
                .FirstOrDefaultAsync(sb => sb.UserId == user.Id);

            if (staffBranch == null)
                return Forbid();

            var transfer = await _context.ItemTransfers
                .Include(t => t.Item)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transfer == null)
                return NotFound();

            if (transfer.ToBranchId != staffBranch.BranchId)
                return Forbid();

            if (transfer.Status != TransferStatus.InTransit)
            {
                TempData["ErrorMessage"] =
                    "Only in-transit transfers can be received.";

                return RedirectToAction(nameof(Index));
            }

            if (transfer.Item.BranchId != transfer.FromBranchId)
            {
                TempData["ErrorMessage"] =
                    "The item is no longer recorded at the source branch.";

                return RedirectToAction(nameof(Index));
            }

            transfer.Item.BranchId = transfer.ToBranchId;
            transfer.Item.Branch = await _context.Branches.FirstAsync(b => b.Id == transfer.ToBranchId);
            transfer.Item.Status = ItemStatus.Available;
            transfer.Status = TransferStatus.Completed;
            transfer.CompletedDate = DateTime.Now;

            // Check if there's a waiting reservation for this item at this branch
var pendingReservation = await _context.Reservations
    .Include(r => r.Borrower)
    .Include(r => r.Item).ThenInclude(i => i.Branch)
    .Where(r => r.ItemId == transfer.ItemId
             && r.BranchId == transfer.ToBranchId
             && r.Status == ReservationStatus.Waiting)
    .OrderBy(r => r.QueuePosition)
    .FirstOrDefaultAsync();

if (pendingReservation != null)
{
    // Item has arrived at the borrower's branch, notify them
    pendingReservation.Status = ReservationStatus.Ready;
    pendingReservation.NotifiedAt = DateTime.Now;
    pendingReservation.ExpiresAt = DateTime.Now.AddHours(48);
    transfer.Item.Status = ItemStatus.Reserved;
    transfer.Item.ReservedForBorrowerId = pendingReservation.BorrowerId;

                _notifications.NotifyItemAvailable(pendingReservation.Borrower, transfer.Item);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Item received and transfer completed.";

            return RedirectToAction(nameof(Index));
        }


        private async Task<List<SelectListItem>> GetTransferableItemsAsync(
            int branchId)
        {
            return await _context.Items
                .Include(i => i.Branch)
                .Where(i =>
                    i.BranchId != branchId &&
                    i.Status == ItemStatus.Available &&
                    !i.OnOrder &&
                    !_context.ItemTransfers.Any(t =>
                        t.ItemId == i.Id &&
                        (t.Status == TransferStatus.Pending ||
                         t.Status == TransferStatus.InTransit)))
                .OrderBy(i => i.Branch.Name)
                .ThenBy(i => i.Name)
                .Select(i => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = i.Id.ToString(),
                    Text = $"{i.LibraryCode} - {i.Name} ({i.Branch.Name})"
                })
                .ToListAsync();
        }

    }
}