using Avalonia.Media.Imaging;
using ClipBoard.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ClipBoard.Services
{
    public class ClipsRepository
    {
        private readonly Db _db;
        public ClipsRepository(Db db)
        {
            _db = db;
            _db.Database.EnsureCreated();
        }

        public async Task<ClipGroupRecord?> GetGroupByIdAsync(Guid clipGroupId)
        {
            return await _db.ClipGroups
                .Where(g => g.Id == clipGroupId)
                .Include(g => g.Clips)
                .Select(g => new ClipGroupRecord
                {
                    Name = g.Name,
                    Description = g.Description,
                    Clips = g.Clips.OrderBy(c => c.SortOrder).ToList(),
                    SortOrder = g.SortOrder
                }).FirstOrDefaultAsync();
        }

        public async Task<List<ClipRecord>> GetClipsByGroupAsync(Guid clipGroupId)
        {
            return await _db.Clips
                .Where(c => c.ClipGroupId == clipGroupId)
                .OrderByDescending(c => c.SortOrder)
                .ToListAsync();
        }

        public async Task AddClipsAsync(IList<ClipRecord> clips)
        {
            await _db.Clips.AddRangeAsync(clips);
            await _db.SaveChangesAsync();
        }
        public async Task DeleteClipAsync(Guid clipId)
        {
            var clip = await _db.Clips.FindAsync(clipId);
            if (clip != null)
            {
                _db.Clips.Remove(clip);
                await _db.SaveChangesAsync();
            }
        }
        public async Task UpdateClipOrdersAsync(Guid clipGroupId, IEnumerable<ClipRecord> clips)
        {
            var clipIds = clips.Select(c => c.Id).ToList();

            var clipRecords = await _db.Clips
                .Where(c => c.ClipGroupId == clipGroupId)
                .Where(c => clipIds.Contains(c.Id))
                .ToListAsync();

            foreach (var clip in clipRecords)
            {
                var update = clips.First(c => c.Id == clip.Id);
                clip.SortOrder = update.SortOrder;
            }

            await _db.SaveChangesAsync();
        }
    }
}
