using Avalonia.Collections;
using Avalonia.Media.Imaging;
using ClipBoard.Models;
using ClipBoard.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ClipBoard.Services
{
    public class SettingsRepository
    {
        private readonly Db _db;
        public SettingsRepository(IServiceProvider services)
        {
            _db = services.GetRequiredService<Db>();
        }

        public async Task<List<SettingsRecord>> GetSettingsAsync()
        {
            return await _db.Settings.ToListAsync();
        }

        public async Task<SettingsRecord> UpdateSettingAsync(SettingsRecord setting)
        {
            var tracked = await _db.Settings.FirstOrDefaultAsync(c => c.Id == setting.Id);

            if (tracked is null) return setting;

            _db.Entry(tracked).CurrentValues.SetValues(setting);

            await _db.SaveChangesAsync();

            return setting;
        }
    }
}