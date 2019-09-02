using System;
using System.Collections.Generic;
using System.Text;
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
        /// <param name="project">The handle to the current project</param>
        /// <param name="bucketName">The name of the bucket to create</param>
        /// <param name="bucketConfig">The configuration the bucket should be created with</param>
        /// <returns>The BucketInfo of the created bucket or throws a BucketCreationException</returns>
        BucketInfo CreateBucket(Project project, string bucketName, BucketConfig bucketConfig);
        /// <summary>
        /// Provides information about a bucket
        /// </summary>
        /// <param name="project">The handle to the current project</param>
        /// <param name="bucketName">The name of the bucket to get the info from</param>
        /// <returns>The BucketInfo of the created bucket or throws a BucketNotFoundException</returns>
        BucketInfo GetBucketInfo(Project project, string bucketName);
        /// <summary>
        /// Opens a bucket for read or write access.
        /// Close the bucket after use with CloseBucket().
        /// </summary>
        /// <param name="project">The handle to the current project</param>
        /// <param name="bucketName">The name of the bucket to open</param>
        /// <param name="encryptionAccess">The access-information to access the bucket-content</param>
        /// <returns>A BucketRef-handle to the opened bucket or throws a BucketNotFoundException</returns>
        BucketRef OpenBucket(Project project, string bucketName, EncryptionAccess encryptionAccess);
        /// <summary>
        /// List the buckets within a project
        /// </summary>
        /// <param name="project">The handle to the current project</param>
        /// <param name="bucketListOptions">Listoptions to control the listed buckets</param>
        /// <returns>The BucketList containing the found buckets or throws a BucketListException</returns>
        BucketList ListBuckets(Project project, BucketListOptions bucketListOptions);
        /// <summary>
        /// Delets a bucket together with it's possible content
        /// </summary>
        /// <param name="project">The handle to the current project</param>
        /// <param name="bucketName">The name of the bucket to delete</param>
        void DeleteBucket(Project project, string bucketName);
        /// <summary>
        /// Closes an opened bucket and releases underlying memory
        /// </summary>
        /// <param name="bucketRef">The BucketRef-handle retrieved from OpenBucket()</param>
        void CloseBucket(BucketRef bucketRef);
    }
}
