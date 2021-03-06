﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MxNet.IO
{
    public class NDArrayIter : DataIter
    {
        private NDArrayList _cache_data;
        private NDArrayList _cache_label;
        private NDArrayDict data;
        private readonly NDArrayList data_list = new NDArrayList();
        private DataBatch first_batch = null;
        private NDArray idx;
        private NDArrayDict label;
        private readonly string last_batch_handle;
        private readonly int num_data;
        private int num_source;
        private readonly bool shuffle;

        public NDArrayIter(NDArrayList data, NDArrayList label = null, int batch_size = 1, bool shuffle = false,
            string last_batch_handle = "pad", string data_name = "data", string label_name = "softmax_label")
        {
            this.data = IOUtils.InitData(data, false, data_name);
            this.label = IOUtils.InitData(label, false, label_name);
            BatchSize = batch_size;
            Cursor = batch_size;
            num_data = data[0].Shape[0];
            this.last_batch_handle = last_batch_handle;
            this.shuffle = shuffle;

            Reset();
            data_list.Add(data);
            data_list.Add(label);
            _cache_data = null;
            _cache_label = null;
        }

        public override DataDesc[] ProvideData
        {
            get
            {
                var result = new List<DataDesc>();
                foreach (var kv in data)
                {
                    var shape = kv.Value.Shape.Data.ToList();
                    shape.RemoveAt(0);
                    shape.Insert(0, BatchSize);
                    result.Add(new DataDesc(kv.Key, new Shape(shape), kv.Value.DataType));
                }

                return result.ToArray();
            }
        }

        public override DataDesc[] ProvideLabel
        {
            get
            {
                var result = new List<DataDesc>();
                foreach (var kv in label)
                {
                    var shape = kv.Value.Shape.Data.ToList();
                    shape.RemoveAt(0);
                    shape.Insert(0, BatchSize);
                    result.Add(new DataDesc(kv.Key, new Shape(shape), kv.Value.DataType));
                }

                return result.ToArray();
            }
        }

        public override NDArrayList GetData()
        {
            return _batchify(data);
        }

        public override int[] GetIndex()
        {
            return Enumerable.Range(0, data.Keys.Length).ToArray();
        }

        public override NDArrayList GetLabel()
        {
            return _batchify(label);
        }

        public override int GetPad()
        {
            if (last_batch_handle == "pad" && Cursor + BatchSize > num_data)
                return Cursor + BatchSize - num_data;
            if (last_batch_handle == "roll_over" && -BatchSize < Cursor && Cursor < 0) return -Cursor;

            return 0;
        }

        public override bool IterNext()
        {
            Cursor += BatchSize;
            return Cursor < num_data;
        }

        public override bool End()
        {
            return !(Cursor + BatchSize < num_data);
        }

        public override DataBatch Next()
        {
            if (!IterNext())
                throw new Exception("Stop Iteration");

            var d = GetData();
            var l = GetLabel();
            // iter should stop when last batch is not complete
            if (d[0].Shape[0] != BatchSize)
            {
                //in this case, cache it for next epoch
                _cache_data = d;
                _cache_label = l;
                throw new Exception("Stop Iteration");
            }


            return new DataBatch(d, l, GetPad());
        }

        private void HardReset()
        {
            if (shuffle)
                ShuffleData();
            Cursor = -BatchSize;
            _cache_data = null;
            _cache_label = null;
        }

        public override void Reset()
        {
            if (shuffle)
                ShuffleData();

            if (last_batch_handle == "roll_over" && num_data - BatchSize < Cursor && Cursor < num_data
            ) // (self.cursor - self.num_data) represents the data we have for the last batch
                Cursor = Cursor - num_data - BatchSize;
            else
                Cursor = -BatchSize;
        }

        public static NDArrayIter FromBatch(DataBatch data_batch)
        {
            var iter = new NDArrayIter(data_batch.Data, data_batch.Label);
            iter.DefaultBucketKey = data_batch.BucketKey.HasValue ? data_batch.BucketKey.Value : new Random().Next();
            return iter;
        }

        private void ShuffleData()
        {
            data = IOUtils.GetDataByIdx(data);
            label = IOUtils.GetDataByIdx(label);
        }

        private NDArrayList _getdata(NDArrayDict data_source, int? start = null, int? end = null)
        {
            if (!start.HasValue && !end.HasValue)
                throw new ArgumentException("Should atleast specify start or end");

            start = start.HasValue ? start : 0;
            end = end.HasValue ? end : data_source.First().Value.Shape[0];

            var result = new NDArrayList();
            foreach (var x in data_source) result.Add(x.Value.Slice(start.Value, end));

            return result.ToArray();
        }

        private NDArrayList _concat(NDArrayList first_data, NDArrayList second_data)
        {
            if (first_data.Length != second_data.Length)
                throw new Exception("Data source should be of same size.");

            var result = new NDArrayList();
            for (var i = 0; i < first_data.Length; i++)
                result.Add(
                    nd.Concat(new NDArrayList(first_data[i], second_data[i]), 0)
                );

            return result.ToArray();
        }

        private NDArrayList _batchify(NDArrayDict data_source)
        {
            if (Cursor > num_data)
                throw new Exception("DataIter need reset");

            if (last_batch_handle == "roll_over" && -BatchSize < Cursor && Cursor < 0)
            {
                if (_cache_data == null && _cache_label == null)
                    throw new Exception("Next epoch should have cached data");

                var cache_data = _cache_data != null ? _cache_data : _cache_label;
                var second_data = _getdata(data_source, end: Cursor + BatchSize);
                if (_cache_data != null)
                    _cache_data = null;
                else
                    _cache_label = null;

                return _concat(cache_data, second_data);
            }

            if (last_batch_handle == "pad" && Cursor + BatchSize > num_data)
            {
                var pad = BatchSize - num_data + Cursor;
                var first_data = _getdata(data_source, Cursor);
                var second_data = _getdata(data_source, end: pad);
                return _concat(first_data, second_data);
            }

            var end_idx = 0;
            if (Cursor + BatchSize < num_data)
                end_idx = Cursor + BatchSize;
            else
                end_idx = num_data;

            return _getdata(data_source, Cursor, end_idx);
        }
    }
}