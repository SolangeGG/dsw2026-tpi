using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.Domain.Interfaces;
using System.Text.Json;

namespace Dsw2026Tpi.Data
{
    public class HolidayProvider : IHolidayProvider
    {
        private readonly HashSet<DateOnly> _holidays;

        public HolidayProvider()
        {
            var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Sources/holidays.json"));
            var dates = JsonSerializer.Deserialize<List<string>>(json) ?? [];

            _holidays = dates
                .Select(d => DateOnly.ParseExact(d, "yyyy-MM-dd"))
                .ToHashSet();
        }

        public bool IsHoliday(DateOnly date)
        {
            return _holidays.Contains(date);
        }
    }
}
