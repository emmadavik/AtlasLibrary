using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AtlasLibrary.Controllers
{
    public class AdvancedAdminController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AdvancedAdminController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (userRole != "Admin")
            {
                TempData["Error"] = "Du har inte behörighet att komma åt denna sida.";
                return RedirectToAction("Index", "Profile");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("AdminApi");

                var adminResponse = await client.GetAsync("api/admin");

                if (!adminResponse.IsSuccessStatusCode)
                {
                    ViewBag.Error = $"Kunde inte hämta admin-data. Statuskod: {adminResponse.StatusCode}";
                    return View();
                }

                var adminJson = await adminResponse.Content.ReadAsStringAsync();

                using (var adminDocument = JsonDocument.Parse(adminJson))
                {
                    var adminRoot = adminDocument.RootElement;
                    ViewBag.ReportCount = adminRoot.GetProperty("reportCount").GetInt32();
                    ViewBag.ReminderCount = adminRoot.GetProperty("reminderCount").GetInt32();
                }

                var reportsResponse = await client.GetAsync("api/reports");

                if (!reportsResponse.IsSuccessStatusCode)
                {
                    ViewBag.Error = $"Kunde inte hämta rapporter. Statuskod: {reportsResponse.StatusCode}";
                    return View();
                }

                var reportsJson = await reportsResponse.Content.ReadAsStringAsync();

                var reports = JsonSerializer.Deserialize<List<ReportDto>>(reportsJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                ViewBag.Reports = reports ?? new List<ReportDto>();
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
            var token = HttpContext.Session.GetString("JwtToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (userRole != "Admin")
            {
                TempData["Error"] = "Du har inte behörighet att komma åt denna sida.";
                return RedirectToAction("Index", "Profile");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("AdminApi");
                var response = await client.GetAsync("api/reports");

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
        public async Task<IActionResult> Reminders()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (userRole != "Admin")
            {
                TempData["Error"] = "Du har inte behörighet att komma åt denna sida.";
                return RedirectToAction("Index", "Profile");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("AdminApi");
                var response = await client.GetAsync("api/reminders");

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
        public async Task<IActionResult> CreateReport()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (userRole != "Admin")
            {
                TempData["Error"] = "Du har inte behörighet att komma åt denna sida.";
                return RedirectToAction("Index", "Profile");
            }

            var model = new CreateReportViewModel
            {
                CompletedObjects = await GetCompletedObjectsAsync()
            };

            if (!model.CompletedObjects.Any())
            {
                ViewBag.Error = "Inga färdiga objekt hittades från Loans API.";
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport(CreateReportViewModel model)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (userRole != "Admin")
            {
                TempData["Error"] = "Du har inte behörighet att komma åt denna sida.";
                return RedirectToAction("Index", "Profile");
            }

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
                        : null
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/reports", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = $"Kunde inte skapa rapport. Statuskod: {response.StatusCode}. Svar från API: {errorBody}";
                    return View(model);
                }

                TempData["Success"] = "Rapporten skapades.";
                return RedirectToAction("Index");
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
            var token = HttpContext.Session.GetString("JwtToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (userRole != "Admin")
            {
                TempData["Error"] = "Du har inte behörighet att komma åt denna sida.";
                return RedirectToAction("Index", "Profile");
            }

            var model = new CreateReminderViewModel
            {
                CompletedObjects = await GetCompletedObjectsAsync()
            };

            if (!model.CompletedObjects.Any())
            {
                ViewBag.Error = "Inga färdiga objekt hittades från Loans API.";
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReminder(CreateReminderViewModel model)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            if (userRole != "Admin")
            {
                TempData["Error"] = "Du har inte behörighet att komma åt denna sida.";
                return RedirectToAction("Index", "Profile");
            }

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

                var response = await client.PostAsync("api/reminders", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = $"Kunde inte skapa påminnelse. Statuskod: {response.StatusCode}. Svar från API: {errorBody}";
                    return View(model);
                }

                TempData["Success"] = "Påminnelsen skapades.";
                return RedirectToAction("Index");
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
                var loansBaseUrl = _configuration["ApiSettings:LoansApiBaseUrl"];
                if (string.IsNullOrWhiteSpace(loansBaseUrl))
                {
                    return new List<CompletedObjectDto>();
                }

                var client = _httpClientFactory.CreateClient();
                var loansUrl = $"{loansBaseUrl.TrimEnd('/')}/api/Loans/admin-report-items";

                var loansResponse = await client.GetAsync(loansUrl);
                if (!loansResponse.IsSuccessStatusCode)
                {
                    return new List<CompletedObjectDto>();
                }

                var loansJson = await loansResponse.Content.ReadAsStringAsync();
                var loanObjects = JsonSerializer.Deserialize<List<CompletedObjectDto>>(loansJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new List<CompletedObjectDto>();

                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrWhiteSpace(token))
                {
                    return loanObjects;
                }

                var usersClient = _httpClientFactory.CreateClient("UsersApi");
                usersClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                foreach (var item in loanObjects)
                {
                    try
                    {
                        var userResponse = await usersClient.GetAsync($"api/users/{item.UserId}");
                        if (!userResponse.IsSuccessStatusCode)
                        {
                            continue;
                        }

                        var userJson = await userResponse.Content.ReadAsStringAsync();
                        var user = JsonSerializer.Deserialize<UserDto>(userJson,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (user != null)
                        {
                            item.BorrowerName = user.Namn ?? item.BorrowerName;
                            item.BorrowerEmail = user.Epost ?? item.BorrowerEmail;
                        }
                    }
                    catch
                    {
                        // Behåll loan-data om user-anropet misslyckas
                    }
                }

                return loanObjects;
            }
            catch
            {
                return new List<CompletedObjectDto>();
            }
        }

        public class UserDto
        {
            public int Id { get; set; }
            public string? Namn { get; set; }
            public string? Epost { get; set; }
            public string? Roll { get; set; }
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
            public int UserId { get; set; }

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
    }
}