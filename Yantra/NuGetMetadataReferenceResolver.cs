﻿using Microsoft.CodeAnalysis;
using Microsoft.Threading;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Yantra.Utils;

namespace Yantra
{
    /// <summary>
    /// A <see cref="MetadataReferenceResolver"/> decorator that handles
    /// references to NuGet packages in scripts.  
    /// </summary>
    public class NuGetMetadataReferenceResolver : MetadataReferenceResolver
    {
        private readonly MetadataReferenceResolver _metadataReferenceResolver;
        readonly string folder;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetMetadataReferenceResolver"/> class.
        /// </summary>
        /// <param name="metadataReferenceResolver">The target <see cref="MetadataReferenceResolver"/>.</param>                
        public NuGetMetadataReferenceResolver(MetadataReferenceResolver metadataReferenceResolver, string folder)
        {
            this.folder = folder;
            _metadataReferenceResolver = metadataReferenceResolver;
        }

        public override bool Equals(object other)
        {
            return _metadataReferenceResolver.Equals(other);
        }

        public override int GetHashCode()
        {
            return _metadataReferenceResolver.GetHashCode();
        }

        public override bool ResolveMissingAssemblies => _metadataReferenceResolver.ResolveMissingAssemblies;

        public override PortableExecutableReference ResolveMissingAssembly(MetadataReference definition, AssemblyIdentity referenceIdentity)
        {
            return _metadataReferenceResolver.ResolveMissingAssembly(definition, referenceIdentity);
        }


        public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties)
        {
            if (reference.StartsWith("nuget", StringComparison.OrdinalIgnoreCase))
            {
                // HACK We need to return something here to "mark" the reference as resolved. 
                // https://github.com/dotnet/roslyn/blob/master/src/Compilers/Core/Portable/ReferenceManager/CommonReferenceManager.Resolution.cs#L838
                IEnumerable<string> files = null;
                AsyncPump.Run(async () => {
                    var loader = new Loader();
                    files = await loader.LoadExtensions(new Loader.ExtensionConfiguration(reference), folder);
                });

                return ImmutableArray<PortableExecutableReference>.Empty.Add(
                    MetadataReference.CreateFromFile(typeof(NuGetMetadataReferenceResolver).GetTypeInfo().Assembly.Location));
            }
            var resolvedReference = _metadataReferenceResolver.ResolveReference(reference, baseFilePath, properties);
            return resolvedReference;
        }
    }
}
