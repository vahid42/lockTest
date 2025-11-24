
/// <summary>
/// نسخه دوم → Fully Locked (صف کشیدن منظم و امن)
/// await _lock.WaitAsync();
/// ✔️ همه درخواست‌ها صف می‌گیرند
///✔️ فقط یک Thread می‌تواند زمان را چک و آپدیت کند
///✔️ هیچ Race Condition اتفاق نمی‌افتد
///✔️ رفتار کاملاً قابل پیش‌بینی
///✔️ فقط یک Thread زمان را می‌خواند
///✔️ فقط یک Thread تصمیم می‌گیرد
///✔️ هیچ دو Thread همزمان به _lastExecution دست نمی‌زنند
///✔️ رفتار دقیق، ثابت و قابل اعتماد است
/// </summary>
public class CooldownLock
{
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    private DateTime _lastExecution = DateTime.MinValue;
    private readonly TimeSpan _cooldown = TimeSpan.FromSeconds(2);

    public async Task<bool> TryEnterAsync()
    {
        await _lock.WaitAsync();
        try
        {
            // اگر هنوز در بازه ۲ ثانیه‌ای هستیم → اجازه نده
            if (DateTime.UtcNow - _lastExecution < _cooldown)
                return false;

            // ثبت زمان ورود
            _lastExecution = DateTime.UtcNow;
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }
}

