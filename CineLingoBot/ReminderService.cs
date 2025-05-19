using Telegram.Bot;
using Telegram.Bot.Types;
using System.Collections.Concurrent;

namespace CineLingoBot.Services;

public class ReminderService
{
    private readonly ITelegramBotClient _bot;
    private readonly ConcurrentDictionary<long, List<TimeSpan>> _userReminders = new();
    public readonly ConcurrentDictionary<long, ReminderState> _userStates = new();

    public ReminderService(ITelegramBotClient bot)
    {
        _bot = bot;
        _ = StartReminderLoop();
    }

    public enum ReminderState
    {
        None,
        AwaitingFirstTime,
        AwaitingAdditionalTime
    }

    public async Task SubscribeAsync(long telegramId, long chatId)
    {
        await StartSettingReminder(telegramId, chatId);
    }

    public void Unsubscribe(long telegramId)
    {
        RemoveAllReminders(telegramId);
    }

    public async Task StartSettingReminder(long telegramId, long chatId)
    {
        _userStates[telegramId] = ReminderState.AwaitingFirstTime;
        _userReminders.TryAdd(telegramId, new List<TimeSpan>());

        await _bot.SendMessage(chatId,
            "⏰ Укажите время для напоминания в формате ЧЧ:ММ (например, 09:30):\n" +
            "Или нажмите /done чтобы завершить настройку.");
    }

    public async Task ProcessTimeInput(long telegramId, long chatId, string input)
    {
        if (!_userStates.TryGetValue(telegramId, out var state)) return;

        if (input.Equals("/done", StringComparison.OrdinalIgnoreCase))
        {
            await FinishSettingReminder(telegramId, chatId);
            return;
        }

        if (TimeSpan.TryParse(input, out var time))
        {
            if (!_userReminders.TryGetValue(telegramId, out var times))
            {
                times = new List<TimeSpan>();
                _userReminders[telegramId] = times;
            }

            times.Add(time);
            _userStates[telegramId] = ReminderState.AwaitingAdditionalTime;

            await _bot.SendMessage(chatId,
                $"⏰ Добавлено время {time:hh\\:mm}. Укажите еще одно время или нажмите /done чтобы завершить:");
        }
        else
        {
            await _bot.SendMessage(chatId,
                "❌ Неверный формат времени. Пожалуйста, укажите время в формате ЧЧ:ММ (например, 09:30):");
        }
    }

    private async Task FinishSettingReminder(long telegramId, long chatId)
    {
        _userStates.TryRemove(telegramId, out _);

        if (_userReminders.TryGetValue(telegramId, out var times) && times.Any())
        {
            var timesStr = string.Join(", ", times.Select(t => t.ToString("hh\\:mm")));
            await _bot.SendMessage(chatId,
                $"🔔 Напоминания установлены на: {timesStr}\n" +
                "Вы будете получать уведомления в указанные времена ежедневно.");
        }
        else
        {
            _userReminders.TryRemove(telegramId, out _);
            await _bot.SendMessage(chatId,
                "❌ Не было добавлено ни одного времени. Напоминания не установлены.");
        }
    }

    public void RemoveAllReminders(long telegramId)
    {
        _userReminders.TryRemove(telegramId, out _);
        _userStates.TryRemove(telegramId, out _);
    }

    public bool HasReminders(long telegramId) =>
        _userReminders.TryGetValue(telegramId, out var times) && times?.Any() == true;

    private async Task StartReminderLoop()
    {
        while (true)
        {
            var now = DateTime.Now;
            var today = DateTime.Today;

            foreach (var (userId, times) in _userReminders.ToArray())
            {
                foreach (var time in times)
                {
                    var reminderTime = today.Add(time);
                    var timeDifference = reminderTime - now;

                    if (now.Hour == reminderTime.Hour && now.Minute == reminderTime.Minute)
                    {
                        try
                        {
                            await _bot.SendMessage(userId,
                                $"⏰ {reminderTime:HH:mm} — пора повторить слова! 💡");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Reminder] Ошибка при отправке {userId}: {ex.Message}");
                            RemoveAllReminders(userId);
                        }
                    }
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(1));
        }
    }

    public bool TryGetReminderTimes(long telegramId, out List<TimeSpan> times)
    {
        if (_userReminders.TryGetValue(telegramId, out var existing))
        {
            times = existing.ToList();
            return times.Any();
        }

        times = null;
        return false;
    }

    public bool RemoveReminderTime(long telegramId, TimeSpan time)
    {
        if (_userReminders.TryGetValue(telegramId, out var existing))
        {
            var removed = existing.RemoveAll(t => t == time);
            return removed > 0;
        }

        return false;
    }
}