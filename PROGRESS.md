# Progress

## Текущая задача

Реструктурировать папки `MessageHub.Core`: для сущностей, у которых есть свои enum'ы, создать папку `Entities/<Entity>/` с файлом сущности и подпапкой `Enums/`; namespace синхронизируется с новой структурой папок (согласовано с пользователем: только сущности с enum получают папку, остальные остаются плоско в `Entities/`; namespace меняется под структуру, не остаётся `Core.Entities`/`Core.Enums`).

Целевая структура:
- `Entities/Message/Message.cs` (ns `MessageHub.Core.Entities.Message`) + `Entities/Message/Enums/{MessageDirection,MessageStatus}.cs` (ns `...Message.Enums`)
- `Entities/ConnectedAccount/ConnectedAccount.cs` (ns `...Entities.ConnectedAccount`) + `Enums/{AccountStatus,ChannelType,HistoryDepth}.cs`
- `Entities/Conversation/Conversation.cs` (ns `...Entities.Conversation`) + `Enums/{ConversationStatus,ConversationType}.cs`
- `Entities/MergeProposal/MergeProposal.cs` (ns `...Entities.MergeProposal`) + `Enums/{MergeConfidence,ProposalStatus}.cs`
- `Entities/RelationChangeLog/RelationChangeLog.cs` (ns `...Entities.RelationChangeLog`) + `Enums/RelationOperation.cs`
- `Entities/SyncState/SyncState.cs` (ns `...Entities.SyncState`) + `Enums/{SyncKind,SyncStatus}.cs`
- Без изменений (остаются плоско, ns `MessageHub.Core.Entities`, нет своих enum): `Person.cs`, `ChannelContact.cs`, `ConversationParticipant.cs`, `UserProfile.cs`

Кросс-ссылки между новыми namespace'ами (добавить `using`):
- `ChannelContact.cs` → `using MessageHub.Core.Entities.ConnectedAccount;` (тип `ConnectedAccount`)
- `ConnectedAccount.cs` → `using MessageHub.Core.Entities;` (тип `ChannelContact`)
- `Conversation.cs` → `using MessageHub.Core.Entities;` (тип `ConversationParticipant`)
- Тесты (`MessageHub.Core.Tests/Entities/*.cs`) — обновить `using` на новые namespace'ы; `ConversationParticipantTests.cs` не трогать (тип остался в `MessageHub.Core.Entities`)

## Чеклист шагов

- [x] Создать папки `Entities/<Entity>/Enums/` и переместить туда файлы entity + enum
- [x] Обновить `namespace`/`using` во всех перемещённых entity-файлах и их enum'ах
- [x] Обновить `using` в `ChannelContact.cs`, `ConnectedAccount.cs`, `Conversation.cs` (кросс-ссылки)
- [x] Обновить `using` в тестах: `ConversationTests`, `MergeProposalTests`, `MessageTests`, `RelationChangeLogTests`, `SyncStateTests`
- [x] Удалить опустевшую папку `MessageHub.Core/Enums`
- [x] `dotnet build` — успешно (0 ошибок)
- [x] `dotnet test` — 46/46 пройдено

## Статус: готово

Коммит не делался (не просили).

### Нюанс: коллизия имени namespace и класса

Когда entity и её namespace-сегмент совпадают по имени (`Entities.ConnectedAccount` — класс `ConnectedAccount`), любой файл, лежащий прямо в родительском `MessageHub.Core.Entities` (`ChannelContact`, `Person`, `ConversationParticipant`, `UserProfile`) и ссылающийся на такой тип по простому имени, получает `CS0118` — компилятор находит вложенный namespace `ConnectedAccount` раньше типа. Единственный такой случай сейчас — `ChannelContact.ConnectedAccount`; исправлено `using`-алиасом:
```csharp
using ConnectedAccountEntity = MessageHub.Core.Entities.ConnectedAccount.ConnectedAccount;
```
При добавлении новых перекрёстных ссылок из «плоских» сущностей на сущности с собственной папкой/namespace — тот же приём (алиас), либо полное квалифицированное имя.

## Из предыдущей задачи (упрощение ТЗ для MVP) — выполнено

Упростить `docs/TZ_Local_Messenger_Hub.md` для MVP — убрать из объёма v1 то, что не критично для первой версии.

Решения по объёму (согласованы с пользователем):
- Каналы (US-02: Telegram/VK/WhatsApp/Email) — **без изменений**, все 4 остаются в v1.
- Merge/Split собеседников (US-05, ФТ-501…515, правила сопоставления 10.9) — **без изменений**, остаётся полностью.
- Поиск (US-09, ФТ-951…953) и уведомления (US-10, ФТ-961…962) — **убрать из v1**, перенести в раздел 20 «Перспективы развития» (не просто оставить «Желательно» — исключить из объёма первой версии).
- «Новые диалоги» как отдельный экран/раздел UI (ФТ-810, ФТ-811) — **упростить**: не заводим отдельный пункт меню и очередь, новые диалоги (`status = Pending`, появившиеся при синхронизации) находятся и обрабатываются через повторное открытие экрана «Выбор кандидатов» (US-08, уже позволяет открыть его повторно — ФТ-809). Доменная модель (`ConversationStatus.Pending`, переходы) не меняется — меняется только представление в UI/ТЗ.
- Настройки (US-03) — упростить: убрать/понизить автозапуск с Windows, смену расположения файла БД, правила хранения истории, настраиваемый уровень логирования (оставить вкл/выкл). Остаются: язык/тема, интервал синхронизации, безопасность (пароль/автоблокировка), настройки аккаунта (синк вкл/выкл, типы кандидатов по умолчанию).

Важно: изменения затрагивают только `docs/TZ_Local_Messenger_Hub.md` (текст требований), не код. Уже написанные сущности `Core` (`Conversation`, `ConversationParticipant`, `MergeProposal`, `RelationChangeLog`, `SyncState`) остаются валидными — merge/split и статус `Pending` никуда не делись, меняется только то, как «новые диалоги» показываются в UI.

## Чеклист шагов

- [x] Прочитать полный текст ТЗ, разобраться в текущем объёме v1
- [x] Уточнить с пользователем, что резать (каналы/merge/поиск/новые диалоги/настройки)
- [x] Раздел 0: версия 2.0 → 2.1, запись в историю изменений
- [x] Раздел 3 (US-03): критерии приёмки — убрать автозапуск с Windows, уведомления, расположение БД, правила хранения истории; логирование — только вкл/выкл
- [x] Раздел 3 (US-08): критерии приёмки — убрать отдельный пункт «Новые диалоги», описать через повторное открытие экрана выбора кандидатов
- [x] Раздел 4.1 — обновить пункты 8, 9 (поиск/уведомления: исключены из v1, не просто «Желательно»)
- [x] Раздел 4.2 — пометить US-09/US-10 перенесёнными в раздел 20
- [x] Раздел 8.1 — НФТ-11 (расположение БД настраивается) понижено до «Вне v1»
- [x] Раздел 10.3 — таблица ФТ-301…308: ФТ-302/305/306 → «Вне v1», ФТ-308 упрощён
- [x] Раздел 10.8 — ФТ-809…811 переформулированы под переиспользуемый экран выбора кандидатов
- [x] Раздел 10.10 — ФТ-951…953, ФТ-961…962 убраны из таблицы, заменены сноской
- [x] Раздел 13 — «Новые диалоги» убраны из главного меню и таблицы экранов
- [x] Раздел 15 — добавлены явные исключения v1 (поиск, уведомления, доп. настройки, отдельный UI «Новые диалоги»)
- [x] Раздел 16 — этап 8 переформулирован (без «Новые диалоги»)
- [x] Раздел 17.2 — ПР-15 переформулирован под экран выбора кандидатов
- [x] Раздел 18 — строки US-09/US-10 убраны из матрицы трассировки
- [x] Раздел 19.1 — ОВ-05, ОВ-09 помечены решёнными (перенесены в раздел 20)
- [x] Раздел 20 — формулировки уточнены под перенесённые пункты (US-09, US-10, настройки)
- [x] Финальная проверка консистентности по всему документу (grep по ФТ-951/961/95x/96x, «Новые диалоги», US-09, US-10) — расхождений не найдено

## Статус: готово

Правки внесены только в `docs/TZ_Local_Messenger_Hub.md`. Код и уже написанные сущности `Core` не менялись — не требуется. Коммит не делался (не просили).

## Дизайн-решения

- «Новые диалоги» не исчезают из модели — исчезает только отдельный UI-раздел. Технически: экран «Выбор кандидатов» (US-08) при открытии показывает как `Candidate` (из адаптера, ещё не в БД), так и уже существующие `Pending` из БД — одним списком с той же вкладочной структурой (личные/группы/broadcast/иные) плюс фильтр «Игнорированные» для возврата. Это не требует нового API-эндпоинта сверх уже описанных в разделе 12 (`/api/conversations?status=Pending`, `/api/accounts/{id}/candidates`).
- Расположение файла БД в v1 фиксировано (стандартная папка данных приложения), не настраивается пользователем — снимает ФТ-305 и меняет НФТ-11.
- Правила хранения истории (авто-удаление старых сообщений) — не входит в v1, история не удаляется автоматически (кроме явного удаления пользователем при удалении собеседника, US-05).

## Из предыдущей задачи (доменные сущности) — выполнено

- Добавлены сущности `SyncState`, `ConversationParticipant`, `Conversation`, `MergeProposal`, `RelationChangeLog` + тесты (46/46), `dotnet build`/`dotnet test` проходят. Коммит не делался (не просили).
- `ChannelType` пока `enum`, не VO — ждёт `IChannelAdapter`/`ChannelRegistry`.
- `ConnectedAccount.SecretRef`/`LastError` помечены `TODO: Удалить`, расходятся с ТЗ (секреты не персистируются) — отдельная задача.
