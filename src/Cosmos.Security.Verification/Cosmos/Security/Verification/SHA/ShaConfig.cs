﻿// ReSharper disable once CheckNamespace
namespace Cosmos.Security.Verification
{
    public class ShaConfig
    {
        /// <summary>
        /// Length of the produced SHA value, in bits.
        /// </summary>
        public int HashSizeInBits { get; internal set; }
        
        /// <summary>
        /// Type of SHA
        /// </summary>
        public ShaTypes Type { get; internal set; }
    }
}