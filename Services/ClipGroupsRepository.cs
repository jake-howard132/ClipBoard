using ClipBoard.Models;
using ClipBoard.ViewModels;
using Microsoft.EntityFrameworkCore;
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
        private readonly Db _db;

        public ClipGroupsRepository(Db db)
        {
            _db = db;
            _db.Database.EnsureCreated();
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
            _db.ClipGroups.Add(clipGroup);
            await _db.SaveChangesAsync();
            return clipGroup;
        }
        public async Task<ClipGroupRecord> UpdateGroupAsync(ClipGroupRecord clipGroup)
        {
            _db.ClipGroups.Update(clipGroup);
            await _db.SaveChangesAsync();
            return clipGroup;
        }
    }
}
