// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.AspNet.Server.Kestrel.Networking
{
    public class Libuv
    {
        public Libuv()
        {
            IsWindows = PlatformApis.IsWindows();
            if (!IsWindows)
            {
                IsDarwin = PlatformApis.IsDarwin();
            }
        }

        public bool IsWindows;
        public bool IsDarwin;

        public Func<string, IntPtr> LoadLibrary;
        public Func<IntPtr, bool> FreeLibrary;
        public Func<IntPtr, string, IntPtr> GetProcAddress;

        public void Load(string dllToLoad)
        {
            PlatformApis.Apply(this);

            var module = LoadLibrary(dllToLoad);

            if (module == IntPtr.Zero)
            {
                var message = "Unable to load libuv.";
                if (!IsWindows && !IsDarwin)
                {
                    // *nix box, so libuv needs to be installed
                    // TODO: fwlink?
                    message += " Make sure libuv is installed and available as libuv.so.1";
                }
                
                throw new InvalidOperationException(message);
            }

            foreach (var field in GetType().GetTypeInfo().DeclaredFields)
            {
                var procAddress = GetProcAddress(module, field.Name.TrimStart('_'));
                if (procAddress == IntPtr.Zero)
                {
                    continue;
                }
                var value = Marshal.GetDelegateForFunctionPointer(procAddress, field.FieldType);
                field.SetValue(this, value);
            }
        }

        public int Check(int statusCode)
        {
            Exception error;
            var result = Check(statusCode, out error);
            if (error != null)
            {
                throw error;
            }
            return statusCode;
        }

        public int Check(int statusCode, out Exception error)
        {
            if (statusCode < 0)
            {
                var errorName = err_name(statusCode);
                var errorDescription = strerror(statusCode);
                error = new Exception("Error " + statusCode + " " + errorName + " " + errorDescription);
            }
            else
            {
                error = null;
            }
            return statusCode;
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int uv_loop_init(UvLoopHandle a0);
        protected uv_loop_init _uv_loop_init = default(uv_loop_init);
        public void loop_init(UvLoopHandle handle)
        {
            Check(_uv_loop_init(handle));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int uv_loop_close(IntPtr a0);
        protected uv_loop_close _uv_loop_close = default(uv_loop_close);
        public void loop_close(UvLoopHandle handle)
        {
            handle.Validate(closed: true);
            Check(_uv_loop_close(handle.InternalGetHandle()));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int uv_run(UvLoopHandle handle, int mode);
        protected uv_run _uv_run = default(uv_run);
        public int run(UvLoopHandle handle, int mode)
        {
            handle.Validate();
            return Check(_uv_run(handle, mode));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate void uv_stop(UvLoopHandle handle);
        protected uv_stop _uv_stop = default(uv_stop);
        public void stop(UvLoopHandle handle)
        {
            handle.Validate();
            _uv_stop(handle);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate void uv_ref(UvHandle handle);
        protected uv_ref _uv_ref = default(uv_ref);
        public void @ref(UvHandle handle)
        {
            handle.Validate();
            _uv_ref(handle);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate void uv_unref(UvHandle handle);
        protected uv_unref _uv_unref = default(uv_unref);
        public void unref(UvHandle handle)
        {
            handle.Validate();
            _uv_unref(handle);
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_close_cb(IntPtr handle);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate void uv_close(IntPtr handle, uv_close_cb close_cb);
        protected uv_close _uv_close = default(uv_close);
        public void close(UvHandle handle, uv_close_cb close_cb)
        {
            handle.Validate(closed: true);
            _uv_close(handle.InternalGetHandle(), close_cb);
        }
        public void close(IntPtr handle, uv_close_cb close_cb)
        {
            _uv_close(handle, close_cb);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_async_cb(IntPtr handle);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int uv_async_init(UvLoopHandle loop, UvAsyncHandle handle, uv_async_cb cb);
        protected uv_async_init _uv_async_init = default(uv_async_init);
        public void async_init(UvLoopHandle loop, UvAsyncHandle handle, uv_async_cb cb)
        {
            loop.Validate();
            handle.Validate();
            Check(_uv_async_init(loop, handle, cb));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int uv_async_send(UvAsyncHandle handle);
        protected uv_async_send _uv_async_send = default(uv_async_send);
        public void async_send(UvAsyncHandle handle)
        {
            Check(_uv_async_send(handle));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int uv_tcp_init(UvLoopHandle loop, UvTcpHandle handle);
        protected uv_tcp_init _uv_tcp_init = default(uv_tcp_init);
        public void tcp_init(UvLoopHandle loop, UvTcpHandle handle)
        {
            loop.Validate();
            handle.Validate();
            Check(_uv_tcp_init(loop, handle));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int uv_tcp_bind(UvTcpHandle handle, ref sockaddr addr, int flags);
        protected uv_tcp_bind _uv_tcp_bind = default(uv_tcp_bind);
        public void tcp_bind(UvTcpHandle handle, ref sockaddr addr, int flags)
        {
            handle.Validate();
            Check(_uv_tcp_bind(handle, ref addr, flags));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int uv_tcp_open(UvTcpHandle handle, IntPtr hSocket);
        protected uv_tcp_open _uv_tcp_open = default(uv_tcp_open);
        public void tcp_open(UvTcpHandle handle, IntPtr hSocket)
        {
            handle.Validate();
            Check(_uv_tcp_open(handle, hSocket));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int uv_pipe_init(UvLoopHandle loop, UvPipeHandle handle, int ipc);
        protected uv_pipe_init _uv_pipe_init = default(uv_pipe_init);
        public void pipe_init(UvLoopHandle loop, UvPipeHandle handle, bool ipc)
        {
            loop.Validate();
            handle.Validate();
            Check(_uv_pipe_init(loop, handle, ipc ? -1 : 0));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        protected delegate int uv_pipe_bind(UvPipeHandle loop, string name);
        protected uv_pipe_bind _uv_pipe_bind = default(uv_pipe_bind);
        public void pipe_bind(UvPipeHandle handle, string name)
        {
            handle.Validate();
            Check(_uv_pipe_bind(handle, name));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_connection_cb(IntPtr server, int status);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int uv_listen(UvStreamHandle handle, int backlog, uv_connection_cb cb);
        protected uv_listen _uv_listen = default(uv_listen);
        public void listen(UvStreamHandle handle, int backlog, uv_connection_cb cb)
        {
            handle.Validate();
            Check(_uv_listen(handle, backlog, cb));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int uv_accept(UvStreamHandle server, UvStreamHandle client);
        protected uv_accept _uv_accept = default(uv_accept);
        public AcceptStatus accept(UvStreamHandle server, UvStreamHandle client)
        {
            server.Validate();
            client.Validate();

            var result = _uv_accept(server, client);
            if (result == (int)AcceptStatus.EAGAIN)
            {
                return AcceptStatus.EAGAIN;
            }

            Check(result);
            return AcceptStatus.Success;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_connect_cb(IntPtr req, int status);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        unsafe protected delegate int uv_pipe_connect(UvConnectRequest req, UvPipeHandle handle, string name, uv_connect_cb cb);
        protected uv_pipe_connect _uv_pipe_connect = default(uv_pipe_connect);
        unsafe public void pipe_connect(UvConnectRequest req, UvPipeHandle handle, string name, uv_connect_cb cb)
        {
            req.Validate();
            handle.Validate();
            Check(_uv_pipe_connect(req, handle, name, cb));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_alloc_cb(IntPtr server, int suggested_size, out uv_buf_t buf);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_read_cb(IntPtr server, int nread, ref uv_buf_t buf);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int uv_read_start(UvStreamHandle handle, uv_alloc_cb alloc_cb, uv_read_cb read_cb);
        protected uv_read_start _uv_read_start = default(uv_read_start);
        public void read_start(UvStreamHandle handle, uv_alloc_cb alloc_cb, uv_read_cb read_cb)
        {
            handle.Validate();
            Check(_uv_read_start(handle, alloc_cb, read_cb));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int uv_read_stop(UvStreamHandle handle);
        protected uv_read_stop _uv_read_stop = default(uv_read_stop);
        public void read_stop(UvStreamHandle handle)
        {
            handle.Validate();
            Check(_uv_read_stop(handle));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int uv_try_write(UvStreamHandle handle, Libuv.uv_buf_t[] bufs, int nbufs);
        protected uv_try_write _uv_try_write = default(uv_try_write);
        public int try_write(UvStreamHandle handle, Libuv.uv_buf_t[] bufs, int nbufs)
        {
            handle.Validate();
            return Check(_uv_try_write(handle, bufs, nbufs));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_write_cb(IntPtr req, int status);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe protected delegate int uv_write(UvRequest req, UvStreamHandle handle, Libuv.uv_buf_t* bufs, int nbufs, uv_write_cb cb);
        protected uv_write _uv_write = default(uv_write);
        unsafe public void write(UvRequest req, UvStreamHandle handle, Libuv.uv_buf_t* bufs, int nbufs, uv_write_cb cb)
        {
            req.Validate();
            handle.Validate();
            Check(_uv_write(req, handle, bufs, nbufs, cb));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe protected delegate int uv_write2(UvRequest req, UvStreamHandle handle, Libuv.uv_buf_t* bufs, int nbufs, UvStreamHandle sendHandle, uv_write_cb cb);
        protected uv_write2 _uv_write2 = default(uv_write2);
        unsafe public void write2(UvRequest req, UvStreamHandle handle, Libuv.uv_buf_t* bufs, int nbufs, UvStreamHandle sendHandle, uv_write_cb cb)
        {
            req.Validate();
            handle.Validate();
            Check(_uv_write2(req, handle, bufs, nbufs, sendHandle, cb));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_shutdown_cb(IntPtr req, int status);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int uv_shutdown(UvShutdownReq req, UvStreamHandle handle, uv_shutdown_cb cb);
        protected uv_shutdown _uv_shutdown = default(uv_shutdown);
        public void shutdown(UvShutdownReq req, UvStreamHandle handle, uv_shutdown_cb cb)
        {
            req.Validate();
            handle.Validate();
            Check(_uv_shutdown(req, handle, cb));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate IntPtr uv_err_name(int err);
        protected uv_err_name _uv_err_name = default(uv_err_name);
        public unsafe String err_name(int err)
        {
            IntPtr ptr = _uv_err_name(err);
            return ptr == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(ptr);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate IntPtr uv_strerror(int err);
        protected uv_strerror _uv_strerror = default(uv_strerror);
        public unsafe String strerror(int err)
        {
            IntPtr ptr = _uv_strerror(err);
            return ptr == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(ptr);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int uv_loop_size();
        protected uv_loop_size _uv_loop_size = default(uv_loop_size);
        public int loop_size()
        {
            return _uv_loop_size();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int uv_handle_size(HandleType handleType);
        protected uv_handle_size _uv_handle_size = default(uv_handle_size);
        public int handle_size(HandleType handleType)
        {
            return _uv_handle_size(handleType);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int uv_req_size(RequestType reqType);
        protected uv_req_size _uv_req_size = default(uv_req_size);
        public int req_size(RequestType reqType)
        {
            return _uv_req_size(reqType);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int uv_ip4_addr(string ip, int port, out sockaddr addr);

        protected uv_ip4_addr _uv_ip4_addr = default(uv_ip4_addr);
        public int ip4_addr(string ip, int port, out sockaddr addr, out Exception error)
        {
            return Check(_uv_ip4_addr(ip, port, out addr), out error);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        protected delegate int uv_ip6_addr(string ip, int port, out sockaddr addr);

        protected uv_ip6_addr _uv_ip6_addr = default(uv_ip6_addr);
        public int ip6_addr(string ip, int port, out sockaddr addr, out Exception error)
        {
            return Check(_uv_ip6_addr(ip, port, out addr), out error);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void uv_walk_cb(IntPtr handle, IntPtr arg);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        unsafe protected delegate int uv_walk(UvLoopHandle loop, uv_walk_cb walk_cb, IntPtr arg);
        protected uv_walk _uv_walk = default(uv_walk);
        unsafe public void walk(UvLoopHandle loop, uv_walk_cb walk_cb, IntPtr arg)
        {
            loop.Validate();
            _uv_walk(loop, walk_cb, arg);
        }

        public uv_buf_t buf_init(IntPtr memory, int len)
        {
            return new uv_buf_t(memory, len, IsWindows);
        }

        public struct sockaddr
        {
            public sockaddr(long ignored) { x3 = x0 = x1 = x2 = x3 = 0; }

            long x0;
            long x1;
            long x2;
            long x3;
        }

        public struct uv_buf_t
        {
            public uv_buf_t(IntPtr memory, int len, bool IsWindows)
            {
                if (IsWindows)
                {
                    x0 = (IntPtr)len;
                    x1 = memory;
                }
                else
                {
                    x0 = memory;
                    x1 = (IntPtr)len;
                }
            }

            public IntPtr x0;
            public IntPtr x1;
        }

        public enum HandleType
        {
            Unknown = 0,
            ASYNC,
            CHECK,
            FS_EVENT,
            FS_POLL,
            HANDLE,
            IDLE,
            NAMED_PIPE,
            POLL,
            PREPARE,
            PROCESS,
            STREAM,
            TCP,
            TIMER,
            TTY,
            UDP,
            SIGNAL,
        }

        public enum RequestType
        {
            Unknown = 0,
            REQ,
            CONNECT,
            WRITE,
            SHUTDOWN,
            UDP_SEND,
            FS,
            WORK,
            GETADDRINFO,
            GETNAMEINFO,
        }        

        public enum AcceptStatus
        {
            Success = 0,
            EAGAIN = -4088
        }
    }
}