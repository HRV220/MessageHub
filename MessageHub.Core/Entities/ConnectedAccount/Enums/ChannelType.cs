namespace MessageHub.Core.Entities.ConnectedAccount.Enums;

// TODO: заменить на VO (или что-то с generic-параметром под настройки канала).
// Подключение у каждого сервиса устроено по-разному: где-то это токен через обычный API
// (Email — IMAP/SMTP, OAuth2), а где-то — клиентский протокол вроде MTProto для Telegram.
// Enum с фиксированным списком этого не выражает и не масштабируется на новый канал без
// изменения Core, что противоречит АРХ-05 (адаптер регистрируется в DI без правок Core).
// Пока сойдёт — переосмыслить при проектировании IChannelAdapter/ChannelRegistry (раздел 11 ТЗ).
public enum ChannelType
{
  Telegram,
  Vk,
  WhatsApp,
  Email
}
