﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MxNet.Interop;

// ReSharper disable once CheckNamespace
namespace MxNet
{
    public sealed partial class mx
    {
        private static readonly string[] DllWhiteList =
            {"libgcc_s_seh-1.dll", "libgfortran-3.dll", "libquadmath-0.dll", "libopenblas.dll", "libmxnet.dll"};

        internal static string AppPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MxNet");

        #region Methods

        public static Context Device { get; set; }

        public static List<Context> MultiDevice { get; set; }

        public static bool UseCudnn { get; set; }

        public static void SetDevice(DeviceType device, params int[] deviceIds)
        {
            MultiDevice = new List<Context>();
            if (deviceIds.Length > 0)
            {
                for (var i = 0; i < deviceIds.Length; i++) MultiDevice.Add(Context.Gpu(deviceIds[i]));

                Device = MultiDevice[0];
            }
            else
            {
                if (device == DeviceType.CPU)
                    Device = Context.Cpu();
                else if (device == DeviceType.GPU)
                    Device = Context.Gpu();
            }
        }

        public static void SetMxNetPath(string mxnetFolder)
        {
            Environment.SetEnvironmentVariable("MXNET_LIBRARY_PATH", mxnetFolder);
            var dlls = Directory.EnumerateFiles(mxnetFolder).Select(Path.GetFileName).ToList();

            foreach (var dllName in DllWhiteList)
                if (dlls.Contains(dllName))
                    NativeMethods.LoadLibrary(Path.Combine(mxnetFolder, dllName));
        }

        public static void MXNotifyShutdown()
        {
            Logging.CHECK_EQ(NativeMethods.MXNotifyShutdown(), NativeMethods.OK);
        }

        public static string Version()
        {
            Logging.CHECK_EQ(NativeMethods.MXGetVersion(out int version), NativeMethods.OK);
            string ver_string = Math.Round(((float)version / 1000) - 9, 2).ToString();

            return ver_string;
        }

        public static Context Cpu(int id = 0) => Context.Cpu(id);

        public static Context Gpu(int id = 0) => Context.Gpu(id);

        #endregion
    }

    public abstract class MXNetObject
    {
        #region Properties

        /// <summary>
        ///     Native pointer of MXNet object
        /// </summary>
        public IntPtr NativePtr { get; internal set; }

        #endregion
    }

    public sealed class MXNetException : Exception
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MXNetException" /> class.
        /// </summary>
        public MXNetException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MXNetException" /> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public MXNetException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MXNetException" /> class with a specified error message and a
        ///     reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The name of the parameter that caused the current exception.</param>
        public MXNetException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #endregion
    }

    public abstract class MXNetSharedObject
    {
        #region Fields

        private int _RefCount = 1;

        #endregion

        #region Properties

        /// <summary>
        ///     Native pointer of MXNet object
        /// </summary>
        public IntPtr Handle { get; set; }

        #endregion

        #region Methods

        public void AddRef()
        {
            _RefCount++;
        }

        public void ReleaseRef()
        {
            _RefCount--;

            if (_RefCount == 0)
            {
                DisposeManaged();
                DisposeUnmanaged();
            }
        }

        #region Overrides

        protected virtual void DisposeManaged()
        {
        }

        protected virtual void DisposeUnmanaged()
        {
        }

        #endregion

        #endregion
    }
}