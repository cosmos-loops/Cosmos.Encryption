﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cosmos.Security.Verification.Core;

// ReSharper disable once CheckNamespace
namespace Cosmos.Security.Verification
{
    internal class PearsonFunction : StreamableHashFunctionBase, IPearson
    {
        private readonly PearsonConfig _config;

        public PearsonFunction(PearsonConfig config)
        {
            if (config is null)
                throw new ArgumentNullException(nameof(config));

            _config = config.Clone();

            if (_config.Table == null)
                throw new ArgumentException($"{nameof(config)}.{nameof(config.Table)} must be non-null.", $"{nameof(config)}.{nameof(config.Table)}");

            if (_config.Table.Count != 256 || _config.Table.Distinct().Count() != 256)
                throw new ArgumentException($"{nameof(config)}.{nameof(config.Table)} must be a permutation of [0, 255].", $"{nameof(config)}.{nameof(config.Table)}");


            if (_config.HashSizeInBits <= 0 || _config.HashSizeInBits % 8 != 0)
                throw new ArgumentOutOfRangeException($"{nameof(config)}.{nameof(config.HashSizeInBits)}", _config.HashSizeInBits, $"{nameof(config)}.{nameof(config.HashSizeInBits)} must be a positive integer that is divisible by 8.");
        }

        public PearsonConfig Config => _config.Clone();

        public override int HashSizeInBits => _config.HashSizeInBits;

        public override IBlockTransformer CreateBlockTransformer() => new BlockTransformer(_config);

        #region Internal Implementation of BlockTransformer

        private class BlockTransformer : BlockTransformerBase<BlockTransformer>
        {
            private IReadOnlyList<byte> _table;

            private bool _anyBytesProcessed;
            private byte[] _hashValue;

            public BlockTransformer() { }

            public BlockTransformer(PearsonConfig config) : this()
            {
                _table = config.Table;

                _anyBytesProcessed = false;
                _hashValue = new byte[config.HashSizeInBits / 8];
            }

            protected override void CopyStateTo(BlockTransformer other)
            {
                base.CopyStateTo(other);

                other._table = _table;

                other._anyBytesProcessed = false;

                // _hashValue
                {
                    other._hashValue = new byte[_hashValue.Length];

                    Array.Copy(_hashValue, other._hashValue, _hashValue.Length);
                }
            }

            protected override void TransformByteGroupsInternal(ArraySegment<byte> data)
            {
                var dataArray = data.Array;
                var dataCount = data.Count;
                var endOffset = data.Offset + dataCount;

                var tempHashValue = _hashValue;
                var tempAnyBytesProcessed = _anyBytesProcessed;

                var tempTable = _table;
                var tempHashValueLength = tempHashValue.Length;


                for (var currentOffset = data.Offset; currentOffset < endOffset; ++currentOffset)
                {
                    for (int y = 0; y < tempHashValueLength; ++y)
                    {
                        if (tempAnyBytesProcessed)
                            tempHashValue[y] = tempTable[tempHashValue[y] ^ dataArray[currentOffset]];
                        else
                            tempHashValue[y] = tempTable[(dataArray[currentOffset] + y) & 0xff];
                    }

                    tempAnyBytesProcessed = true;
                }

                _anyBytesProcessed = tempAnyBytesProcessed;
            }

            protected override IHashValue FinalizeHashValueInternal(CancellationToken cancellationToken)
            {
                return new HashValue(_hashValue, _hashValue.Length * 8);
            }
        }

        #endregion
    }
}