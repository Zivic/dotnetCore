﻿using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Entities.ErrorModel;

public class ErrorDetails
{
    public int StatusCode { get; set; }
    public string Message { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this);
}