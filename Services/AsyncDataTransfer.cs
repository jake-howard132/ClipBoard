using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

/*
    An implementation of both IAsyncDataTransfer and IDataTransfer interfaces for syncand async clipboards depending on platform.
    This class allows adding data in various formats and provides both synchronous and asynchronous access to the data.
*/
namespace ClipBoard.Services
{
    public sealed class AsyncDataTransfer : IAsyncDataTransfer, IDataTransfer
    {
        private readonly List<DataTransferItem> _syncItems = new();
        private readonly List<IAsyncDataTransferItem> _asyncItems = new();
        private readonly List<DataFormat> _formats = new();

        public IReadOnlyList<DataFormat> Formats => _formats;
        IReadOnlyList<IDataTransferItem> IDataTransfer.Items => _syncItems;
        public IReadOnlyList<IAsyncDataTransferItem> Items => _asyncItems;

        public void Add(DataFormat dataFormat, object? value)
        {
            if (!_formats.Contains(dataFormat))
                _formats.Add(dataFormat);

            //Async item
            var asyncGetter = new Dictionary<DataFormat, Func<Task<object?>>>
            {
                [dataFormat] = () =>
                {
                    if (value is Stream s)
                    {
                        var ms = new MemoryStream();
                        s.Position = 0;
                        s.CopyTo(ms);
                        ms.Position = 0;
                        return Task.FromResult<object?>(ms);
                    }

                    return Task.FromResult(value);
                }
            };

            _asyncItems.Add(new AsyncDataTransferItem(asyncGetter));

            // Sync item
            var syncValue = new Dictionary<DataFormat, object?>
            {
                [dataFormat] = value
            };

            _syncItems.Add(new DataTransferItem(syncValue));
        }

        public void Dispose()
        {
            foreach (var item in _syncItems)
            {
                foreach (var fmt in item.Formats)
                {
                    if (item.GetData(fmt) is Stream s)
                        s.Dispose();
                }
            }
        }
    }

    // -------------------------------------------------------------

    public sealed class DataTransferItem : IDataTransferItem
    {
        public IReadOnlyList<DataFormat> Formats { get; }

        private readonly Dictionary<DataFormat, object?> _values;

        public DataTransferItem(IDictionary<DataFormat, object?> values)
        {
            _values = new Dictionary<DataFormat, object?>(values);
            Formats = _values.Keys.ToList();
        }
        public object? GetData(DataFormat format)
        {
            _values.TryGetValue(format, out var val);
            return val;
        }

        public object? TryGetRaw(DataFormat format)
        {
            throw new NotImplementedException();
        }
    }

    // -------------------------------------------------------------

    public sealed class AsyncDataTransferItem : IAsyncDataTransferItem
    {
        public IReadOnlyList<DataFormat> Formats { get; }
        private Dictionary<DataFormat, Func<Task<object?>>> _getters { get; }

        public AsyncDataTransferItem(IDictionary<DataFormat, Func<Task<object?>>> formatValuePairs)
        {
            _getters = new Dictionary<DataFormat, Func<Task<object?>>>(formatValuePairs);
            Formats = _getters.Keys.ToList();
        }

        public Task<object?> TryGetRawAsync(DataFormat format)
        {
            if (_getters.TryGetValue(format, out var getter))
                return getter();

            return Task.FromResult<object?>(null);
        }
    }
}