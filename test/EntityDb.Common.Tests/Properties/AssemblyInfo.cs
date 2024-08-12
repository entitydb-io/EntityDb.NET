using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: ExcludeFromCodeCoverage(Justification = "Do not report coverage for Test projects.")]
[assembly: InternalsVisibleTo("EntityDb.MongoDb.Tests")]
