using System.Diagnostics.CodeAnalysis;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: ExcludeFromCodeCoverage(Justification = "Do not report coverage for Test projects.")]
