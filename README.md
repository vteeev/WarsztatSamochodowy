# 🧑‍🔧 Projekt zaliczeniowy – ASP.NET Core  
## System zarządzania warsztatem samochodowym 2.0

---

## 🧠 Cel projektu
Zaprojektowanie i implementacja aplikacji webowej do obsługi warsztatu samochodowego.  
Aplikacja umożliwia:

- zarządzanie klientami i pojazdami,
- tworzenie zleceń serwisowych z czynnościami i częściami,
- przypisywanie zadań mechanikom,
- komentowanie zleceń,
- generowanie raportów PDF,
- dodawanie zdjęć pojazdów,
- filtrowanie i raportowanie napraw.

### Technologie i dobre praktyki:
- ASP.NET Core 7/8
- EF Core (Code First + migracje)
- ASP.NET Identity (autoryzacja, role)
- Mapperly (mapowanie DTO ↔️ encje)
- Dependency Injection
- Swagger (dokumentacja API)
- Razor Pages + Bootstrap (lub SPA)
- Upload plików
- Generowanie PDF
- xUnit / NUnit (testy jednostkowe)

---

## 🧑‍🤝‍🧑 Zespół
- Liczba osób: 2  
- Praca w repozytorium GitHub / GitLab z historią commitów

---

## ✅ Wymagania funkcjonalne

| Moduł               | Wymagania                                                                 |
|---------------------|---------------------------------------------------------------------------|
| 🔐 Uwierzytelnianie  | Rejestracja, logowanie, role: Admin, Mechanik, Recepcjonista             |
| 👤 Klienci           | CRUD klientów, wyszukiwanie, lista pojazdów klienta                      |
| 🚘 Pojazdy           | CRUD, zdjęcie (upload), VIN, rejestracja                                 |
| 🧾 Zlecenia          | Tworzenie zleceń, statusy, przypisywanie mechaników                      |
| 🔧 Czynności         | Lista czynności: opis + koszt robocizny                                  |
| ⚙️ Części            | Wybór z katalogu, ilość, koszt                                            |
| 💬 Komentarze        | Komentarze wewnętrzne do zleceń, historia przebiegu                      |
| 📦 Katalog części    | CRUD części (dostęp dla Admin / Recepcjonista)                           |
| 📈 Raporty           | Koszt napraw klienta / pojazdu / miesiąca + eksport do PDF               |

---

## ✅ Pozostałe wymagania

### 🧩 1. Indeksy – optymalizacja zapytań
- Dodanie nieklastrowanych indeksów
- Zrzuty planów zapytań przed/po + analiza
- 📎 `raport-indeksy.pdf`

### 📡 2. SQL Profiler – nasłuch endpointu
- Użycie SQL Server Profiler lub EF Core Logging
- Screenshoty, opis działania zapytania
- 📎 `raport-sql-profiler.pdf`

### ⚙️ 3. GitHub Actions – CI/CD
- Konfiguracja workflow:
  - `dotnet build`
  - `dotnet test`
  - opcjonalnie: Docker build + push
- 📎 `README.md` + `dotnet-ci.yml`

### 📝 4. Logowanie błędów – NLog
- Logowanie wyjątków i zdarzeń
- Pliki logów: `logs/errors.log`
- DI: `ILogger<T>`

### 📤 5. BackgroundService – raport e-mail
- Usługa generująca PDF raport otwartych zleceń
- Wysyłka mailem (SMTP)
- 📎 `raport-otwarte-naprawy.pdf`
- 📎 `OpenOrderReportBackgroundService.cs`

### 🚀 6. NBomber – testy wydajności
- Test endpointu z 50 użytkownikami i 100 żądaniami
- 📎 `nbomber-report.pdf`
- 📎 `PerformanceTests/OrdersLoadTest.cs`

---

## 🧱 Modele danych (przykładowe)

```csharp
class Customer { ... }
class Vehicle { string ImageUrl; ... }
class ServiceOrder {
    Status,
    AssignedMechanic,
    List<ServiceTask>,
    List<Comment>
}
class ServiceTask {
    Description,
    LaborCost,
    List<UsedPart>
}
class Part { Name, UnitPrice }
class UsedPart { Part, Quantity }
class Comment { Author, Content, Timestamp }
