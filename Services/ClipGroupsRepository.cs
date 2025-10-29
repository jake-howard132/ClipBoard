using ClipBoard.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
        //public async Task<ClipGroup> AddGroupAsync(int id, string name, string description, int order )
        //{
        //    var group = new ClipGroup ( id, name, description, order );
        //    _db.ClipGroups.Add(group);
        //    await _db.SaveChangesAsync();
        //    return group;
        //}
    }
}
