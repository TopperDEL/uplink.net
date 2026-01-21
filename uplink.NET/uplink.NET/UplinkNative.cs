// storj_uplink.cs
// C# P/Invoke bindings for Storj Uplink C library (uplink-c).
// Based on storj/uplink-c v1.8.0+ doxygen and current repo exports (v1.10.1).
// Docs / structs: https://storj.github.io/uplink-c (doxygen)  [see citation in chat]
// Function exports (example): uplink_open_project, uplink_close_project, uplink_revoke_access  [see citation in chat]

#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace uplink.NET
{
    // Low-level native interop. Prefer wrapping with a higher-level C# facade in your app.
    internal static partial class UplinkNative
    {
        private const string LibraryName = "storj_uplink";

        // ---------
        // Blittable mirrors of C structs (subset required for P/Invoke signatures)
        // Derived from: uplink_definitions.h (doxygen)
        // ---------

        // Opaque handles - C side stores an internal handle at size_t _handle
        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkHandle { public nuint handle; }                 // UplinkHandle._handle

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkAccess { public nuint handle; }                 // UplinkAccess._handle

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkProject { public nuint handle; }                // UplinkProject._handle

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkDownload { public nuint handle; }               // UplinkDownload._handle

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkUpload { public nuint handle; }                 // UplinkUpload._handle

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkEncryptionKey { public nuint handle; }          // UplinkEncryptionKey._handle

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkPartUpload { public nuint handle; }             // UplinkPartUpload._handle

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct UplinkBucket
        {
            public long created;      // int64_t
            public IntPtr name;       // char*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct UplinkSystemMetadata
        {
            public long content_length;   // int64_t
            public long created;          // int64_t
            public long expires;          // int64_t
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct UplinkCustomMetadataEntry
        {
            public IntPtr key;        // char*
            public UIntPtr key_length;
            public IntPtr value;      // char*
            public UIntPtr value_length;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkCustomMetadata
        {
            public UIntPtr count;
            public IntPtr entries; // UplinkCustomMetadataEntry*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct UplinkObject
        {
            public UplinkCustomMetadata custom;
            [MarshalAs(UnmanagedType.I1)] public bool is_prefix;
            public IntPtr key;                // char*
            public UplinkSystemMetadata system;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkUploadOptions
        {
            public long expires; // int64_t; <=0 means no expiration
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkDownloadOptions
        {
            public long length; // negative => read until end
            public long offset; // negative => suffix
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct UplinkListObjectsOptions
        {
            public IntPtr cursor;    // const char*
            [MarshalAs(UnmanagedType.I1)] public bool custom;
            public IntPtr prefix;    // const char*
            [MarshalAs(UnmanagedType.I1)] public bool recursive;
            [MarshalAs(UnmanagedType.I1)] public bool system;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct UplinkListUploadsOptions
        {
            public IntPtr cursor;    // const char*
            [MarshalAs(UnmanagedType.I1)] public bool custom;
            public IntPtr prefix;    // const char*
            [MarshalAs(UnmanagedType.I1)] public bool recursive;
            [MarshalAs(UnmanagedType.I1)] public bool system;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct UplinkListBucketsOptions
        {
            public IntPtr cursor;    // const char*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkObjectIterator { public nuint handle; }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkBucketIterator { public nuint handle; }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkUploadIterator { public nuint handle; }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkPartIterator { public nuint handle; }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkPermission
        {
            [MarshalAs(UnmanagedType.I1)] public bool allow_delete;
            [MarshalAs(UnmanagedType.I1)] public bool allow_download;
            [MarshalAs(UnmanagedType.I1)] public bool allow_list;
            [MarshalAs(UnmanagedType.I1)] public bool allow_upload;
            public long not_after;   // unix seconds (0 disabled)
            public long not_before;  // unix seconds (0 disabled)
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct UplinkSharePrefix
        {
            public IntPtr bucket;    // const char*
            public IntPtr prefix;    // const char*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct UplinkPart
        {
            public IntPtr etag;          // char*
            public UIntPtr etag_length;
            public long modified;        // int64_t
            public uint part_number;     // uint32_t
            public UIntPtr size;
        }

        // Result wrappers
        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkError
        {
            public int code;             // int32_t
            public IntPtr message;       // char*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkAccessResult
        {
            public IntPtr access;        // UplinkAccess*
            public IntPtr error;         // UplinkError*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkProjectResult
        {
            public IntPtr error;         // UplinkError*
            public IntPtr project;       // UplinkProject*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkBucketResult
        {
            public IntPtr bucket;        // UplinkBucket*
            public IntPtr error;         // UplinkError*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkObjectResult
        {
            public IntPtr error;         // UplinkError*
            public IntPtr object_;       // UplinkObject*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkUploadResult
        {
            public IntPtr error;         // UplinkError*
            public IntPtr upload;        // UplinkUpload*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkPartUploadResult
        {
            public IntPtr error;         // UplinkError*
            public IntPtr part_upload;   // UplinkPartUpload*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkDownloadResult
        {
            public IntPtr download;      // UplinkDownload*
            public IntPtr error;         // UplinkError*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkWriteResult
        {
            public UIntPtr bytes_written;
            public IntPtr error;         // UplinkError*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkReadResult
        {
            public UIntPtr bytes_read;
            public IntPtr error;         // UplinkError*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct UplinkStringResult
        {
            public IntPtr error;         // UplinkError*
            public IntPtr @string;       // char*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkEncryptionKeyResult
        {
            public IntPtr encryption_key; // UplinkEncryptionKey*
            public IntPtr error;          // UplinkError*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct UplinkUploadInfo
        {
            public UplinkCustomMetadata custom;
            [MarshalAs(UnmanagedType.I1)] public bool is_prefix;
            public IntPtr key;                // char*
            public UplinkSystemMetadata system;
            public IntPtr upload_id;          // char*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkUploadInfoResult
        {
            public IntPtr error;              // UplinkError*
            public IntPtr info;               // UplinkUploadInfo*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkCommitUploadOptions { /* empty */ }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkCommitUploadResult
        {
            public IntPtr error;              // UplinkError*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkPartResult
        {
            public IntPtr error;              // UplinkError*
            public IntPtr part;               // UplinkPart*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct UplinkListUploadPartsOptions
        {
            public IntPtr cursor;             // const char*
        }

        // Edge structs
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct EdgeConfig
        {
            public IntPtr auth_service_address;   // const char*
            public IntPtr gateway_service;        // const char*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct EdgeRegisterAccessOptions
        {
            public IntPtr public_share;           // const char* (optional)
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct EdgeCredentials
        {
            public IntPtr access_key_id;          // char*
            public IntPtr secret_key;             // char*
            public IntPtr endpoint;               // char*
            public IntPtr region;                 // char*
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EdgeCredentialsResult
        {
            public IntPtr error;                  // UplinkError*
            public IntPtr creds;                  // EdgeCredentials*
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct EdgeShareURLOptions
        {
            public IntPtr not_before;             // const char* timestamp or NULL
            public IntPtr not_after;              // const char* timestamp or NULL
            public IntPtr host;                   // const char* override
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkMoveObjectOptions { /* empty */ }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct UplinkUploadObjectMetadataOptions
        {
            [MarshalAs(UnmanagedType.I1)] public bool set_expires;
            public long expires_unix; // int64_t
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UplinkCopyObjectOptions { /* reserved / empty */ }

        // ---------
        // Error helpers
        // ---------

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_error")]
        internal static partial void uplink_free_error(IntPtr error);

        // ---------
        // Access (grants, sharing, serialization)
        // ---------

        [LibraryImport(LibraryName, EntryPoint = "uplink_parse_access", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial UplinkAccessResult uplink_parse_access(string accessGrant);

        [LibraryImport(LibraryName, EntryPoint = "uplink_access_serialize")]
        internal static partial UplinkStringResult uplink_access_serialize(ref UplinkAccess access);

        [LibraryImport(LibraryName, EntryPoint = "uplink_access_share")]
        internal static partial UplinkAccessResult uplink_access_share(
            ref UplinkAccess access,
            UplinkPermission permission,
            IntPtr sharePrefixes /* UplinkSharePrefix* */,
            UIntPtr sharePrefixCount);

        [LibraryImport(LibraryName, EntryPoint = "uplink_access_override_encryption_key", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial IntPtr /* UplinkError* */ uplink_access_override_encryption_key(
            ref UplinkAccess access,
            string bucket,
            string prefix,
            ref UplinkEncryptionKey encryptionKey);

        [LibraryImport(LibraryName, EntryPoint = "uplink_derive_encryption_key", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial UplinkEncryptionKeyResult uplink_derive_encryption_key(string passphrase, string salt);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_access_result")]
        internal static partial void uplink_free_access_result(UplinkAccessResult result);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_string_result")]
        internal static partial void uplink_free_string_result(UplinkStringResult result);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_encryption_key_result")]
        internal static partial void uplink_free_encryption_key_result(UplinkEncryptionKeyResult result);

        // ---------
        // Project
        // ---------

        [LibraryImport(LibraryName, EntryPoint = "uplink_open_project")]
        internal static partial UplinkProjectResult uplink_open_project(ref UplinkAccess access);

        [LibraryImport(LibraryName, EntryPoint = "uplink_close_project")]
        internal static partial IntPtr /* UplinkError* */ uplink_close_project(ref UplinkProject project);

        [LibraryImport(LibraryName, EntryPoint = "uplink_revoke_access")]
        internal static partial IntPtr /* UplinkError* */ uplink_revoke_access(ref UplinkProject project, ref UplinkAccess access);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_project_result")]
        internal static partial void uplink_free_project_result(UplinkProjectResult result);

        // ---------
        // Buckets
        // ---------

        [LibraryImport(LibraryName, EntryPoint = "uplink_ensure_bucket", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial UplinkBucketResult uplink_ensure_bucket(ref UplinkProject project, string bucket);

        [LibraryImport(LibraryName, EntryPoint = "uplink_delete_bucket", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial UplinkBucketResult uplink_delete_bucket(ref UplinkProject project, string bucket);

        [LibraryImport(LibraryName, EntryPoint = "uplink_stat_bucket", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial UplinkBucketResult uplink_stat_bucket(ref UplinkProject project, string bucket);

        [LibraryImport(LibraryName, EntryPoint = "uplink_list_buckets")]
        internal static partial IntPtr /* UplinkBucketIterator* */ uplink_list_buckets(ref UplinkProject project, UplinkListBucketsOptions options);

        [LibraryImport(LibraryName, EntryPoint = "bucket_iterator_next")]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static partial bool bucket_iterator_next(ref UplinkBucketIterator it);

        [LibraryImport(LibraryName, EntryPoint = "bucket_iterator_err")]
        internal static partial IntPtr /* UplinkError* */ bucket_iterator_err(ref UplinkBucketIterator it);

        [LibraryImport(LibraryName, EntryPoint = "bucket_iterator_item")]
        internal static partial IntPtr /* UplinkBucket* */ bucket_iterator_item(ref UplinkBucketIterator it);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_bucket_result")]
        internal static partial void uplink_free_bucket_result(UplinkBucketResult result);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_bucket_iterator")]
        internal static partial void uplink_free_bucket_iterator(ref UplinkBucketIterator it);

        // ---------
        // Objects (stat/delete/list)
        // ---------

        [LibraryImport(LibraryName, EntryPoint = "uplink_stat_object", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial UplinkObjectResult uplink_stat_object(ref UplinkProject project, string bucket, string key);

        [LibraryImport(LibraryName, EntryPoint = "uplink_delete_object", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial UplinkObjectResult uplink_delete_object(ref UplinkProject project, string bucket, string key);

        [LibraryImport(LibraryName, EntryPoint = "uplink_list_objects", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial IntPtr /* UplinkObjectIterator* */ uplink_list_objects(ref UplinkProject project, string bucket, UplinkListObjectsOptions options);

        [LibraryImport(LibraryName, EntryPoint = "object_iterator_next")]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static partial bool object_iterator_next(ref UplinkObjectIterator it);

        [LibraryImport(LibraryName, EntryPoint = "object_iterator_err")]
        internal static partial IntPtr /* UplinkError* */ object_iterator_err(ref UplinkObjectIterator it);

        [LibraryImport(LibraryName, EntryPoint = "object_iterator_item")]
        internal static partial IntPtr /* UplinkObject* */ object_iterator_item(ref UplinkObjectIterator it);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_object_result")]
        internal static partial void uplink_free_object_result(UplinkObjectResult result);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_object_iterator")]
        internal static partial void uplink_free_object_iterator(ref UplinkObjectIterator it);

        // ---------
        // Download
        // ---------

        [LibraryImport(LibraryName, EntryPoint = "uplink_download_object", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial UplinkDownloadResult uplink_download_object(ref UplinkProject project, string bucket, string key, UplinkDownloadOptions options);

        [LibraryImport(LibraryName, EntryPoint = "uplink_download_read")]
        internal static partial UplinkReadResult uplink_download_read(ref UplinkDownload download, IntPtr buffer, UIntPtr bufferLen);

        [LibraryImport(LibraryName, EntryPoint = "uplink_close_download")]
        internal static partial IntPtr /* UplinkError* */ uplink_close_download(ref UplinkDownload download);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_download_result")]
        internal static partial void uplink_free_download_result(UplinkDownloadResult result);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_read_result")]
        internal static partial void uplink_free_read_result(UplinkReadResult result);

        // ---------
        // Upload (single-part)
        // ---------

        [LibraryImport(LibraryName, EntryPoint = "uplink_upload_object", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial UplinkUploadResult uplink_upload_object(ref UplinkProject project, string bucket, string key, UplinkUploadOptions options);

        [LibraryImport(LibraryName, EntryPoint = "uplink_upload_write")]
        internal static partial UplinkWriteResult uplink_upload_write(ref UplinkUpload upload, IntPtr data, UIntPtr dataLen);

        [LibraryImport(LibraryName, EntryPoint = "uplink_upload_set_custom_metadata")]
        internal static partial IntPtr /* UplinkError* */ uplink_upload_set_custom_metadata(ref UplinkUpload upload, UplinkCustomMetadata custom);

        [LibraryImport(LibraryName, EntryPoint = "uplink_upload_commit")]
        internal static partial IntPtr /* UplinkError* */ uplink_upload_commit(ref UplinkUpload upload);

        [LibraryImport(LibraryName, EntryPoint = "uplink_upload_abort")]
        internal static partial IntPtr /* UplinkError* */ uplink_upload_abort(ref UplinkUpload upload);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_upload_result")]
        internal static partial void uplink_free_upload_result(UplinkUploadResult result);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_write_result")]
        internal static partial void uplink_free_write_result(UplinkWriteResult result);

        // ---------
        // Multipart (initiated uploads, parts)
        // ---------

        [LibraryImport(LibraryName, EntryPoint = "uplink_begin_upload", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial UplinkUploadInfoResult uplink_begin_upload(ref UplinkProject project, string bucket, string key, UplinkUploadOptions options);

        [LibraryImport(LibraryName, EntryPoint = "uplink_commit_upload")]
        internal static partial UplinkCommitUploadResult uplink_commit_upload(ref UplinkProject project, ref UplinkUploadInfo info, UplinkCommitUploadOptions options);

        [LibraryImport(LibraryName, EntryPoint = "uplink_abort_upload")]
        internal static partial IntPtr /* UplinkError* */ uplink_abort_upload(ref UplinkProject project, ref UplinkUploadInfo info);

        [LibraryImport(LibraryName, EntryPoint = "uplink_upload_part")]
        internal static partial UplinkPartUploadResult uplink_upload_part(ref UplinkProject project, ref UplinkUploadInfo info, uint partNumber, UIntPtr size);

        [LibraryImport(LibraryName, EntryPoint = "uplink_part_upload_write")]
        internal static partial UplinkWriteResult uplink_part_upload_write(ref UplinkPartUpload partUpload, IntPtr data, UIntPtr dataLen);

        [LibraryImport(LibraryName, EntryPoint = "uplink_part_upload_commit")]
        internal static partial UplinkPartResult uplink_part_upload_commit(ref UplinkPartUpload partUpload);

        [LibraryImport(LibraryName, EntryPoint = "uplink_list_uploads", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial IntPtr /* UplinkUploadIterator* */ uplink_list_uploads(ref UplinkProject project, string bucket, UplinkListUploadsOptions options);

        [LibraryImport(LibraryName, EntryPoint = "upload_iterator_next")]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static partial bool upload_iterator_next(ref UplinkUploadIterator it);

        [LibraryImport(LibraryName, EntryPoint = "upload_iterator_err")]
        internal static partial IntPtr /* UplinkError* */ upload_iterator_err(ref UplinkUploadIterator it);

        [LibraryImport(LibraryName, EntryPoint = "upload_iterator_item")]
        internal static partial IntPtr /* UplinkUploadInfo* */ upload_iterator_item(ref UplinkUploadIterator it);

        [LibraryImport(LibraryName, EntryPoint = "uplink_list_upload_parts")]
        internal static partial IntPtr /* UplinkPartIterator* */ uplink_list_upload_parts(ref UplinkProject project, ref UplinkUploadInfo info, UplinkListUploadPartsOptions options);

        [LibraryImport(LibraryName, EntryPoint = "part_iterator_next")]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static partial bool part_iterator_next(ref UplinkPartIterator it);

        [LibraryImport(LibraryName, EntryPoint = "part_iterator_err")]
        internal static partial IntPtr /* UplinkError* */ part_iterator_err(ref UplinkPartIterator it);

        [LibraryImport(LibraryName, EntryPoint = "part_iterator_item")]
        internal static partial IntPtr /* UplinkPart* */ part_iterator_item(ref UplinkPartIterator it);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_upload_info_result")]
        internal static partial void uplink_free_upload_info_result(UplinkUploadInfoResult result);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_commit_upload_result")]
        internal static partial void uplink_free_commit_upload_result(UplinkCommitUploadResult result);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_part_result")]
        internal static partial void uplink_free_part_result(UplinkPartResult result);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_upload_iterator")]
        internal static partial void uplink_free_upload_iterator(ref UplinkUploadIterator it);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_part_iterator")]
        internal static partial void uplink_free_part_iterator(ref UplinkPartIterator it);

        // ---------
        // Server-side object ops
        // ---------

        [LibraryImport(LibraryName, EntryPoint = "uplink_move_object", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial IntPtr /* UplinkError* */ uplink_move_object(
            ref UplinkProject project,
            string srcBucket, string srcKey,
            string dstBucket, string dstKey,
            UplinkMoveObjectOptions options);

        [LibraryImport(LibraryName, EntryPoint = "uplink_copy_object", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial IntPtr /* UplinkError* */ uplink_copy_object(
            ref UplinkProject project,
            string srcBucket, string srcKey,
            string dstBucket, string dstKey,
            UplinkCopyObjectOptions options);

        [LibraryImport(LibraryName, EntryPoint = "uplink_update_object_metadata", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial IntPtr /* UplinkError* */ uplink_update_object_metadata(
            ref UplinkProject project,
            string bucket, string key,
            UplinkUploadObjectMetadataOptions options);

        // ---------
        // Edge: temporary S3 creds and share URLs
        // ---------

        [LibraryImport(LibraryName, EntryPoint = "edge_register_access", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial EdgeCredentialsResult edge_register_access(
            EdgeConfig config,
            ref UplinkAccess access,
            EdgeRegisterAccessOptions options);

        [LibraryImport(LibraryName, EntryPoint = "edge_free_credentials_result")]
        internal static partial void edge_free_credentials_result(EdgeCredentialsResult result);

        [LibraryImport(LibraryName, EntryPoint = "edge_share_url", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial UplinkStringResult edge_share_url(
            EdgeConfig config,
            ref UplinkAccess access,
            EdgeShareURLOptions options);

        // ---------
        // Utility free helpers for heap-allocated value types
        // ---------

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_bucket")]
        internal static partial void uplink_free_bucket(IntPtr bucket /* UplinkBucket* */);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_object")]
        internal static partial void uplink_free_object(IntPtr obj /* UplinkObject* */);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_upload_info")]
        internal static partial void uplink_free_upload_info(IntPtr info /* UplinkUploadInfo* */);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_part")]
        internal static partial void uplink_free_part(IntPtr part /* UplinkPart* */);

        [LibraryImport(LibraryName, EntryPoint = "uplink_free_string")]
        internal static partial void uplink_free_string(IntPtr cstr /* char* */);
    }
}
