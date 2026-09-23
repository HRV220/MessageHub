---
name: new-domain-entity
description: Скаффолдинг новой доменной сущности в MessageHub.Core по уже устоявшемуся в проекте паттерну (приватный конструктор, статическая фабрика Create с валидацией, приватные сеттеры через field-keyword, XML-доки, тест-заглушка). Использовать, когда нужно добавить новую сущность в Core (например, из раздела 7 «Доменная модель» ТЗ) — по имени и списку полей.
---

# new-domain-entity

Создаёт новую доменную сущность в `MessageHub.Core/Entities/` по паттерну, который уже используют `Message`, `Person`, `UserProfile`, `ChannelContact`, `ConnectedAccount`.

## Перед созданием

1. Спроси у пользователя (если не дано): имя сущности, список полей (имя, тип, обязательное/опциональное), нужны ли переходы состояния (как `MarkAsSent` у `Message`) или связи с другими сущностями (как `ChannelContacts` у `Person`).
2. Если сущность или её поля упомянуты в `docs/TZ_Local_Messenger_Hub.md` (раздел 7 «Доменная модель») — прочитай соответствующий фрагмент и используй точные названия/семантику полей оттуда, сохрани ссылку на пункт ТЗ в XML-комментарии, если она есть в тексте ТЗ.

## Паттерн файла `MessageHub.Core/Entities/<Entity>.cs`

- `namespace MessageHub.Core.Entities;`, `using MessageHub.Core.Enums;` если нужны enum'ы.
- 2-пробельный отступ (не 4).
- Класс: `public class <Entity>`.
- Обязательные string-свойства — через `field`-keyword accessor:
  ```csharp
  public string Foo
  {
    get;
    private set
    {
      if (string.IsNullOrWhiteSpace(value))
        throw new ArgumentException("Foo must not be empty.", nameof(value));
      field = value;
    }
  } = null!;
  ```
- Опциональные/не-строковые свойства — просто `{ get; private set; }`.
- Приватный конструктор, принимающий все поля включая `Guid id`.
- Публичная статическая фабрика `Create(...)`, которая валидирует поля, не покрытые accessor'ом (например, `Guid`, не равный `Guid.Empty`), и вызывает `new <Entity>(Guid.NewGuid(), ...)`.
- Коллекции дочерних сущностей — `private readonly List<T> _items = [];` + `public IReadOnlyList<T> Items => _items;`, мутирующие методы (`AddX`/`RemoveX`) — на этом же агрегате.
- XML doc-комментарий (`///`) на каждом публичном члене и `<exception>` на методах, которые бросают исключения.
- Методы переходов состояния — проверяют текущее состояние, бросают `InvalidOperationException` при недопустимом переходе.

## После создания

1. Создай `MessageHub.Core.Tests/Entities/<Entity>Tests.cs` по образцу `MessageTests.cs`: успешное создание, по одному тесту на каждое условие валидации, по одному тесту на каждый переход состояния.
2. Прогони `dotnet build` и `dotnet test`, покажи результат.
3. Не добавляй `ProjectReference` из `Core` наружу и не открывай публичный конструктор — см. корневой `CLAUDE.md`.
