using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: ExcludeFromCodeCoverage(Justification = "Provisioner is not production application code.")]
[assembly: InternalsVisibleTo("EntityDb.MongoDb.Tests")]
[assembly: InternalsVisibleTo("EntityDb.RedisMongoDb.Tests")]
