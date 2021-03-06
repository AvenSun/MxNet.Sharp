﻿// ReSharper disable once CheckNamespace

using MxNet.Interop;

namespace MxNet
{
    public sealed class Context
    {
        #region Constructors

        public Context(DeviceType type = DeviceType.CPU, int id = 0)
        {
            _Type = type;
            _Id = id;
        }

        #endregion

        public static Context CurrentContext
        {
            get
            {
                if (current != null)
                    return current;

                return Cpu();
            }
            set => current = value;
        }

        public override string ToString()
        {
            if (GetDeviceType() == DeviceType.GPU)
                return string.Format("gpu({0})", GetDeviceId());
            if (GetDeviceType() == DeviceType.CPUPinned)
                return string.Format("cpu_pinned({0})", GetDeviceId());
            if (GetDeviceType() == DeviceType.CPUShared)
                return string.Format("cpu_shared({0})", GetDeviceId());
            return string.Format("cpu({0})", GetDeviceId());
        }

        public override bool Equals(object obj)
        {
            var other = (Context) obj;
            return ToString() == other.ToString();
        }

        #region Fields

        private readonly DeviceType _Type;

        private readonly int _Id = -1;

        private static Context current;

        #endregion

        #region Methods

        public static Context Cpu(int deviceId = 0)
        {
            return new Context(DeviceType.CPU, deviceId);
        }

        public static Context Gpu(int deviceId = 0)
        {
            return new Context(DeviceType.GPU, deviceId);
        }

        public static Context CpuPinned(int deviceId = 0)
        {
            return new Context(DeviceType.CPUPinned, deviceId);
        }

        public static Context CpuShared(int deviceId = 0)
        {
            return new Context(DeviceType.CPUShared, deviceId);
        }

        public int GetDeviceId()
        {
            return _Id;
        }

        public DeviceType GetDeviceType()
        {
            return _Type;
        }

        public void EmptyCache()
        {
            NativeMethods.MXStorageEmptyCache((int) _Type, _Id);
        }

        public static (long, long) GpuMemoryInfo(int deviceId = 0)
        {
            long freeMem = 0;
            long totalMem = 0;

            NativeMethods.MXGetGPUMemoryInformation64(deviceId, ref freeMem, ref totalMem);
            return (freeMem, totalMem);
        }

        public static int NumGpus()
        {
            var count = 0;
            NativeMethods.MXGetGPUCount(ref count);
            return count;
        }

        #endregion
    }
}