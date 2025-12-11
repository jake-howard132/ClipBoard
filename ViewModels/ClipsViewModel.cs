using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using ClipBoard.Models;
using ClipBoard.Services;
using ClipBoard.Views;
using DryIoc.FastExpressionCompiler.LightExpression;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tmds.DBus.Protocol;

namespace ClipBoard.ViewModels
{
    public partial class ClipsViewModel : ReactiveObject, IScreen
    {
        // Services
        private readonly IServiceProvider _services;

        // UI Properties
        [Reactive] public AvaloniaList<ClipGroup> ClipGroups { get; set; } = new();
        [Reactive] public ClipGroup? SelectedClipGroup { get; set; }
        [Reactive] public bool ClipGroupNameEditing { get; set; }
        [Reactive] public Clip? OpenClip { get; set; }

        //Routing
        public RoutingState Router => throw new NotImplementedException();

        // Commands
        public ReactiveCommand<Unit, Unit> LoadClipGroupsCommand { get; }
        public ReactiveCommand<Unit, Unit> AddClipGroupCommand { get; }
        public ReactiveCommand<ClipGroup, Unit> DeleteClipGroupCommand { get; }
        public ReactiveCommand<Unit, Unit> AddClipCommand { get; }
        public ReactiveCommand<IAvaloniaList<Clip>, Unit> AddClipsCommand { get; }
        public ReactiveCommand<ClipGroup, Unit> ResequenceClipsCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseCommand { get; }

        //Interactions
        public Interaction<string, bool> ConfirmDelete { get; } = new();

        public ClipsViewModel(IServiceProvider services)
        {
            _services = services;

            LoadClipGroupsCommand = ReactiveCommand.CreateFromTask(LoadGroupsAsync);
            AddClipGroupCommand = ReactiveCommand.CreateFromTask(AddClipGroupAsync);
            AddClipsCommand = ReactiveCommand.CreateFromTask<IAvaloniaList<Clip>>(clips => AddClipsByGroupAsync(clips));
            AddClipCommand = ReactiveCommand.CreateFromTask(AddClipAsync);
            DeleteClipGroupCommand = ReactiveCommand.CreateFromTask<ClipGroup>(clipGroup => DeleteClipGroupAsync(clipGroup));
            ResequenceClipsCommand = ReactiveCommand.CreateFromTask<ClipGroup>(clipGroup => ResequenceClipsAsync(clipGroup));
            CloseCommand = ReactiveCommand.Create(() => { });
        }

        private async Task LoadGroupsAsync()
        {
            var clipGroupsRepository = _services.GetRequiredService<ClipGroupsRepository>();

            var groups = await clipGroupsRepository.GetAllGroupsAsync();

            if (groups == null) return;

            ClipGroups.AddRange<ClipGroup>(groups);

            SelectedClipGroup = ClipGroups.FirstOrDefault();
        }

        private async Task AddClipGroupAsync()
        {
            var clipGroupsRepository = _services.GetRequiredService<ClipGroupsRepository>();

            var clipGroup = new ClipGroup(
                _services,
                null,
                "New ClipGroup",
                "",
                new AvaloniaList<Clip>(),
                ClipGroups.Count,
                true);

            var record = await clipGroupsRepository.AddClipGroupAsync(clipGroup.ToRecord());

            ClipGroups.Add(ClipGroup.ToModel(_services, record));

            SelectedClipGroup = ClipGroups.Last();
            SelectedClipGroup.IsEditing = true;
        }

        private async Task DeleteClipGroupAsync(ClipGroup clipGroup)
        {
            var clipGroupsRepository = _services.GetRequiredService<ClipGroupsRepository>();

            var result = await ConfirmDelete.Handle("Are you sure?");
            if (!result) return;

            await clipGroupsRepository.DeleteClipGroupAsync(clipGroup.ToRecord());
            ClipGroups.Remove(clipGroup);
            await ResequenceClipsAsync(clipGroup);
            SelectedClipGroup = ClipGroups.FirstOrDefault();
        }

        private async Task AddClipAsync()
        {
            if (SelectedClipGroup == null) return;

            var clipsRepository = _services.GetRequiredService<ClipsRepository>();
            var clip = new Clip(
              _services,
              (int)SelectedClipGroup.Id!,
              ClipGroups.Count);

            var record = await clipsRepository.AddClipAsync(clip.ToRecord());
            var newModel = Clip.ToModel(_services, record);
            SelectedClipGroup?.Clips.Add(newModel);
            await newModel.OpenClipCommand.Execute();
        }

        private async Task AddClipsByGroupAsync(IAvaloniaList<Clip> newClips)
        {
            var clipsRepository = _services.GetRequiredService<ClipsRepository>();
            var groupedClips = newClips.GroupBy(c => c.ClipGroupId);

            foreach (var group in groupedClips)
            {
                var clipGroup = ClipGroups.FirstOrDefault(g => g.Id == group.Key);
                clipGroup?.Clips.AddRange(group.ToList());
            }

            await clipsRepository.AddClipsAsync(newClips);
        }

        private async Task ResequenceClipsAsync(ClipGroup clipGroup)
        {
            var clipsRepository = _services.GetRequiredService<ClipsRepository>();

            for (int i = 0; i < clipGroup.Clips.Count; i++)
            {
                clipGroup.Clips[i].SortOrder = i;
            }
            await clipsRepository.UpdateClipOrdersAsync((int)clipGroup.Id!, clipGroup.Clips);
        }
    }
}
