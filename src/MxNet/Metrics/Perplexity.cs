﻿using System;

namespace MxNet.Metrics
{
    public class Perplexity : EvalMetric
    {
        public Perplexity(int? ignore_label, int axis = -1, string output_name = null, string label_name = null) : base(
            "perplexity", output_name, label_name, true)
        {
            IgnoreLabel = ignore_label;
            Axis = axis;
        }

        public int? IgnoreLabel { get; }

        public int Axis { get; }

        public override void Update(NDArray labels, NDArray preds)
        {
            float loss = 0;
            var num = 0;

            labels = labels.AsInContext(preds.Context).Reshape(preds.Size);
            preds = nd.Pick(preds, labels.AsType(DType.Int32), Axis);
            if (IgnoreLabel.HasValue)
            {
                var ignore = nd.EqualScalar(labels, IgnoreLabel.Value).AsType(preds.DataType);
                num -= nd.Sum(ignore).AsScalar<int>();
                preds = preds * (1 - ignore) + ignore;
            }

            loss -= nd.Sum(nd.Log(nd.MaximumScalar(preds, 1e-10f))).AsScalar<float>();
            num += preds.Size;

            sum_metric += loss;
            global_sum_metric += loss;
            num_inst += num;
            global_num_inst += num;
        }

        public override (string, float) Get()
        {
            if (num_inst == 0)
                return (Name, float.NaN);

            return (Name, (float) Math.Exp(sum_metric / num_inst));
        }

        public override (string, float) GetGlobal()
        {
            if (hasGlobalStats)
            {
                if (global_num_inst == 0)
                    return (Name, float.NaN);

                return (Name, (float) Math.Exp(global_sum_metric / global_num_inst));
            }

            return Get();
        }
    }
}