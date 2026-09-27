# Текущая задача: исправить расхождения EF-маппинга (T-006) со schema.dbml

Найдено при ревью сгенерированного `InitialCreate.sql` относительно `docs/schema.dbml`:

1. Таблица/колонки `Settings` остаются в PascalCase — `Settings` регистрируется в модели
   только внутри `modelBuilder.Entity<Settings>(...)`, который идёт **после** цикла
   snake_case-переименования в `MessageHubDbContext.OnModelCreating` — цикл её не видит.
2. По той же причине колонка `connected_accounts.ChannelType` не переименована — она
   попадает в модель только через `b.Property(e => e.ChannelType).HasConversion(...)`,
   тоже расположенный после цикла.
3. Таблица `persons` вместо `person` — имя берётся из `DbSet<Person> Persons`, а не из
   класса `Person`; нужен явный `.ToTable("person")`.
4. Все временные поля — `TEXT` вместо `INTEGER` Unix ms UTC, как требует шапка
   `schema.dbml` — нигде не настроен конвертер `DateTime → long`.

Решения с пользователем:
- `user_profiles.username` (код) — DBML/ТЗ поправить под `username` (не наоборот).
- Литералы enum'ов в DBML (`settings_scope`, `history_depth`) — поправить под текущие
  C#-имена (`Global`/`Account`, `None`/`SevenDays`/`ThirtyDays`/`NinetyDays`/`All`),
  конвертеры под кастомные строки не вводим.
- CHECK-ограничения — оставляем решение T-006 в силе (не генерируем в SQLite,
  инварианты проверяются в доменных фабриках Core).

## Чеклист шагов

- [x] `MessageHubDbContext.cs`: перенести snake_case-цикл в конец `OnModelCreating`
      (после всех `modelBuilder.Entity<T>()`), чтобы он видел полностью собранную модель
- [x] В том же (перенесённом) цикле добавить конвертер `DateTime`/`DateTime?` → `long`
      (Unix ms UTC) для всех свойств с этим CLR-типом
- [x] `modelBuilder.Entity<Person>()`: явный `e.ToTable("person")`
- [x] Добавлен `DbSet<Settings> Settings` в `MessageHubDbContext` (для консистентности
      с остальными агрегатами)
- [x] `docs/schema.dbml`: `user_profiles.name` → `username`; `settings_scope` →
      `Global`/`Account`; `history_depth` → `None`/`SevenDays`/`ThirtyDays`/`NinetyDays`/`All`;
      заодно поправлены `merge_confidence` (`HIGH`/`MEDIUM` → `High`/`Medium`) и
      `sync_kind` (`updates`/`history` → `Updates`/`History`) — та же причина расхождения
      (C#-имя enum-члена ≠ буквальный литерал из DBML), найдено при сверке остальных enum'ов
- [x] `docs/TZ_Local_Messenger_Hub.md` §8.3 — проверено: поколоночного списка `user_profiles`
      там нет (только сводная таблица назначений таблиц), править нечего
- [x] Перегенерирована миграция (`Migrations/20260927134931_InitialCreate.*`,
      `MessageHubDbContextModelSnapshot.cs`, `InitialCreate.sql`)
- [x] `dotnet build` + `dotnet test` — 79/79 зелёные
- [x] Сверился с новым SQL-скриптом вручную: `person` (не `persons`), `settings`/`channel_type`
      в snake_case, все временные поля `INTEGER`, `username` есть, enum-литералы в фильтрах
      индексов совпадают с C#-именами
- [x] `docs/TASKS.md` T-006 отмечен выполненным

## Статус: готово

Не закоммичено — ждёт явной просьбы пользователя.
