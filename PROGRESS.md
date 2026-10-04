# Текущая задача: T-007 — интеграционные тесты миграции и PRAGMA (НФТ-12, НФТ-13)

Проект `MessageHub.Infrastructure.Tests` (xUnit). Паттерн: «миграция один раз + rollback на тест».

## Проблемы существующего черновика (`MessageHubDbContext.cs` в тестах)

- Передаёт EF готовое `:memory:`-соединение → `SqliteConnectionInterceptor` не срабатывает
  (интерцептор `internal`, подключается только в `OnConfiguring` при ненастроенных опциях).
  Тест проверял бы дефолты SQLite, а не наш код.
- `journal_mode=WAL` на `:memory:` невозможен → нужна файловая БД во временной папке
  (не `%LOCALAPPDATA%` — `DatabasePath` для тестов не использовать).

## Решения

- Prod: публичный `UseMessageHubSqlite(this DbContextOptionsBuilder, string connectionString)`
  в Infrastructure = `UseSqlite` + интерцептор; `OnConfiguring` использует его же.
  Тесты идут тем же путём, что и приложение (нет дублирования wiring).
- `MigratedDatabaseFixture` (`IAsyncLifetime`, `IClassFixture`): временный файл БД, миграции один раз,
  одно открытое соединение (открывает EF → интерцептор сработал).
- Транзакция на тест: `IAsyncLifetime` в тест-классе — `BeginTransaction` в `InitializeAsync`,
  `Rollback` в `DisposeAsync`.
- Свежая БД на тест (`FreshDatabase` helper) — только схема/миграции/PRAGMA.

## Чеклист

- [x] Исследование (DbContext, интерцептор, миграция, TASKS.md T-007)
- [x] `UseMessageHubSqlite` в Infrastructure + использование в `OnConfiguring`
- [x] Хелпер `TempDatabase` (файл во temp, очистка вкл. `-wal`/`-shm`, `SqliteConnection.ClearAllPools`)
- [x] `MigratedDatabaseFixture`
- [x] Тесты «свежая БД»: миграция на чистой БД (все 11 таблиц + история), повторный `Migrate` идемпотентен,
      `foreign_keys=1`, `journal_mode=wal`, `synchronous=1`, `busy_timeout=5000`, PRAGMA на каждом новом соединении
- [x] Тесты «fixture + rollback»: FK реально отклоняет сироту; изоляция (два теста вставляют одну и ту же уникальную строку)
- [x] Удалить черновой `MessageHubDbContext.cs` из тестов (заменяется новыми файлами)
- [x] `dotnet build` + `dotnet test`
- [x] Отметить T-007 в `docs/TASKS.md` (после зелёных тестов)
