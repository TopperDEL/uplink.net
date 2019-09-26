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
        /// <param name="bucketConfig">The configuration the bucket should be created with</param>
        /// <returns>The BucketInfo of the created bucket or throws a BucketCreationException</returns>
        Task<BucketInfo> CreateBucketAsync(string bucketName, BucketConfig bucketConfig);
        /// <summary>
        /// Provides information about a bucket
        /// </summary>
        /// <param name="bucketName">The name of the bucket to get the info from</param>
        /// <returns>The BucketInfo of the created bucket or throws a BucketNotFoundException</returns>
        Task<BucketInfo> GetBucketInfoAsync(string bucketName);
        /// <summary>
        /// Opens a bucket for read or write access.
        /// Close the bucket after use with CloseBucket().
        /// </summary>
        /// <param name="bucketName">The name of the bucket to open</param>
        /// <returns>A BucketRef-handle to the opened bucket or throws a BucketNotFoundException</returns>
        Task<BucketRef> OpenBucketAsync(string bucketName);
        /// <summary>
        /// Opens a bucket for read or write access.
        /// Close the bucket after use with CloseBucket().
        /// </summary>
        /// <param name="bucketName">The name of the bucket to open</param>
        /// <param name="encryptionAccess">The (optional) access-information to access the bucket-content. If left empty it is retrieved from the Storj-Environment.</param>
        /// <returns>A BucketRef-handle to the opened bucket or throws a BucketNotFoundException</returns>
        Task<BucketRef> OpenBucketAsync(string bucketName, EncryptionAccess encryptionAccess);
        /// <summary>
        /// List the buckets within a project
        /// </summary>
        /// <param name="bucketListOptions">Listoptions to control the listed buckets</param>
        /// <returns>The BucketList containing the found buckets or throws a BucketListException</returns>
        Task<BucketList> ListBucketsAsync(BucketListOptions bucketListOptions);
        /// <summary>
        /// Delets a bucket together with it's possible content
        /// </summary>
        /// <param name="bucketName">The name of the bucket to delete</param>
        Task DeleteBucketAsync(string bucketName);
        /// <summary>
        /// Closes an opened bucket and releases underlying memory
        /// </summary>
        /// <param name="bucketRef">The BucketRef-handle retrieved from OpenBucket()</param>
        Task CloseBucketAsync(BucketRef bucketRef);
    }
}
