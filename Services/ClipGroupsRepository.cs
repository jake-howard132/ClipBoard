using ClipBoard.Models;
using ClipBoard.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipBoard.Services
{
    public class ClipGroupsRepository
    {
        private readonly IServiceProvider _services;
        private readonly Db _db;

        public ClipGroupsRepository(IServiceProvider services)
        {
            _services = services;
            _db = _services.GetRequiredService<Db>();
        }
        public async Task<IEnumerable<ClipGroup>?> GetAllGroupsAsync()
        {
            return await _db.ClipGroups
            .Include(g => g.Clips)
            .OrderBy(g => g.SortOrder)
            .Select(g => ClipGroup.ToModel(_services, g))
            .ToListAsync();
        }
        public async Task<ClipGroupRecord> AddClipGroupAsync(ClipGroupRecord clipGroup)
        {
            await _db.ClipGroups.AddAsync(clipGroup);
            await _db.SaveChangesAsync();
            return clipGroup;
        }
        public async Task<ClipGroupRecord> DeleteClipGroupAsync(ClipGroupRecord clipGroup)
        {
            await _db.ClipGroups
                    .Where(g => g.Id == clipGroup.Id)
                    .ExecuteDeleteAsync();
            return clipGroup;
        }
        public async Task UpdateClipGroupAsync(ClipGroupRecord clipGroup)
        {
            var tracked = await _db.ClipGroups.FirstOrDefaultAsync(g => g.Id == clipGroup.Id);

            if (tracked is null) return;

            _db.Entry(tracked).CurrentValues.SetValues(clipGroup);

            await _db.SaveChangesAsync();
        }
    }
}
