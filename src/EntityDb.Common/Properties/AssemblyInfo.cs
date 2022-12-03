using System.Runtime.CompilerServices;

// src
[assembly: InternalsVisibleTo("EntityDb.SqlDb")]
[assembly: InternalsVisibleTo("EntityDb.Npgsql")]
[assembly: InternalsVisibleTo("EntityDb.InMemory")]
[assembly: InternalsVisibleTo("EntityDb.MongoDb")]
[assembly: InternalsVisibleTo("EntityDb.Provisioner")]
[assembly: InternalsVisibleTo("EntityDb.Mvc")]
[assembly: InternalsVisibleTo("EntityDb.Redis")]
[assembly: InternalsVisibleTo("EntityDb.Void")]
[assembly: InternalsVisibleTo("EntityDb.Json")]

// test
[assembly: InternalsVisibleTo("EntityDb.Common.Tests")]
[assembly: InternalsVisibleTo("EntityDb.MongoDb.Tests")]
[assembly: InternalsVisibleTo("EntityDb.Mvc.Tests")]
[assembly: InternalsVisibleTo("EntityDb.Redis.Tests")]
