﻿// Copyright (c) 2019 Florian Zimmermann.
// Licensed under the MIT License: https://opensource.org/licenses/MIT
using System;
using System.Runtime.Serialization;

namespace PmlUnit
{
    [Serializable]
    public class ParserException : FormatException
    {
        public ParserException()
        {
        }

        public ParserException(string message)
            : base(message)
        {
        }

        public ParserException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ParserException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
