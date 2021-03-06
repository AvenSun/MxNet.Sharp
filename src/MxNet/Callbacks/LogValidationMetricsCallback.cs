﻿using System;
using MxNet.Metrics;

namespace MxNet.Callbacks
{
    public class LogValidationMetricsCallback : IEvalEndCallback
    {
        public void Invoke(int epoch, EvalMetric eval_metric)
        {
            if (eval_metric == null) return;

            var name_value = eval_metric.GetNameValue();
            foreach (var item in name_value)
                Logger.Log(string.Format("Epoch[{0}] Validation-{1}={2}", epoch, item.Key, Math.Round(item.Value, 2)));
        }
    }
}