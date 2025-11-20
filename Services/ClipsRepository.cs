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
    public class ClipsRepository
    {
        private readonly IServiceProvider _services;
        private readonly Db _db;
        public ClipsRepository(IServiceProvider services)
        {
            _services = services;
            _db = _services.GetRequiredService<Db>();
        }

        public async Task<ClipGroupRecord?> GetGroupByIdAsync(int clipGroupId)
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

        public async Task<IEnumerable<ClipRecord>> GetClipsByGroupAsync(int clipGroupId)
        {
            return await _db.Clips
                .Where(c => c.ClipGroupId == clipGroupId)
                .OrderByDescending(c => c.SortOrder)
                .ToListAsync();
        }

        public async Task<ClipRecord> AddClipAsync(ClipRecord clip)
        {
            try
            {
                await _db.Clips.AddAsync(clip);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return clip;
        }

        public async Task AddClipsAsync(IAvaloniaList<Clip> clips)
        {
            await _db.Clips.AddRangeAsync(clips.Select(c => c.ToRecord()));
            await _db.SaveChangesAsync();
        }

        public async Task DeleteClipAsync(int clipId)
        {
            var clip = await _db.Clips.FindAsync(clipId);
            if (clip != null)
            {
                _db.Clips.Remove(clip);
                await _db.SaveChangesAsync();
            }
        }
        public async Task UpdateClipAsync(ClipRecord clip)
        {
            var tracked = await _db.Clips.FirstOrDefaultAsync(c => c.Id == clip.Id);

            if (tracked is null) return;

            _db.Entry(tracked).CurrentValues.SetValues(clip);

            await _db.SaveChangesAsync();
        }

        public async Task UpdateClipOrdersAsync(int clipGroupId, IAvaloniaList<Clip> clips)
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