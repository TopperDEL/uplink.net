using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using uplink.NET.Models;

namespace uplink.NET.Interfaces
{
    /// <summary>
    /// Provides access to the buckets
    /// </summary>
    public interface IBucketService
    {
        /// <summary>
        /// Creates a new bucket
        /// </summary>
        /// <param name="bucketName">The name of the bucket to create</param>
        /// <returns>The created bucket or throws a BucketCreationException</returns>
        Task<Bucket> CreateBucketAsync(string bucketName);
        /// <summary>
        /// Creates a new bucket and ignores the error when it already exists.
        /// </summary>
        /// <param name="bucketName">The name of the bucket to create</param>
        /// <returns>The created bucket or throws a BucketCreationException</returns>
        Task<Bucket> EnsureBucketAsync(string bucketName);
        /// <summary>
        /// Loads a bucket
        /// </summary>
        /// <param name="bucketName">The name of the bucket to load</param>
        /// <returns>The loaded bucket or throws a BucketNotFoundException</returns>
        Task<Bucket> GetBucketAsync(string bucketName);
        /// <summary>
        /// Lists the buckets within a project
        /// </summary>
        /// <param name="listBucketsOptions">ListBucketsOptions to control the listed buckets</param>
        /// <returns>The BucketList containing the found buckets or throws a BucketListException</returns>
        Task<BucketList> ListBucketsAsync(ListBucketsOptions listBucketsOptions);
        /// <summary>
        /// Delets a bucket if it's empty
        /// </summary>
        /// <param name="bucketName">The name of the bucket to delete</param>
        Task DeleteBucketAsync(string bucketName);
        /// <summary>
        /// Delets a bucket together with it's possible content
        /// </summary>
        /// <param name="bucketName">The name of the bucket to delete</param>
        Task DeleteBucketWithObjectsAsync(string bucketName);
    }
}
