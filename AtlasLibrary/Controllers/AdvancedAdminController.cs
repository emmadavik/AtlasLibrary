using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace AtlasLibrary.Controllers
{
    public class AdvancedAdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdvancedAdminController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private IActionResult? CheckAdminAccess()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Admin")
            {
                TempData["Error"] = "Du har inte behörighet att komma åt denna sida.";
                return RedirectToAction("Index", "Profile");
            }

            return null;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            try
            {
                var client = _httpClientFactory.CreateClient("AdminApi");

                var reportsResponse = await client.GetAsync("api/Reports");
                var remindersResponse = await client.GetAsync("api/Reminders");

                if (!reportsResponse.IsSuccessStatusCode || !remindersResponse.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Kunde inte hämta data från AdminDashbord API.";
                    return View();
                }

                var reportsJson = await reportsResponse.Content.ReadAsStringAsync();
                var remindersJson = await remindersResponse.Content.ReadAsStringAsync();

                var reports = JsonSerializer.Deserialize<List<ReportDto>>(reportsJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<ReportDto>();

                var reminders = JsonSerializer.Deserialize<List<ReminderDto>>(remindersJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<ReminderDto>();

                ViewBag.ReportCount = reports.Count;
                ViewBag.ReminderCount = reminders.Count;
                ViewBag.Reports = reports;
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Fel vid anrop till admin-API: {ex.Message}";
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            try
            {
                var client = _httpClientFactory.CreateClient("AdminApi");
                var response = await client.GetAsync("api/Reports");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = $"Kunde inte hämta rapporter. Statuskod: {response.StatusCode}";
                    return View(new List<ReportDto>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var reports = JsonSerializer.Deserialize<List<ReportDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return View(reports ?? new List<ReportDto>());
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Fel vid hämtning av rapporter: {ex.Message}";
                return View(new List<ReportDto>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditReport(int id)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            try
            {
                var client = _httpClientFactory.CreateClient("AdminApi");
                var response = await client.GetAsync($"api/Reports/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Kunde inte hämta rapporten.";
                    return RedirectToAction(nameof(Reports));
                }

                var json = await response.Content.ReadAsStringAsync();
                var report = JsonSerializer.Deserialize<ReportDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (report == null)
                    return RedirectToAction(nameof(Reports));

                var selectedIds = new List<int>();

                if (!string.IsNullOrWhiteSpace(report.SelectedObjectIds))
                {
                    selectedIds = report.SelectedObjectIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => int.TryParse(x.Trim(), out var idValue) ? idValue : 0)
                        .Where(x => x > 0)
                        .ToList();
                }

                var model = new EditReportViewModel
                {
                    Id = report.Id,
                    Title = report.Title ?? string.Empty,
                    Summary = report.Summary ?? string.Empty,
                    SelectedObjectIds = selectedIds,
                    CompletedObjects = await GetCompletedObjectsAsync()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Fel vid hämtning av rapport: {ex.Message}";
                return RedirectToAction(nameof(Reports));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditReport(EditReportViewModel model)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            model.CompletedObjects = await GetCompletedObjectsAsync();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var client = _httpClientFactory.CreateClient("AdminApi");

                var requestBody = new
                {
                    id = model.Id,
                    title = model.Title,
                    summary = model.Summary,
                    selectedObjectIds = model.SelectedObjectIds != null && model.SelectedObjectIds.Any()
                        ? string.Join(",", model.SelectedObjectIds)
                        : string.Empty
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"api/Reports/{model.Id}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = $"Kunde inte uppdatera rapport. Statuskod: {response.StatusCode}. Svar från API: {errorBody}";
                    return View(model);
                }

                TempData["Success"] = "Rapporten uppdaterades.";
                return RedirectToAction(nameof(Reports));
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Fel vid uppdatering av rapport: {ex.Message}";
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            try
            {
                var client = _httpClientFactory.CreateClient("AdminApi");
                var response = await client.DeleteAsync($"api/Reports/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Kunde inte ta bort rapporten.";
                }
                else
                {
                    TempData["Success"] = "Rapporten togs bort.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Fel vid borttagning av rapport: {ex.Message}";
            }

            return RedirectToAction(nameof(Reports));
        }

        [HttpGet]
        public async Task<IActionResult> Reminders()
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            try
            {
                var client = _httpClientFactory.CreateClient("AdminApi");
                var response = await client.GetAsync("api/Reminders");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = $"Kunde inte hämta påminnelser. Statuskod: {response.StatusCode}";
                    return View(new List<ReminderDto>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var reminders = JsonSerializer.Deserialize<List<ReminderDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return View(reminders ?? new List<ReminderDto>());
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Fel vid hämtning av påminnelser: {ex.Message}";
                return View(new List<ReminderDto>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditReminder(int id)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            try
            {
                var client = _httpClientFactory.CreateClient("AdminApi");
                var response = await client.GetAsync($"api/Reminders/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Kunde inte hämta påminnelsen.";
                    return RedirectToAction(nameof(Reminders));
                }

                var json = await response.Content.ReadAsStringAsync();
                var reminder = JsonSerializer.Deserialize<ReminderDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (reminder == null)
                    return RedirectToAction(nameof(Reminders));

                var model = new EditReminderViewModel
                {
                    Id = reminder.Id,
                    CompletedObjectId = reminder.CompletedObjectId,
                    BorrowerName = reminder.BorrowerName ?? string.Empty,
                    BorrowerEmail = reminder.BorrowerEmail ?? string.Empty,
                    ObjectTitle = reminder.ObjectTitle ?? string.Empty,
                    Message = reminder.Message ?? string.Empty,
                    ReminderDate = reminder.ReminderDate,
                    CompletedObjects = await GetCompletedObjectsAsync()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Fel vid hämtning av påminnelse: {ex.Message}";
                return RedirectToAction(nameof(Reminders));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditReminder(EditReminderViewModel model)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            model.CompletedObjects = await GetCompletedObjectsAsync();

            if (!ModelState.IsValid)
                return View(model);

            var selectedObject = model.CompletedObjects.FirstOrDefault(x => x.Id == model.CompletedObjectId);

            if (selectedObject == null)
            {
                ViewBag.Error = "Det valda objektet kunde inte hittas.";
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("AdminApi");

                var requestBody = new
                {
                    id = model.Id,
                    completedObjectId = selectedObject.Id,
                    borrowerName = selectedObject.BorrowerName,
                    borrowerEmail = selectedObject.BorrowerEmail,
                    objectTitle = selectedObject.Title,
                    message = model.Message,
                    reminderDate = model.ReminderDate
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"api/Reminders/{model.Id}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = $"Kunde inte uppdatera påminnelse. Statuskod: {response.StatusCode}. Svar från API: {errorBody}";
                    return View(model);
                }

                TempData["Success"] = "Påminnelsen uppdaterades.";
                return RedirectToAction(nameof(Reminders));
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Fel vid uppdatering av påminnelse: {ex.Message}";
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReminder(int id)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            try
            {
                var client = _httpClientFactory.CreateClient("AdminApi");
                var response = await client.DeleteAsync($"api/Reminders/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Kunde inte ta bort påminnelsen.";
                }
                else
                {
                    TempData["Success"] = "Påminnelsen togs bort.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Fel vid borttagning av påminnelse: {ex.Message}";
            }

            return RedirectToAction(nameof(Reminders));
        }

        [HttpGet]
        public async Task<IActionResult> CreateReport()
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            var model = new CreateReportViewModel
            {
                CompletedObjects = await GetCompletedObjectsAsync()
            };

            if (!model.CompletedObjects.Any())
            {
                ViewBag.Error = "Inga färdiga objekt hittades från AdminDashbord API.";
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport(CreateReportViewModel model)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            model.CompletedObjects = await GetCompletedObjectsAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("AdminApi");

                var requestBody = new
                {
                    title = model.Title,
                    summary = model.Summary,
                    selectedObjectIds = model.SelectedObjectIds != null && model.SelectedObjectIds.Any()
                        ? string.Join(",", model.SelectedObjectIds)
                        : string.Empty
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/Reports", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = $"Kunde inte skapa rapport. Statuskod: {response.StatusCode}. Svar från API: {errorBody}";
                    return View(model);
                }

                TempData["Success"] = "Rapporten skapades.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Fel vid skapande av rapport: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateReminder()
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            var model = new CreateReminderViewModel
            {
                CompletedObjects = await GetCompletedObjectsAsync()
            };

            if (!model.CompletedObjects.Any())
            {
                ViewBag.Error = "Inga färdiga objekt hittades från AdminDashbord API.";
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReminder(CreateReminderViewModel model)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            model.CompletedObjects = await GetCompletedObjectsAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var selectedObject = model.CompletedObjects.FirstOrDefault(x => x.Id == model.CompletedObjectId);

            if (selectedObject == null)
            {
                ViewBag.Error = "Det valda objektet kunde inte hittas.";
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("AdminApi");

                var requestBody = new
                {
                    completedObjectId = selectedObject.Id,
                    borrowerName = selectedObject.BorrowerName,
                    borrowerEmail = selectedObject.BorrowerEmail,
                    objectTitle = selectedObject.Title,
                    message = model.Message,
                    reminderDate = model.ReminderDate
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/Reminders", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = $"Kunde inte skapa påminnelse. Statuskod: {response.StatusCode}. Svar från API: {errorBody}";
                    return View(model);
                }

                TempData["Success"] = "Påminnelsen skapades.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Fel vid skapande av påminnelse: {ex.Message}";
                return View(model);
            }
        }

        private async Task<List<CompletedObjectDto>> GetCompletedObjectsAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AdminApi");
                var response = await client.GetAsync("api/Reports/completed-objects");

                if (!response.IsSuccessStatusCode)
                {
                    return new List<CompletedObjectDto>();
                }

                var json = await response.Content.ReadAsStringAsync();

                var items = JsonSerializer.Deserialize<List<CompletedObjectDto>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return items ?? new List<CompletedObjectDto>();
            }
            catch
            {
                return new List<CompletedObjectDto>();
            }
        }

        public class ReportDto
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public string? Summary { get; set; }
            public DateTime CreatedAt { get; set; }
            public string? SelectedObjectIds { get; set; }
        }

        public class ReminderDto
        {
            public int Id { get; set; }
            public int CompletedObjectId { get; set; }
            public string? BorrowerName { get; set; }
            public string? BorrowerEmail { get; set; }
            public string? ObjectTitle { get; set; }
            public string? Message { get; set; }
            public DateTime ReminderDate { get; set; }
        }

        public class CompletedObjectDto
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string ObjectType { get; set; } = string.Empty;
            public string BorrowerName { get; set; } = string.Empty;
            public string BorrowerEmail { get; set; } = string.Empty;
            public DateTime BorrowedDate { get; set; }
            public DateTime? ReturnedDate { get; set; }
            public string Status { get; set; } = string.Empty;
            public int Quantity { get; set; }
        }

        public class CreateReportViewModel
        {
            public string Title { get; set; } = string.Empty;
            public List<int> SelectedObjectIds { get; set; } = new();
            public string Summary { get; set; } = string.Empty;
            public List<CompletedObjectDto> CompletedObjects { get; set; } = new();
        }

        public class CreateReminderViewModel
        {
            public int CompletedObjectId { get; set; }
            public string Message { get; set; } = string.Empty;
            public DateTime ReminderDate { get; set; } = DateTime.Today;
            public List<CompletedObjectDto> CompletedObjects { get; set; } = new();
        }

        public class EditReportViewModel
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Summary { get; set; } = string.Empty;
            public List<int> SelectedObjectIds { get; set; } = new();
            public List<CompletedObjectDto> CompletedObjects { get; set; } = new();
        }

        public class EditReminderViewModel
        {
            public int Id { get; set; }
            public int CompletedObjectId { get; set; }
            public string BorrowerName { get; set; } = string.Empty;
            public string BorrowerEmail { get; set; } = string.Empty;
            public string ObjectTitle { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public DateTime ReminderDate { get; set; }
            public List<CompletedObjectDto> CompletedObjects { get; set; } = new();
        }
    }
}