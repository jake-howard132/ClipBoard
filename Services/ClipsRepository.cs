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

        public async Task<List<ClipRecord>> GetClipsByGroupAsync(int groupId)
        {
            return await _db.Clips
                .Where(c => c.ClipGroupRecordId == groupId)
                .OrderByDescending(c => c.SortOrder)
                .ToListAsync();
        }

        //public async Task AddClipAsync(Clip clip)
        //{
        //    _db.Clips.Add(clip);
        //    await _db.SaveChangesAsync();
        //}
        public async Task DeleteClipAsync(int clipId)
        {
            var clip = await _db.Clips.FindAsync(clipId);
            if (clip != null)
            {
                _db.Clips.Remove(clip);
                await _db.SaveChangesAsync();
            }
        }
    }
}
