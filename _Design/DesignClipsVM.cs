using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using ClipBoard.Models;
using ClipBoard.Services;
using ClipBoard.ViewModels;
using ClipBoard.Views;
using DryIoc.FastExpressionCompiler.LightExpression;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tmds.DBus.Protocol;


namespace ClipBoard._Design
{
    public class DesignClipsVM : ClipsViewModel
    {
        public DesignClipsVM() : base() {

            var clipGroup = new ClipGroup(
                new ServiceCollection().BuildServiceProvider(),
                null,
                "New ClipGroup",
                "",
                new AvaloniaList<Clip> { new DesignClip(), new DesignClip(), new DesignClip() },
                ClipGroups.Count,
                false);

            var clipGroups = new List<ClipGroup> { clipGroup, clipGroup, clipGroup };

            ClipGroups.AddRange<ClipGroup>(clipGroups);
        }
    }
}
