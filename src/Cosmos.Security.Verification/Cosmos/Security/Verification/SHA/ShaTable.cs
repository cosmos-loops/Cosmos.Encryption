﻿using System;

namespace Cosmos.Security.Verification.SHA
{
    internal static class ShaTable
    {
        private static (int, int) Dict(ShaTypes type)
        {
            return type switch
            {
                ShaTypes.Sha1 => (160, 0),
                ShaTypes.Sha224 => (224, 0),
                ShaTypes.Sha256 => (256, 0),
                ShaTypes.Sha384 => (384, 0),
                ShaTypes.Sha512 => (512, 0),
                ShaTypes.Sha512_224 => (224, 0),
                ShaTypes.Sha512_256 => (256, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static ShaConfig Map(ShaTypes type)
        {
            var (hashSize, _) = Dict(type);

            return new()
            {
                Type = type,
                HashSizeInBits = hashSize
            };
        }
    }
}