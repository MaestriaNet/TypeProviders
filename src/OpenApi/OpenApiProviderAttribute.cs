﻿//----------------------
// <auto-generated>
//     Generated using Maestria.TypeProviders (https://github.com/MaestriaNet/TypeProviders) and use NSwag.Generation tools (https://www.nuget.org/packages/NSwag.Generation)
// </auto-generated>
//----------------------

using System;

[AttributeUsage(AttributeTargets.Class)]
public class OpenApiProviderAttribute : Attribute
{
    public const string TypeFullName = "OpenApiAttribute";

    /// <summary>
    /// OpenAPI (Swagger) specification file from disk or URL
    /// </summary>
    public string Document { get; set; }

    /// <summary>
    /// Path to cache downloaded specification document and increase build time
    /// </summary>
    public string CacheFilePath { get; set; }
}
