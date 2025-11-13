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
        public async Task<List<ClipGroupRecord>> GetAllGroupsAsync()
        {
            return await _db.ClipGroups
                .Include(g => g.Clips)
                .OrderBy(g => g.SortOrder)
                .Select(g => new ClipGroupRecord
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    Clips = g.Clips.OrderBy(c => c.SortOrder).ToList(),
                    SortOrder = g.SortOrder
                })
                .ToListAsync();
        }
        public async Task<ClipGroupRecord> AddClipGroupAsync(ClipGroupRecord clipGroup)
        {
            await _db.ClipGroups.AddAsync(clipGroup);
            return clipGroup;
        }
        public async Task<ClipGroupRecord> DeleteClipGroupAsync(ClipGroupRecord clipGroup)
        {
            await _db.ClipGroups.Where(g => g.Id == clipGroup.Id).ExecuteDeleteAsync();
            return clipGroup;
        }
        public async Task UpdateGroupAsync(ClipGroupRecord clipGroup)
        {
            try
            {
                var existing = await _db.ClipGroups.FindAsync(clipGroup.Id);
                if (existing == null) return;

                existing.Name = clipGroup.Name;
                existing.Description = clipGroup.Description;
                existing.SortOrder = clipGroup.SortOrder;

                _db.ClipGroups.Attach(existing);
                _db.Entry(existing).Property(x => x.Id).IsModified = false; // EF ignores Id
                _db.Entry(existing).State = EntityState.Modified;

                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var test = ex;
            }

        }
    }
}
