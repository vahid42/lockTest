using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lockTest
{
    /// <summary>
    /// نسخه نهایی (Atomic + Non-Blocking)
    /// if (!await _lock.WaitAsync(0))
    /// ✔️ ۱. Non-blocking کامل
    /// اگر کسی داخل بود → اصلاً صبر نمی‌کند → سریع خارج می‌شود.
    /// ✔️ ۲. چک زمان داخل لاک انجام می‌شود
    ///اجازه نمی‌دهد چند درخواست همزمان چک کنند.
    ///کاملاً ایمن و بدون Race.
    ///✔️ ۳. رفتار دقیق و قابل پیش‌بینی
    ///فقط و فقط اولین درخواست اجرا می‌شود و بقیه رد می‌شوند.
    /// </summary>
    internal class AccsessLock
    {
        private readonly SemaphoreSlim lockTime = new SemaphoreSlim(1, 1);
        private DateTime lastAccessTime = DateTime.MinValue;

        private readonly TimeSpan waiteTime = TimeSpan.FromSeconds(1);

        public async Task<bool> TryEnterAsync()
        {
            //  تلاش برای گرفتن لاک بدون منتظر ماندن (فوری)
            if (!await lockTime.WaitAsync(0))
                return false;   // یکی داخل است → سریع خارج شو

            try
            {
                //  حالا که وارد بخش حساس شدیم، چک زمان امن است
                if (DateTime.UtcNow - lastAccessTime < waiteTime)
                    return false;   // هنوز ۲ ثانیه نگذشته → رد کن

                // ثبت زمان
                lastAccessTime = DateTime.UtcNow;
                return true;
            }
            finally
            {
                lockTime.Release();
            }
        }

    }
}

