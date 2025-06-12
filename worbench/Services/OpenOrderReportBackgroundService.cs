using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using worbench.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Net.Mail;
using System.Net;

namespace worbench.Services
{
    public class OpenOrderReportBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public OpenOrderReportBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<WorkshopDbContext>();
                    var orders = await context.ServiceOrders
                        .Include(o => o.Vehicle)
                        .Where(o => o.Status != "Gotowe")
                        .ToListAsync();

                    var pdfPath = Path.Combine("wwwroot", "open_orders.pdf");
                    GeneratePdfReport(orders, pdfPath);
                    
                    Console.WriteLine("Generowanie i wysyłka raportu...");

                    await SendEmailWithAttachmentAsync(
                        "damian.skiba.9413.2@gmail.com", // <- adres admina
                        "Raport otwartych zleceń",
                        "W załączniku raport PDF z otwartymi zleceniami.",
                        pdfPath
                    );
                }

                await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken); // zmień na 1 dzień w produkcji
            }
        }

        private void GeneratePdfReport(System.Collections.Generic.List<Models.ServiceOrder> orders, string path)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header().Text("Raport otwartych zleceń").FontSize(20).Bold();
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("ID").Bold();
                            header.Cell().Text("Pojazd").Bold();
                            header.Cell().Text("Status").Bold();
                        });

                        foreach (var o in orders)
                        {
                            table.Cell().Text(o.Id.ToString());
                            table.Cell().Text(o.Vehicle?.RegistrationNumber ?? "-");
                            table.Cell().Text(o.Status);
                        }
                    });
                    page.Footer().AlignCenter().Text($"Wygenerowano: {DateTime.Now}");
                });
            });
            doc.GeneratePdf(path);
        }

        private async Task SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath)
        {
            var message = new MailMessage("important@interia.com", to, subject, body);
            message.Attachments.Add(new Attachment(attachmentPath));

            using (var client = new SmtpClient("poczta.interia.pl")
            {
                Port = 587,
                Credentials = new NetworkCredential("important@interia.com", "canon1"),
                EnableSsl = true
            })
            {
                await client.SendMailAsync(message);
                Console.WriteLine("Wysłano e-mail do: " + to);
            }
        }
    }
}