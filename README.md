# Santander Currency Converter

Pełnostackowa aplikacja do konwersji walut z uwierzytelnianiem użytkowników, kursami walut pobieranymi z polskiego API NBP oraz obsługą płatności przez PayU.  
Użytkownicy mogą się rejestrować i logować, przeglądać oraz odświeżać kursy walut, przeliczać waluty (co uruchamia proces płatności PayU) oraz sprawdzać swoje salda.

---

## Autor projektu

**Andrii Moskalenko**  
Numer indeksu: **53548**

---

## Spis treści

- [Funkcjonalności](#funkcjonalności)
- [Architektura](#architektura)
- [Stos technologiczny](#stos-technologiczny)
- [Wymagania wstępne](#wymagania-wstępne)
- [Uruchamianie z Dockerem](#uruchamianie-z-dockerem)
- [Konfiguracja](#konfiguracja)
- [Przegląd API](#przegląd-api)
- [Korzystanie z aplikacji](#korzystanie-z-aplikacji)
- [Struktura projektu](#struktura-projektu)

---

## Funkcjonalności

- **Uwierzytelnianie** – rejestracja, logowanie, wylogowanie, przypomnienie hasła (JWT).
- **Kursy walut** – podgląd kursów, pobieranie i odświeżanie z API NBP; automatyczne pobranie przy pustej tabeli.
- **Kalkulator walut** – wprowadzenie kwoty i waluty oraz rozpoczęcie płatności PayU w celu wykonania konwersji.
- **Saldo** – podgląd sald użytkownika dla poszczególnych walut.
- **Integracja z PayU** – tworzenie płatności, przekierowanie do PayU, obsługa zakończenia płatności i aktualizacja sald.

---

## Architektura

- **Frontend** – React (TypeScript), aplikacja typu SPA komunikująca się z backendowym API.
- **Backend** – ASP.NET Core 9 Web API, JWT, Identity, Entity Framework Core.
- **Baza danych** – SQL Server (lokalnie lub w Dockerze); migracje uruchamiane przy starcie aplikacji.
- **Usługi zewnętrzne** – API NBP (kursy walut), PayU (obsługa płatności).

---

## Stos technologiczny

| Warstwa | Technologia |
|-------|------------|
| Frontend | React 18, TypeScript, Create React App, Axios |
| Backend | ASP.NET Core 9, Entity Framework Core 9 |
| Baza danych | SQL Server |
| Autoryzacja | ASP.NET Core Identity, JWT Bearer |
| Płatności | PayU (OAuth + REST API) |
| Kursy walut | API Narodowego Banku Polskiego |
| DevOps | Docker, Docker Compose |

---

## Wymagania wstępne

### Opcja zalecana
- Docker
- Docker Compose

### Uruchamianie lokalne
- Node.js 18+ oraz npm
- .NET SDK 9
- SQL Server (lokalnie lub w kontenerze)

---

## Uruchamianie z Dockerem

1. **Sklonuj repozytorium**
   ```bash
   git clone <repo-url>
   cd project
Uruchom aplikację

docker compose up --build
Przy pierwszym uruchomieniu obrazy są budowane (kilka minut).

API automatycznie wykonuje migracje bazy danych.

Dostęp do usług

Frontend: http://localhost:3000

Backend (Swagger): http://localhost:5000/swagger

SQL Server: localhost,1433

użytkownik: sa

hasło: StrongPass!123

Zatrzymanie kontenerów

docker compose down
lub:

docker compose down -v
Konfiguracja
Backend
Pliki konfiguracyjne:

appsettings.json

appsettings.Development.json

appsettings.Docker.json

Najważniejsze ustawienia:

ConnectionStrings:DefaultConnection – połączenie z SQL Server

JWT:Issuer, JWT:Audience, JWT:SigningKey

PayU:PosId, ClientId, ClientSecret, BaseUrl, NotifyUrl, ReturnUrl

project/
├── backend/                 # ASP.NET Core 9 API
│   ├── Controllers/
│   ├── Data/
│   ├── Dtos/
│   ├── Models/
│   ├── Services/
│   └── Migrations/
├── frontend/                # React (TypeScript)
│   ├── src/
│   ├── public/
│   └── package.json
└── docker-compose.yml