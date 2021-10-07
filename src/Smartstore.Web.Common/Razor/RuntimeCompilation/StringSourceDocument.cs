﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Razor.Language;

namespace Smartstore.Web.Razor.RuntimeCompilation
{
    internal class StringSourceDocument : RazorSourceDocument
    {
        private readonly string _content;
        //private readonly RazorSourceLineCollection _lines;
        private byte[] _checksum;

        public StringSourceDocument(string content, Encoding encoding, RazorSourceDocumentProperties properties)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            _content = content;
            Encoding = encoding;
            FilePath = properties.FilePath;
            RelativePath = properties.RelativePath;

            //_lines = new DefaultRazorSourceLineCollection(this);
        }

        public override char this[int position] => _content[position];

        public override Encoding Encoding { get; }

        public override string FilePath { get; }

        public override int Length => _content.Length;

        public override RazorSourceLineCollection Lines => null;

        public override string RelativePath { get; }

        public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (sourceIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            }

            if (destinationIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex));
            }

            if (count < 0 || count > Length - sourceIndex || count > destination.Length - destinationIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count == 0)
            {
                return;
            }

            _content.CopyTo(sourceIndex, destination, destinationIndex, count);
        }

        public override string GetChecksumAlgorithm()
        {
            return HashAlgorithmName.SHA256.Name;
        }

        public override byte[] GetChecksum()
        {
            if (_checksum == null)
            {
                var charBuffer = _content.ToCharArray();
                var encoder = Encoding.GetEncoder();
                var byteCount = encoder.GetByteCount(charBuffer, 0, charBuffer.Length, flush: true);
                var bytes = new byte[byteCount];
                encoder.GetBytes(charBuffer, 0, charBuffer.Length, bytes, 0, flush: true);

                using (var hashAlgorithm = SHA256.Create())
                {
                    _checksum = hashAlgorithm.ComputeHash(bytes);
                }
            }

            var copiedChecksum = new byte[_checksum.Length];
            _checksum.CopyTo(copiedChecksum, 0);

            return copiedChecksum;
        }
    }
}